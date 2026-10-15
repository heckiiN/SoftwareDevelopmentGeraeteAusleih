using CommunityToolkit.Mvvm.ComponentModel;
using Geraeteausleihe.Core.Dienste;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class HauptViewModel : ObservableObject
{
    private readonly IZeitquelle _zeit;

    public HauptViewModel(UebersichtViewModel uebersicht, GeraeteViewModel geraete, EntleiherViewModel entleiher,
        ReservierungenViewModel reservierungen, AusgabeViewModel ausgabe, IZeitquelle zeit)
    {
        _zeit = zeit;
        Stand = "";
        Navigation =
        [
            new("Übersicht", "", "Geräteausleihe", "Vereinsinterne Geräteverwaltung", uebersicht),
            new("Geräte", "", "Geräteausleihe", "Vereinsinterne Geräteverwaltung", geraete),
            new("Entleiher", "", "Entleiherverwaltung", "Übersicht aller eingetragenen Vereinsmitglieder",
                entleiher),
            new("Reservierungen", "", "Reservierungsübersicht",
                "Übersicht aller anstehenden und laufenden Reservierungen", reservierungen),
            new("Ausgabe & Rückgabe", "", "Ausgabe & Rückgabe",
                "Geräte an Entleiher übergeben oder zurücknehmen", ausgabe)
        ];
        AktuellerPunkt = Navigation[0];
    }

    public IReadOnlyList<Navigationspunkt> Navigation { get; }

    [ObservableProperty]
    public partial Navigationspunkt AktuellerPunkt { get; set; }

    // Zeigt, wann die angezeigten Daten geladen wurden.
    [ObservableProperty]
    public partial string Stand { get; set; }

    partial void OnAktuellerPunktChanged(Navigationspunkt value)
    {
        if (value.Seite is IAktualisierbar seite)
        {
            seite.Aktualisieren();
            Stand = $"Stand: {_zeit.Jetzt:dd.MM.yyyy, HH:mm} Uhr";
        }
    }
}
