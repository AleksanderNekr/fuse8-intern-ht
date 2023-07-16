using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Fuse8_ByteMinds.SummerSchool.Domain;

namespace Fuse8_ByteMinds.SummerSchool.Benchmarks;

public class AccountProcessorBenchmarks
{
    /// <summary>
    /// |             Method |       Mean |    Error |    StdDev | Rank |   Gen0 | Allocated |
    /// |------------------- |-----------:|---------:|----------:|-----:|-------:|----------:|
    /// |   CalculateDefault | 1,174.5 ns | 57.54 ns | 169.65 ns |    2 | 2.1420 |    6720 B |
    /// | CalculatePerformed |   489.7 ns | 19.10 ns |  56.33 ns |    1 |      - |         - |
    ///
    /// Как видно из результатов, улучшенная версия работает более чем в 2 раза быстрее, чем стандартная, так как
    /// с помощью ключевого слова `in` происходит передача по неизменяемой ссылке структуры, а не ее копирование, как
    /// в стандартном методе. Также не происходит аллокаций, так как была определена улучшенная версия вычисления операции 3,
    /// которая не оборачивает структуру в интерфейс ITotalAmount, тем самым создавая ее боксинг, как в стандартной версии,
    /// а происходит также передача по ссылке самой структуры, при условии что она реализует этот интерфейс, что было
    /// достигнуто благодаря обобщению.
    ///
    /// </summary>
    [MemoryDiagnoser]
    [RankColumn]
    public class AccountProcessorBenchmark
    {
        private AccountProcessor _accountProcessor = null!;
        private BankAccount      _bankAccount;

        [GlobalSetup]
        public void SetUp()
        {
            this._accountProcessor = new AccountProcessor();
            this._bankAccount = new BankAccount();
        }

        [Benchmark]
        public decimal CalculateDefault()
        {
            return this._accountProcessor.Calculate(this._bankAccount);
        }

        [Benchmark]
        public decimal CalculatePerformed()
        {
            return this._accountProcessor.CalculatePerformed(in this._bankAccount);
        }
    }

    public static void Run()
    {
        BenchmarkRunner.Run<AccountProcessorBenchmark>();
    }
}
