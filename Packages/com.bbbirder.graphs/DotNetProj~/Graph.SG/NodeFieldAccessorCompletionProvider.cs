using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using GraphProcessor;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Document = Microsoft.CodeAnalysis.Document;

namespace Graph.SG
{
    [ExportCompletionProvider(nameof(NodeFieldAccessorCompletionProvider),LanguageNames.CSharp)]
    public class NodeFieldAccessorCompletionProvider : CompletionProvider
    {
        public override async Task ProvideCompletionsAsync(CompletionContext context)
        {
            if (!context.Document.SupportsSemanticModel) return;
            if (!context.Document.SupportsSyntaxTree) return;
            
            var cancellationToken = context.CancellationToken;
            
            var model = await context.Document.GetSemanticModelAsync(cancellationToken);
            if (model is null) return;

            var tree = await context.Document.GetSyntaxTreeAsync(cancellationToken);
            if (tree is null) return;

            var root = tree.GetRoot();
            var node = root.FindNode(context.CompletionListSpan);
            
            var targetNode = node.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().FirstOrDefault()!;
            // var node = GetDeclarationSyntax(tree, context.Position);
            // if (node is null) return;

            var nodeDeclarationSyntax = node.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault() as ClassDeclarationSyntax;
            if (nodeDeclarationSyntax is null) return;
            
            var nodeSymbol = model.GetDeclaredSymbol(nodeDeclarationSyntax)!;

            if (!nodeSymbol.IsSubclassOf<BaseNode>()) return;

            context.CompletionListSpan = targetNode.Span;
            var leadingTrivia = targetNode.GetLeadingTrivia().LastOrDefault();
            var indentSpaces = leadingTrivia is { } && leadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia)
                ? new string(leadingTrivia.ToString()[0], leadingTrivia.Span.Length)
                : "";
            foreach (var f in nodeSymbol.GetMembers().OfType<IFieldSymbol>())
            {
                var tags = ImmutableArray.Create(new[] { "Method" });
                if (f.Type is not INamedTypeSymbol ts || ts.IsFullNameEquals<ExecutionLink>())
                    continue;

                if (f.HasAttribute<InputAttribute>())
                {
                    var hasAccessorMethod=  nodeSymbol.GetMembers().OfType<IMethodSymbol>().Any(s =>!s.IsStatic
                        && s.Name==$"set_{f.Name}"
                        && s.ReturnsVoid
                        && s.Parameters.Length==1
                        && SymbolEqualityComparer.Default.Equals(s.Parameters[0].Type,f.Type));
                    if (hasAccessorMethod) continue;

                    var snippet = $"set_{f.Name}";
                    var item = CompletionItem.Create(
                        snippet,
                        snippet,
                        tags: tags,
                        properties: ImmutableDictionary<string, string>.Empty
                            .Add("replacement",$@"
{indentSpaces}private void set_{f.Name}({f.Type} value)
{indentSpaces}{{
{indentSpaces}    {f.Name} = value;
{indentSpaces}}}
")
                    );
                    context.AddItem(item);

                }

                if (f.HasAttribute<OutputAttribute>())
                {
                    var hasAccessorMethod=  nodeSymbol.GetMembers().OfType<IMethodSymbol>().Any(s =>!s.IsStatic
                        && s.Name==$"get_{f.Name}"
                        && s.Parameters.Length==0
                        && SymbolEqualityComparer.Default.Equals(s.ReturnType,f.Type));
                    if (hasAccessorMethod) continue;
                    
                    var snippet = $"get_{f.Name}";
                    var item = CompletionItem.Create(
                        snippet,
                        snippet,
                        tags: tags,
                        properties: ImmutableDictionary<string, string>.Empty
                            .Add("replacement",$@"
{indentSpaces}private {f.Type} get_{f.Name}()
{indentSpaces}{{
{indentSpaces}    return {f.Name};
{indentSpaces}}}
")
                    );

                    context.AddItem(item);
                }
            }
        }

        public override async Task<CompletionChange> GetChangeAsync(Document document, CompletionItem item, char? commitKey, CancellationToken cancellationToken)
        {
            if (item.Properties.TryGetValue("replacement", out var replacement))
            {
                return CompletionChange.Create(new TextChange(item.Span, replacement));
            }

            return await base.GetChangeAsync(document, item, commitKey, cancellationToken);
        }

    }
}