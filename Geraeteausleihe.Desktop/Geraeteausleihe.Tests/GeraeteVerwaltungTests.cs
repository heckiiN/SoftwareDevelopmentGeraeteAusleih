using Geraeteausleihe.Core.Daten;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Tests;

public sealed class GeraeteVerwaltungTests : IDisposable
{
    private readonly TestDatenbank _db = new();
    private readonly GeraeteVerwaltung _verwaltung;

    public GeraeteVerwaltungTests()
    {
        _verwaltung = new GeraeteVerwaltung(new SqliteGeraeteRepository(_db.Datenbank));
    }

    public void Dispose() => _db.Dispose();

    private static Geraet Beamer() => new()
    {
        Inventarnummer = "G-001",
        Bezeichnung = "Beamer 01",
        Beschreibung = "Epson EB-W51"
    };

    [Fact]
    public void Neues_Geraet_wird_gespeichert_und_wieder_geladen()
    {
        _verwaltung.Speichern(Beamer());

        var geraet = Assert.Single(_verwaltung.AlleLaden());
        Assert.True(geraet.Id > 0);
        Assert.Equal("G-001", geraet.Inventarnummer);
        Assert.Equal("Beamer 01", geraet.Bezeichnung);
        Assert.Equal("Epson EB-W51", geraet.Beschreibung);
        Assert.True(geraet.Aktiv);
    }

    [Fact]
    public void Geraet_laesst_sich_bearbeiten()
    {
        var geraet = Beamer();
        _verwaltung.Speichern(geraet);

        geraet.Bezeichnung = "Beamer 01 (Saal)";
        geraet.Beschreibung = "  ";
        _verwaltung.Speichern(geraet);

        var geladen = Assert.Single(_verwaltung.AlleLaden());
        Assert.Equal("Beamer 01 (Saal)", geladen.Bezeichnung);
        Assert.Null(geladen.Beschreibung);
    }

    [Theory]
    [InlineData("", "Beamer 01")]
    [InlineData("   ", "Beamer 01")]
    [InlineData("G-001", "")]
    [InlineData("G-001", "   ")]
    public void Fehlende_Pflichtfelder_werden_abgelehnt(string inventarnummer, string bezeichnung)
    {
        var geraet = new Geraet { Inventarnummer = inventarnummer, Bezeichnung = bezeichnung };

        Assert.Throws<FachlicherFehler>(() => _verwaltung.Speichern(geraet));
        Assert.Empty(_verwaltung.AlleLaden());
    }

    [Theory]
    [InlineData("G-001")]
    [InlineData("g-001")]
    [InlineData(" G-001 ")]
    public void Doppelte_Inventarnummer_wird_abgelehnt(string inventarnummer)
    {
        _verwaltung.Speichern(Beamer());
        var zweites = new Geraet { Inventarnummer = inventarnummer, Bezeichnung = "Leinwand 02" };

        Assert.Throws<FachlicherFehler>(() => _verwaltung.Speichern(zweites));
        Assert.Single(_verwaltung.AlleLaden());
    }

    [Fact]
    public void Eigene_Inventarnummer_bleibt_beim_Bearbeiten_gueltig()
    {
        var geraet = Beamer();
        _verwaltung.Speichern(geraet);

        geraet.Beschreibung = "Epson EB-W52";
        _verwaltung.Speichern(geraet);

        Assert.Equal("Epson EB-W52", Assert.Single(_verwaltung.AlleLaden()).Beschreibung);
    }

    [Fact]
    public void Geraete_bleiben_nach_erneutem_Oeffnen_der_Datenbank_erhalten()
    {
        _verwaltung.Speichern(Beamer());

        var nachNeustart = new GeraeteVerwaltung(new SqliteGeraeteRepository(_db.NeuOeffnen()));

        Assert.Equal("G-001", Assert.Single(nachNeustart.AlleLaden()).Inventarnummer);
    }
}
