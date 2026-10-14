using CommunityToolkit.Mvvm.ComponentModel;
using Geraeteausleihe.Core.Dienste;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class ReservierungenViewModel : ObservableObject, IAktualisierbar
{
    private readonly ReservierungsVerwaltung _reservierungen;
    private readonly GeraeteVerwaltung _geraete;
    private readonly EntleiherVerwaltung _entleiher;
    private IReadOnlyList<ReservierungsZeile> _alleZeilen = [];

    public ReservierungenViewModel(
        ReservierungsVerwaltung reservierungen, GeraeteVerwaltung geraete, EntleiherVerwaltung entleiher)
    {
        _reservierungen = reservierungen;
        _geraete = geraete;
        _entleiher = entleiher;
        Suchtext = "";
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Zeilen))]
    public partial string Suchtext { get; set; }

    public IReadOnlyList<ReservierungsZeile> Zeilen => _alleZeilen
        .Where(z => Passt(z.Geraet) || Passt(z.Entleiher))
        .ToList();

    public void Aktualisieren()
    {
        var geraete = _geraete.AlleLaden().ToDictionary(g => g.Id, g => g.Bezeichnung);
        var entleiher = _entleiher.AlleLaden().ToDictionary(e => e.Id, e => e.Name);

        _alleZeilen = _reservierungen.AlleLaden()
            .Select(r => new ReservierungsZeile(r, geraete[r.GeraetId], entleiher[r.EntleiherId]))
            .ToList();
        OnPropertyChanged(nameof(Zeilen));
    }

    private bool Passt(string feld) => feld.Contains(Suchtext.Trim(), StringComparison.CurrentCultureIgnoreCase);
}
