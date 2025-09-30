using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using GraphProcessor;

namespace Graph.SG
{
    [Generator(LanguageNames.CSharp)]
    public class Generator : IIncrementalGenerator
    {
        private static string GetInitializer(GeneratorAttributeSyntaxContext ctx)
        {
            var syntax = ctx.TargetNode as PropertyDeclarationSyntax;
            var init = syntax?.Initializer?.ToString();

            if (init == null)
                return "";

            return init + ";";
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var inputContexts = context.SyntaxProvider.ForAttributeWithMetadataName(
                typeof(InputAttribute).FullName!,
                (n, _) => true,
                (n, _) => (typeof(InputAttribute), n));
            var outputContexts = context.SyntaxProvider.ForAttributeWithMetadataName(
                typeof(OutputAttribute).FullName!,
                (n, _) => true,
                (n, _) => (typeof(OutputAttribute), n));

            var combinedContext = inputContexts.Collect()
                    .Combine(outputContexts.Collect())
                ;

            context.RegisterSourceOutput(combinedContext, static (context, combinedContext) =>
            {
                var (inputContexts, outputContexts) = combinedContext;
                var dictInfos = new Dictionary<INamedTypeSymbol,
                    (List<GeneratorAttributeSyntaxContext> ictxs,
                    List<GeneratorAttributeSyntaxContext> octxs)>(
                    comparer: SymbolEqualityComparer.Default);

                foreach (var (attrType, ctx) in inputContexts.Concat(outputContexts))
                {
                    var isInput = attrType == typeof(InputAttribute);
                    var attrName = attrType.Name;
                    var targetSymbol = ctx.TargetSymbol;
                    var targetLocation = ctx.TargetNode.GetLocation();
                    if (targetSymbol is not IFieldSymbol fieldSymbol)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                            "BB018",
                            "invalid attribute usage",
                            $"{attrName} can only be attached to fields",
                            "bbbirder", DiagnosticSeverity.Error, true), targetLocation));
                        continue;
                    }

                    if (!fieldSymbol.CanBeReferencedByName || fieldSymbol.IsImplicitlyDeclared ||
                        fieldSymbol.IsStatic)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                            "BB019",
                            "invalid attribute usage",
                            $"Invalid field for {attrName}",
                            "bbbirder", DiagnosticSeverity.Error, true), targetLocation));
                        continue;
                    }

                    if (!targetSymbol.ContainingType.IsSubclassOf<BaseNode>())
                    {
                        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                            "BB019",
                            "invalid attribute usage",
                            $"Invalid {attrName} usage ,declaring type is not a BaseNode",
                            "bbbirder", DiagnosticSeverity.Error, true), targetLocation));
                        continue;
                    }

                    if (!dictInfos.TryGetValue(targetSymbol.ContainingType, out var pair))
                    {
                        dictInfos[targetSymbol.ContainingType] = pair = (new(),new());
                    }

                    if (isInput)
                    {
                        pair.ictxs.Add(ctx);
                    }
                    else
                    {
                        pair.octxs.Add(ctx);
                    }
                }

                var template = Template.Parse(Templates.DataIOTemplate);
                foreach (var kvp in dictInfos)
                {
                    var nodeSymbol = kvp.Key;
                    var (ictxs,octxs) = kvp.Value;
                    try
                    {
                        context.AddSource($"{nodeSymbol.GetFullName()}-graph-io.g.cs", template.Render(new
                        {
                            node_name = nodeSymbol.Name,
                            target_namespace = nodeSymbol.ContainingNamespace.GetSimpleName(),
                            module_name = nodeSymbol.ContainingModule.ToDisplayString(),
                            input_fields = ictxs.Select(c =>
                            {
                                var m = c.TargetSymbol as IFieldSymbol;
                                var hasAccessorMethod=  nodeSymbol.GetMembers().OfType<IMethodSymbol>().Any(s =>!s.IsStatic
                                    && s.Name==$"set_{m.Name}"
                                    && s.ReturnsVoid
                                    && s.Parameters.Length==1
                                    && SymbolEqualityComparer.Default.Equals(s.Parameters[0].Type,m.Type));
                                return new
                                {
                                    name = m.Name,
                                    type = m.Type.GetFullName(),
                                    hasAccessorMethod,
                                };
                            }),
                            output_fields = octxs.Select(c =>
                            {
                                var m = c.TargetSymbol as IFieldSymbol;
                                var hasAccessorMethod=  nodeSymbol.GetMembers().OfType<IMethodSymbol>().Any(s =>!s.IsStatic
                                    && s.Name==$"get_{m.Name}"
                                    && s.Parameters.Length==0
                                    && SymbolEqualityComparer.Default.Equals(s.ReturnType,m.Type));
                                return new
                                {
                                    name = m.Name,
                                    type = m.Type.GetFullName(),
                                    hasAccessorMethod,
                                };
                            }),
                        }));
                    }
                    catch (Exception e)
                    {
                        var locationOfSharedObjectSymbol = nodeSymbol.DeclaringSyntaxReferences.FirstOrDefault()
                            ?.GetSyntax()?.GetLocation();
                        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                            "BB010",
                            "source generator inner error",
                            e.Message,
                            "bbbirder", DiagnosticSeverity.Error, true), locationOfSharedObjectSymbol));
                    }
                }
            });
        }
    }
}
