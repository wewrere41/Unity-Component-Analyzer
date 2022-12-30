using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ComponentInitializationAnalyzer : DiagnosticAnalyzer
    {
        private const string DiagnosticId = "ComponentInitializationAnalyzer";
        private const string Category = "InitializationSafety";
        private static readonly LocalizableString Title = "Component Initialization Analyzer";
        private static readonly LocalizableString MessageFormat = "'{0}' {1} Component must be initialized";
        private static readonly LocalizableString Description = "Component must be initialized before access";
        private const string HelpLinkUri = "";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description,
            helpLinkUri: HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);


        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.SimpleMemberAccessExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var memberAccessExp = (MemberAccessExpressionSyntax)context.Node;
            var memberTypeSymbol = context.SemanticModel.GetTypeInfo(memberAccessExp.Expression).ConvertedType;

            if (IsComponentType(memberTypeSymbol))
            {
                var classDeclaration = memberAccessExp.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                if (classDeclaration == null)
                    return;

                var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
                if (classSymbol == null)
                    return;

                var memberName = memberAccessExp.Expression.ToString();

                var componentSymbol = GetPrivateFieldWithName(classSymbol, memberName);

                if (componentSymbol == null || HasSerializeFieldAttribute(componentSymbol))
                    return;

                var invocations = classDeclaration.DescendantNodes().OfType<AssignmentExpressionSyntax>();

                foreach (var assignment in invocations)
                {
                    if (assignment.Left is IdentifierNameSyntax left)
                    {
                        var leftSymbol = context.SemanticModel.GetSymbolInfo(left).Symbol;
                        if (leftSymbol != null && leftSymbol.Name == memberName)
                            if (!(assignment.Right is ObjectCreationExpressionSyntax))
                                return;
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(Rule, memberAccessExp.Expression.GetLocation(),
                    $"{memberName}", $"({memberTypeSymbol})"));
            }
        }

        private bool IsComponentType(ITypeSymbol type)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == "Component")
                    return true;
                baseType = baseType.BaseType;
            }

            return false;
        }

        private bool HasSerializeFieldAttribute(ISymbol componentSymbol)
        {
            return componentSymbol.GetAttributes()
                .FirstOrDefault(ad => ad?.AttributeClass?.ToDisplayString() == "UnityEngine.SerializeField") != null;
        }

        private ISymbol GetPrivateFieldWithName(INamedTypeSymbol classSymbol, string memberName)
        {
            return classSymbol.GetMembers(memberName)
                .FirstOrDefault(typeSymbol =>
                    typeSymbol.Kind == SymbolKind.Field &&
                    typeSymbol.DeclaredAccessibility != Accessibility.Public);
        }
    }
}