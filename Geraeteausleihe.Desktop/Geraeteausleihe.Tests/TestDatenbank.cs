using System.IO;
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

    // Simuliert einen Datenbankfehler: Jeder Schreibversuch endet danach mit einer SqliteException.
    public void Schreibschutz()
    {
        PoolLeeren();
        File.SetAttributes(_pfad, FileAttributes.ReadOnly);
    }

    public void Dispose()
    {
        PoolLeeren();
        File.SetAttributes(_pfad, FileAttributes.Normal);
        File.Delete(_pfad);
    }

    // Microsoft.Data.Sqlite hält Verbindungen im Pool offen, sonst ließe sich die Datei weder sperren noch löschen.
    // Geleert wird nur der Pool dieser Datei: ClearAllPools träfe auch die Verbindungen anderer Testklassen,
    // die parallel laufen, mitten in deren Befehlen.
    private void PoolLeeren()
    {
        using var verbindung = Datenbank.Oeffnen();
        SqliteConnection.ClearPool(verbindung);
    }
}
