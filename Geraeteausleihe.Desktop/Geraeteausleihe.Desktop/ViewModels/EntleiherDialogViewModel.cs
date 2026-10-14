using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class EntleiherDialogViewModel : DialogViewModel
{
    private readonly EntleiherVerwaltung _verwaltung;
    private readonly Entleiher? _vorhandener;

    public EntleiherDialogViewModel(EntleiherVerwaltung verwaltung, Entleiher? vorhandener = null)
    {
        _verwaltung = verwaltung;
        _vorhandener = vorhandener;
        Name = vorhandener?.Name ?? "";
        Kontakt = vorhandener?.Kontakt ?? "";
    }

    public override string Titel => _vorhandener is null ? "Neuen Entleiher anlegen" : "Entleiher bearbeiten";

    // Im Reservierungsformular heißt der Button „Speichern und auswählen“.
    public string SpeichernText { get; init; } = "Speichern";

    public Entleiher? Gespeichert { get; private set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Kontakt { get; set; }

    [RelayCommand]
    private void Speichern()
    {
        var person = new Entleiher
        {
            Id = _vorhandener?.Id ?? 0,
            Aktiv = _vorhandener?.Aktiv ?? true,
            Name = Name,
            Kontakt = Kontakt
        };

        try
        {
            _verwaltung.Speichern(person);
            Gespeichert = person;
            Beenden(true);
        }
        catch (FachlicherFehler fehler)
        {
            Fehlermeldung = fehler.Message;
        }
    }
}
