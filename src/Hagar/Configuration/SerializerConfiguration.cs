using System;
using System.Collections.Generic;

namespace Hagar.Configuration
{
    public sealed class SerializerConfiguration
    {
        public HashSet<Type> Activators { get; } = new HashSet<Type>();

        public HashSet<Type> FieldCodecs { get; } = new HashSet<Type>();

        public HashSet<Type> Serializers { get; } = new HashSet<Type>();

        public HashSet<Type> InterfaceProxies { get; } = new HashSet<Type>();
    }

    public sealed class TypeConfiguration
    {
        public Dictionary<uint, Type> WellKnownTypes { get; } = new Dictionary<uint, Type>();

        public Dictionary<string, Type> TypeAliases = new Dictionary<string, Type>();
    }
}