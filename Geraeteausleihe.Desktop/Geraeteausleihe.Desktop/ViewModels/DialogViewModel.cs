using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geraeteausleihe.Core.Domain;
using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Desktop.ViewModels;

public abstract partial class DialogViewModel : ObservableObject
{
    // true, wenn der Dialog mit Speichern beendet wurde
    public event Action<bool>? Schliessen;

    public abstract string Titel { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HatFehler))]
    public partial string? Fehlermeldung { get; set; }

    public bool HatFehler => Fehlermeldung is not null;

    protected void Beenden(bool gespeichert) => Schliessen?.Invoke(gespeichert);

    // Bei einem Fehler bleibt der Dialog offen und zeigt die Meldung, damit man korrigieren und erneut speichern kann.
    protected bool Versuchen(Action aktion)
    {
        try
        {
            aktion();
            return true;
        }
        catch (FachlicherFehler fehler)
        {
            Fehlermeldung = fehler.Message;
        }
        catch (SqliteException)
        {
            Fehlermeldung = Meldungen.Datenbankfehler;
        }
        return false;
    }

    [RelayCommand]
    private void Abbrechen() => Beenden(false);
}
