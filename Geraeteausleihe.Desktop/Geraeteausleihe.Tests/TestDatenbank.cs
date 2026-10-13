using Geraeteausleihe.Core.Daten;
using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Tests;

// Jede Testklasse bekommt eine eigene, frische Datenbankdatei, die danach wieder gelöscht wird.
public sealed class TestDatenbank : IDisposable
{
    private readonly string _pfad = Path.Combine(Path.GetTempPath(), $"geraeteausleihe-test-{Guid.NewGuid():N}.db");

    public TestDatenbank()
    {
        Datenbank = new Datenbank(_pfad);
        Datenbank.Einrichten();
    }

    public Datenbank Datenbank { get; }

    public Datenbank NeuOeffnen() => new(_pfad);

    public void Dispose()
    {
        // Microsoft.Data.Sqlite hält Verbindungen im Pool offen, sonst ließe sich die Datei nicht löschen.
        SqliteConnection.ClearAllPools();
        File.Delete(_pfad);
    }
}
