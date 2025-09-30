using System;
using System.Collections.Generic;

namespace BBBirder
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TypeHandleFilterAttribute : Attribute
    {
        public readonly Type BaseType;
        public TypeHandleFilterAttribute(Type baseType)
        {
            this.BaseType = baseType;
        }
    }

    [Serializable]
    public struct TypeHandle
    {
        static Dictionary<string, Type> s_cache = new();
        public string AQN;
        public Type Type
        {
            get
            {
                if (!s_cache.TryGetValue(AQN, out var type))
                {
                    s_cache[AQN] = type = Type.GetType(AQN, false);
                }

                return type;
            }
        }
    }
}
