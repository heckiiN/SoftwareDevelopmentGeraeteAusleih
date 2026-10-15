using System.IO;
using System.Windows;
using System.Windows.Threading;
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
        DispatcherUnhandledException += UnbehandelterFehler;

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

        var geraete = new SqliteGeraeteRepository(datenbank);
        var entleiher = new SqliteEntleiherRepository(datenbank);
        var reservierungen = new SqliteReservierungsRepository(datenbank);

        var geraeteVerwaltung = new GeraeteVerwaltung(geraete, reservierungen);
        var entleiherVerwaltung = new EntleiherVerwaltung(entleiher, reservierungen);
        var zeit = new Systemzeit();
        var reservierungsVerwaltung = new ReservierungsVerwaltung(reservierungen, geraete, entleiher, zeit);

        var dialoge = new DialogDienst();

        var hauptViewModel = new HauptViewModel(
            new UebersichtViewModel(reservierungsVerwaltung, geraeteVerwaltung, entleiherVerwaltung, dialoge),
            new GeraeteViewModel(geraeteVerwaltung, dialoge),
            new EntleiherViewModel(entleiherVerwaltung, dialoge),
            new ReservierungenViewModel(reservierungsVerwaltung, geraeteVerwaltung, entleiherVerwaltung, dialoge),
            new AusgabeViewModel(reservierungsVerwaltung, geraeteVerwaltung, entleiherVerwaltung, dialoge),
            zeit, dialoge);
        new Hauptfenster { DataContext = hauptViewModel }.Show();
    }

    // Speichervorgänge und Seitenwechsel fangen ihre Fehler selbst ab. Hier landet der Rest, etwa ein
    // Ladefehler beim Neuladen nach einer Aktion. Bei Datenbankfehlern soll man weiterarbeiten können.
    private void UnbehandelterFehler(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        // Ohne Hauptfenster ist der Fehler schon beim Start passiert, dann gibt es nichts zum Weiterarbeiten.
        if (e.Exception is SqliteException && MainWindow is not null)
        {
            MessageBox.Show(Meldungen.Ladefehler, "Geräteausleihe", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        MessageBox.Show($"Ein unerwarteter Fehler ist aufgetreten. Die Anwendung wird beendet.\n\n{e.Exception.Message}",
            "Geräteausleihe", MessageBoxButton.OK, MessageBoxImage.Error);
        Shutdown(1);
    }
}
