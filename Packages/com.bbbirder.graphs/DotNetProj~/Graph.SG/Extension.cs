using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Graph.SG
{
    internal static class Extensions
    {
        public static string GetSimpleName(this INamespaceSymbol ns)
        {
            if (ns.IsGlobalNamespace)
            {
                return "";
            }
            return ns.ToString();
        }

        public static bool IsFullNameEquals<T>(this INamedTypeSymbol type)
        {
            return IsFullNameEquals(type, typeof(T));
        }

        public static bool IsFullNameEquals(this INamedTypeSymbol type,Type runtimeType)
        {
            return type.ContainingNamespace.GetSimpleName() == runtimeType.Namespace
                   && (type.IsGenericType
                       ? type.Name +'`' + type.TypeArguments.Length == runtimeType.Name
                       : type.Name == runtimeType.Name
                   );
        }

        public static string GetFullName(this ITypeSymbol type)
        {
            return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
        }

        public static bool IsSubclassOf<T>(this INamedTypeSymbol type)
        {
            type = type.BaseType;
            while (type != null)
            {
                if (type.IsFullNameEquals<T>())
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        public static bool IsTypeOrSubclassOf<T>(this INamedTypeSymbol type)
        {
            while (type != null)
            {
                if (type.IsFullNameEquals<T>())
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        //public static string GetFullName(INamedTypeSymbol type)
        //{
        //    var typesChain = type.GetContainingTypesIncludingSelf().Reverse();

        //    var ns = typesChain.First().ContainingNamespace.GetSimpleName();

        //    if (!string.IsNullOrEmpty(ns))
        //    {
        //        return ns + "." + string.Join(".", typesChain.Select(t=>t.Name));
        //    }
        //    else
        //    {
        //        return string.Join(".", typesChain.Select(t => t.Name));
        //    }
        //}

        public static IEnumerable<INamedTypeSymbol> GetAncestorBaseTypes(this INamedTypeSymbol type, bool includingSelf = false)
        {
            if (includingSelf) yield return type;

            type = type.BaseType;
            while (type != null)
            {
                yield return type;

                type = type.BaseType;
            }
        }

        private static IEnumerable<INamedTypeSymbol> GetContainingTypesIncludingSelf(this INamedTypeSymbol type)
        {

            while (type != null)
            {
                yield return type;
                type = type.ContainingType;
            }
        }

        public static IEnumerable<T> GetAttributes<T>(this ISymbol type) where T : Attribute
        {
            return type.GetAttributes().Where(a => a.AttributeClass.IsFullNameEquals<T>()).Select(ToAttribute<T>);
        }

        public static T GetAttribute<T>(this ISymbol type) where T : Attribute
        {
            return type.GetAttributes<T>().FirstOrDefault();
        }

        public static bool HasAttribute<T>(this ISymbol type) where T : Attribute
        {
            return type.GetAttribute<T>() != null;
        }


        public static AttributeUsageAttribute GetAttributeUsage(this AttributeData a)
        {
            return a.AttributeClass.GetAttribute<AttributeUsageAttribute>();
        }

        public static AttributeTargets GetAttributeTargets(this AttributeData a)
        {
            var usage = a.GetAttributeUsage();
            if (usage != null) return usage.ValidOn;
            return AttributeTargets.All;
        }

        private static T ToAttribute<T>(this AttributeData data) where T : Attribute
        {
            if (data is null) return null;
            var constructorArguments = data.ConstructorArguments.Select(a => a.Value).ToArray();
            var attribute = default(T);
            try
            {
                attribute = Activator.CreateInstance(typeof(T), constructorArguments) as T;
            }
            catch
            {
                attribute = FormatterServices.GetUninitializedObject(typeof(T)) as T;
            }



            foreach (var pair in data.NamedArguments)
            {
                var name = pair.Key;
                var value = pair.Value.Value;
                attribute.GetType().GetProperty(name)?.SetValue(attribute, value);
                attribute.GetType().GetField(name)?.SetValue(attribute, value);
            }
            return attribute as T;
        }


    }
}
