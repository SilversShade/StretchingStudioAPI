using System.Text.RegularExpressions;

namespace StretchingStudioAPI.Middleware.Transformers;

public partial class ToKebabParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value) => value is not null 
        ? MyRegex().Replace(value.ToString()!, "$1-$2").ToLower()
        : null;
    
    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex MyRegex();
}