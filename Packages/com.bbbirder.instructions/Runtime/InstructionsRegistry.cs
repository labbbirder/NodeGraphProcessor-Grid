using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.bbbirder;

namespace BBBirder.Instructions
{
    public static class InstructionsRegistry
    {
        static Dictionary<Type, Type> s_returnTypes = new();
        static Dictionary<Type, List<Type>> s_validInstructions = new();
        static Dictionary<Type, InstructionInfo> s_instructionInfos;
        static List<InstructionInfo> s_sortedInstructionInfos;

        public struct InstructionInfo
        {
            public Type type;
            public string category;
            public string displayName;
        }

        public static List<InstructionInfo> GetSortedInstructionInfos()
        {
            EnsureInstructionInfosBuilt();
            return s_sortedInstructionInfos;
        }

        public static InstructionInfo GetInstructionInfo(Type instructionType)
        {

            EnsureInstructionInfosBuilt();
            return s_instructionInfos[instructionType];
        }

        static void EnsureInstructionInfosBuilt()
        {
            if (s_instructionInfos == null)
            {
                s_instructionInfos = new();
                foreach (var type in GetValidInstructions(typeof(void)))
                {
                    var path = type.GetCustomAttribute<CategoryAttribute>()?.Path;
                    var displayName = type.Name;
                    var categoryName = "";
                    if (string.IsNullOrEmpty(path))
                    {
                        // do nothing...
                    }
                    else if (path.IndexOf("/") is int ichar and not ~0)
                    {
                        categoryName = path[..ichar];
                        displayName = path[-~ichar..];
                    }
                    else
                    {
                        categoryName = path;
                    }

                    s_instructionInfos[type] = new()
                    {
                        type = type,
                        category = categoryName,
                        displayName = displayName,
                    };
                }

                s_sortedInstructionInfos = s_instructionInfos.Values.ToList();
                s_sortedInstructionInfos.Sort((a, b) =>
                {
                    var cmp = string.Compare(a.category, b.category);
                    if (cmp != 0)
                    {
                        return cmp;
                    }
                    else
                    {
                        return string.Compare(a.displayName, b.displayName);
                    }
                });

            }
        }


        public static Type GetInstructionReturnType(Type instructionType)
        {
            if (!s_returnTypes.TryGetValue(instructionType, out var returnType))
            {
                s_returnTypes[instructionType] = returnType = ExtractGenericArgumentOf(instructionType, typeof(IInstruction<>));
            }

            return returnType;
        }

        static Type ExtractGenericArgumentOf(Type type, Type genericType)
        {
            for (var baseType = type; baseType != null; baseType = baseType.BaseType)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == genericType)
                {
                    return baseType.GetGenericArguments()[0];
                }
            }

            foreach (var itype in type.GetInterfaces())
            {
                if (itype.IsGenericType && itype.GetGenericTypeDefinition() == genericType)
                {
                    return itype.GetGenericArguments()[0];
                }
            }

            return typeof(void);
        }

        public static bool IsInstructCompatibleForReturnType(Type instrType, Type desiredReturnType)
        {
            var instrReturnType = GetInstructionReturnType(instrType);

            // Unity don't support a generic SerializeReference
            if (instrReturnType.IsGenericTypeParameter)
            {
                return false;
            }

            // type void means no limitation
            if (desiredReturnType == typeof(void))
            {
                return true;
            }

            // desired some return but none was providen
            if (instrReturnType == typeof(void))
            {
                return false;
            }

            // can freely cast to target type
            if (desiredReturnType == instrReturnType || desiredReturnType.IsAssignableFrom(instrReturnType))
            {
                return true;
            }

            // need convert
            if (RuntimeConverter.CanConvert(instrReturnType, desiredReturnType))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static List<Type> GetValidInstructions(Type evaluationType)
        {
            if (!s_validInstructions.TryGetValue(evaluationType, out var candidates))
            {
                var desiredReturnType = ExtractGenericArgumentOf(evaluationType, typeof(Evaluation<>));
                var allSubtypes = Retriever.GetAllSubtypes<IInstruction>();
                s_validInstructions[evaluationType] = candidates = allSubtypes
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .Where(t => IsInstructCompatibleForReturnType(t, desiredReturnType))
                    .ToList();
            }

            return candidates;
        }
    }
}
