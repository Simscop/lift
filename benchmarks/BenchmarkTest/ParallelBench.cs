using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Disassemblers;
using OpenCvSharp;

namespace BenchmarkTest;

public class ParallelBench
{
    public double Max { get; }

    public double Min { get; }

    public Mat[] Mats { get; }

    public int Width { get; }

    public int Height { get; }

    public int Depth { get; }

    public int Count { get; }

    [Params(4, 8, 16, 32)]
    public int ThreadCount { get; set; } = 32;

    public ParallelBench()
    {
        var path = @"C:\Users\haeer\Desktop\Stack.tif";
        Cv2.ImReadMulti(path, out var mats, ImreadModes.AnyDepth);

        Mats = mats;

        Width = mats[0].Width;
        Height = mats[0].Height;
        Depth = mats.Length;

        Count = Width * Height * Depth;

        double min = byte.MaxValue;
        double max = byte.MinValue;

        for (var i = 0; i < Depth; i++)
        {
            var mat = Mats[i];

            //mat.GetArray(out short[] da);
            mat.MinMaxLoc(out double vmin, out double vmax);
            min = Math.Min(min, vmin);
            max = Math.Max(max, vmax);
        }

        var range = max - min;

        this.Max = max;
        this.Min = min;
    }

    [Benchmark]
    [Arguments(4)]
    [Arguments(8)]
    [Arguments(16)]
    [Arguments(32)]
    public float[] ReadWithParalle(int threadCount)
    {
        var data = new float[Count];
        var range = Max - Min;

        Parallel.For(0, Depth, new ParallelOptions() { MaxDegreeOfParallelism = threadCount }, (z) =>
        {
            var mat = Mats[z];
            var t = mat.Type();

            var index = z * Width * Height;

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++, ++index)
                {
                    var value = mat.At<ushort>(y, x);
                    var w = (float) (value - Min) / ((float) range * 1.0f);
                    w = w < 0.5 ? 0 : w;
                    data[index] = w;
                }
            }
        });

        return data;
    }

    [Benchmark]
    public float[] ReadWithSerial()
    {
        var data = new float[Count];
        var range = Max - Min;

        for (var i = 0; i < Depth; i++)
        {
            var mat = Mats[i];
            var t = mat.Type();

            for (var j = 0; j < Height; j++)
                for (var k = 0; k < Width; k++)
                {
                    var value = mat.At<ushort>(j, k);
                    var w = (float) (value - Min) / ((float) range * 1.0f);
                    w = w < 0.5 ? 0 : w;
                    data[i * Height * Width + j * Width + k] = w;
                }
        }
        return data;
    }

    public static bool Valid(int count, float threshold = 0.000000001f)
    {
        var bench = new ParallelBench();

        var data1 = bench.ReadWithParalle();
        Console.WriteLine("data1 done.");

        var data2 = bench.ReadWithSerial();
        Console.WriteLine("data2 done.");

        var random = new Random();

        for (var i = 0; i < count; i++)
        {
            var index = random.NextInt64(0, data1.Length);
            if (Math.Abs(data1[index] - data2[index]) < threshold) continue;
            else return false;
        }

        return true;
    }
}
