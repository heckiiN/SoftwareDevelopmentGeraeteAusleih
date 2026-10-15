using CommunityToolkit.Mvvm.ComponentModel;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Desktop.Dienste;
using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class HauptViewModel : ObservableObject
{
    private readonly IZeitquelle _zeit;
    private readonly IDialogDienst _dialoge;

    public HauptViewModel(UebersichtViewModel uebersicht, GeraeteViewModel geraete, EntleiherViewModel entleiher,
        ReservierungenViewModel reservierungen, AusgabeViewModel ausgabe, IZeitquelle zeit, IDialogDienst dialoge)
    {
        _zeit = zeit;
        _dialoge = dialoge;
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
        if (value.Seite is not IAktualisierbar seite)
        {
            return;
        }

        // Der Seitenwechsel kommt über ein Binding, und WPF verschluckt dort Ausnahmen still.
        // Deshalb wird ein Ladefehler hier selbst gemeldet. Der Stand bleibt beim letzten erfolgreichen Laden.
        try
        {
            seite.Aktualisieren();
            Stand = $"Stand: {_zeit.Jetzt:dd.MM.yyyy, HH:mm} Uhr";
        }
        catch (SqliteException)
        {
            _dialoge.Hinweis(Meldungen.Ladefehler);
        }
    }
}
