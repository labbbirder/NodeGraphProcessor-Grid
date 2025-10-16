using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BBBirder.Instructions;
using Cysharp.Threading.Tasks;
using GraphProcessor;
using UnityEngine;

namespace BBBirder.Graphs
{
    /// <summary>
    /// A token source that can be reused until Cancel() be called.
    /// </summary>
    public class PooledCancallationTokenSource : IDisposable
    {
        public static Stack<PooledCancallationTokenSource> s_pool = new();

        public static PooledCancallationTokenSource Get()
        {
#if DEBUG
            if (Thread.CurrentThread.ManagedThreadId != 1)
            {
                throw new("PooledCancallationTokenSource can only be used in main thread.");
            }
#endif

            if (s_pool.TryPop(out var inst))
            {
                inst._disposed = false;
                return inst;
            }

            return new();
        }

        private readonly CancellationTokenSource _tokenSource;
        private bool _disposed;

        public CancellationToken Token => _tokenSource.Token;

        private PooledCancallationTokenSource()
        {
            _tokenSource = new();
        }

        public void Cancel()
        {
            _tokenSource.Cancel();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            if (_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Dispose();
            }
            else
            {
                s_pool.Push(this);
            }
        }
    }

    [System.Serializable, NodeMenuItem("" + DisplayName)]
    public class BTTaskNode : BTNode
    {
        [Input, Vertical]
        public ExecutionLink executed;

        const string DisplayName = "Task";
        public override string name => DisplayName;
        public override bool isRenamable => true;

        [SerializeField, ShowInInspector] Evaluation<bool> condition = true;
        [Space(20)]
        [SerializeField, ShowInInspector] Procedure runInstructions;

        UniTask task;
        protected override NodeStatus Run()
        {
            var tokenSource = (graph as BehaviorTree).TokenSource;
            if (!condition.Run()) return NodeStatus.Fault;

            if (Status is NodeStatus.Normal)
            {
                task = runInstructions.RunAsync(tokenSource?.Token ?? default);
            }

            return task.Status switch
            {
                UniTaskStatus.Succeeded => NodeStatus.Success,
                UniTaskStatus.Pending => NodeStatus.Running,
                UniTaskStatus.Canceled or UniTaskStatus.Faulted or _ => NodeStatus.Fault,
            };
        }

        public override void Abort()
        {
            if (task.Status is UniTaskStatus.Pending)
            {
                var tokenSource = (graph as BehaviorTree).TokenSource;
                tokenSource.Cancel();
                tokenSource.Dispose();
            }

            base.Abort();
        }
    }
}
