using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;
using Geraeteausleihe.Desktop.Dienste;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class GeraeteViewModel : ObservableObject
{
    private readonly GeraeteVerwaltung _verwaltung;
    private readonly IDialogDienst _dialoge;
    private IReadOnlyList<Geraet> _alleGeraete = [];

    public GeraeteViewModel(GeraeteVerwaltung verwaltung, IDialogDienst dialoge)
    {
        _verwaltung = verwaltung;
        _dialoge = dialoge;
        Suchtext = "";
        NurAktive = true;
        Laden();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Geraete))]
    public partial string Suchtext { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Geraete))]
    public partial bool NurAktive { get; set; }

    public IReadOnlyList<Geraet> Geraete => _alleGeraete
        .Where(g => !NurAktive || g.Aktiv)
        .Where(g => Passt(g.Inventarnummer) || Passt(g.Bezeichnung) || Passt(g.Beschreibung))
        .ToList();

    [RelayCommand]
    private void NeuesGeraet()
    {
        if (_dialoge.Zeigen(new GeraetDialogViewModel(_verwaltung)))
            Laden();
    }

    private void Laden()
    {
        _alleGeraete = _verwaltung.AlleLaden();
        OnPropertyChanged(nameof(Geraete));
    }

    private bool Passt(string? feld) =>
        feld is not null && feld.Contains(Suchtext.Trim(), StringComparison.CurrentCultureIgnoreCase);
}
