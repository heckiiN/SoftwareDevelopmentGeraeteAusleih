using Geraeteausleihe.Core.Domain;
using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Core.Daten;

public class SqliteGeraeteRepository(Datenbank datenbank) : IGeraeteRepository
{
    public IReadOnlyList<Geraet> AlleLaden()
    {
        using var verbindung = datenbank.Oeffnen();
        using var befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            SELECT Id, Inventarnummer, Bezeichnung, Beschreibung, Aktiv
            FROM Geraet
            ORDER BY Inventarnummer
            """;

        using var leser = befehl.ExecuteReader();
        var geraete = new List<Geraet>();
        while (leser.Read())
        {
            geraete.Add(Lesen(leser));
        }
        return geraete;
    }

    public bool InventarnummerVergeben(string inventarnummer, int ausserGeraetId)
    {
        using var verbindung = datenbank.Oeffnen();
        using var befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            SELECT EXISTS (
                SELECT 1 FROM Geraet
                WHERE Inventarnummer = $inventarnummer AND Id <> $id)
            """;
        befehl.Parameters.AddWithValue("$inventarnummer", inventarnummer);
        befehl.Parameters.AddWithValue("$id", ausserGeraetId);

        return Convert.ToBoolean(befehl.ExecuteScalar());
    }

    public void Hinzufuegen(Geraet geraet)
    {
        using var verbindung = datenbank.Oeffnen();
        using var befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            INSERT INTO Geraet (Inventarnummer, Bezeichnung, Beschreibung, Aktiv)
            VALUES ($inventarnummer, $bezeichnung, $beschreibung, $aktiv);
            SELECT last_insert_rowid();
            """;
        ParameterSetzen(befehl, geraet);

        geraet.Id = Convert.ToInt32(befehl.ExecuteScalar());
    }

    public void Aktualisieren(Geraet geraet)
    {
        using var verbindung = datenbank.Oeffnen();
        using var befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            UPDATE Geraet
            SET Inventarnummer = $inventarnummer,
                Bezeichnung = $bezeichnung,
                Beschreibung = $beschreibung,
                Aktiv = $aktiv
            WHERE Id = $id
            """;
        ParameterSetzen(befehl, geraet);
        befehl.Parameters.AddWithValue("$id", geraet.Id);

        befehl.ExecuteNonQuery();
    }

    private static void ParameterSetzen(SqliteCommand befehl, Geraet geraet)
    {
        befehl.Parameters.AddWithValue("$inventarnummer", geraet.Inventarnummer);
        befehl.Parameters.AddWithValue("$bezeichnung", geraet.Bezeichnung);
        befehl.Parameters.AddWithValue("$beschreibung", (object?)geraet.Beschreibung ?? DBNull.Value);
        befehl.Parameters.AddWithValue("$aktiv", geraet.Aktiv);
    }

    private static Geraet Lesen(SqliteDataReader leser) => new()
    {
        Id = leser.GetInt32(0),
        Inventarnummer = leser.GetString(1),
        Bezeichnung = leser.GetString(2),
        Beschreibung = leser.IsDBNull(3) ? null : leser.GetString(3),
        Aktiv = leser.GetBoolean(4)
    };
}
