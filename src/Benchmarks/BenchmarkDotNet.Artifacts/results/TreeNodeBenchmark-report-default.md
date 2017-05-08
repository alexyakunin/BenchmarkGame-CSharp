
BenchmarkDotNet=v0.10.5, OS=Windows 10.0.15063
Processor=Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), ProcessorCount=8
Frequency=2742186 Hz, Resolution=364.6726 ns, Timer=TSC
dotnet cli version=1.0.3
  [Host]     : .NET Core 4.6.25009.03, 64bit RyuJIT
  DefaultJob : .NET Core 4.6.25009.03, 64bit RyuJIT


                Method |      Mean |     Error |    StdDev |
---------------------- |----------:|----------:|----------:|
       CreateTreeNodes | 578.64 ns | 5.4150 ns | 5.0652 ns |
 CreateTreeNodeStructs | 313.21 ns | 2.5041 ns | 2.2198 ns |
        CountTreeNodes |  92.51 ns | 0.4994 ns | 0.4672 ns |
  CountTreeNodeStructs | 100.75 ns | 0.3920 ns | 0.3667 ns |
