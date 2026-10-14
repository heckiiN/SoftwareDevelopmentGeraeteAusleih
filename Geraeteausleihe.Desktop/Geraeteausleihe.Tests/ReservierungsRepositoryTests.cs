using Geraeteausleihe.Core.Daten;
using Geraeteausleihe.Core.Domain;
using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Tests;

public sealed class ReservierungsRepositoryTests : IDisposable
{
    private readonly TestDatenbank _db = new();
    private readonly SqliteReservierungsRepository _reservierungen;
    private readonly int _beamerId;
    private readonly int _maxId;

    public ReservierungsRepositoryTests()
    {
        _reservierungen = new SqliteReservierungsRepository(_db.Datenbank);

        var beamer = new Geraet { Inventarnummer = "G-001", Bezeichnung = "Beamer 01" };
        new SqliteGeraeteRepository(_db.Datenbank).Hinzufuegen(beamer);
        var max = new Entleiher { Name = "Max Mustermann" };
        new SqliteEntleiherRepository(_db.Datenbank).Hinzufuegen(max);

        _beamerId = beamer.Id;
        _maxId = max.Id;
    }

    public void Dispose() => _db.Dispose();

    private Reservierung Beamer(int vonStunde, int bisStunde) => new()
    {
        GeraetId = _beamerId,
        EntleiherId = _maxId,
        Beginn = new DateTime(2026, 11, 1, vonStunde, 0, 0),
        Ende = new DateTime(2026, 11, 1, bisStunde, 0, 0)
    };

    [Fact]
    public void Freie_Reservierung_wird_gespeichert_und_bleibt_nach_Neustart_erhalten()
    {
        var neu = Beamer(9, 12);

        Assert.Null(_reservierungen.AnlegenWennFrei(neu));

        var nachNeustart = new SqliteReservierungsRepository(_db.NeuOeffnen());
        var geladen = Assert.Single(nachNeustart.AlleLaden());
        Assert.Equal(neu.Id, geladen.Id);
        Assert.Equal(new DateTime(2026, 11, 1, 9, 0, 0), geladen.Beginn);
        Assert.Equal(new DateTime(2026, 11, 1, 12, 0, 0), geladen.Ende);
        Assert.Equal(ReservierungsStatus.Reserviert, geladen.Status);
        Assert.Null(geladen.Ausgabezeitpunkt);
    }

    [Fact]
    public void Ueberschneidende_Reservierung_wird_abgelehnt_und_nicht_gespeichert()
    {
        var erste = Beamer(9, 12);
        _reservierungen.AnlegenWennFrei(erste);

        var konflikt = _reservierungen.AnlegenWennFrei(Beamer(11, 14));

        Assert.NotNull(konflikt);
        Assert.Equal(erste.Id, konflikt.Id);
        Assert.Single(_reservierungen.AlleLaden());
    }

    [Fact]
    public void Angrenzende_Reservierung_wird_gespeichert()
    {
        _reservierungen.AnlegenWennFrei(Beamer(9, 12));

        Assert.Null(_reservierungen.AnlegenWennFrei(Beamer(12, 14)));
        Assert.Equal(2, _reservierungen.AlleLaden().Count);
    }

    [Fact]
    public void Fehlgeschlagenes_Speichern_hinterlaesst_keine_Teildaten()
    {
        var unbekanntesGeraet = Beamer(9, 12);
        unbekanntesGeraet.GeraetId = 999;

        Assert.Throws<SqliteException>(() => _reservierungen.AnlegenWennFrei(unbekanntesGeraet));

        Assert.Empty(_reservierungen.AlleLaden());
        // Die Transaktion wurde zurückgerollt, die Datenbank ist also nicht mehr gesperrt.
        Assert.Null(_reservierungen.AnlegenWennFrei(Beamer(9, 12)));
    }
}
