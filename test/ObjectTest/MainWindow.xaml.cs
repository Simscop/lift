using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ObjectTest;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new MainViewModel();
    }

}

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private Point3D[] _points;

    [ObservableProperty]
    private double[] _values;

    public MainViewModel()
    {
        var x = 2;
        var y = 2;
        var z = 2;

        _points = new Point3D[x * y * z];
        _values = new double[x * y * z];

        var random = new Random();

        for (var i = 0; i < x; i++)
            for (var j = 0; j < y; j++)
                for (var k = 0; k < z; k++)
                {
                    var index = i * y * z + j * z + k;

                    _points[index] = new Point3D(i, j, k);
                    _values[index] =0.5;
                }
    }
}
