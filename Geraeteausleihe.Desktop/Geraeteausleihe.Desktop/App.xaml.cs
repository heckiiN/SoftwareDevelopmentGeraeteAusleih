using System.Windows;
using Geraeteausleihe.Desktop.ViewModels;
using Geraeteausleihe.Desktop.Views;

namespace Geraeteausleihe.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var fenster = new Hauptfenster { DataContext = new HauptViewModel() };
        fenster.Show();
    }
}
