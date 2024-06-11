﻿namespace Moq.Analyzers;

internal class MoqAsMethodDescriptor : MoqMethodDescriptorBase
{
    private const string MethodName = "As";

    public override bool IsMatch(SemanticModel semanticModel, MemberAccessExpressionSyntax memberAccessSyntax, CancellationToken cancellationToken)
    {
        if (!IsFastMatch(memberAccessSyntax, MethodName.AsSpan()))
        {
            return false;
        }

        ISymbol? symbol = semanticModel.GetSymbolInfo(memberAccessSyntax, cancellationToken).Symbol;

        if (symbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        if (!IsContainedInMockType(methodSymbol))
        {
            return false;
        }

        return methodSymbol.Name.AsSpan().SequenceEqual(MethodName.AsSpan()) && methodSymbol.IsGenericMethod;
    }
}
