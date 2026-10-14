using Geraeteausleihe.Core.Daten;
using Geraeteausleihe.Core.Domain;
using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Tests;

// T8: Die Datenbank selbst schützt vor ungültigen Daten, auch wenn die Prüfung
// in der Verwaltung umgangen wird, und ein abgelehnter Schreibvorgang ändert nichts.
public sealed class DatenhaltungTests : IDisposable
{
    private readonly TestDatenbank _db = new();
    private readonly SqliteGeraeteRepository _geraete;

    public DatenhaltungTests()
    {
        _geraete = new SqliteGeraeteRepository(_db.Datenbank);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public void Doppelte_Inventarnummer_wird_von_der_Datenbank_abgelehnt()
    {
        _geraete.Hinzufuegen(new Geraet { Inventarnummer = "G-001", Bezeichnung = "Beamer 01" });

        Assert.Throws<SqliteException>(() =>
            _geraete.Hinzufuegen(new Geraet { Inventarnummer = "g-001", Bezeichnung = "Beamer 02" }));

        var geraet = Assert.Single(_geraete.AlleLaden());
        Assert.Equal("Beamer 01", geraet.Bezeichnung);
    }

    [Fact]
    public void Abgelehnte_Aenderung_laesst_das_Geraet_unveraendert()
    {
        _geraete.Hinzufuegen(new Geraet { Inventarnummer = "G-001", Bezeichnung = "Beamer 01" });
        var leinwand = new Geraet { Inventarnummer = "G-002", Bezeichnung = "Leinwand 02" };
        _geraete.Hinzufuegen(leinwand);

        leinwand.Inventarnummer = "G-001";
        leinwand.Bezeichnung = "Geändert";
        Assert.Throws<SqliteException>(() => _geraete.Aktualisieren(leinwand));

        var gespeichert = _geraete.AlleLaden().Single(g => g.Id == leinwand.Id);
        Assert.Equal("G-002", gespeichert.Inventarnummer);
        Assert.Equal("Leinwand 02", gespeichert.Bezeichnung);
    }
}
