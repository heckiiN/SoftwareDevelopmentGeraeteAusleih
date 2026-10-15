using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;
using Geraeteausleihe.Desktop.Dienste;
using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class AusgabeViewModel : ObservableObject, IAktualisierbar
{
    private readonly ReservierungsVerwaltung _reservierungen;
    private readonly GeraeteVerwaltung _geraete;
    private readonly EntleiherVerwaltung _entleiher;
    private readonly IDialogDienst _dialoge;

    public AusgabeViewModel(
        ReservierungsVerwaltung reservierungen, GeraeteVerwaltung geraete, EntleiherVerwaltung entleiher,
        IDialogDienst dialoge)
    {
        _reservierungen = reservierungen;
        _geraete = geraete;
        _entleiher = entleiher;
        _dialoge = dialoge;
        Zeilen = [];
    }

    [ObservableProperty]
    public partial IReadOnlyList<AusgabeZeile> Zeilen { get; set; }

    [RelayCommand]
    private void Ausgeben(AusgabeZeile zeile)
    {
        if (_dialoge.Bestaetigen($"{zeile.Geraet} an {zeile.Entleiher} übergeben?"))
        {
            Ausfuehren(() => _reservierungen.Ausgeben(zeile.Reservierung.Id));
        }
    }

    [RelayCommand]
    private void Zurueckgeben(AusgabeZeile zeile)
    {
        if (_dialoge.Bestaetigen($"{zeile.Geraet} von {zeile.Entleiher} zurücknehmen?"))
        {
            Ausfuehren(() => _reservierungen.Zurueckgeben(zeile.Reservierung.Id));
        }
    }

    public void Aktualisieren()
    {
        var geraete = _geraete.AlleLaden().ToDictionary(g => g.Id, g => g.Bezeichnung);
        var entleiher = _entleiher.AlleLaden().ToDictionary(e => e.Id, e => e.Name);
        var alle = _reservierungen.AlleLaden();

        Zeilen = alle
            .Where(r => r.Status is ReservierungsStatus.Reserviert or ReservierungsStatus.Ausgegeben)
            .OrderBy(r => r.Beginn)
            .Select(r => new AusgabeZeile(r, geraete[r.GeraetId], entleiher[r.EntleiherId],
                _reservierungen.IstUeberfaellig(r),
                r.Status == ReservierungsStatus.Reserviert ? _reservierungen.AusgabeHindernis(r, alle) : null))
            .ToList();
    }

    private void Ausfuehren(Action aktion)
    {
        try
        {
            aktion();
        }
        catch (FachlicherFehler fehler)
        {
            _dialoge.Hinweis(fehler.Message);
        }
        catch (SqliteException)
        {
            _dialoge.Hinweis(Meldungen.Datenbankfehler);
        }
        Aktualisieren();
    }
}
