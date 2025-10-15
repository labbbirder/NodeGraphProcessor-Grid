using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BBBirder.Instructions
{
    [Serializable]
    public struct InstructAction
    {
        [SerializeField] Instruction entryInstr;

        public UniTask Invoke()
        {
            var ee = ExecutionEngine.RuntimeShared;
            using var scope = RuntimeInvocationScope.Create(ee);
            return entryInstr.Execute(default);
        }
    }

    [Serializable]
    public struct InstructAction<T0>
    {
        [SerializeField] Instruction<T0> entryInstr;

        public UniTask Invoke(T0 arg0)
        {
            var ee = ExecutionEngine.RuntimeShared;
            using var scope = RuntimeInvocationScope.Create(ee);
            ee.PushArgumentT<T0>(arg0);
            return entryInstr.Execute(default);
        }
    }
}
