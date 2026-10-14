using CommunityToolkit.Mvvm.ComponentModel;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class HauptViewModel : ObservableObject
{
    public HauptViewModel(GeraeteViewModel geraete, EntleiherViewModel entleiher, ReservierungenViewModel reservierungen)
    {
        Navigation =
        [
            new("Übersicht", "", new PlatzhalterViewModel("Übersicht")),
            new("Geräte", "", geraete),
            new("Entleiher", "", entleiher),
            new("Reservierungen", "", reservierungen),
            new("Ausgabe & Rückgabe", "", new PlatzhalterViewModel("Ausgabe & Rückgabe"))
        ];
        AktuellerPunkt = Navigation[0];
    }

    public IReadOnlyList<Navigationspunkt> Navigation { get; }

    [ObservableProperty]
    public partial Navigationspunkt AktuellerPunkt { get; set; }
}
