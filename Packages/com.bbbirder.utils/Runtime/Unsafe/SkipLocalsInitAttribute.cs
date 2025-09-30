#if REDEFINE_SKIP_LOCALS_INIT
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
    public class SkipLocalsInitAttribute : Attribute { }
}
#endif
