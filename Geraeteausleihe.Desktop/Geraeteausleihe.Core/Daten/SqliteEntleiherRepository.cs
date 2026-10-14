using Geraeteausleihe.Core.Domain;
using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Core.Daten;

public class SqliteEntleiherRepository(Datenbank datenbank) : IEntleiherRepository
{
    public IReadOnlyList<Entleiher> AlleLaden()
    {
        using var verbindung = datenbank.Oeffnen();
        using var befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            SELECT Id, Name, Kontakt, Aktiv
            FROM Entleiher
            ORDER BY Name
            """;

        using var leser = befehl.ExecuteReader();
        var entleiher = new List<Entleiher>();
        while (leser.Read())
        {
            entleiher.Add(Lesen(leser));
        }
        return entleiher;
    }

    public void Hinzufuegen(Entleiher entleiher)
    {
        using var verbindung = datenbank.Oeffnen();
        using var befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            INSERT INTO Entleiher (Name, Kontakt, Aktiv)
            VALUES ($name, $kontakt, $aktiv);
            SELECT last_insert_rowid();
            """;
        ParameterSetzen(befehl, entleiher);

        entleiher.Id = Convert.ToInt32(befehl.ExecuteScalar());
    }

    public void Aktualisieren(Entleiher entleiher)
    {
        using var verbindung = datenbank.Oeffnen();
        using var befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            UPDATE Entleiher
            SET Name = $name,
                Kontakt = $kontakt,
                Aktiv = $aktiv
            WHERE Id = $id
            """;
        ParameterSetzen(befehl, entleiher);
        befehl.Parameters.AddWithValue("$id", entleiher.Id);

        befehl.ExecuteNonQuery();
    }

    private static void ParameterSetzen(SqliteCommand befehl, Entleiher entleiher)
    {
        befehl.Parameters.AddWithValue("$name", entleiher.Name);
        befehl.Parameters.AddWithValue("$kontakt", (object?)entleiher.Kontakt ?? DBNull.Value);
        befehl.Parameters.AddWithValue("$aktiv", entleiher.Aktiv);
    }

    private static Entleiher Lesen(SqliteDataReader leser) => new()
    {
        Id = leser.GetInt32(0),
        Name = leser.GetString(1),
        Kontakt = leser.IsDBNull(2) ? null : leser.GetString(2),
        Aktiv = leser.GetBoolean(3)
    };
}
