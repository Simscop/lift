using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using HelixToolkit.Wpf.SharpDX.Utilities;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Color4 = SharpDX.Color4;
using Colors = System.Windows.Media.Colors;
using Media3D = System.Windows.Media.Media3D;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using HelixToolkit.Wpf.SharpDX;
using OpenCvSharp;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;
using System.Windows.Markup;
using CommunityToolkit.Mvvm.Input;
using Lift.Core;
using SharpDX.Direct2D1.Effects;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;
using Microsoft.Win32;
using OpenCvSharp.Flann;

namespace ObjectTest;

public partial class DirectXView
{
    public DirectXView()
    {
        InitializeComponent();

        this.DataContext = new DirectXViewModel();

        ((DirectXViewModel) this.DataContext).AddBound += () =>
        {
            var vm = this.DataContext as DirectXViewModel;

            var builder = new MeshBuilder();

            var x = Model.Transform.Value.M11;
            var y = Model.Transform.Value.M22;
            var z = Model.Transform.Value.M33;

            var ox = Model.Transform.Value.OffsetX;
            var oy = Model.Transform.Value.OffsetY;
            var oz = Model.Transform.Value.OffsetZ;

            var cx = -(x - ox) / 2;
            var cy = -(y - oy) / 2;
            var cz = -(z - oz) / 2;

            var diameter = double.MaxValue;
            diameter = Math.Min(diameter, x);
            diameter = Math.Min(diameter, y);
            diameter = Math.Min(diameter, z);

            diameter *= 0.005;

            builder.AddBoundingBox(new Media3D.Rect3D(cx, cy, cz, x, y, z), diameter);

            Bound.Geometry = builder.ToMesh();
            Bound.Material = PhongMaterials.White;
        };

    }

    public List<string> List = new List<string>
    {

    };

    private void Export(object sender, RoutedEventArgs e)
    {
        var path = string.Empty;
        var dialog = new SaveFileDialog
        {
            Filter = $"{HelixToolkit.Wpf.SharpDX.Assimp.Exporter.SupportedFormatsString}"
        };

        if (dialog.ShowDialog() != true) return;


        var test = false;
        if (test)
        {
            var exporter = new HelixToolkit.Wpf.SharpDX.Assimp.Exporter();

            for (int i = 0; i < 21; i++)
            {
                //var list = HelixToolkit.Wpf.SharpDX.Assimp.Exporter.SupportedFormatsString;

                var id = HelixToolkit.Wpf.SharpDX.Assimp.Exporter.SupportedFormats[i].FormatId;

                var code = exporter.ExportToFile($"demo.{i}", Model.SceneNode, id);
                Debug.WriteLine($"{code} -> {i} -> {HelixToolkit.Wpf.SharpDX.Assimp.Exporter.SupportedFormats[i].FileExtension}");
            }
        }

        else
        {
            var exporter = new HelixToolkit.Wpf.SharpDX.Assimp.Exporter();
            var id = HelixToolkit.Wpf.SharpDX.Assimp.Exporter.SupportedFormats[dialog.FilterIndex - 1].FormatId;
            var code = exporter.ExportToFile(dialog.FileName, Model.SceneNode, id);
            Debug.WriteLine($"{code} -> {dialog.FileName}");

        }
    }
}

public partial class BaseDirectXViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private string _title = "Title";

    [ObservableProperty]
    private string _subTitle = "SubTitle";

    [ObservableProperty]
    private IEffectsManager? _effectsManager = null;

    [ObservableProperty]
    private Camera _camera = new PerspectiveCamera();

    #region Dispose

    private bool _disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
            if (EffectsManager != null)
            {
                var effectManager = EffectsManager as IDisposable;
                Disposer.RemoveAndDispose(ref effectManager);
            }
            _disposedValue = true;
            GC.SuppressFinalize(this);
        }
    }

    ~BaseDirectXViewModel()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(false);
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        // GC.SuppressFinalize(this);
    }

    #endregion

}

public partial class DirectXViewModel : BaseDirectXViewModel
{
    public event Action? AddBound;

    public DirectXViewModel()
    {
        EffectsManager = new DefaultEffectsManager();

        Camera = new OrthographicCamera()
        {
            Position = new Point3D(0, 0, 0),
            LookDirection = new Vector3D(0, 0, 5),
            UpDirection = new Vector3D(0, 1, 0)
        };
        //Camera = new PerspectiveCamera();
        Camera.Reset();

    }

    [ObservableProperty] private Material? _volumeMaterial = null;

    [ObservableProperty] private Media3D.Transform3D? _transform = null;

    (Material material, Media3D.Transform3D transform) Generate()
    {
        var x = 100;
        var y = 100;
        var z = 100;

        var material = new VolumeTextureDiffuseMaterial();
        var transform = new Media3D.ScaleTransform3D(1, 1, 1);

        var data = new float[x * y * z];

        var random = new Random();
        //
        //for (var i = 0; i < x * y * z; i++)
        //    //data[i] = 1;
        //    data[i] = random.NextFloat(0, 1);

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    data[i * y * z + j * z + k] = random.NextFloat(0, 1);
                }
            }
        }


        var gradients = VolumeDataHelper.GenerateGradients(data, x, y, z, 1);
        VolumeDataHelper.FilterNxNxN(gradients, x, y, z, 3);

        material.Texture = new VolumeTextureGradientParams(gradients, x, y, z);
        material.Color = new Color4(0, 1, 0, 0.01f);

        transform.Freeze();
        material.Freeze();

        return (material, transform);
    }

    [RelayCommand]
    void LoadData()
    {
        //var (material, transform) = Generate();
        ////var (material, transform) = SimpleGenerate();
        //var (material, transform) = GenerateFromPath(@"E:\test\pollen.tif");
        //var (material, transform) = GenerateFromPath(@"E:\test\mouse3D.tif");
        //var (material, transform) = GenerateFromPath(@"E:\test\profiling001.tif");
        //var (material, transform) = GenerateFromPath(@"E:\test\profiling002.tif");
        var (material, transform) = GenerateFromPath(@"E:\test\profiling001.tif");
        //var (material, transform) = GenerateFromPath(@"C:\Users\haeer\Desktop\Stack.tif");
        VolumeMaterial = material.Clone() as Material;
        //VolumeMaterial!.Freeze();
        Transform = transform;

        AddBound?.Invoke();
    }


    (Material material, Media3D.Transform3D transform) SimpleGenerate()
    {
        var x = 1;
        var y = 1;
        var z = 1;

        var material = new VolumeTextureDiffuseMaterial();
        var transform = new Media3D.ScaleTransform3D(1, 1, 1);

        var data = new Half4[x * y * z];
        var random = new Random();

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {

                    data[i * y * z + j * z + k] = new Half4(i, j, k, 1f);

                }
            }
        }


        material.Texture = new VolumeTextureGradientParams(data, x, y, z);
        material.Color = new Color4(0, 1, 0, 0.01f);

        transform.Freeze();
        material.Freeze();

        return (material, transform);
    }

    VolumeTextureDiffuseMaterial material = new();


    (Material material, Media3D.Transform3D transform) GenerateFromPath(string path)
    {
        Cv2.ImReadMulti(path, out var imgs, ImreadModes.AnyDepth);

        double scale = 1;
        float wscale = 1 * (1.0f / (float) scale);
        float hscale = 1 * (1.0f / (float) scale);
        float dscale = 1f;

        var width = (int) (imgs[0].Width * scale);
        var height = (int) (imgs[0].Height * scale);
        var depth = imgs.Length;

        var mats = new Mat[depth];
        for (int i = 0; i < depth; i++)
        {
            mats[i] = imgs[i].Resize(new OpenCvSharp.Size(width, height));
        }
        var data = new float[width * height * depth];

        double min = byte.MaxValue;
        double max = byte.MinValue;

        for (var i = 0; i < depth; i++)
        {
            var mat = mats[i];

            mat.GetArray(out ushort[] da);
            mat.MinMaxLoc(out double vmin, out double vmax);
            min = Math.Min(min, vmin);
            max = Math.Max(max, vmax);
        }

        var range = max - min;

        Debug.WriteLine($"计算阈值通过");


        Parallel.For(0, depth, new ParallelOptions() { MaxDegreeOfParallelism = 32 }, (z) =>
        {
            var mat = mats[z];
            var t = mat.Type();

            var index = z * width * height;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++, ++index)
                {
                    var value = mat.At<ushort>(y, x);
                    var w = (float) (value - min) / ((float) range * 1.0f);
                    //w = w < 0.1 ? 0.0001f : w;
                    data[index] = w;
                }
            }
        });
        Debug.WriteLine($"读取所有图像数据");


        var gradients = VolumeDataHelper.GenerateGradients(data, width, height, depth, 5);
        //var gradients = VolumeDataHelper.DoNothing(data, width, height, depth, wscale, hscale, dscale);
        //VolumeDataHelper.FilterNxNxN(gradients, width, height, depth, 3);

        material.Texture = new VolumeTextureGradientParams(gradients, width, height, depth);

        var transform = new Media3D.ScaleTransform3D(wscale, hscale, dscale);
        //transform = new Media3D.ScaleTransform3D();
        material.Color = new Color4(0, 1, 0, 0.1f);

        //material.IsoValue = 0.2;
        transform.Freeze();
        //material.Freeze();


        return (material, transform);
    }

    (Material material, Media3D.Transform3D transform) GenerateFromPathWithoutGradient(string path)
    {
        var material = new VolumeTextureDiffuseMaterial();
        var transform = new Media3D.ScaleTransform3D(1, 1, 1);

        Cv2.ImReadMulti(path, out var mats, ImreadModes.AnyDepth);

        var width = mats[0].Width;
        var height = mats[0].Height;
        var depth = mats.Length;

        var data = new Half4[width * height * depth];

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
                    w = w < 0.5 ? 0 : w;
                    data[i * height * width + j * width + k] = new Half4((float) i, (float) j, (float) k * 0.1f, w);
                }

            Debug.WriteLine($"Current is {i}");
        }

        //material.Texture = new VolumeTextureGradientParams(gradients, width, height, depth);
        material.Texture = new VolumeTextureGradientParams(data, width, height, depth);
        material.Color = new Color4(0, 1, 0, 0.01f);

        transform.Freeze();
        material.Freeze();

        return (material, transform);
    }

    [ObservableProperty]
    private double _alpha = 500;

    partial void OnAlphaChanged(double value)
    {
        //material.Color = new Color4(0, 1, 0, (float) value / 10000f);

        var vol = VolumeMaterial as VolumeTextureDiffuseMaterial;
        vol.Color = new Color4(0, 1, 0, (float) value / 10000f);
        //VolumeMaterial = material.Clone() as Material;
    }
}
