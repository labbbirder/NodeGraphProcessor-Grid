using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using com.bbbirder;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BBBirder.Instructions
{
    [RetrieveSubtype]
    public interface IInstruction
    {
        UniTask Execute(CancellationToken cancellation);
        UniTask<T> ExecuteWithConvert<T>(CancellationToken cancellation);
    }

    public interface IInstruction<T> : IInstruction
    {
        UniTask IInstruction.Execute(CancellationToken cancellation) => Execute(cancellation);
        new UniTask<T> Execute(CancellationToken cancellation);
    }

    [Serializable]
    public abstract class Instruction : IInstruction
    {
        public abstract UniTask Execute(CancellationToken cancellation);
        public UniTask<T> ExecuteWithConvert<T>(CancellationToken cancellation)
        {
            return UniTask.FromResult(default(T));
        }
    }

    [Serializable]
    public abstract class Instruction<T> : IInstruction<T>
    {
        public abstract UniTask<T> Execute(CancellationToken cancellation);
        async UniTask<TTo> IInstruction.ExecuteWithConvert<TTo>(CancellationToken cancellation)
        {
            var result = await Execute(cancellation);
            return RuntimeConverter.Convert<T, TTo>(result);
        }
    }

    public interface IEvaluation
    {
        object RunTypeless();
    }

    [Serializable]
    public struct Evaluation : IEvaluation
    {
        [SerializeReference] public IInstruction instruction;

        public UniTask EvalAsync(CancellationToken cancellation = default)
        {
            if (instruction is IInstruction instr)
            {
                return instr.Execute(cancellation);
            }
            else
            {
                return default;
            }
        }

        object IEvaluation.RunTypeless()
        {
            var task = EvalAsync(default);
            if (task.Status is UniTaskStatus.Pending)
            {
                throw new("Evaluation is async");
            }

            return null;
        }

        public Evaluation Clone()
        {
            return JsonUtility.FromJson<Evaluation>(JsonUtility.ToJson(this));
        }
    }

    [Serializable]
    public struct Evaluation<T> : IEvaluation
    {
        public bool useInstruction;
        public T constantValue;
        [SerializeReference] public IInstruction instruction;

        public UniTask<T> RunAsync(CancellationToken cancellation = default)
        {
            if (useInstruction)
            {
                if (instruction is IInstruction<T> instr)
                {
                    return instr.Execute(cancellation);
                }
                else if (instruction is null)
                {
                    return UniTask.FromResult(default(T));
                }
                else if (RuntimeConverter.CanConvert(InstructionsRegistry.GetInstructionReturnType(instruction.GetType()), typeof(T)))
                {
                    return instruction.ExecuteWithConvert<T>(cancellation);
                }
                else
                {
                    return UniTask.FromResult(default(T));
                }
            }
            else
            {
                return UniTask.FromResult(constantValue);
            }
        }

        object IEvaluation.RunTypeless()
        {
            return Run();
        }

        public T Run()
        {
            var task = RunAsync(default);
            if (task.Status is UniTaskStatus.Pending)
            {
                throw new("Evaluation is async");
            }

            return task.GetAwaiter().GetResult();
        }

        public static implicit operator Evaluation<T>(T value)
        {
            return new Evaluation<T>()
            {
                useInstruction = false,
                constantValue = value,
            };
        }
    }

    [Serializable]
    public struct Procedure
    {
        public List<Evaluation> instructions;

        public async UniTask RunAsync(CancellationToken cancellation = default)
        {
            if (instructions == null) return;

            foreach (var evaluation in instructions)
            {
                await evaluation.EvalAsync(cancellation);
            }
        }

    }

    [Category("Bool")]
    public class AndInstruction : Instruction<bool>
    {
        public Evaluation<bool>[] conditions;
        public override UniTask<bool> Execute(CancellationToken cancellation)
        {
            var result = true;
            foreach (var c in conditions)
            {
                result &= c.Run();
                if (!result) break;
            }

            return UniTask.FromResult(result);
        }
    }

    [Category("Bool")]
    public class OrInstruction : Instruction<bool>
    {
        public Evaluation<bool>[] conditions;
        public override UniTask<bool> Execute(CancellationToken cancellation)
        {
            var result = false;
            foreach (var c in conditions)
            {
                result |= c.Run();
                if (result) break;
            }

            return UniTask.FromResult(result);
        }
    }

    [Category("Common")]
    [Serializable]
    public class Log : Instruction
    {
        public Evaluation<string> message;
        public override async UniTask Execute(CancellationToken cancellation)
        {
            Debug.Log(await message.RunAsync(cancellation));
        }
    }

    [Category("Common")]
    [Serializable]
    public class Wait : Instruction
    {
        public Evaluation<float> seconds;
        public override async UniTask Execute(CancellationToken cancellation)
        {
            var secondsValue = await seconds.RunAsync(cancellation);
            await UniTask.Delay((int)(secondsValue * 1000), cancellationToken: cancellation);
        }
    }

    // [Serializable]
    // public class GetNearestUnit : InstructionImpl<Unit>
    // {
    //     public Instruction<Unit> source;
    //     public Instruction<float> range;
    //     public override async UniTask<Unit> Execute(CancellationToken cancellation)
    //     {
    //         return await source.Execute(cancellation);
    //     }
    // }

    [Category("Math")]
    [Serializable]
    public class AddNumber : Instruction<float>
    {
        public Evaluation<float> a;
        public Evaluation<float> b;
        public override async UniTask<float> Execute(CancellationToken cancellation)
        {
            return (await a.RunAsync(cancellation)) + (await b.RunAsync(cancellation));
        }
    }

    [Category("Math"), Description("取相反数")]
    [Serializable]
    public class InvertNumber : Instruction<float>
    {
        public Evaluation<float> value;
        public override async UniTask<float> Execute(CancellationToken cancellation)
        {
            return -await value.RunAsync(cancellation);
        }
    }

    [Category("String")]
    public class GetStringLength : Instruction<float>
    {
        public Evaluation<string> text;

        public override async UniTask<float> Execute(CancellationToken cancellation)
        {
            return (await text.RunAsync(cancellation)).Length;
        }
    }

    [Category("Math")]
    [Serializable]
    public class Vector2Distance : Instruction<float>
    {
        public Evaluation<Vector2> a;
        public Evaluation<Vector2> b;
        public override async UniTask<float> Execute(CancellationToken cancellation)
        {
            return Vector2.Distance(await a.RunAsync(cancellation), await b.RunAsync(cancellation));
        }
    }

    // [Serializable]
    // public class GetUnitPosition : InstructionImpl<Vector2>
    // {
    //     public Instruction<Unit> unit;
    //     public override async UniTask<Vector2> Execute(CancellationToken cancellation)
    //     {
    //         return (await unit.Execute(cancellation)).Position;
    //     }
    // }

    [Serializable]
    public class GetParameter<T> : Instruction<T>
    {
        public ArgumentIndex argIdx;

        public override UniTask<T> Execute(CancellationToken cancellation)
        {
            var arg = ExecutionEngine.RuntimeShared.GetArgument<T>(-1, argIdx.stackOffset);
            return UniTask.FromResult(arg);
        }
    }
}
