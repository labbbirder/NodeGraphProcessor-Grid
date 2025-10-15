using System;
using System.Linq.Expressions;
using System.Reflection;
using com.bbbirder;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace BBBirder.Instructions
{
    // [RetrieveSubtype]
    public interface IInvokable
    {

    }

    public interface IInvokable<T> : IInvokable
    {

    }

    [Serializable]
    public struct InstructFunc<TResult> : IInvokable<TResult>
    {
        [SerializeField] Instruction<TResult> entryInstr;
        public UniTask<TResult> Invoke()
        {
            var ee = ExecutionEngine.RuntimeShared;
            using var scope = RuntimeInvocationScope.Create(ee);
            return entryInstr.Execute(default);
        }
    }

    [Serializable]
    public struct InstructFunc<T0, TResult> : IInvokable<TResult>
    {
        [SerializeField] Instruction<TResult> entryInstr;
        public UniTask<TResult> Invoke(T0 arg0)
        {
            var ee = ExecutionEngine.RuntimeShared;
            using var scope = RuntimeInvocationScope.Create(ee);
            ee.PushArgumentT<T0>(arg0);
            return entryInstr.Execute(default);
        }

    }

    // [Serializable]
    // public class Wrapper : ISerializationCallbackReceiver
    // {
    //     [SerializeField] protected string type;
    //     [SerializeField] protected string content;
    //     [NonSerialized] public IInstructionImpl poly;

    //     public virtual void OnBeforeSerialize()
    //     {
    //         if (poly == null)
    //         {
    //             type = null;
    //         }
    //         else
    //         {
    //             type = poly.GetType().FullName;
    //             content = JsonUtility.ToJson(poly);
    //         }
    //     }

    //     public virtual void OnAfterDeserialize()
    //     {
    //         if (string.IsNullOrEmpty(type))
    //         {
    //             poly = null;
    //         }
    //         else
    //         {
    //             poly = JsonUtility.FromJson(content, Type.GetType(type)) as IInstructionImpl;
    //         }
    //     }
    // }

    // [Serializable]
    // public class Wrapper<T> : Wrapper
    // {
    // }


    // class Foo
    // {
    //     [ParamDesc(0, "self", "单位自身")]
    //     public InstructFunc<Unit, int> func;

    //     void Fo()
    //     {
    //         func.Invoke(default(Unit));
    //     }
    // }
    public static class InvokerExtensions
    {
        public static UniTask<R> Invoke<R>(Expression<Func<InstructFunc<R>>> getter)
        {
            var bodyExp = getter.Body is UnaryExpression castOp ? castOp.Operand : getter.Body;
            var memberExp = bodyExp as MemberExpression;
            Assert.IsNotNull(memberExp, $"{bodyExp.NodeType} must be a member getter");
            var field = memberExp.Member as FieldInfo;
            Assert.IsNotNull(field, "member must be a field");
            var instrFunc = getter.Compile()();
            var attr = field.GetCustomAttribute<InspectorNameAttribute>();
            return instrFunc.Invoke();
        }
    }
}
