using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;
using Geraeteausleihe.Desktop.Dienste;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class EntleiherViewModel : ObservableObject
{
    private readonly EntleiherVerwaltung _verwaltung;
    private readonly IDialogDienst _dialoge;
    private IReadOnlyList<Entleiher> _alleEntleiher = [];

    public EntleiherViewModel(EntleiherVerwaltung verwaltung, IDialogDienst dialoge)
    {
        _verwaltung = verwaltung;
        _dialoge = dialoge;
        Suchtext = "";
        Laden();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Entleiher))]
    public partial string Suchtext { get; set; }

    public IReadOnlyList<Entleiher> Entleiher => _alleEntleiher
        .Where(e => Passt(e.Name) || Passt(e.Kontakt))
        .ToList();

    [RelayCommand]
    private void NeuerEntleiher()
    {
        if (_dialoge.Zeigen(new EntleiherDialogViewModel(_verwaltung)))
            Laden();
    }

    [RelayCommand]
    private void Bearbeiten(Entleiher person)
    {
        if (_dialoge.Zeigen(new EntleiherDialogViewModel(_verwaltung, person)))
            Laden();
    }

    private void Laden()
    {
        _alleEntleiher = _verwaltung.AlleLaden();
        OnPropertyChanged(nameof(Entleiher));
    }

    private bool Passt(string? feld) =>
        feld is not null && feld.Contains(Suchtext.Trim(), StringComparison.CurrentCultureIgnoreCase);
}
