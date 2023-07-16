using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<StringInternBenchmark>();

/// <summary>
/// |             Method |                 word |        Mean |      Error |     StdDev |      Median | Ratio | RatioSD | Rank |   Gen0 | Allocated | Alloc Ratio |
/// |------------------- |--------------------- |------------:|-----------:|-----------:|------------:|------:|--------:|-----:|-------:|----------:|------------:|
/// <b>Из начала</b>
/// <i>SB</i>
/// | WordIsExistsIntern |              Пермь/F |    13.84 us |   0.271 us |   0.405 us |    13.76 us |  0.68 |    0.06 |    1 | 0.0305 |     128 B |        1.00 |
/// |       WordIsExists |              Пермь/F |    19.86 us |   0.819 us |   2.415 us |    19.24 us |  1.00 |    0.00 |    2 | 0.0305 |     128 B |        1.00 |
/// |                    |                      |             |            |            |             |       |         |      |        |           |             |
/// <i>Константа</i>
/// |       WordIsExists |           закупщик/K |   197.99 us |   3.944 us |   7.408 us |   199.56 us |  1.00 |    0.00 |    1 |      - |     128 B |        1.00 |
/// | WordIsExistsIntern |           закупщик/K |   205.59 us |   8.270 us |  24.383 us |   210.84 us |  1.05 |    0.17 |    2 |      - |     128 B |        1.00 |
/// |                    |                      |             |            |            |             |       |         |      |        |           |             |
/// <b>Из середины</b>
/// <i>SB</i>
/// | WordIsExistsIntern | проде(...)/LRTU [23] |   676.08 us |  13.252 us |  23.896 us |   677.02 us |  0.75 |    0.06 |    1 |      - |     128 B |        0.99 |
/// |       WordIsExists | проде(...)/LRTU [23] |   897.94 us |  21.418 us |  58.992 us |   888.36 us |  1.00 |    0.00 |    2 |      - |     129 B |        1.00 |
/// |                    |                      |             |            |            |             |       |         |      |        |           |             |
/// <i>Константа</i>
/// | WordIsExistsIntern |                 прод |   838.06 us |  40.362 us | 119.009 us |   809.35 us |  0.67 |    0.14 |    1 |      - |     128 B |        1.00 |
/// |       WordIsExists |                 прод | 1,267.49 us |  31.934 us |  94.158 us | 1,294.69 us |  1.00 |    0.00 |    2 |      - |     128 B |        1.00 |
/// |                    |                      |             |            |            |             |       |         |      |        |           |             |
/// <b>С конца</b>
/// <i>SB</i>
/// | WordIsExistsIntern |              доход/K | 1,584.71 us |  71.765 us | 211.599 us | 1,493.43 us |  0.63 |    0.08 |    1 |      - |     129 B |        0.99 |
/// |       WordIsExists |              доход/K | 2,509.40 us |  70.744 us | 208.589 us | 2,531.75 us |  1.00 |    0.00 |    2 |      - |     130 B |        1.00 |
/// |                    |                      |             |            |            |             |       |         |      |        |           |             |
/// <i>Константа</i>
/// | WordIsExistsIntern |      догружающийся/A | 2,015.22 us |  47.145 us | 139.009 us | 2,042.35 us |  0.69 |    0.05 |    1 |      - |     130 B |        1.00 |
/// |       WordIsExists |      догружающийся/A | 2,946.15 us |  58.660 us | 152.464 us | 2,989.25 us |  1.00 |    0.00 |    2 |      - |     130 B |        1.00 |
/// |                    |                      |             |            |            |             |       |         |      |        |           |             |
/// <b>Не из словаря</b>
/// <i>SB</i>
/// | WordIsExistsIntern |          неизсловаря | 1,628.33 us |  37.914 us | 103.789 us | 1,604.61 us |  0.64 |    0.10 |    1 |      - |     129 B |        0.99 |
/// |       WordIsExists |          неизсловаря | 2,571.25 us | 135.811 us | 400.441 us | 2,438.09 us |  1.00 |    0.00 |    2 |      - |     130 B |        1.00 |
/// |                    |                      |             |            |            |             |       |         |      |        |           |             |
/// <i>Константа</i>
/// | WordIsExistsIntern |           внесловаря | 1,828.52 us |  85.787 us | 252.945 us | 1,839.73 us |  0.63 |    0.09 |    1 |      - |     131 B |        1.01 |
/// |       WordIsExists |           внесловаря | 3,048.53 us |  60.492 us | 144.936 us | 3,024.13 us |  1.00 |    0.00 |    2 |      - |     130 B |        1.00 |
///
/// Как видно из результатов поиска слов с использованием интернирования и без его него, разница во времени выполнения не значительна, особенно
/// при поиске слов из начала или середины, при этом даже при поиске слов с конца разница не превосходит 2 раз, разница затрат по памяти
/// также незначительна. Из чего можно сделать вывод, что интернирование может быть полезным в случае работы со значительными объемами строк
/// (примерно более 200 000), и, желательно, повторяющимися. Однако и при больших количествах строк пул строк будет значительно расширяться, что
/// также будет влиять на потребление памяти, так как этот пул только расширяется. Поэтому использование интернирования сомнительно и полезно
/// в редких случаях.
///
/// </summary>
[MemoryDiagnoser(displayGenColumns: true)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class StringInternBenchmark
{
    private readonly List<string> _words = new();

    public StringInternBenchmark()
    {
        foreach (string word in File.ReadLines(@".\SpellingDictionaries\ru_RU.dic"))
        {
            this._words.Add(string.Intern(word));
        }
    }

    [Benchmark(Baseline = true)]
    [ArgumentsSource(nameof(SampleData))]
    public bool WordIsExists(string word)
    {
        return this._words.Any(item => word.Equals(item, StringComparison.Ordinal));
    }

    [Benchmark]
    [ArgumentsSource(nameof(SampleData))]
    public bool WordIsExistsIntern(string word)
    {
        string internedWord = string.Intern(word);

        return this._words.Any(item => ReferenceEquals(internedWord, item));
    }

    public IEnumerable<string> SampleData()
    {
        // Из начала
        yield return new StringBuilder().Append("Пермь").Append("/F").ToString();
        yield return "закупщик/K";

        // Из середины
        yield return new StringBuilder().Append("продезинфици").Append("ровать/LRTU").ToString();
        yield return "прод";

        // С конца
        yield return new StringBuilder().Append("доход").Append("/K").ToString();
        yield return "догружающийся/A";

        // Не из словаря
        yield return new StringBuilder().Append("неиз").Append("словаря").ToString();
        yield return "внесловаря";
    }
}
