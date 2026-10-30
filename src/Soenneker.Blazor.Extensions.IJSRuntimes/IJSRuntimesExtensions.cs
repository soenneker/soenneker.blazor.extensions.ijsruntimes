using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Blazor.Extensions.IJSRuntimes;

/// <summary>
/// Adds operation context to JavaScript invocation failures.
/// </summary>
/// <remarks>
/// Script failures retain their original exception as the inner exception. Disconnect and cancellation
/// exceptions propagate unchanged. Overloads without a token or timeout preserve the runtime's default timeout.
/// Metadata overloads support ordinary JSON results, not JS object references or streams that require
/// runtime converters. Their metadata controls result deserialization only; arguments still use runtime serialization.
/// </remarks>
public static class IJSRuntimesExtensions
{
    /// <summary>
    /// Invokes a JavaScript function without a result using the runtime's default timeout.
    /// </summary>
    public static async ValueTask InvokeVoidWithContextAsync(this IJSRuntime runtime, string identifier,
        params object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        try
        {
            await runtime.InvokeVoidAsync(identifier, args).NoSync();
        }
        catch (JSException exception)
        {
            throw new JSException($"JavaScript operation '{identifier}' failed: {exception.Message}", exception);
        }
    }

    /// <summary>
    /// Invokes a JavaScript function and returns its result using the runtime's default timeout.
    /// </summary>
    public static async ValueTask<T> InvokeWithContextAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        this IJSRuntime runtime, string identifier, params object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        try
        {
            return await runtime.InvokeAsync<T>(identifier, args).NoSync();
        }
        catch (JSException exception)
        {
            throw new JSException($"JavaScript operation '{identifier}' failed: {exception.Message}", exception);
        }
    }

    /// <summary>
    /// Invokes a JavaScript function using the runtime's default timeout and deserializes its JSON result
    /// with the supplied metadata.
    /// </summary>
    /// <remarks>JSON deserialization exceptions propagate unchanged. Use the original overloads for JS references and streams.</remarks>
    /// <returns>The deserialized result, which may be null.</returns>
    public static async ValueTask<T?> InvokeWithContextAsync<T>(this IJSRuntime runtime, string identifier,
        JsonTypeInfo<T> jsonTypeInfo, params object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);
        JsonElement result = await runtime.InvokeWithContextAsync<JsonElement>(identifier, args).NoSync();
        return result.Deserialize(jsonTypeInfo);
    }
    /// <summary>
    /// Invokes a JavaScript function without a result using the supplied cancellation token, overriding the default timeout.
    /// </summary>
    public static async ValueTask InvokeVoidWithContextAsync(this IJSRuntime runtime, string identifier,
        CancellationToken cancellationToken, params object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        try
        {
            await runtime.InvokeVoidAsync(identifier, cancellationToken, args).NoSync();
        }
        catch (JSException exception)
        {
            throw new JSException($"JavaScript operation '{identifier}' failed: {exception.Message}", exception);
        }
    }

    /// <summary>
    /// Invokes a JavaScript function and returns its result using the supplied cancellation token, overriding the default timeout.
    /// </summary>
    public static async ValueTask<T> InvokeWithContextAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        this IJSRuntime runtime, string identifier, CancellationToken cancellationToken, params object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        try
        {
            return await runtime.InvokeAsync<T>(identifier, cancellationToken, args).NoSync();
        }
        catch (JSException exception)
        {
            throw new JSException($"JavaScript operation '{identifier}' failed: {exception.Message}", exception);
        }
    }

    /// <summary>
    /// Invokes a JavaScript function using the supplied cancellation token, overriding the default timeout and deserializes its JSON result
    /// with the supplied metadata.
    /// </summary>
    /// <remarks>JSON deserialization exceptions propagate unchanged. Use the original overloads for JS references and streams.</remarks>
    /// <returns>The deserialized result, which may be null.</returns>
    public static async ValueTask<T?> InvokeWithContextAsync<T>(this IJSRuntime runtime, string identifier,
        JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken, params object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);
        JsonElement result = await runtime.InvokeWithContextAsync<JsonElement>(identifier, cancellationToken, args).NoSync();
        return result.Deserialize(jsonTypeInfo);
    }
    /// <summary>
    /// Invokes a JavaScript function without a result using the supplied timeout.
    /// </summary>
    public static async ValueTask InvokeVoidWithContextAsync(this IJSRuntime runtime, string identifier,
        TimeSpan timeout, params object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        try
        {
            await runtime.InvokeVoidAsync(identifier, timeout, args).NoSync();
        }
        catch (JSException exception)
        {
            throw new JSException($"JavaScript operation '{identifier}' failed: {exception.Message}", exception);
        }
    }

    /// <summary>
    /// Invokes a JavaScript function and returns its result using the supplied timeout.
    /// </summary>
    public static async ValueTask<T> InvokeWithContextAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        this IJSRuntime runtime, string identifier, TimeSpan timeout, params object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        try
        {
            return await runtime.InvokeAsync<T>(identifier, timeout, args).NoSync();
        }
        catch (JSException exception)
        {
            throw new JSException($"JavaScript operation '{identifier}' failed: {exception.Message}", exception);
        }
    }

    /// <summary>
    /// Invokes a JavaScript function using the supplied timeout and deserializes its JSON result
    /// with the supplied metadata.
    /// </summary>
    /// <remarks>JSON deserialization exceptions propagate unchanged. Use the original overloads for JS references and streams.</remarks>
    /// <returns>The deserialized result, which may be null.</returns>
    public static async ValueTask<T?> InvokeWithContextAsync<T>(this IJSRuntime runtime, string identifier,
        JsonTypeInfo<T> jsonTypeInfo, TimeSpan timeout, params object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);
        JsonElement result = await runtime.InvokeWithContextAsync<JsonElement>(identifier, timeout, args).NoSync();
        return result.Deserialize(jsonTypeInfo);
    }
}


