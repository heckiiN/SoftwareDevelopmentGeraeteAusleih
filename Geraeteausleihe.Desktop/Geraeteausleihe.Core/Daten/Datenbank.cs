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

            CREATE TABLE IF NOT EXISTS Entleiher (
                Id      INTEGER PRIMARY KEY,
                Name    TEXT NOT NULL,
                Kontakt TEXT,
                Aktiv   INTEGER NOT NULL DEFAULT 1
            );

            -- Zeitpunkte als lokale Zeit im ISO-Format, z. B. 2026-11-01T09:00:00
            CREATE TABLE IF NOT EXISTS Reservierung (
                Id                 INTEGER PRIMARY KEY,
                GeraetId           INTEGER NOT NULL REFERENCES Geraet (Id),
                EntleiherId        INTEGER NOT NULL REFERENCES Entleiher (Id),
                Beginn             TEXT NOT NULL,
                Ende               TEXT NOT NULL,
                Status             TEXT NOT NULL DEFAULT 'Reserviert'
                                   CHECK (Status IN ('Reserviert', 'Ausgegeben', 'Zurueckgegeben', 'Storniert')),
                Ausgabezeitpunkt   TEXT,
                Rueckgabezeitpunkt TEXT,
                CHECK (Ende > Beginn)
            );

            CREATE INDEX IF NOT EXISTS IX_Reservierung_GeraetId ON Reservierung (GeraetId);
            """;
        befehl.ExecuteNonQuery();
    }
}
