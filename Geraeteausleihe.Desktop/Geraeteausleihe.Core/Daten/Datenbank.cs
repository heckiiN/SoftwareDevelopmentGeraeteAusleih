using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Core.Daten;

public class Datenbank
{
    private readonly string _verbindungstext;

    public Datenbank(string dateipfad)
    {
        _verbindungstext = new SqliteConnectionStringBuilder
        {
            DataSource = dateipfad,
            ForeignKeys = true
        }.ToString();
    }

    public SqliteConnection Oeffnen()
    {
        var verbindung = new SqliteConnection(_verbindungstext);
        verbindung.Open();
        return verbindung;
    }

    public void Einrichten()
    {
        using var verbindung = Oeffnen();
        using var befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            CREATE TABLE IF NOT EXISTS Geraet (
                Id             INTEGER PRIMARY KEY,
                Inventarnummer TEXT NOT NULL UNIQUE COLLATE NOCASE,
                Bezeichnung    TEXT NOT NULL,
                Beschreibung   TEXT,
                Aktiv          INTEGER NOT NULL DEFAULT 1
            );
            """;
        befehl.ExecuteNonQuery();
    }
}
