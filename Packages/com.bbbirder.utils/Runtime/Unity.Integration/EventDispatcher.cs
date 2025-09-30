using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BBBirder.UnitySync
{
    /// <summary>
    /// The utility to send event to `IEventListener`s on gameObjects
    /// </summary>
    public static class EventDispatcher
    {
        public struct BroadcastEvent<T>
        {
            public GameObject EventTarget;
            public T CurrentTarget;
        }

        /// <summary>
        /// Send an event to target gameObject
        /// </summary>
        /// <typeparam name="TLis"></typeparam>
        /// <param name="gameObject">Sending target</param>
        /// <param name="action">Action to invoke for each gameObject</param>
        /// <returns>Is consumed by arbitrary component</returns>
        public static bool Send<TLis>(GameObject gameObject, Action<TLis> action)// where TLis : IEventListener
        {
            if (!gameObject) return false;

            using (CollectionPool.Get<List<Component>>(out var comps))
            {
                gameObject.GetComponents(comps);
                var received = false;
                foreach (var comp in comps)
                {
                    if (comp is not TLis lis) continue;
                    // got a valid listener
                    received = true;
                    try
                    {
                        action(lis);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                return received;
            }
        }

        private static bool SendWithEvent<TLis>(GameObject gameObject, Action<BroadcastEvent<TLis>> action, BroadcastEvent<TLis> evt)// where TLis : IEventListener
        {
            if (!gameObject) return false;

            using (CollectionPool.Get<List<Component>>(out var comps))
            {
                gameObject.GetComponents(comps);
                var received = false;
                foreach (var comp in comps)
                {
                    if (comp is not TLis lis) continue;
                    // got a valid listener
                    received = true;
                    try
                    {
                        evt.CurrentTarget = lis;
                        action(evt);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                return received;
            }
        }

        ///<inheritdoc cref="Send"/>
        public static bool Send<TLis>(Component component, Action<TLis> action)// where TLis : IEventListener
        {
            if (!component) return false;
            return Send(component.gameObject, action);
        }

        /// <summary>
        /// BFS-based approach to broadcast event to target hierarchy
        /// </summary>
        /// <typeparam name="TLis"></typeparam>
        /// <param name="gameObject">Sending target</param>
        /// <param name="action">Action to invoke for each gameObject</param>
        public static void Broadcast<TLis>(GameObject gameObject, Action<TLis> action, Func<GameObject, bool> childFilter = null, bool includeFiltered = false) //where TLis : IEventListener
        {
            if (!gameObject) return;

            using (CollectionPool.Get<Queue<GameObject>>(out var comps))
            {
                comps.Enqueue(gameObject);
                while (comps.Count > 0)
                {
                    var frontObj = comps.Dequeue();
                    if (childFilter != null && childFilter(frontObj) == false)
                    {
                        if (includeFiltered)
                        {
                            Send(frontObj, action);
                        }

                        continue;
                    }

                    Send(frontObj, action);

                    var childCount = frontObj.transform.childCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        var child = frontObj.transform.GetChild(i);
                        comps.Enqueue(child.gameObject);
                    }
                }
            }

            /*** Update: BFS approach could be more reasonable in most cases  ***/

            // foreach (Transform child in gameObject.transform)
            // {
            //     if (childFilter != null && childFilter(child) == false) continue;
            //     Broadcast(child.gameObject, action);
            // }
            // Send(gameObject, action);
        }

        public static void BroadcastWithEvent<TLis>(GameObject gameObject, Action<BroadcastEvent<TLis>> action, Func<GameObject, bool> childFilter = null, bool includeFiltered = false) //where TLis : IEventListener
        {
            if (!gameObject) return;

            var evt = new BroadcastEvent<TLis>()
            {
                EventTarget = gameObject,
            };

            using (CollectionPool.Get<Queue<GameObject>>(out var comps))
            {
                comps.Enqueue(gameObject);
                while (comps.Count > 0)
                {
                    var frontObj = comps.Dequeue();
                    if (childFilter != null && childFilter(frontObj) == false)
                    {
                        if (includeFiltered)
                        {
                            SendWithEvent(frontObj, action, evt);
                        }

                        continue;
                    }

                    SendWithEvent(frontObj, action, evt);

                    var childCount = frontObj.transform.childCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        var child = frontObj.transform.GetChild(i);
                        comps.Enqueue(child.gameObject);
                    }
                }
            }
        }

        /// <inheritdoc cref="Broadcast"/>
        public static void Broadcast<TLis>(Component component, Action<TLis> action, Func<GameObject, bool> childFilter = null, bool includeFiltered = false) //where TLis : IEventListener
        {
            if (!component) return;

            Broadcast(component.gameObject, action, childFilter, includeFiltered);
        }

        /// <summary>
        /// Send event upward to ancestors
        /// </summary>
        /// <typeparam name="TLis"></typeparam>
        /// <param name="gameObject">Sending target</param>
        /// <param name="action">Action to invoke for each gameObject</param>
        /// <param name="stopOnFirst">Stops on first gameObject received</param>
        /// <param name="includeSelf">Send to self first</param>
        /// <returns>Is consumed by arbitrary component</returns>
        public static bool PopUpwards<TLis>(GameObject gameObject, Action<TLis> action, bool stopOnFirst = true, bool includeSelf = true)// where TLis : IEventListener
        {
            if (!gameObject) return false;

            if (!includeSelf)
                gameObject = gameObject.transform.parent.gameObject;

            var received = false;
            while (gameObject)
            {
                received |= Send(gameObject, action);
                if (stopOnFirst && received)
                    break;

                gameObject = gameObject.transform.parent.gameObject;
            }

            return received;
        }
    }
}
