using System.IO;
using System.Windows;
using Geraeteausleihe.Core.Daten;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Desktop.Dienste;
using Geraeteausleihe.Desktop.ViewModels;
using Geraeteausleihe.Desktop.Views;
using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var ordner = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Geraeteausleihe");
        var datenbank = new Datenbank(Path.Combine(ordner, "geraeteausleihe.db"));

        try
        {
            Directory.CreateDirectory(ordner);
            datenbank.Einrichten();
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Die Datenbank konnte nicht geöffnet werden.\n\n{fehler.Message}",
                "Geräteausleihe", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        var geraeteVerwaltung = new GeraeteVerwaltung(new SqliteGeraeteRepository(datenbank));

        var dialoge = new DialogDienst();

        var hauptViewModel = new HauptViewModel(new GeraeteViewModel(geraeteVerwaltung, dialoge));
        new Hauptfenster { DataContext = hauptViewModel }.Show();
    }
}
