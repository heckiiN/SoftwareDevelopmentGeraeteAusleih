using System.Globalization;
using Geraeteausleihe.Core.Domain;
using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Core.Daten;

public class SqliteReservierungsRepository(Datenbank datenbank) : IReservierungsRepository
{
    private const string Spalten =
        "Id, GeraetId, EntleiherId, Beginn, Ende, Status, Ausgabezeitpunkt, Rueckgabezeitpunkt";

    public IReadOnlyList<Reservierung> AlleLaden()
    {
        using var verbindung = datenbank.Oeffnen();
        using var befehl = verbindung.CreateCommand();
        befehl.CommandText = $"SELECT {Spalten} FROM Reservierung ORDER BY Beginn";
        return Lesen(befehl);
    }

    public Reservierung? Laden(int id)
    {
        using var verbindung = datenbank.Oeffnen();
        using var befehl = verbindung.CreateCommand();
        befehl.CommandText = $"SELECT {Spalten} FROM Reservierung WHERE Id = $id";
        befehl.Parameters.AddWithValue("$id", id);
        return Lesen(befehl).SingleOrDefault();
    }

    public IReadOnlyList<Reservierung> BlockierendeLaden(int geraetId)
    {
        using var verbindung = datenbank.Oeffnen();
        return BlockierendeLaden(verbindung, null, geraetId);
    }

    public bool GeraetHatOffene(int geraetId) => Existiert("""
        SELECT EXISTS (
            SELECT 1 FROM Reservierung
            WHERE GeraetId = $id AND Status IN ('Reserviert', 'Ausgegeben'))
        """, geraetId);

    public bool EntleiherHatOffene(int entleiherId) => Existiert("""
        SELECT EXISTS (
            SELECT 1 FROM Reservierung
            WHERE EntleiherId = $id AND Status IN ('Reserviert', 'Ausgegeben'))
        """, entleiherId);

    public Reservierung? AnlegenWennFrei(Reservierung neu)
    {
        using var verbindung = datenbank.Oeffnen();
        // BeginTransaction sperrt die Datenbank sofort für andere Schreiber (BEGIN IMMEDIATE).
        // So kann zwischen Prüfung und Einfügen niemand eine andere Reservierung dazwischen schieben.
        using var transaktion = verbindung.BeginTransaction();

        var bestehende = BlockierendeLaden(verbindung, transaktion, neu.GeraetId);
        var konflikt = Verfuegbarkeit.FindeKonflikt(bestehende, neu.GeraetId, neu.Beginn, neu.Ende);
        if (konflikt is not null)
        {
            return konflikt;
        }

        using var befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = """
            INSERT INTO Reservierung (GeraetId, EntleiherId, Beginn, Ende, Status, Ausgabezeitpunkt, Rueckgabezeitpunkt)
            VALUES ($geraetId, $entleiherId, $beginn, $ende, $status, $ausgabe, $rueckgabe);
            SELECT last_insert_rowid();
            """;
        befehl.Parameters.AddWithValue("$geraetId", neu.GeraetId);
        befehl.Parameters.AddWithValue("$entleiherId", neu.EntleiherId);
        befehl.Parameters.AddWithValue("$beginn", AlsText(neu.Beginn));
        befehl.Parameters.AddWithValue("$ende", AlsText(neu.Ende));
        befehl.Parameters.AddWithValue("$status", neu.Status.ToString());
        befehl.Parameters.AddWithValue("$ausgabe", (object?)AlsText(neu.Ausgabezeitpunkt) ?? DBNull.Value);
        befehl.Parameters.AddWithValue("$rueckgabe", (object?)AlsText(neu.Rueckgabezeitpunkt) ?? DBNull.Value);

        neu.Id = Convert.ToInt32(befehl.ExecuteScalar());
        transaktion.Commit();
        return null;
    }

    public void StatusSpeichern(Reservierung reservierung)
    {
        using var verbindung = datenbank.Oeffnen();
        using var befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            UPDATE Reservierung
            SET Status = $status, Ausgabezeitpunkt = $ausgabe, Rueckgabezeitpunkt = $rueckgabe
            WHERE Id = $id
            """;
        befehl.Parameters.AddWithValue("$id", reservierung.Id);
        befehl.Parameters.AddWithValue("$status", reservierung.Status.ToString());
        befehl.Parameters.AddWithValue("$ausgabe", (object?)AlsText(reservierung.Ausgabezeitpunkt) ?? DBNull.Value);
        befehl.Parameters.AddWithValue("$rueckgabe", (object?)AlsText(reservierung.Rueckgabezeitpunkt) ?? DBNull.Value);
        befehl.ExecuteNonQuery();
    }

    private bool Existiert(string sql, int id)
    {
        using var verbindung = datenbank.Oeffnen();
        using var befehl = verbindung.CreateCommand();
        befehl.CommandText = sql;
        befehl.Parameters.AddWithValue("$id", id);

        return Convert.ToBoolean(befehl.ExecuteScalar());
    }

    private static IReadOnlyList<Reservierung> BlockierendeLaden(
        SqliteConnection verbindung, SqliteTransaction? transaktion, int geraetId)
    {
        using var befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = $"""
            SELECT {Spalten} FROM Reservierung
            WHERE GeraetId = $geraetId AND Status IN ('Reserviert', 'Ausgegeben')
            ORDER BY Beginn
            """;
        befehl.Parameters.AddWithValue("$geraetId", geraetId);
        return Lesen(befehl);
    }

    private static List<Reservierung> Lesen(SqliteCommand befehl)
    {
        using var leser = befehl.ExecuteReader();
        var reservierungen = new List<Reservierung>();
        while (leser.Read())
        {
            reservierungen.Add(new Reservierung
            {
                Id = leser.GetInt32(0),
                GeraetId = leser.GetInt32(1),
                EntleiherId = leser.GetInt32(2),
                Beginn = leser.GetDateTime(3),
                Ende = leser.GetDateTime(4),
                Status = Enum.Parse<ReservierungsStatus>(leser.GetString(5)),
                Ausgabezeitpunkt = leser.IsDBNull(6) ? null : leser.GetDateTime(6),
                Rueckgabezeitpunkt = leser.IsDBNull(7) ? null : leser.GetDateTime(7)
            });
        }
        return reservierungen;
    }

    // Festes ISO-Format, damit die Texte in SQL in zeitlicher Reihenfolge verglichen werden können
    private static string AlsText(DateTime zeitpunkt) => zeitpunkt.ToString("s", CultureInfo.InvariantCulture);

    private static string? AlsText(DateTime? zeitpunkt) => zeitpunkt is { } wert ? AlsText(wert) : null;
}
