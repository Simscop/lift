using System;
using System.Windows;

namespace Lift.Helix.Themes;

public class Themes : ResourceDictionary
{
    public Themes()
    {
        MergedDictionaries.Add(ControlsResources);
    }

    public static ResourceDictionary ControlsResources
        => _controlsResources ??= new ResourceDictionary { Source = new Uri("pack://application:,,,/Lift.Helix;component/Themes/Generic.xaml") };

    private static ResourceDictionary? _controlsResources;
}
