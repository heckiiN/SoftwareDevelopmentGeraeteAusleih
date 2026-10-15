using Geraeteausleihe.Core.Daten;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;
using Geraeteausleihe.Desktop.ViewModels;

namespace Geraeteausleihe.Tests;

// Kennzahlen der Übersicht, angesehen am 01.11.2026 um 10:00
public sealed class UebersichtTests : IDisposable
{
    private readonly TestDatenbank _db = new();
    private readonly FakeDialogDienst _dialoge = new();
    private readonly FesteZeit _zeit = new(Am(1, 10));
    private readonly GeraeteVerwaltung _geraete;
    private readonly EntleiherVerwaltung _entleiher;
    private readonly ReservierungsVerwaltung _reservierungen;
    private readonly UebersichtViewModel _uebersicht;
    private readonly Entleiher _max = new() { Name = "Max Mustermann" };

    public UebersichtTests()
    {
        var geraeteRepository = new SqliteGeraeteRepository(_db.Datenbank);
        var entleiherRepository = new SqliteEntleiherRepository(_db.Datenbank);
        var reservierungsRepository = new SqliteReservierungsRepository(_db.Datenbank);
        _geraete = new GeraeteVerwaltung(geraeteRepository, reservierungsRepository);
        _entleiher = new EntleiherVerwaltung(entleiherRepository, reservierungsRepository);
        _reservierungen = new ReservierungsVerwaltung(
            reservierungsRepository, geraeteRepository, entleiherRepository, _zeit);
        _uebersicht = new UebersichtViewModel(_reservierungen, _geraete, _entleiher, _dialoge);

        _entleiher.Speichern(_max);
    }

    public void Dispose() => _db.Dispose();

    private static DateTime Am(int tag, int stunde) => new(2026, 11, tag, stunde, 0, 0);

    private void Ausleihen(string geraet, DateTime beginn, DateTime ende, bool ausgeben = true)
    {
        var neuesGeraet = new Geraet { Inventarnummer = geraet, Bezeichnung = geraet };
        _geraete.Speichern(neuesGeraet);

        var reservierung = _reservierungen.Reservieren(neuesGeraet.Id, _max.Id, beginn, ende);
        if (ausgeben)
        {
            var heute = _zeit.Jetzt;
            _zeit.Jetzt = beginn;
            _reservierungen.Ausgeben(reservierung.Id);
            _zeit.Jetzt = heute;
        }
    }

    [Fact]
    public void Kennzahlen_zaehlen_laufende_heute_faellige_und_ueberfaellige_Ausleihen()
    {
        Ausleihen("Lautsprecher 01", Am(1, 9), Am(3, 17));
        Ausleihen("Beamer 01", Am(1, 8), Am(1, 18));
        Ausleihen("Leinwand 02", Am(1, 7), Am(1, 9));
        Ausleihen("Mikrofon 01", Am(5, 9), Am(5, 17), ausgeben: false);

        _uebersicht.Aktualisieren();

        Assert.Equal(3, _uebersicht.Laufende);
        Assert.Equal(1, _uebersicht.HeuteFaellige);
        Assert.Equal(1, _uebersicht.Ueberfaellige);
        Assert.Equal(["Leinwand 02", "Beamer 01", "Lautsprecher 01"], _uebersicht.Zeilen.Select(z => z.Geraet));
        Assert.Equal(["Überfällig", "Heute fällig", "Ausgegeben"], _uebersicht.Zeilen.Select(z => z.StatusText));
    }

    [Fact]
    public void Ohne_Ausleihen_ist_die_Liste_leer()
    {
        _uebersicht.Aktualisieren();

        Assert.True(_uebersicht.KeineAusleihen);
        Assert.False(_uebersicht.HatUeberfaellige);
    }

    [Fact]
    public void Neue_Reservierung_oeffnet_das_Formular()
    {
        DialogViewModel? geoeffnet = null;
        _dialoge.Bedienen = dialog => geoeffnet = dialog;

        _uebersicht.NeueReservierungCommand.Execute(null);

        Assert.IsType<ReservierungDialogViewModel>(geoeffnet);
    }
}
