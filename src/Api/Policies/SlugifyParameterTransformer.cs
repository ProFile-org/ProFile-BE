using System.Text.RegularExpressions;

namespace Api.Policies;

// Transform api uri into lowercase, dash-like endpoints
public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string TransformOutbound(object? value)
    {
        // Slugify value
        return value == null ? null : Regex.Replace(value.ToString(), "([a-z])([A-Z])", "$1-$2").ToLower();
    }
}