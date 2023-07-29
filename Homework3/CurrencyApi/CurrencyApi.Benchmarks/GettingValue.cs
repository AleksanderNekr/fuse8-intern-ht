using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Newtonsoft.Json.Linq;
using NuGet.ProjectModel;

var test = new CompareGettingValue();
if (test.GetCurrencyFromResponseNewtonsoft() != test.GetCurrencyFromResponseMicrosoft())
{
    throw new Exception("Results are not equal");
}

BenchmarkRunner.Run<CompareGettingValue>();

/// <summary>
/// |                            Method |     Mean |     Error |    StdDev |   Gen0 | Allocated |
/// |---------------------------------- |---------:|----------:|----------:|-------:|----------:|
/// | GetCurrencyFromResponseNewtonsoft | 5.243 us | 0.1532 us | 0.4394 us | 1.8768 |    5896 B |
/// |  GetCurrencyFromResponseMicrosoft | 1.542 us | 0.0308 us | 0.0779 us | 0.2060 |     648 B |
/// </summary>
[MemoryDiagnoser]
public class CompareGettingValue
{
    private const string CurrencyCode = "RUB";

    private const string ResponseBody = """
{
    "meta": {
        "last_updated_at": "2023-07-28T23:59:59Z"
    },
    "data": {
        "RUB": {
            "code": "RUB",
            "value": 92.060390023
        }
    }
}
""";

    // Using Newtonsoft.Json
    [Benchmark]
    public decimal GetCurrencyFromResponseNewtonsoft()
    {
        JObject responseParsed  = JObject.Parse(ResponseBody);
        var     dataSection     = responseParsed.GetValue<JObject>("data");
        var     currencySection = dataSection.GetValue<JObject>(CurrencyCode);
        var     value           = currencySection.GetValue<decimal>("value");

        return value;
    }

    // Using Microsoft.Json
    [Benchmark]
    public decimal GetCurrencyFromResponseMicrosoft()
    {
        JsonDocument responseParsed  = JsonDocument.Parse(ResponseBody);
        JsonElement  dataSection     = responseParsed.RootElement.GetProperty("data");
        JsonElement  currencySection = dataSection.GetProperty(CurrencyCode);
        decimal      value           = currencySection.GetProperty("value").GetDecimal();

        return value;
    }
}
