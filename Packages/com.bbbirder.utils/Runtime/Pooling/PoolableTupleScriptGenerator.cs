using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR && BBBIRDER_DEVELOPMENT
using Scriban;
using UnityEditor;
#endif

namespace BBBirder
{
#if UNITY_EDITOR && BBBIRDER_DEVELOPMENT
    [InitializeOnLoad]
    class PoolableTupleScriptGenerator
    {
        static PoolableTupleScriptGenerator()
        {
            GenerateScript();
        }

        static void GenerateScript([CallerFilePath] string csFilePath = null)
        {
            var templatePath = Path.Combine(csFilePath, "..", "PoolableTuple.sbn");
            var templateContent = File.ReadAllText(templatePath);
            var template = Template.Parse(templateContent, templatePath);

            var outputPath = Path.Combine(csFilePath, "..", "PoolableTuple.cs");
            File.WriteAllText(outputPath, template.Render());
        }
    }
#endif

    /// <summary>
    /// Poolable tuple. 
    /// This can be helpful when you need to pass a bundle of arguments or states where must be boxed to an object.
    /// <example>
    /// <code>
    /// <![CDATA[
    /// // unsafe of PoolableTuple.Create(...)
    /// using (var tp = PoolableTuple.Create(3.14f, "pi"))
    /// {
    ///     var (value,name) = tp;
    ///     print(value); // 3.14f
    /// } // tp will be implicitly cleared and released here
    /// 
    /// // unsafe of (...).ToPoolable()
    /// using (var tp = (3.14f, "pi").ToPoolable())
    /// {
    ///     var (value,name) = tp;
    ///     print(value); // 3.14f
    /// } // tp will be implicitly cleared and released here
    /// ]]>
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>
    /// Note: Instances can still be collected by gc without returning to pool
    /// </remarks>
    public static partial class PoolableTuple
    {
        public static PoolableTuple<T1> Get<T1>(T1 value)
        {
            return Storage<T1>.Get(value);
        }

        internal static class Storage<T1>
        {
            static Stack<PoolableTuple<T1>> stack = new();
            public static PoolableTuple<T1> Get(T1 arg1)
            {
                if (stack.TryPop(out var result))
                {
                    result.value = arg1;
                    return result;
                }
                else
                {
                    return new(arg1);
                }
            }

            public static void Release(PoolableTuple<T1> tuple)
            {
                stack.Push(tuple);
            }
        }
    }

    public class PoolableTuple<T1> : IDisposable
    {
        public T1 value;
        public PoolableTuple(T1 value)
        {
            this.value = value;
        }

        public void Dispose()
        {
            PoolableTuple.Storage<T1>.Release(this);
        }
    }
}
