using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;
using Geraeteausleihe.Desktop.Dienste;
using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class GeraeteViewModel : ObservableObject, IAktualisierbar
{
    private readonly GeraeteVerwaltung _verwaltung;
    private readonly IDialogDienst _dialoge;
    private IReadOnlyList<Geraet> _alleGeraete = [];

    public GeraeteViewModel(GeraeteVerwaltung verwaltung, IDialogDienst dialoge)
    {
        _verwaltung = verwaltung;
        _dialoge = dialoge;
        Suchtext = "";
        NurAktive = true;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Geraete))]
    public partial string Suchtext { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Geraete))]
    public partial bool NurAktive { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UmschaltenText))]
    [NotifyCanExecuteChangedFor(nameof(AktivUmschaltenCommand))]
    public partial Geraet? AusgewaehltesGeraet { get; set; }

    public string UmschaltenText => AusgewaehltesGeraet is { Aktiv: false } ? "Aktivieren" : "Deaktivieren";

    public IReadOnlyList<Geraet> Geraete => _alleGeraete
        .Where(g => !NurAktive || g.Aktiv)
        .Where(g => Passt(g.Inventarnummer) || Passt(g.Bezeichnung) || Passt(g.Beschreibung))
        .ToList();

    [RelayCommand]
    private void NeuesGeraet()
    {
        if (_dialoge.Zeigen(new GeraetDialogViewModel(_verwaltung)))
        {
            Aktualisieren();
        }
    }

    [RelayCommand]
    private void Bearbeiten(Geraet geraet)
    {
        if (_dialoge.Zeigen(new GeraetDialogViewModel(_verwaltung, geraet)))
        {
            Aktualisieren();
        }
    }

    [RelayCommand(CanExecute = nameof(IstGeraetAusgewaehlt))]
    private void AktivUmschalten()
    {
        var geraet = AusgewaehltesGeraet!;
        try
        {
            if (geraet.Aktiv)
            {
                _verwaltung.Deaktivieren(geraet);
            }
            else
            {
                _verwaltung.Aktivieren(geraet);
            }
            Aktualisieren();
        }
        catch (FachlicherFehler fehler)
        {
            _dialoge.Hinweis(fehler.Message);
        }
        catch (SqliteException)
        {
            _dialoge.Hinweis(Meldungen.Datenbankfehler);
        }
    }

    private bool IstGeraetAusgewaehlt() => AusgewaehltesGeraet is not null;

    public void Aktualisieren()
    {
        _alleGeraete = _verwaltung.AlleLaden();
        OnPropertyChanged(nameof(Geraete));
    }

    private bool Passt(string? feld) =>
        feld is not null && feld.Contains(Suchtext.Trim(), StringComparison.CurrentCultureIgnoreCase);
}
