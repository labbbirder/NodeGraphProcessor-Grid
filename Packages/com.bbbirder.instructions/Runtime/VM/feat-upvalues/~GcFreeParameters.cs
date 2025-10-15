using System;
using System.Collections;
using System.Collections.Generic;

namespace BBBirder.Instructions
{
    public class GcFreeParameters
    {
        Dictionary</*parameter type*/Type, /*dict<string, struct?T:object>*/IDictionary> dicts = new();

        public IEnumerable<Type> Types => dicts.Keys;

        public void SetValue<TValue>(string key, TValue value)
        {
            if (typeof(TValue).IsValueType)
            {
                if (!dicts.TryGetValue(typeof(TValue), out var dict))
                {
                    dicts[typeof(TValue)] = dict = DictionaryPool.Rent<TValue>();
                }

                ; ((Dictionary<string, TValue>)dict)[key] = value;
            }
            else
            {
                if (!dicts.TryGetValue(typeof(TValue), out var dict))
                {
                    dicts[typeof(TValue)] = dict = DictionaryPool.Rent<object>();
                }

                ; ((Dictionary<string, object>)dict)[key] = value;
            }
        }

        public void SetValue<TValue>(string key, TValue value, out IDictionary container)
        {
            if (typeof(TValue).IsValueType)
            {
                if (!dicts.TryGetValue(typeof(TValue), out var dict))
                {
                    dicts[typeof(TValue)] = dict = DictionaryPool.Rent<TValue>();
                }

                ; (container = (Dictionary<string, TValue>)dict)[key] = value;
            }
            else
            {
                if (!dicts.TryGetValue(typeof(TValue), out var dict))
                {
                    dicts[typeof(TValue)] = dict = DictionaryPool.Rent<object>();
                }

                ; (container = (Dictionary<string, object>)dict)[key] = value;
            }
        }

        internal void DropValuesWithType(Type type)
        {
            dicts.Remove(type);
        }

        /// <summary>
        /// Move parameters from the other one to self
        /// </summary>
        /// <param name="other"></param>
        internal void MoveParametersFrom(GcFreeParameters other)
        {
            var otherDicts = other.dicts;
            if (otherDicts != null)
            {
                foreach (var (type, dict) in otherDicts)
                {
                    dicts.Add(type, dict);
                }
                otherDicts.Clear();
            }
        }

        public bool TryGetValue<TValue>(string key, out TValue value)// where TValue : struct
        {
            if (typeof(TValue).IsValueType)
            {
                if (dicts.TryGetValue(typeof(TValue), out var dict))
                {
                    value = ((Dictionary<string, TValue>)dict)[key];
                    return true;
                }
                else
                {
                    value = default(TValue);
                    return false;
                }
            }
            else
            {
                if (dicts.TryGetValue(typeof(TValue), out var dict))
                {
                    value = (TValue)((Dictionary<string, object>)dict)[key];
                    return true;
                }
                else
                {
                    value = default(TValue);
                    return false;
                }
            }
        }

        internal IDictionary GetValuesWithType(Type type)
        {
            if (dicts.TryGetValue(type, out var dict))
            {
                return dict;
            }
            return null;
        }

        internal IReadOnlyDictionary<string, T> GetValuesWithType<T>()
        {
            return (IReadOnlyDictionary<string, T>)GetValuesWithType(typeof(T));
        }

        public void DeapClear()
        {
            if (dicts.Count > 0)
            {
                foreach (var (valueType, genericDict) in dicts)
                {
                    DictionaryPool.Return(genericDict);
                }
                dicts.Clear();
            }
        }
    }


    internal static class DictionaryPool
    {
        static Dictionary</*dict type*/Type, Stack<IDictionary>> pools = new();

        public static Dictionary<string, TValue> Rent<TValue>()
        {
            if (pools.TryGetValue(typeof(Dictionary<string, TValue>), out var stack) && stack.Count > 0)
            {
                return (Dictionary<string, TValue>)stack.Pop();
            }
            else
            {
                return new();
            }
        }

        public static void Return(IDictionary dict)
        {
            var dictType = dict.GetType();
            if (pools.TryGetValue(dictType, out var stack))
            {
                pools[dictType] = stack = new();
            }

            dict.Clear();
            stack.Push(dict);
        }
    }

}
