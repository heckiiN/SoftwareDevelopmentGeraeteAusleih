using Geraeteausleihe.Core.Daten;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;
using Geraeteausleihe.Desktop.ViewModels;

namespace Geraeteausleihe.Tests;

// T10 bis T13: neuer Entleiher direkt aus dem Reservierungsformular, gegen eine echte Testdatenbank
public sealed class ReservierungDialogTests : IDisposable
{
    private readonly TestDatenbank _db = new();
    private readonly FakeDialogDienst _dialoge = new();
    private readonly EntleiherVerwaltung _entleiher;
    private readonly ReservierungsVerwaltung _reservierungen;
    private readonly ReservierungDialogViewModel _formular;

    public ReservierungDialogTests()
    {
        var geraeteRepository = new SqliteGeraeteRepository(_db.Datenbank);
        var entleiherRepository = new SqliteEntleiherRepository(_db.Datenbank);
        var reservierungsRepository = new SqliteReservierungsRepository(_db.Datenbank);
        var geraete = new GeraeteVerwaltung(geraeteRepository, reservierungsRepository);
        _entleiher = new EntleiherVerwaltung(entleiherRepository, reservierungsRepository);
        _reservierungen = new ReservierungsVerwaltung(
            reservierungsRepository, geraeteRepository, entleiherRepository, new Systemzeit());

        geraete.Speichern(new Geraet { Inventarnummer = "G-002", Bezeichnung = "Leinwand 02" });
        _entleiher.Speichern(new Entleiher { Name = "Max Mustermann" });

        _formular = new ReservierungDialogViewModel(_reservierungen, geraete, _entleiher, _dialoge);
        _formular.AusgewaehltesGeraet = _formular.Geraete.Single();
        _formular.AusgewaehlterEntleiher = _formular.Entleiher.Single();
        _formular.Beginn = "01.11.2026, 09:00";
        _formular.Ende = "03.11.2026, 17:00";
    }

    public void Dispose() => _db.Dispose();

    private static Action<DialogViewModel> Eingeben(string name) => dialog =>
    {
        var entleiherDialog = Assert.IsType<EntleiherDialogViewModel>(dialog);
        entleiherDialog.Name = name;
        entleiherDialog.SpeichernCommand.Execute(null);
    };

    private void FormularBleibtErhalten()
    {
        Assert.Equal("Leinwand 02", _formular.AusgewaehltesGeraet?.Bezeichnung);
        Assert.Equal("01.11.2026, 09:00", _formular.Beginn);
        Assert.Equal("03.11.2026, 17:00", _formular.Ende);
    }

    [Fact]
    public void T10_Neuer_Entleiher_wird_gespeichert_und_ausgewaehlt()
    {
        _dialoge.Bedienen = Eingeben("Lena Beispiel");

        _formular.NeuerEntleiherCommand.Execute(null);

        Assert.Single(_entleiher.AlleLaden(), e => e.Name == "Lena Beispiel");
        Assert.Equal("Lena Beispiel", _formular.AusgewaehlterEntleiher?.Name);
        FormularBleibtErhalten();
        Assert.Empty(_reservierungen.AlleLaden());

        _formular.ReservierenCommand.Execute(null);

        var reservierung = Assert.Single(_reservierungen.AlleLaden());
        Assert.Equal(_formular.AusgewaehlterEntleiher!.Id, reservierung.EntleiherId);
    }

    [Fact]
    public void T11_Abbrechen_legt_keinen_Entleiher_an()
    {
        _dialoge.Bedienen = dialog =>
        {
            ((EntleiherDialogViewModel)dialog).Name = "Lena Beispiel";
            dialog.AbbrechenCommand.Execute(null);
        };

        _formular.NeuerEntleiherCommand.Execute(null);

        Assert.Single(_entleiher.AlleLaden());
        Assert.Equal("Max Mustermann", _formular.AusgewaehlterEntleiher?.Name);
        FormularBleibtErhalten();
    }

    [Fact]
    public void T12_Leerer_Name_zeigt_Meldung_und_erlaubt_neuen_Versuch()
    {
        _dialoge.Bedienen = dialog =>
        {
            Eingeben("  ")(dialog);
            Assert.True(dialog.HatFehler);
            Assert.Single(_entleiher.AlleLaden());

            Eingeben("Lena Beispiel")(dialog);
        };

        _formular.NeuerEntleiherCommand.Execute(null);

        Assert.Equal(2, _entleiher.AlleLaden().Count);
        Assert.Equal("Lena Beispiel", _formular.AusgewaehlterEntleiher?.Name);
    }

    [Fact]
    public void T12_Datenbankfehler_zeigt_Meldung_im_offenen_Dialog()
    {
        _db.Schreibschutz();
        _dialoge.Bedienen = dialog =>
        {
            Eingeben("Lena Beispiel")(dialog);
            Assert.Equal(Meldungen.Datenbankfehler, dialog.Fehlermeldung);
            dialog.AbbrechenCommand.Execute(null);
        };

        _formular.NeuerEntleiherCommand.Execute(null);

        Assert.Single(_entleiher.AlleLaden());
        Assert.Equal("Max Mustermann", _formular.AusgewaehlterEntleiher?.Name);
    }

    [Fact]
    public void Datenbankfehler_beim_Reservieren_laesst_das_Formular_offen()
    {
        var geschlossen = false;
        _formular.Schliessen += _ => geschlossen = true;
        _db.Schreibschutz();

        _formular.ReservierenCommand.Execute(null);

        Assert.False(geschlossen);
        Assert.Equal(Meldungen.Datenbankfehler, _formular.Fehlermeldung);
        Assert.Empty(_reservierungen.AlleLaden());
    }

    [Fact]
    public void T13_Abgebrochene_Reservierung_behaelt_den_neuen_Entleiher()
    {
        _dialoge.Bedienen = Eingeben("Lena Beispiel");
        _formular.NeuerEntleiherCommand.Execute(null);

        _formular.AbbrechenCommand.Execute(null);

        Assert.Single(_entleiher.AlleLaden(), e => e.Name == "Lena Beispiel");
        Assert.Empty(_reservierungen.AlleLaden());
    }
}
