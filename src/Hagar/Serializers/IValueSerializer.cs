﻿using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Hagar.Buffers;
using Hagar.Session;

namespace Hagar.Serializers
{
    /// <summary>
    /// Serializes the content of a value type without framing the type itself.
    /// </summary>
    /// <typeparam name="T">The type which this implementation can serialize and deserialize.</typeparam>
    [SuppressMessage("ReSharper", "TypeParameterCanBeVariant")]
    public interface IValueSerializer<T> where T : struct
    {
        void Serialize<TBufferWriter>(ref Writer<TBufferWriter> writer, SerializerSession session, ref T value) where TBufferWriter : IBufferWriter<byte>;
        void Deserialize(ref Reader reader, SerializerSession session, ref T value);
    }
}