``` ini

BenchmarkDotNet=v0.10.5, OS=Windows 10.0.15063
Processor=Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), ProcessorCount=8
Frequency=2742186 Hz, Resolution=364.6726 ns, Timer=TSC
dotnet cli version=1.0.3
  [Host]     : .NET Core 4.6.25009.03, 64bit RyuJIT
  DefaultJob : .NET Core 4.6.25009.03, 64bit RyuJIT


```
 |                Method |     Mean |     Error |    StdDev |
 |---------------------- |---------:|----------:|----------:|
 |       CreateTreeNodes | 545.4 ns | 2.8630 ns | 2.6780 ns |
 | CreateTreeNodeStructs | 317.8 ns | 3.2890 ns | 3.0765 ns |
 |        CountTreeNodes | 100.3 ns | 0.3675 ns | 0.3437 ns |
 |  CountTreeNodeStructs | 101.1 ns | 0.9764 ns | 0.8655 ns |
