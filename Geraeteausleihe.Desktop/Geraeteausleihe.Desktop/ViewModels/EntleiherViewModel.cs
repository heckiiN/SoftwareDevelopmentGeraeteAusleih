using CommunityToolkit.Mvvm.ComponentModel;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class EntleiherViewModel : ObservableObject
{
    private readonly EntleiherVerwaltung _verwaltung;
    private IReadOnlyList<Entleiher> _alleEntleiher = [];

    public EntleiherViewModel(EntleiherVerwaltung verwaltung)
    {
        _verwaltung = verwaltung;
        Suchtext = "";
        Laden();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Entleiher))]
    public partial string Suchtext { get; set; }

    public IReadOnlyList<Entleiher> Entleiher => _alleEntleiher
        .Where(e => Passt(e.Name) || Passt(e.Kontakt))
        .ToList();

    private void Laden()
    {
        _alleEntleiher = _verwaltung.AlleLaden();
        OnPropertyChanged(nameof(Entleiher));
    }

    private bool Passt(string? feld) =>
        feld is not null && feld.Contains(Suchtext.Trim(), StringComparison.CurrentCultureIgnoreCase);
}
