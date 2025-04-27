namespace Models.Mappings;

public static class LLMNameToTokenPrice
{
    private static readonly Dictionary<string, decimal> Mapping = new()
    {
        ["gpt-4o-in"] = 0.0000025m,
        ["gpt-4o-in-cached"] = 0.00000125m,
        ["gpt-4o-out"] = 0.00001m,
        ["gpt-4o-mini-in"] = 0.00000015m,
        ["gpt-4o-mini-in-cached"] = 0.000000075m,
        ["gpt-4o-mini-out"] = 0.0000006m,
        ["gpt-4.1-mini-in"] = 0.0000004m,
        ["gpt-4.1-mini-in-cached"] = 0.0000001m,
        ["gpt-4.1-mini-out"] = 0.0000016m,
        ["gpt-4.1-nano-in"] = 0.0000001m,
        ["gpt-4.1-nano-in-cached"] = 0.000000025m,
        ["gpt-4.1-nano-out"] = 0.0000004m
    };

    public static decimal CalculatePrice(string LLMNameAndQualifier)
    {
        return Mapping[LLMNameAndQualifier];
    }
}