using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Moq.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConstructorArgumentsShouldMatchAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        Diagnostics.ConstructorArgumentsShouldMatchId,
        Diagnostics.ConstructorArgumentsShouldMatchTitle,
        Diagnostics.ConstructorArgumentsShouldMatchMessage,
        Diagnostics.Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get { return ImmutableArray.Create(Rule); }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ObjectCreationExpression);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

        var genericName = GetGenericNameSyntax(objectCreation.Type);
        if (genericName == null) return;

        if (!IsMockGenericType(genericName)) return;

        // Full check that we are calling new Mock<T>()
        var constructorSymbol = GetConstructorSymbol(context, objectCreation);

        // Vararg parameter is the one that takes all arguments for mocked class constructor
        var varArgsConstructorParameter = constructorSymbol.Parameters.FirstOrDefault(x => x.IsParams);

        // Vararg parameter are not used, so there are no arguments for mocked class constructor
        if (varArgsConstructorParameter == null) return;
        var varArgsConstructorParameterIdx = constructorSymbol.Parameters.IndexOf(varArgsConstructorParameter);

        // Find mocked type
        var mockedTypeSymbol = GetMockedSymbol(context, genericName);
        if (mockedTypeSymbol == null) return;

        // Skip first argument if it is not vararg - typically it is MockingBehavior argument
        var constructorArguments = objectCreation.ArgumentList.Arguments.Skip(varArgsConstructorParameterIdx == 0 ? 0 : 1).ToArray();

        if (!mockedTypeSymbol.IsAbstract)
        {
            if (IsConstructorMismatch(context, objectCreation, genericName, constructorArguments))
            {
                var diagnostic = Diagnostic.Create(Rule, objectCreation.ArgumentList.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
        else
        {
            // Issue #1: Currently detection does not work well for abstract classes because they cannot be instantiated

            // The mocked symbol is abstract, so we need to check if the constructor arguments match the abstract class constructor

            // Extract types of arguments passed in the constructor call
            var argumentTypes = constructorArguments
                .Select(arg => context.SemanticModel.GetTypeInfo(arg.Expression, context.CancellationToken).Type)
                .ToArray();

            // Check all constructors of the abstract type
            foreach (var constructor in mockedTypeSymbol.Constructors)
            {
                if (AreParametersMatching(constructor.Parameters, argumentTypes))
                {
                    return; // Found a matching constructor
                }
            }

            var diagnostic = Diagnostic.Create(Rule, objectCreation.ArgumentList.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static INamedTypeSymbol GetMockedSymbol(
        SyntaxNodeAnalysisContext context,
        GenericNameSyntax genericName)
    {
        var typeArguments = genericName.TypeArgumentList.Arguments;
        if (typeArguments == null || typeArguments.Count != 1) return null;
        var mockedTypeSymbolInfo = context.SemanticModel.GetSymbolInfo(typeArguments[0], context.CancellationToken);
        var mockedTypeSymbol = mockedTypeSymbolInfo.Symbol as INamedTypeSymbol;
        if (mockedTypeSymbol == null || mockedTypeSymbol.TypeKind != TypeKind.Class) return null;
        return mockedTypeSymbol;
    }

    private static bool AreParametersMatching(ImmutableArray<IParameterSymbol> constructorParameters, ITypeSymbol[] argumentTypes2)
    {
        // Check if the number of parameters matches
        if (constructorParameters.Length != argumentTypes2.Length)
        {
            return false;
        }

        // Check if each parameter type matches in order
        for (int i = 0; i < constructorParameters.Length; i++)
        {
            if (!constructorParameters[i].Type.Equals(argumentTypes2[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static GenericNameSyntax GetGenericNameSyntax(TypeSyntax typeSyntax)
    {
        if (typeSyntax is GenericNameSyntax genericNameSyntax)
        {
            return genericNameSyntax;
        }

        if (typeSyntax is QualifiedNameSyntax qualifiedNameSyntax)
        {
            return qualifiedNameSyntax.Right as GenericNameSyntax;
        }

        return null;
    }

    private static bool IsMockGenericType(GenericNameSyntax genericName)
    {
        return genericName?.Identifier.Text == "Mock" && genericName.TypeArgumentList.Arguments.Count == 1;
    }

    private static IMethodSymbol GetConstructorSymbol(SyntaxNodeAnalysisContext context, ObjectCreationExpressionSyntax objectCreation)
    {
        var constructorSymbolInfo = context.SemanticModel.GetSymbolInfo(objectCreation, context.CancellationToken);
        var constructorSymbol = constructorSymbolInfo.Symbol as IMethodSymbol;
        return constructorSymbol?.MethodKind == MethodKind.Constructor &&
               constructorSymbol.ContainingType?.ConstructedFrom.ToDisplayString() == "Moq.Mock<T>"
            ? constructorSymbol
            : null;
    }

    private static bool IsConstructorMismatch(SyntaxNodeAnalysisContext context, ObjectCreationExpressionSyntax objectCreation, GenericNameSyntax genericName, ArgumentSyntax[] constructorArguments)
    {
        var fakeConstructorCall = SyntaxFactory.ObjectCreationExpression(
            genericName.TypeArgumentList.Arguments.First(),
            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(constructorArguments)),
            null);

        var mockedClassConstructorSymbolInfo = context.SemanticModel.GetSpeculativeSymbolInfo(
            objectCreation.SpanStart, fakeConstructorCall, SpeculativeBindingOption.BindAsExpression);

        return mockedClassConstructorSymbolInfo.Symbol == null;
    }
}
