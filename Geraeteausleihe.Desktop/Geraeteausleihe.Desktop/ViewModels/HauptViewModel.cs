using CommunityToolkit.Mvvm.ComponentModel;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class HauptViewModel : ObservableObject
{
    public HauptViewModel(GeraeteViewModel geraete, EntleiherViewModel entleiher, ReservierungenViewModel reservierungen)
    {
        Navigation =
        [
            new("Übersicht", "", "Geräteausleihe", "Vereinsinterne Geräteverwaltung",
                new PlatzhalterViewModel("Übersicht")),
            new("Geräte", "", "Geräteausleihe", "Vereinsinterne Geräteverwaltung", geraete),
            new("Entleiher", "", "Entleiherverwaltung", "Übersicht aller eingetragenen Vereinsmitglieder",
                entleiher),
            new("Reservierungen", "", "Reservierungsübersicht",
                "Übersicht aller anstehenden und laufenden Reservierungen", reservierungen),
            new("Ausgabe & Rückgabe", "", "Ausgabe & Rückgabe",
                "Geräte an Entleiher übergeben oder zurücknehmen", new PlatzhalterViewModel("Ausgabe & Rückgabe"))
        ];
        AktuellerPunkt = Navigation[0];
    }

    public IReadOnlyList<Navigationspunkt> Navigation { get; }

    [ObservableProperty]
    public partial Navigationspunkt AktuellerPunkt { get; set; }

    partial void OnAktuellerPunktChanged(Navigationspunkt value)
    {
        if (value.Seite is IAktualisierbar seite)
        {
            seite.Aktualisieren();
        }
    }
}
