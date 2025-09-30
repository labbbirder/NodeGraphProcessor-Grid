using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.Pool;

namespace BBBirder
{
    /********** TODO: Implement this traits ************
    [AttributeUsage(AttributeTargets.GenericParameter)]
    public class AdditionalConstraintsAttribute : Attribute
    {
        public AdditionalConstraintsAttribute(params Type[] types) { }
    }
    ****************************************************/

    /// <summary>
    /// An utility to pooling and reuse collections like List, Dictionary, Queue, Stack, etc...
    /// <example>
    /// <code>
    /// <![CDATA[
    /// // get an empty list
    /// using (CollectionPool.Get<List<int>>(out var list))
    /// {
    ///     list.Add(100);
    ///     print(list.Count); // 1
    /// } // list will be implicitly cleared and recycled here
    /// ]]>
    /// </code>
    /// </example>
    /// </summary>
    public static class CollectionPool
    {
        static Dictionary<Type, TypedCollectionPool> pools = new();

        static TypedCollectionPool GetPool(Type type)
        {
            if (!pools.TryGetValue(type, out var pool))
            {
                var concreteType = typeof(TypedCollection<>).MakeGenericType(type);
                RuntimeHelpers.RunClassConstructor(concreteType.TypeHandle);
                pool = pools[type];
            }

            return pool;
        }

        public static T Get</*[AdditionalConstraints(typeof(ICollection<>))]*/ T>() where T : class, IEnumerable, new()
        {
            return TypedCollection<T>.poolInstance.Get() as T;
        }

        public static RentScope<T> Get</*[AdditionalConstraints(typeof(ICollection<>))]*/ T>(out T result) where T : class, IEnumerable, new()
        {
            result = TypedCollection<T>.poolInstance.Get() as T;
            return new RentScope<T>(result);
        }

        public static IEnumerable Get(Type type)
        {
            return GetPool(type).Get();
        }

        public static void Release<T>(ICollection<T> instance)
        {
            GetPool(instance.GetType()).Release(instance);
        }

        public static void ReleaseWithoutTypeCheck(IEnumerable instance)
        {
            GetPool(instance.GetType()).Release(instance);
        }

        // Assert: T should be the actual type of instance
        public struct RentScope<T> : IDisposable where T : class, IEnumerable, new()
        {
            internal IEnumerable collection;

            // Adding a pool field? No, it's wasting stack space!

            internal RentScope(IEnumerable collection)
            {
                this.collection = collection;
            }

            public void Dispose()
            {
                if (collection != null)
                {
                    TypedCollection<T>.poolInstance.Release(collection);
                }
            }
        }

        abstract class TypedCollectionPool
        {
            public abstract IEnumerable Get();
            public abstract void Release(IEnumerable collection);
        }

        enum CollectionMetatype
        {
            NotSupported,
            ICollection_T,
            Queue_T,
            Stack_T,
        }

        class TypedCollection<TCollection> where TCollection : class, IEnumerable, new()
        {
            public static TypedCollectionPool poolInstance;
            static CollectionMetatype metatype;

            static TypedCollection()
            {
                var collectionType = typeof(TCollection);
                var elementType = default(Type);
                for (var baseType = collectionType; baseType != null; baseType = baseType.BaseType)
                {
                    if (baseType.IsGenericType)
                    {
                        var genericTypeDefinition = baseType.GetGenericTypeDefinition();
                        if (genericTypeDefinition == typeof(Stack<>))
                        {
                            elementType = baseType.GetGenericArguments()[0];
                            metatype = CollectionMetatype.Stack_T;
                            break;
                        }

                        if (genericTypeDefinition == typeof(Queue<>))
                        {
                            elementType = baseType.GetGenericArguments()[0];
                            metatype = CollectionMetatype.Queue_T;
                            break;
                        }
                    }
                }

                if (metatype == CollectionMetatype.NotSupported)
                {
                    var iType = typeof(TCollection).GetInterfaces()
                        .SingleOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));

                    if (iType != null)
                    {
                        metatype = CollectionMetatype.ICollection_T;
                        elementType = iType.GetGenericArguments()[0];
                    }
                }

                var concreteType = typeof(TypedCollection<>.TypedCollectionPool<>).MakeGenericType(typeof(TCollection), elementType);
                RuntimeHelpers.RunClassConstructor(concreteType.TypeHandle);
            }

            sealed class TypedCollectionPool<TElement> : TypedCollectionPool
            {
                static ObjectPool<TCollection> innerPool = new(
                    static () => new(),
                    actionOnRelease: static c => (c as ICollection<TElement>).Clear()
                );

                static TypedCollectionPool()
                {
                    innerPool = metatype switch
                    {
                        CollectionMetatype.ICollection_T => new ObjectPool<TCollection>(
                            () => new(),
                            actionOnRelease: c => (c as ICollection<TElement>).Clear()
                        ),
                        CollectionMetatype.Queue_T => new ObjectPool<TCollection>(
                           () => new(),
                           actionOnRelease: c => (c as Queue<TElement>).Clear()
                        ),
                        CollectionMetatype.Stack_T => new ObjectPool<TCollection>(
                           () => new(),
                           actionOnRelease: c => (c as Stack<TElement>).Clear()
                        ),
                        _ => throw new($"Type {typeof(TCollection)} is not supported"),
                    };

                    pools[typeof(TCollection)] = poolInstance = new TypedCollectionPool<TElement>();

                }

                public override IEnumerable Get()
                {
                    return innerPool.Get();
                }

                public override void Release(IEnumerable collection)
                {
                    innerPool.Release(collection as TCollection);
                }
            }
        }
    }
}
