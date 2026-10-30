using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using TUnit.Core;

namespace Soenneker.Blazor.Extensions.IJSRuntimes.Tests;

public sealed class IJSRuntimesExtensionsTests
{
    [Test]
    public async Task DefaultOverloadsPreserveRuntimeTimeoutAndArguments()
    {
        var runtime = new RecordingJsRuntime();
        object?[] args = [42, "value"];
        await runtime.InvokeVoidWithContextAsync("run", args);
        Check(runtime.UsedDefaultTimeout && runtime.Identifier == "run" && ReferenceEquals(args, runtime.Arguments));
        runtime = new RecordingJsRuntime { Result = 42 };
        Check(await runtime.InvokeWithContextAsync<int>("read", args) == 42 && runtime.UsedDefaultTimeout);
        runtime = new RecordingJsRuntime { Result = JsonSerializer.SerializeToElement("value", TestJsonContext.Default.String) };
        Check(await runtime.InvokeWithContextAsync("read", TestJsonContext.Default.String, args) == "value" && runtime.UsedDefaultTimeout);
    }

    [Test]
    public async Task CancellationOverloadsForwardToken()
    {
        using var cts = new CancellationTokenSource();
        var runtime = new RecordingJsRuntime();
        await runtime.InvokeVoidWithContextAsync("run", cts.Token);
        Check(runtime.Token == cts.Token && !runtime.UsedDefaultTimeout);
        await runtime.InvokeWithContextAsync<int>("read", cts.Token);
        Check(runtime.Token == cts.Token && !runtime.UsedDefaultTimeout);
        runtime.Result = JsonSerializer.SerializeToElement("value", TestJsonContext.Default.String);
        Check(await runtime.InvokeWithContextAsync("read", TestJsonContext.Default.String, cts.Token) == "value");
        Check(runtime.Token == cts.Token && !runtime.UsedDefaultTimeout);
    }

    [Test]
    public async Task ScriptFailuresAreWrappedOnceAndLifecycleFailuresAreUnchanged()
    {
        foreach (Exception failure in new Exception[] { new JSException("broken"), new JSDisconnectedException("gone"), new OperationCanceledException(), new ObjectDisposedException("runtime") })
        {
            for (int kind = 0; kind < 9; kind++)
            {
                var runtime = new RecordingJsRuntime { Failure = failure };
                Exception actual = await Capture(async () =>
                {
                    if (kind == 0) await runtime.InvokeVoidWithContextAsync("operation");
                    else if (kind == 1) await runtime.InvokeWithContextAsync<int>("operation");
                    else if (kind == 2) await runtime.InvokeWithContextAsync("operation", TestJsonContext.Default.String);
                    else if (kind == 3) await runtime.InvokeVoidWithContextAsync("operation", CancellationToken.None);
                    else if (kind == 4) await runtime.InvokeWithContextAsync<int>("operation", CancellationToken.None);
                    else if (kind == 5) await runtime.InvokeWithContextAsync("operation", TestJsonContext.Default.String, CancellationToken.None);
                    else if (kind == 6) await runtime.InvokeVoidWithContextAsync("operation", Timeout.InfiniteTimeSpan);
                    else if (kind == 7) await runtime.InvokeWithContextAsync<int>("operation", Timeout.InfiniteTimeSpan);
                    else await runtime.InvokeWithContextAsync("operation", TestJsonContext.Default.String, Timeout.InfiniteTimeSpan);
                });
                if (failure is JSException)
                    Check(actual is JSException && ReferenceEquals(actual.InnerException, failure) && actual.Message.Contains("'operation'"));
                else
                    Check(ReferenceEquals(actual, failure));
            }
        }
    }

    [Test]
    public async Task MetadataSupportsNullAndReportsInvalidJson()
    {
        var runtime = new RecordingJsRuntime { Result = JsonSerializer.SerializeToElement<string?>(null, TestJsonContext.Default.String) };
        Check(await runtime.InvokeWithContextAsync("read", TestJsonContext.Default.String) is null);
        runtime.Result = JsonSerializer.SerializeToElement(42);
        Check(await Capture(async () => { await runtime.InvokeWithContextAsync("read", TestJsonContext.Default.String); }) is JsonException);
        Check(await Capture(async () => { await runtime.InvokeWithContextAsync("read", (System.Text.Json.Serialization.Metadata.JsonTypeInfo<string>)null!); }) is ArgumentNullException);
    }

    [Test]
    public async Task TimeoutOverloadsCancelPendingInvocations()
    {
        for (int kind = 0; kind < 3; kind++)
        {
            var runtime = new RecordingJsRuntime { WaitForCancellation = true };
            Exception actual = await Capture(async () =>
            {
                if (kind == 0) await runtime.InvokeVoidWithContextAsync("wait", TimeSpan.Zero);
                else if (kind == 1) await runtime.InvokeWithContextAsync<int>("wait", TimeSpan.Zero);
                else await runtime.InvokeWithContextAsync("wait", TestJsonContext.Default.String, TimeSpan.Zero);
            });
            Check(actual is OperationCanceledException && runtime.Token.CanBeCanceled && !runtime.UsedDefaultTimeout);
        }
    }

    private static void Check(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Contract assertion failed.");
    }

    private static async Task<Exception> Capture(Func<Task> action)
    {
        try { await action(); }
        catch (Exception exception) { return exception; }
        throw new InvalidOperationException("Expected an exception.");
    }
}

