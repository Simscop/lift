using BenchmarkDotNet.Running;
using BenchmarkTest;

BenchmarkRunner.Run<ParallelBench>();

//Console.WriteLine($"{ParallelBench.Valid(100)}");
