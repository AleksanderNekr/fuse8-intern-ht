using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Newtonsoft.Json.Linq;
using NuGet.ProjectModel;

namespace CurrencyApi.Benchmarks;

public class GettingObject
{
    public static void Run()
    {
        var test = new CompareGettingObject();
        if (test.GetFromNewtonsoft() != test.GetFromMicrosoft())
        {
            throw new Exception("Results are not equal");
        }

        BenchmarkRunner.Run<CompareGettingObject>();
    }
}

/// <summary>
/// |            Method |      Mean |     Error |    StdDev |    Median |   Gen0 | Allocated |
/// |------------------ |----------:|----------:|----------:|----------:|-------:|----------:|
/// | GetFromNewtonsoft | 10.908 us | 0.3232 us | 0.9530 us | 11.258 us | 2.3956 |   7.36 KB |
/// |  GetFromMicrosoft |  2.515 us | 0.0914 us | 0.2694 us |  2.506 us | 0.3662 |   1.13 KB |
///  </summary>
[MemoryDiagnoser]
public class CompareGettingObject
{
    private const string ResponseBody = """
{
    "account_id": 206705450979299328,
    "quotas": {
        "month": {
            "total": 300,
            "used": 51,
            "remaining": 249
        },
        "grace": {
            "total": 0,
            "used": 0,
            "remaining": 0
        }
    }
}
""";

    // Using Newtonsoft.Json
    [Benchmark]
    public MonthSection GetFromNewtonsoft()
    {
        JObject responseParsed = JObject.Parse(ResponseBody);
        var     quotasSection  = responseParsed.GetValue<JObject>("quotas");
        var     monthSection   = quotasSection.GetValue<JObject>("month").ToObject<MonthSection>();

        return monthSection;
    }

    // Using Microsoft.Json
    [Benchmark]
    public MonthSection GetFromMicrosoft()
    {
        JsonDocument responseParsed = JsonDocument.Parse(ResponseBody);
        JsonElement  quotasSection  = responseParsed.RootElement.GetProperty("quotas");
        JsonElement  monthSection   = quotasSection.GetProperty("month");

        return new MonthSection
               {
                   Total     = monthSection.GetProperty("total").GetInt32(),
                   Remaining = monthSection.GetProperty("remaining").GetInt32(),
                   Used      = monthSection.GetProperty("used").GetInt32()
               };
    }
}
