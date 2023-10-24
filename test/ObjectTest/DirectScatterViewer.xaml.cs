using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelixToolkit.Wpf.SharpDX;
using OpenCvSharp;
using SharpDX;

namespace ObjectTest;

public partial class DirectScatterViewer
{
    public DirectScatterViewer()
    {
        InitializeComponent();

        this.DataContext = new DirectScatterViewModel();

    }
}

public partial class DirectScatterViewModel : BaseDirectXViewModel
{
    public PointGeometry3D Points { get; }

    public Transform3D PointTransform { get; set; }

    public DirectScatterViewModel()
    {
        EffectsManager = new DefaultEffectsManager();

        Camera = new HelixToolkit.Wpf.SharpDX.PerspectiveCamera();

        Points = new PointGeometry3D();

        //var (colors, points) = GenerateCube();
        var (colors, points) = GenerateImage(@"E:\test\mouse3D.tif");
        Points.Positions = points;
        Points.Colors = colors;

    }

    [RelayCommand]
    void Load()
    {
        Camera.Reset();
    }

    (Color4Collection colors, Vector3Collection points) GenerateCube()
    {
        var points = new Vector3Collection();
        var colors = new Color4Collection();

        var width = 100;
        var height = 100;
        var depth = 100;

        for (var i = 0; i < depth; i++)
            for (var j = 0; j < height; j++)
                for (var k = 0; k < width; k++)
                {
                    points.Add(new Vector3(j, k, i));
                    colors.Add(new Color4(0, 1, i * 1.0f / depth, 1));
                }

        PointTransform = new TranslateTransform3D(-width / 2f, -height / 2f, -depth / 2f);
        return (colors, points);
    }

    (Color4Collection colors, Vector3Collection points) GenerateImage(string path)
    {
        var points = new Vector3Collection();
        var colors = new Color4Collection();

        Cv2.ImReadMulti(path, out var mats, ImreadModes.AnyDepth);

        var width = mats[0].Width;
        var height = mats[0].Height;
        var depth = mats.Length;

        double min = byte.MaxValue;
        double max = byte.MinValue;

        for (var i = 0; i < depth; i++)
        {
            var mat = mats[i];

            mat.GetArray(out short[] da);
            mat.MinMaxLoc(out double vmin, out double vmax);
            min = Math.Min(min, vmin);
            max = Math.Max(max, vmax);
        }

        var range = max - min;

        for (var i = 0; i < depth; i++)
        {
            var mat = mats[i];
            var t = mat.Type();

            for (var j = 0; j < height; j++)
                for (var k = 0; k < width; k++)
                {
                    var value = mat.At<ushort>(j, k);
                    var w = (float) (value - min) / ((float) range * 1.0f);

                    points.Add(new Vector3(j, k, i));
                    colors.Add(w > 0.3 ? new Color4(0, 1, 0, w) : new Color4(0, 0, 0, 0));
                }

            Debug.WriteLine($"Current is {i}");
        }

        PointTransform = new TranslateTransform3D(-width / 2f, -height / 2f, -depth / 2f);

        return (colors, points);
    }

}
