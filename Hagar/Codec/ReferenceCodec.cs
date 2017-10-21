using System;
using Hagar.Buffers;
using Hagar.Serializer;
using Hagar.Session;
using Hagar.Utilities;
using Hagar.WireProtocol;

namespace Hagar.Codec
{
    public static class ReferenceCodec
    {
        /// <summary>
        /// Indicates that the field being serialized or deserialized is a value type.
        /// </summary>
        /// <param name="session">The serializer session.</param>
        public static void MarkValueField(SerializerSession session)
        {
            session.ReferencedObjects.MarkValueField();
        }

        public static bool TryWriteReferenceField(
            Writer writer,
            SerializerSession session,
            uint fieldId,
            Type expectedType,
            object value)
        {
            if (!session.ReferencedObjects.GetOrAddReference(value, out uint reference))
            {
                return false;
            }

            writer.WriteFieldHeader(session, fieldId, expectedType, value?.GetType(), WireType.Reference);
            writer.WriteVarInt(reference);
            return true;
        }

        public static T ReadReference<T>(Reader reader, SerializerSession session, Field field, ICodecProvider serializers)
        {
            return (T) ReadReference(reader, session, field, serializers, typeof(T));
        }

        public static object ReadReference(Reader reader, SerializerSession session, Field field, ICodecProvider serializers, Type expectedType)
        {
            var reference = reader.ReadVarUInt32();
            if (!session.ReferencedObjects.TryGetReferencedObject(reference, out object value))
            {
                ThrowReferenceNotFound(expectedType, reference);
            }

            if (value is UnknownFieldMarker marker)
            {
                return DeserializeFromMarker(reader, session, field, serializers, marker, reference, expectedType);
            }

            return value;
        }

        private static object DeserializeFromMarker(
            Reader reader,
            SerializerSession session,
            Field field,
            ICodecProvider serializers,
            UnknownFieldMarker marker,
            uint reference,
            Type lastResortFieldType)
        {
            // Create a reader at the position specified by the marker.
            var referencedReader = new Reader(reader.GetBuffers());
            referencedReader.Advance(marker.Offset);

            // Determine the correct type for the field.
            var fieldType = marker.Field.FieldType ?? field.FieldType ?? lastResortFieldType;

            // Get a serializer for that type.
            var specificSerializer = serializers.GetCodec(fieldType);

            // Reset the session's reference id so that the deserialized object overwrites the marker.
            var originalCurrentReferenceId = session.ReferencedObjects.CurrentReferenceId;
            session.ReferencedObjects.CurrentReferenceId = reference - 1;

            // Deserialize the object, replacing the marker in the session.
            try
            {
                return specificSerializer.ReadValue(referencedReader, session, marker.Field);
            }
            finally
            {
                // Revert the reference id.
                session.ReferencedObjects.CurrentReferenceId = originalCurrentReferenceId;
            }
        }

        public static void RecordObject(SerializerSession session, object value) => session.ReferencedObjects.RecordReferenceField(value);
        
        private static void ThrowReferenceNotFound(Type expectedType, uint reference)
        {
            throw new ReferenceNotFoundException(expectedType, reference);
        }
    }
}