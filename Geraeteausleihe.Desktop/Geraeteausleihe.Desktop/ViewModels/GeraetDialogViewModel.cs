using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class GeraetDialogViewModel : DialogViewModel
{
    private readonly GeraeteVerwaltung _verwaltung;

    public GeraetDialogViewModel(GeraeteVerwaltung verwaltung)
    {
        _verwaltung = verwaltung;
        Inventarnummer = "";
        Bezeichnung = "";
        Beschreibung = "";
    }

    public override string Titel => "Neues Gerät";

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
        var geraet = new Geraet
        {
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
