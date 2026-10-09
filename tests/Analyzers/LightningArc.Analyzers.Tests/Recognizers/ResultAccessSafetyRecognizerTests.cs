using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Recognizers;

/// <summary>
/// Pins the true/false boundaries of
/// <c>LightningArc.Analyzers.ResultAccessSafetyRecognizer</c>.
/// </summary>
/// <remarks>
/// True API shapes verified against
/// <c>src/Analyzers/Recognizers/ResultAccessSafetyRecognizer.cs</c>:
/// <c>IsGuardedByIsSuccess</c>, <c>IsGuardedByIsFailure</c>,
/// <c>IsGuardedByTryCall(access, model, tryMethodName)</c>, and
/// <c>IsGuardedByPrecedingExit(access, model, triggerProperty)</c> (<c>"IsFailure"</c>
/// guards <c>.Value</c>, <c>"IsSuccess"</c> guards <c>.Error</c> — same trigger arguments the
/// LARC001/LARC002 analyzers pass). Each case builds a tiny compilation over the real
/// <c>LightningArc.Results</c> assembly and checks the single <c>.Value</c>/<c>.Error</c>
/// access in the snippet.
/// </remarks>
[Category("Recognizers")]
public class ResultAccessSafetyRecognizerTests
{
    private static readonly Type RecognizerType = RecognizerTestCompilations.GetRecognizerType(
        "ResultAccessSafetyRecognizer"
    );

    private static bool CheckGuard(
        string body,
        string guardMethod,
        string accessName,
        params object?[] extraArgs
    )
    {
        string source =
            """
            using LightningArc.Results;

            public class GuardProbe
            {
                public object? Run(Result<int> result, bool flag)
                {
            """
            + body
            + """
                }
            }
            """;
        CSharpCompilation compilation = RecognizerTestCompilations.Create(source);
        (MemberAccessExpressionSyntax access, SemanticModel model) =
            RecognizerTestCompilations.GetSingleMemberAccess(compilation, accessName);
        object?[] args = [access, model, .. extraArgs];
        return RecognizerTestCompilations.InvokeStatic(RecognizerType, guardMethod, args);
    }

    private static bool IsGuardedByIsSuccess(string body) =>
        CheckGuard(body, "IsGuardedByIsSuccess", "Value");

    private static bool IsGuardedByIsFailure(string body) =>
        CheckGuard(body, "IsGuardedByIsFailure", "Error");

    private static bool IsGuardedByTryCall(string body, string accessName, string tryMethod) =>
        CheckGuard(body, "IsGuardedByTryCall", accessName, tryMethod);

    private static bool IsGuardedByPrecedingExit(string body, string accessName, string trigger) =>
        CheckGuard(body, "IsGuardedByPrecedingExit", accessName, trigger);

    // --- IsSuccess wrapping guards (guarding .Value) ---

    [Test]
    public async Task Value_In_IsSuccess_Then_True()
    {
        await Assert.That(
            IsGuardedByIsSuccess("{ if (result.IsSuccess) { return result.Value; } return null; }")
        ).IsTrue();
    }

    [Test]
    public async Task Value_In_IsFailure_Else_True()
    {
        await Assert.That(
            IsGuardedByIsSuccess(
                "{ if (result.IsFailure) { return null; } else { return result.Value; } return null; }"
            )
        ).IsTrue();
    }

    [Test]
    public async Task Value_In_Ternary_WhenTrue_True()
    {
        await Assert.That(
            IsGuardedByIsSuccess("{ return result.IsSuccess ? result.Value : 0; }")
        ).IsTrue();
    }

    [Test]
    public async Task Value_In_Negated_Ternary_WhenTrue_True()
    {
        await Assert.That(
            IsGuardedByIsSuccess("{ return !result.IsFailure ? result.Value : 0; }")
        ).IsTrue();
    }

    [Test]
    public async Task Value_In_And_Right_True()
    {
        await Assert.That(
            IsGuardedByIsSuccess(
                "{ bool ok = result.IsSuccess && result.Value > 0; return ok; }"
            )
        ).IsTrue();
    }

    [Test]
    public async Task Value_In_Or_Right_With_IsFailure_True()
    {
        await Assert.That(
            IsGuardedByIsSuccess(
                "{ bool ok = result.IsFailure || result.Value > 0; return ok; }"
            )
        ).IsTrue();
    }

    [Test]
    public async Task Value_In_IsFailure_Then_False()
    {
        await Assert.That(
            IsGuardedByIsSuccess("{ if (result.IsFailure) { return result.Value; } return null; }")
        ).IsFalse();
    }

    [Test]
    public async Task Value_In_Ternary_WhenFalse_False()
    {
        await Assert.That(
            IsGuardedByIsSuccess("{ return result.IsSuccess ? 0 : result.Value; }")
        ).IsFalse();
    }

    [Test]
    public async Task Value_In_Left_Of_Or_False()
    {
        await Assert.That(
            IsGuardedByIsSuccess(
                "{ bool ok = result.Value > 0 || result.IsFailure; return ok; }"
            )
        ).IsFalse();
    }

    [Test]
    public async Task Unguarded_Value_False()
    {
        await Assert.That(IsGuardedByIsSuccess("{ return result.Value; }")).IsFalse();
    }

    // --- IsFailure wrapping guards (guarding .Error) ---

    [Test]
    public async Task Error_In_IsFailure_Then_True()
    {
        await Assert.That(
            IsGuardedByIsFailure("{ if (result.IsFailure) { return result.Error; } return null; }")
        ).IsTrue();
    }

    [Test]
    public async Task Error_In_Ternary_WhenFalse_With_IsSuccess_True()
    {
        await Assert.That(
            IsGuardedByIsFailure("{ return result.IsSuccess ? null : result.Error; }")
        ).IsTrue();
    }

    [Test]
    public async Task Error_In_IsSuccess_Then_False()
    {
        await Assert.That(
            IsGuardedByIsFailure("{ if (result.IsSuccess) { return result.Error; } return null; }")
        ).IsFalse();
    }

    [Test]
    public async Task Unguarded_Error_False()
    {
        await Assert.That(IsGuardedByIsFailure("{ return result.Error; }")).IsFalse();
    }

    // --- TryCall guards ---

    [Test]
    public async Task Value_After_TryGetValue_True()
    {
        await Assert.That(
            IsGuardedByTryCall(
                "{ if (result.TryGetValue(out var v)) { return result.Value; } return null; }",
                "Value",
                "TryGetValue"
            )
        ).IsTrue();
    }

    [Test]
    public async Task Error_After_TryGetError_True()
    {
        await Assert.That(
            IsGuardedByTryCall(
                "{ if (result.TryGetError(out var e)) { return result.Error; } return null; }",
                "Error",
                "TryGetError"
            )
        ).IsTrue();
    }

    [Test]
    public async Task Value_After_TryGetValue_Checked_With_Wrong_Name_False()
    {
        await Assert.That(
            IsGuardedByTryCall(
                "{ if (result.TryGetValue(out var v)) { return result.Value; } return null; }",
                "Value",
                "TryGetError"
            )
        ).IsFalse();
    }

    [Test]
    public async Task Unguarded_Value_TryCall_False()
    {
        await Assert.That(IsGuardedByTryCall("{ return result.Value; }", "Value", "TryGetValue"))
            .IsFalse();
    }

    // --- Preceding-exit guards (guarding .Value with trigger "IsFailure") ---

    [Test]
    public async Task Value_After_IsFailure_Return_True()
    {
        await Assert.That(
            IsGuardedByPrecedingExit(
                "{ if (result.IsFailure) { return null; } return result.Value; }",
                "Value",
                "IsFailure"
            )
        ).IsTrue();
    }

    [Test]
    public async Task Value_After_IsFailure_Throw_True()
    {
        await Assert.That(
            IsGuardedByPrecedingExit(
                "{ if (result.IsFailure) { throw new System.Exception(); } return result.Value; }",
                "Value",
                "IsFailure"
            )
        ).IsTrue();
    }

    [Test]
    public async Task Value_After_IsFailure_Continue_True()
    {
        await Assert.That(
            IsGuardedByPrecedingExit(
                "{ while (flag) { if (result.IsFailure) { continue; } return result.Value; } return null; }",
                "Value",
                "IsFailure"
            )
        ).IsTrue();
    }

    [Test]
    public async Task Value_After_IsFailure_Break_True()
    {
        await Assert.That(
            IsGuardedByPrecedingExit(
                "{ while (flag) { if (result.IsFailure) { break; } return result.Value; } return null; }",
                "Value",
                "IsFailure"
            )
        ).IsTrue();
    }

    [Test]
    public async Task Value_After_IsFailure_Block_Ending_In_Exit_True()
    {
        await Assert.That(
            IsGuardedByPrecedingExit(
                "{ if (result.IsFailure) { flag = true; return null; } return result.Value; }",
                "Value",
                "IsFailure"
            )
        ).IsTrue();
    }

    [Test]
    public async Task Error_After_IsSuccess_Return_True()
    {
        await Assert.That(
            IsGuardedByPrecedingExit(
                "{ if (result.IsSuccess) { return null; } return result.Error; }",
                "Error",
                "IsSuccess"
            )
        ).IsTrue();
    }

    [Test]
    public async Task Value_After_Guard_With_Else_False()
    {
        await Assert.That(
            IsGuardedByPrecedingExit(
                "{ if (result.IsFailure) { return null; } else { return null; } return result.Value; }",
                "Value",
                "IsFailure"
            )
        ).IsFalse();
    }

    [Test]
    public async Task Value_After_NonExiting_Guard_False()
    {
        await Assert.That(
            IsGuardedByPrecedingExit(
                "{ if (result.IsFailure) { flag = true; } return result.Value; }",
                "Value",
                "IsFailure"
            )
        ).IsFalse();
    }

    [Test]
    public async Task Value_After_Wrong_Property_Guard_False()
    {
        await Assert.That(
            IsGuardedByPrecedingExit(
                "{ if (result.IsSuccess) { return null; } return result.Value; }",
                "Value",
                "IsFailure"
            )
        ).IsFalse();
    }

    [Test]
    public async Task Value_Guarded_In_Outer_Block_Only_False()
    {
        await Assert.That(
            IsGuardedByPrecedingExit(
                "{ if (result.IsFailure) { return null; } if (flag) { return result.Value; } return null; }",
                "Value",
                "IsFailure"
            )
        ).IsFalse();
    }
}
