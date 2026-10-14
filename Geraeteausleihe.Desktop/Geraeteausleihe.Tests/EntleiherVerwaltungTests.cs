using Geraeteausleihe.Core.Daten;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Tests;

public sealed class EntleiherVerwaltungTests : IDisposable
{
    private readonly TestDatenbank _db = new();
    private readonly EntleiherVerwaltung _verwaltung;

    public EntleiherVerwaltungTests()
    {
        _verwaltung = new EntleiherVerwaltung(new SqliteEntleiherRepository(_db.Datenbank));
    }

    public void Dispose() => _db.Dispose();

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

    [Fact]
    public void Entleiher_bleiben_nach_erneutem_Oeffnen_der_Datenbank_erhalten()
    {
        _verwaltung.Speichern(Mustermann());

        var nachNeustart = new EntleiherVerwaltung(new SqliteEntleiherRepository(_db.NeuOeffnen()));

        Assert.Equal("Max Mustermann", Assert.Single(nachNeustart.AlleLaden()).Name);
    }
}
