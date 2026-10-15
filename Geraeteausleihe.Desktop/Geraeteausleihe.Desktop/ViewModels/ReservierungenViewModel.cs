using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;
using Geraeteausleihe.Desktop.Dienste;
using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class ReservierungenViewModel : ObservableObject, IAktualisierbar
{
    private readonly ReservierungsVerwaltung _reservierungen;
    private readonly GeraeteVerwaltung _geraete;
    private readonly EntleiherVerwaltung _entleiher;
    private readonly IDialogDienst _dialoge;
    private IReadOnlyList<ReservierungsZeile> _alleZeilen = [];

    public ReservierungenViewModel(
        ReservierungsVerwaltung reservierungen, GeraeteVerwaltung geraete, EntleiherVerwaltung entleiher,
        IDialogDienst dialoge)
    {
        _reservierungen = reservierungen;
        _geraete = geraete;
        _entleiher = entleiher;
        _dialoge = dialoge;
        Suchtext = "";
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Zeilen))]
    public partial string Suchtext { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StornierenCommand))]
    public partial ReservierungsZeile? AusgewaehlteZeile { get; set; }

    public IReadOnlyList<ReservierungsZeile> Zeilen => _alleZeilen
        .Where(z => Passt(z.Geraet) || Passt(z.Entleiher))
        .ToList();

    [RelayCommand]
    private void NeueReservierung()
    {
        if (_dialoge.Zeigen(new ReservierungDialogViewModel(_reservierungen, _geraete, _entleiher, _dialoge)))
        {
            Aktualisieren();
        }
    }

    [RelayCommand(CanExecute = nameof(IstStornierbar))]
    private void Stornieren()
    {
        var zeile = AusgewaehlteZeile!;
        var reservierung = zeile.Reservierung;
        var frage = $"Reservierung von {zeile.Geraet} für {zeile.Entleiher} " +
            $"({reservierung.Beginn:dd.MM.yyyy, HH:mm} bis {reservierung.Ende:dd.MM.yyyy, HH:mm}) stornieren?";
        if (!_dialoge.Bestaetigen(frage))
        {
            return;
        }

        try
        {
            _reservierungen.Stornieren(reservierung.Id);
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

    private bool IstStornierbar() => AusgewaehlteZeile?.Reservierung.Status == ReservierungsStatus.Reserviert;

    public void Aktualisieren()
    {
        var geraete = _geraete.AlleLaden().ToDictionary(g => g.Id, g => g.Bezeichnung);
        var entleiher = _entleiher.AlleLaden().ToDictionary(e => e.Id, e => e.Name);

        _alleZeilen = _reservierungen.AlleLaden()
            .Select(r => new ReservierungsZeile(r, geraete[r.GeraetId], entleiher[r.EntleiherId],
                _reservierungen.IstUeberfaellig(r)))
            .ToList();
        OnPropertyChanged(nameof(Zeilen));
    }

    private bool Passt(string feld) => feld.Contains(Suchtext.Trim(), StringComparison.CurrentCultureIgnoreCase);
}
