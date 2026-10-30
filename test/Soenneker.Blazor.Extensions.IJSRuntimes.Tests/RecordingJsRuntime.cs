using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Soenneker.Blazor.Extensions.IJSRuntimes.Tests;

internal sealed class RecordingJsRuntime : IJSRuntime
{
    public bool UsedDefaultTimeout { get; private set; }
    public string? Identifier { get; private set; }
    public object?[]? Arguments { get; private set; }
    public CancellationToken Token { get; private set; }
    public object? Result { get; set; }
    public Exception? Failure { get; set; }
    public bool WaitForCancellation { get; set; }

    public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, object?[]? args)
    {
        UsedDefaultTimeout = true;
        return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    }

    public async ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        Identifier = identifier;
        Arguments = args;
        Token = cancellationToken;
        await Task.Yield();
        if (WaitForCancellation)
            await Task.Delay(Timeout.Infinite, cancellationToken);
        if (Failure is not null)
            throw Failure;
        return Result is null ? default! : (TValue)Result;
    }


}

