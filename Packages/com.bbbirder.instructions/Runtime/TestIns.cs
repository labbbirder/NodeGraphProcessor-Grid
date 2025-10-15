using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using BBBirder.Instructions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

// struct Foo
// {
//     public int value;
// }

// struct Bar
// {
//     public Foo foo;

// }

// object bar = new Bar()
// {
//     foo = new Foo()
//     {
//         value = 123213,
//     }
// };

// var fiFoo = typeof(Bar).GetField("foo");
// var fiValue = typeof(Foo).GetField("value");

// fiValue.SetValue(fiFoo.GetValue(__makeref(bar)), 777);

// ((Bar)bar).foo.value

class TestIns : MonoBehaviour
{
    [SerializeField] Instruction<float> gggg;
    Dictionary<string, object> dict = new();
    static int ti_id;
    private int id;
    List<Func<object>> ddd = new();

    // public Wrapper wrapper2 = new Wrapper()
    // {
    //     poly = new Constant<float>()
    //     {
    //         value = 23
    //     }
    // };
    ExecutionEngine ee;
    [Button]
    public void Invoke(int trace = -1, int stack = -1)
    {
        ee = ExecutionEngine.RuntimeShared;

        using (RuntimeInvocationScope.Create(ee))
        {
            ee.PushArgument("sss");
            ee.PushArgument<int>(123);
            ee.PushArgument<double>(123.13);
            ee.PushArgument("asdasd");
            using (RuntimeInvocationScope.Create(ee))
            {
                ee.PushArgument("aaaa");
                ee.PushArgument<int>(123213);
                ee.PushArgument("bbbb");
                ee.PushArgument("cccc");
                print(ee.GetArgument<string>(trace, stack));
            }
        }
    }

    void Awake()
    {
        id = Interlocked.Increment(ref ti_id) - 1;
        ddd.AddRange(Enumerable.Range(0, 18).Select(_ => default(Func<object>)));
        ddd[(int)TypeCode.Int32] = () => "3";
        ddd[(int)TypeCode.Double] = () => "3";
        ddd[(int)TypeCode.Single] = () => "3";
        ddd[(int)TypeCode.Int64] = () => "3";
        ddd[(int)TypeCode.Boolean] = () => "3";
        ddd[(int)TypeCode.Char] = () => "3";
        dict.Add("sd", "123");
        dict.Add("3sd", "123");
        dict.Add("s2d", "123");
    }

    void Update()
    {
        const int cnt = 10000;
        Profiler.BeginSample("srtd");
        for (int i = 0; i < cnt; i++)
        {
            Std<int>();
            Std<float>();
            Std<List<int>>();
            Std<List<float>>();
        }
        Profiler.EndSample();

        Profiler.BeginSample("opt1");
        for (int i = 0; i < cnt; i++)
        {
            Opt1<int>();
            Opt1<float>();
            Opt1<List<int>>();
            Opt1<List<float>>();
        }
        Profiler.EndSample();


        Profiler.BeginSample("opt2");
        for (int i = 0; i < cnt; i++)
        {
            Opt2<int>();
            Opt2<float>();
            Opt2<List<int>>();
            Opt2<List<float>>();
        }
        Profiler.EndSample();

        // Profiler.BeginSample("dele");
        // for (int i = 0; i < cnt; i++)
        // {
        //     Func3(typeof(int));
        //     Func3(typeof(float));
        //     Func3(typeof(double));
        //     Func3(typeof(long));
        // }
        // Profiler.EndSample();

        // Profiler.BeginSample("dele");
        // for (int i = 0; i < cnt; i++)
        // {
        //     Func2(typeof(int));
        //     Func2(typeof(float));
        //     Func2(typeof(double));
        //     Func2(typeof(long));
        // }
        // Profiler.EndSample();

        // for (int i = 0; i < cnt; i++)
        // {

        // }

        // for (int i = 0; i < cnt; i++)
        // {

        // }

    }
    public string Fako;
    public void GetByName()
    {
        var fieldInfo = typeof(TestIns).GetField("Fako");
        var v = fieldInfo.GetValue(this);
    }
    public void Invoke(Expression<Func<object>> getter)
    {
        var bodyExp = getter.Body is UnaryExpression castOp ? castOp.Operand : getter.Body;
        var memberExp = bodyExp as MemberExpression;
        // Assert.IsNotNull(memberExp, $"{bodyExp.NodeType} must be a member getter");
        var field = memberExp.Member as FieldInfo;
        // Assert.IsNotNull(field, "member must be a field");
        var exprInst = memberExp.Expression as ConstantExpression;
        var inst = exprInst.Value;
        var v = field.GetValue(inst);
        // var instrFunc = getter.Compile()();
        // var attr = field.GetCustomAttribute<InspectorNameAttribute>();
    }

    object Func3(Type type)
    {
        return ddd[(int)Type.GetTypeCode(type)]();
    }

    Dictionary<TypeCode, object> ddd2 = new(){
        {TypeCode.Int32,"3"},
        {TypeCode.Double,"3f"},
        {TypeCode.Single,"3f"},
        {TypeCode.Int64,"3f"},
        {TypeCode.Boolean,"3f"},
        {TypeCode.Char,"3f"},
    };
    object Func2(Type type)
    {
        return ddd2.TryGetValue(Type.GetTypeCode(type), out var v) ? v : null;
    }

    object Func1(Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Int32:
                return "1";
            case TypeCode.Double:
                return "1d";
            case TypeCode.Single:
                return "1f";
            case TypeCode.Int64:
                return "1l";
            case TypeCode.Boolean:
                return "true";
            case TypeCode.Char:
                return "'a'";
            default:
                return "null";
        }
    }
    Dictionary<Type, IList> dic = new();
    List<IList> lst = new();
    List<IList> lst2 = new();
    private IList Std<T>()
    {
        if (!dic.TryGetValue(typeof(T), out var result))
        {
            dic[typeof(T)] = result = new List<T>();
        }
        return (IList)result;
    }
    private IList Opt1<T>()
    {
        var token = Tokens.GetType<T>.token;

        // ensure capacity
        if (lst.Count <= token)
        {
            lst.Capacity = token + 1;
        }

        // extend list
        for (int i = lst.Count; i <= token; i++)
        {
            lst.Add(null);
        }

        return (IList)(lst[token] ??= new List<T>());
    }



    private IList Opt2<T>()
    {
        var token = Tokens2.AsType<T>.GetToken(this);

        return (IList)lst2[token];
    }



    private struct Tokens
    {
        private static int s_id;
        internal class GetType<T>
        {
            public static int token;
            static GetType()
            {
                token = Interlocked.Increment(ref s_id);
            }
        }
    }

    private struct Tokens2
    {
        private static int s_id;
        internal static class AsType<T>
        {
            static int[] indices = new int[1] { -1 };
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetToken(TestIns inst)
            {
                var id = inst.id;
                CollectionUtils.EnsureSize(ref indices, id + 1, -1);
                ref var idx = ref indices[id];
                if (idx == -1)
                {
                    idx = inst.lst2.Count;
                    inst.lst2.Add(new List<T>());
                }
                return idx;
            }
        }
    }

    static class CollectionUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureSize<T>(List<T> list, int size, T defaultValue = default)
        {
            if (list.Count >= size)
            {
                return;
            }
            list.Capacity = size + (size >> 1);

            for (int i = list.Count; i < size; i++)
            {
                list.Add(defaultValue);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureSize<T>(ref T[] list, int size, T defaultValue = default)
        {
            var len = list.Length;
            if (len >= size)
            {
                return;
            }
            Array.Resize(ref list, size + (size >> 1));
            for (int i = len; i < list.Length; i++)
            {
                list[i] = defaultValue;
            }
        }
    }
}
