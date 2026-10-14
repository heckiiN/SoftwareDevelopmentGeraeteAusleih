using Geraeteausleihe.Core.Daten;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Tests;

public sealed class EntleiherVerwaltungTests : IDisposable
{
    private readonly TestDatenbank _db = new();
    private readonly SqliteReservierungsRepository _reservierungen;
    private readonly EntleiherVerwaltung _verwaltung;

    public EntleiherVerwaltungTests()
    {
        _reservierungen = new SqliteReservierungsRepository(_db.Datenbank);
        _verwaltung = new EntleiherVerwaltung(new SqliteEntleiherRepository(_db.Datenbank), _reservierungen);
    }

    public void Dispose() => _db.Dispose();

    private void Reservieren(Entleiher person, ReservierungsStatus status)
    {
        var beamer = new Geraet { Inventarnummer = "G-001", Bezeichnung = "Beamer 01" };
        new SqliteGeraeteRepository(_db.Datenbank).Hinzufuegen(beamer);
        _reservierungen.AnlegenWennFrei(new Reservierung
        {
            GeraetId = beamer.Id,
            EntleiherId = person.Id,
            Beginn = new DateTime(2026, 11, 1, 9, 0, 0),
            Ende = new DateTime(2026, 11, 1, 12, 0, 0),
            Status = status
        });
    }

    private static Entleiher Mustermann() => new()
    {
        Name = "Max Mustermann",
        Kontakt = "max@example.org"
    };

    [Fact]
    public void Neuer_Entleiher_wird_gespeichert_und_wieder_geladen()
    {
        _verwaltung.Speichern(Mustermann());

        var person = Assert.Single(_verwaltung.AlleLaden());
        Assert.True(person.Id > 0);
        Assert.Equal("Max Mustermann", person.Name);
        Assert.Equal("max@example.org", person.Kontakt);
        Assert.True(person.Aktiv);
    }

    [Fact]
    public void Kontakt_ist_optional()
    {
        _verwaltung.Speichern(new Entleiher { Name = "Erika Musterfrau", Kontakt = "  " });

        Assert.Null(Assert.Single(_verwaltung.AlleLaden()).Kontakt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fehlender_Name_wird_abgelehnt(string name)
    {
        Assert.Throws<FachlicherFehler>(() => _verwaltung.Speichern(new Entleiher { Name = name }));
        Assert.Empty(_verwaltung.AlleLaden());
    }

    [Fact]
    public void Entleiher_laesst_sich_bearbeiten()
    {
        var person = Mustermann();
        _verwaltung.Speichern(person);

        person.Kontakt = "0151 1234567";
        _verwaltung.Speichern(person);

        Assert.Equal("0151 1234567", Assert.Single(_verwaltung.AlleLaden()).Kontakt);
    }

    [Fact]
    public void Entleiher_laesst_sich_deaktivieren_und_wieder_aktivieren()
    {
        var person = Mustermann();
        _verwaltung.Speichern(person);

        _verwaltung.Deaktivieren(person);
        Assert.False(Assert.Single(_verwaltung.AlleLaden()).Aktiv);

        _verwaltung.Aktivieren(person);
        Assert.True(Assert.Single(_verwaltung.AlleLaden()).Aktiv);
    }

    [Theory]
    [InlineData(ReservierungsStatus.Reserviert)]
    [InlineData(ReservierungsStatus.Ausgegeben)]
    public void Entleiher_mit_offener_Reservierung_laesst_sich_nicht_deaktivieren(ReservierungsStatus status)
    {
        var person = Mustermann();
        _verwaltung.Speichern(person);
        Reservieren(person, status);

        Assert.Throws<FachlicherFehler>(() => _verwaltung.Deaktivieren(person));
        Assert.True(Assert.Single(_verwaltung.AlleLaden()).Aktiv);
    }

    [Theory]
    [InlineData(ReservierungsStatus.Storniert)]
    [InlineData(ReservierungsStatus.Zurueckgegeben)]
    public void Entleiher_mit_abgeschlossenen_Reservierungen_laesst_sich_deaktivieren(ReservierungsStatus status)
    {
        var person = Mustermann();
        _verwaltung.Speichern(person);
        Reservieren(person, status);

        _verwaltung.Deaktivieren(person);

        Assert.False(Assert.Single(_verwaltung.AlleLaden()).Aktiv);
    }

    [Fact]
    public void Entleiher_bleiben_nach_erneutem_Oeffnen_der_Datenbank_erhalten()
    {
        _verwaltung.Speichern(Mustermann());

        var datenbank = _db.NeuOeffnen();
        var nachNeustart = new EntleiherVerwaltung(
            new SqliteEntleiherRepository(datenbank), new SqliteReservierungsRepository(datenbank));

        Assert.Equal("Max Mustermann", Assert.Single(nachNeustart.AlleLaden()).Name);
    }
}
