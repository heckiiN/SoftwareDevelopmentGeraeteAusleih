using CommunityToolkit.Mvvm.ComponentModel;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class GeraeteViewModel : ObservableObject
{
    private readonly GeraeteVerwaltung _verwaltung;

    public GeraeteViewModel(GeraeteVerwaltung verwaltung)
    {
        _verwaltung = verwaltung;
        Geraete = _verwaltung.AlleLaden();
    }

    [ObservableProperty]
    public partial IReadOnlyList<Geraet> Geraete { get; private set; }
}
