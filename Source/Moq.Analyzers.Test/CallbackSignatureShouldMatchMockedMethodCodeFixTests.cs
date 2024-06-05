﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Moq.Analyzers.Test;

public class CallbackSignatureShouldMatchMockedMethodCodeFixTests : CallbackSignatureShouldMatchMockedMethodBase
{
    // [Fact]
    public Task ShouldPassWhenCorrectSetupAndReturns()
    {
        return Verify(VerifyCSharpFix(GoodSetupAndReturns));
    }

    // [Fact]
    public Task ShouldSuggestQuickFixWhenIncorrectCallbacks()
    {
        return Verify(VerifyCSharpFix(BadCallbacks));
    }

    // [Fact]
    public Task ShouldPassWhenCorrectSetupAndCallbacks()
    {
        return Verify(VerifyCSharpFix(GoodSetupAndCallback));
    }

    // [Fact]
    public Task ShouldPassWhenCorrectSetupAndParameterlessCallbacks()
    {
        return Verify(VerifyCSharpFix(GoodSetupAndParameterlessCallback));
    }

    // [Fact]
    public Task ShouldPassWhenCorrectSetupAndReturnsAndCallbacks()
    {
        return Verify(VerifyCSharpFix(GoodSetupAndReturnsAndCallback));
    }
}
