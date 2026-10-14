using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class GeraetDialogViewModel : DialogViewModel
{
    private readonly GeraeteVerwaltung _verwaltung;
    private readonly Geraet? _vorhandenes;

    // Ohne Gerät legt der Dialog ein neues an, sonst bearbeitet er das übergebene.
    public GeraetDialogViewModel(GeraeteVerwaltung verwaltung, Geraet? vorhandenes = null)
    {
        _verwaltung = verwaltung;
        _vorhandenes = vorhandenes;
        Inventarnummer = vorhandenes?.Inventarnummer ?? "";
        Bezeichnung = vorhandenes?.Bezeichnung ?? "";
        Beschreibung = vorhandenes?.Beschreibung ?? "";
    }

    public override string Titel => _vorhandenes is null ? "Neues Gerät" : "Gerät bearbeiten";

    [ObservableProperty]
    public partial string Inventarnummer { get; set; }

    [ObservableProperty]
    public partial string Bezeichnung { get; set; }

    [ObservableProperty]
    public partial string Beschreibung { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HatFehler))]
    public partial string? Fehlermeldung { get; set; }

    public bool HatFehler => Fehlermeldung is not null;

    [RelayCommand]
    private void Speichern()
    {
        // Neues Objekt, damit die Liste bei einem Fehler nicht schon geänderte Werte zeigt
        var geraet = new Geraet
        {
            Id = _vorhandenes?.Id ?? 0,
            Aktiv = _vorhandenes?.Aktiv ?? true,
            Inventarnummer = Inventarnummer,
            Bezeichnung = Bezeichnung,
            Beschreibung = Beschreibung
        };

        try
        {
            _verwaltung.Speichern(geraet);
            Beenden(true);
        }
        catch (FachlicherFehler fehler)
        {
            Fehlermeldung = fehler.Message;
        }
    }
}
