using Geraeteausleihe.Core.Daten;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Tests;

public sealed class ReservierungsVerwaltungTests : IDisposable
{
    private readonly TestDatenbank _db = new();
    private readonly SqliteGeraeteRepository _geraete;
    private readonly SqliteEntleiherRepository _entleiher;
    private readonly ReservierungsVerwaltung _verwaltung;
    private readonly FesteZeit _zeit = new(new DateTime(2026, 11, 1, 10, 0, 0));
    private readonly Geraet _beamer = new() { Inventarnummer = "G-001", Bezeichnung = "Beamer 01" };
    private readonly Entleiher _max = new() { Name = "Max Mustermann" };

    public ReservierungsVerwaltungTests()
    {
        _geraete = new SqliteGeraeteRepository(_db.Datenbank);
        _entleiher = new SqliteEntleiherRepository(_db.Datenbank);
        _verwaltung = new ReservierungsVerwaltung(
            new SqliteReservierungsRepository(_db.Datenbank), _geraete, _entleiher, _zeit);

        _geraete.Hinzufuegen(_beamer);
        _entleiher.Hinzufuegen(_max);
    }

    public void Dispose() => _db.Dispose();

    private static DateTime Um(int stunde) => new(2026, 11, 1, stunde, 0, 0);

    [Fact]
    public void Freier_Zeitraum_wird_reserviert()
    {
        var reservierung = _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(9), Um(12));

        Assert.True(reservierung.Id > 0);
        Assert.Equal(ReservierungsStatus.Reserviert, Assert.Single(_verwaltung.AlleLaden()).Status);
    }

    [Theory]
    [InlineData(12, 12)]
    [InlineData(12, 9)]
    public void Ende_gleich_oder_vor_Beginn_wird_abgelehnt(int beginn, int ende)
    {
        var fehler = Assert.Throws<FachlicherFehler>(() => _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(beginn), Um(ende)));

        Assert.Equal("Das Ende muss nach dem Beginn liegen.", fehler.Message);
        Assert.Empty(_verwaltung.AlleLaden());
    }

    [Fact]
    public void Ueberschneidung_wird_mit_dem_belegten_Zeitraum_gemeldet()
    {
        _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(9), Um(12));

        var fehler = Assert.Throws<FachlicherFehler>(() => _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(11), Um(14)));

        Assert.Equal("Das Gerät ist von 01.11.2026 09:00 bis 01.11.2026 12:00 bereits reserviert.", fehler.Message);
        Assert.Single(_verwaltung.AlleLaden());
    }

    [Fact]
    public void Inaktives_Geraet_kann_nicht_reserviert_werden()
    {
        _beamer.Aktiv = false;
        _geraete.Aktualisieren(_beamer);

        Assert.Throws<FachlicherFehler>(() => _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(9), Um(12)));
        Assert.Empty(_verwaltung.AlleLaden());
    }

    [Fact]
    public void Inaktiver_Entleiher_kann_nicht_reservieren()
    {
        _max.Aktiv = false;
        _entleiher.Aktualisieren(_max);

        Assert.Throws<FachlicherFehler>(() => _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(9), Um(12)));
        Assert.Empty(_verwaltung.AlleLaden());
    }

    [Fact]
    public void Verfuegbarkeit_pruefen_meldet_freien_und_belegten_Zeitraum()
    {
        _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(9), Um(12));

        Assert.NotNull(_verwaltung.VerfuegbarkeitPruefen(_beamer.Id, Um(10), Um(11)));
        Assert.Null(_verwaltung.VerfuegbarkeitPruefen(_beamer.Id, Um(12), Um(13)));
    }

    [Fact]
    public void Stornierte_Reservierung_bleibt_sichtbar_und_gibt_den_Zeitraum_frei()
    {
        var alt = _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(9), Um(12));

        _verwaltung.Stornieren(alt.Id);
        _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(9), Um(12));

        var alle = _verwaltung.AlleLaden();
        Assert.Equal(2, alle.Count);
        Assert.Equal(ReservierungsStatus.Storniert, alle.Single(r => r.Id == alt.Id).Status);
    }

    [Fact]
    public void Doppeltes_Stornieren_wird_abgelehnt()
    {
        var reservierung = _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(9), Um(12));
        _verwaltung.Stornieren(reservierung.Id);

        var fehler = Assert.Throws<FachlicherFehler>(() => _verwaltung.Stornieren(reservierung.Id));

        Assert.Equal("Die Reservierung ist bereits storniert.", fehler.Message);
    }

    [Fact]
    public void Ausgabe_und_Rueckgabe_speichern_Status_und_Zeitpunkt()
    {
        var reservierung = _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(9), Um(12));

        _verwaltung.Ausgeben(reservierung.Id);
        _zeit.Jetzt = Um(13);
        _verwaltung.Zurueckgeben(reservierung.Id);

        var gespeichert = Assert.Single(_verwaltung.AlleLaden());
        Assert.Equal(ReservierungsStatus.Zurueckgegeben, gespeichert.Status);
        Assert.Equal(Um(10), gespeichert.Ausgabezeitpunkt);
        Assert.Equal(Um(13), gespeichert.Rueckgabezeitpunkt);
    }

    [Fact]
    public void Ueberfaelligkeit_richtet_sich_nach_der_Zeitquelle()
    {
        var reservierung = _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(9), Um(12));
        _verwaltung.Ausgeben(reservierung.Id);
        var gespeichert = Assert.Single(_verwaltung.AlleLaden());

        _zeit.Jetzt = Um(12);
        Assert.False(_verwaltung.IstUeberfaellig(gespeichert));

        _zeit.Jetzt = Um(13);
        Assert.True(_verwaltung.IstUeberfaellig(gespeichert));
    }

    [Fact]
    public void Noch_ausgegebenes_Geraet_blockiert_die_Folgeausgabe_mit_Grund()
    {
        var erste = _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(9), Um(12));
        var folge = _verwaltung.Reservieren(_beamer.Id, _max.Id, Um(12), Um(15));
        _verwaltung.Ausgeben(erste.Id);
        _zeit.Jetzt = Um(13);

        var fehler = Assert.Throws<FachlicherFehler>(() => _verwaltung.Ausgeben(folge.Id));

        Assert.Equal("Das Gerät ist noch bei Max Mustermann (Rückgabe war bis 01.11.2026 12:00 geplant).", fehler.Message);
        Assert.Equal(fehler.Message, _verwaltung.AusgabeHindernis(folge, _verwaltung.AlleLaden()));

        _verwaltung.Zurueckgeben(erste.Id);
        Assert.Null(_verwaltung.AusgabeHindernis(folge, _verwaltung.AlleLaden()));
        _verwaltung.Ausgeben(folge.Id);
        Assert.Equal(ReservierungsStatus.Ausgegeben, _verwaltung.AlleLaden().Single(r => r.Id == folge.Id).Status);
    }
}
