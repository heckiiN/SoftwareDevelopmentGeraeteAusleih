using CommunityToolkit.Mvvm.ComponentModel;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class HauptViewModel : ObservableObject
{
    public HauptViewModel()
    {
        Navigation =
        [
            new("Übersicht", "", new PlatzhalterViewModel("Übersicht")),
            new("Geräte", "", new PlatzhalterViewModel("Geräte")),
            new("Entleiher", "", new PlatzhalterViewModel("Entleiher")),
            new("Reservierungen", "", new PlatzhalterViewModel("Reservierungen")),
            new("Ausgabe & Rückgabe", "", new PlatzhalterViewModel("Ausgabe & Rückgabe"))
        ];
        AktuellerPunkt = Navigation[0];
    }

    public IReadOnlyList<Navigationspunkt> Navigation { get; }

    [ObservableProperty]
    public partial Navigationspunkt AktuellerPunkt { get; set; }
}
