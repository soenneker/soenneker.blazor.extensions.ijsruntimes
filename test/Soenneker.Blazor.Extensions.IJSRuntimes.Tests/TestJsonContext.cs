using System.Text.Json.Serialization;

namespace Soenneker.Blazor.Extensions.IJSRuntimes.Tests;

[JsonSerializable(typeof(string))]
internal partial class TestJsonContext : JsonSerializerContext;

