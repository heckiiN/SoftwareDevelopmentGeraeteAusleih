using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;
using Geraeteausleihe.Desktop.Dienste;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class UebersichtViewModel : ObservableObject, IAktualisierbar
{
    private readonly ReservierungsVerwaltung _reservierungen;
    private readonly GeraeteVerwaltung _geraete;
    private readonly EntleiherVerwaltung _entleiher;
    private readonly IDialogDienst _dialoge;

    public UebersichtViewModel(
        ReservierungsVerwaltung reservierungen, GeraeteVerwaltung geraete, EntleiherVerwaltung entleiher,
        IDialogDienst dialoge)
    {
        _reservierungen = reservierungen;
        _geraete = geraete;
        _entleiher = entleiher;
        _dialoge = dialoge;
        Zeilen = [];
    }

    // Die offenen Ausleihen: alles, was gerade ausgegeben ist, auch die überfälligen.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Laufende), nameof(HeuteFaellige), nameof(Ueberfaellige),
        nameof(HatHeuteFaellige), nameof(HatUeberfaellige), nameof(KeineAusleihen))]
    public partial IReadOnlyList<ReservierungsZeile> Zeilen { get; set; }

    public int Laufende => Zeilen.Count;
    public int HeuteFaellige => Zeilen.Count(z => z.HeuteFaellig);
    public int Ueberfaellige => Zeilen.Count(z => z.Ueberfaellig);
    public bool HatHeuteFaellige => HeuteFaellige > 0;
    public bool HatUeberfaellige => Ueberfaellige > 0;
    public bool KeineAusleihen => Zeilen.Count == 0;

    [RelayCommand]
    private void NeueReservierung()
    {
        _dialoge.Zeigen(new ReservierungDialogViewModel(_reservierungen, _geraete, _entleiher, _dialoge));
    }

    public void Aktualisieren()
    {
        var geraete = _geraete.AlleLaden().ToDictionary(g => g.Id, g => g.Bezeichnung);
        var entleiher = _entleiher.AlleLaden().ToDictionary(e => e.Id, e => e.Name);

        Zeilen = _reservierungen.AlleLaden()
            .Where(r => r.Status == ReservierungsStatus.Ausgegeben)
            .OrderBy(r => r.Ende)
            .Select(r => new ReservierungsZeile(r, geraete[r.GeraetId], entleiher[r.EntleiherId],
                _reservierungen.IstUeberfaellig(r))
            {
                HeuteFaellig = _reservierungen.IstHeuteFaellig(r)
            })
            .ToList();
    }
}
