using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class NoOpHostedServiceTests
{
    private const string HostedServiceBase = """
        using Microsoft.Extensions.Hosting;
        using System.Threading;
        using System.Threading.Tasks;
        using System;

        namespace Microsoft.Extensions.Hosting
        {
            public interface IHostedService
            {
                System.Threading.Tasks.Task StartAsync(System.Threading.CancellationToken cancellationToken);
                System.Threading.Tasks.Task StopAsync(System.Threading.CancellationToken cancellationToken);
            }
        }
        """;

    [Test]
    public async Task NoOp_StartAsync_BlockBody_Should_Report_Diagnostic()
    {
        string code = $$"""
            {{HostedServiceBase}}

            class MyService : IHostedService
            {
                public Task [|StartAsync|](CancellationToken cancellationToken)
                {
                    return Task.CompletedTask;
                }

                public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await AnalyzerVerifier<NoOpHostedServiceAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task NoOp_StartAsync_ArrowBody_Should_Report_Diagnostic()
    {
        string code = $$"""
            {{HostedServiceBase}}

            class MyService : IHostedService
            {
                public Task [|StartAsync|](CancellationToken cancellationToken) => Task.CompletedTask;

                public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await AnalyzerVerifier<NoOpHostedServiceAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task StartAsync_With_Work_Should_Not_Report_Diagnostic()
    {
        string code = $$"""
            {{HostedServiceBase}}

            class MyService : IHostedService
            {
                public Task StartAsync(CancellationToken cancellationToken)
                {
                    Console.WriteLine("Starting...");
                    return Task.CompletedTask;
                }

                public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await AnalyzerVerifier<NoOpHostedServiceAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
