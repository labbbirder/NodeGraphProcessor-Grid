using System;
using System.Collections.Generic;
using UnityEngine;

namespace BBBirder
{
    public abstract class DelegateListBase
    {
        public abstract Type ActionType { get; }
        public abstract int Count { get; }
    }

    public abstract class DelegateListBase<TAction> : DelegateListBase
    {
        protected DenseLink<TAction> actions;

        public override sealed Type ActionType => typeof(TAction);
        public override sealed int Count => actions.Count;

        public void AddNew(TAction action)
        {
            actions ??= new();
            actions.Add(action);
        }

        public bool Contains(TAction action)
        {
            if (actions is null) return false;

            return actions.Contains(action);
        }

        public void Remove(TAction action)
        {
            if (actions is not null)
            {
                actions.Remove(action);
            }
        }

        public void AddIfNotExists(TAction action)
        {
            actions ??= new();
            if (!Contains(action))
            {
                actions.Add(action);
            }
        }

        public void Clear()
        {
            actions?.Clear();
        }

        protected CollectionPool.RentScope<List<TAction>> GetPooledCopy(out List<TAction> copy)
        {
            return CollectionPool.Get(out copy);
        }

        protected void LogExcpetionInCallback(Exception e)
        {
            // Logger.Error(e);
            Debug.LogException(e);
        }
    }

    public sealed class DelegateList : DelegateListBase<Action>
    {
        public void Invoke()
        {
            if (actions is null) return;

            using (GetPooledCopy(out var tempActions))
            {
                tempActions.AddRange(actions);
                foreach (var action in tempActions)
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception e)
                    {
                        LogExcpetionInCallback(e);
                    }
                }
            }
        }

        public void InvokeAndClear()
        {
            Invoke();
            Clear();
        }
    }

    public sealed class DelegateList<T1> : DelegateListBase<Action<T1>>
    {
        public void Invoke(T1 arg1)
        {
            if (actions is null) return;

            using (GetPooledCopy(out var tempActions))
            {
                tempActions.AddRange(actions);
                foreach (var action in tempActions)
                {
                    try
                    {
                        action.Invoke(arg1);
                    }
                    catch (Exception e)
                    {
                        LogExcpetionInCallback(e);
                    }
                }
            }
        }

        public void InvokeAndClear(T1 arg1)
        {
            Invoke(arg1);
            Clear();
        }
    }

    public sealed class DelegateList<T1, T2> : DelegateListBase<Action<T1, T2>>
    {
        public void Invoke(T1 arg1, T2 arg2)
        {
            if (actions is null) return;

            using (GetPooledCopy(out var tempActions))
            {
                tempActions.AddRange(actions);
                foreach (var action in tempActions)
                {
                    try
                    {
                        action.Invoke(arg1, arg2);
                    }
                    catch (Exception e)
                    {
                        LogExcpetionInCallback(e);
                    }
                }
            }
        }

        public void InvokeAndClear(T1 arg1, T2 arg2)
        {
            Invoke(arg1, arg2);
            Clear();
        }
    }


    public sealed class DelegateList<T1, T2, T3> : DelegateListBase<Action<T1, T2, T3>>
    {
        public void Invoke(T1 arg1, T2 arg2, T3 arg3)
        {
            if (actions is null) return;

            using (GetPooledCopy(out var tempActions))
            {
                tempActions.AddRange(actions);
                foreach (var action in tempActions)
                {
                    try
                    {
                        action.Invoke(arg1, arg2, arg3);
                    }
                    catch (Exception e)
                    {
                        LogExcpetionInCallback(e);
                    }
                }
            }
        }

        public void InvokeAndClear(T1 arg1, T2 arg2, T3 arg3)
        {
            Invoke(arg1, arg2, arg3);
            Clear();
        }
    }
}
