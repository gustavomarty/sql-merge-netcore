using BenchmarkDotNet.Attributes;

[RPlotExporter]
public class Md5VsSha256
{
    [Params(1000, 10000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {

    }

    [Benchmark]
    public void Teste1()
    {
        Task.Delay(1000).Wait();
    }

    [Benchmark]
    public void Teste2()
    {
        Task.Delay(1500).Wait();
    }
}