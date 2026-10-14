using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;
using Geraeteausleihe.Desktop.Dienste;
using Microsoft.Data.Sqlite;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class EntleiherViewModel : ObservableObject, IAktualisierbar
{
    private readonly EntleiherVerwaltung _verwaltung;
    private readonly IDialogDienst _dialoge;
    private IReadOnlyList<Entleiher> _alleEntleiher = [];

    public EntleiherViewModel(EntleiherVerwaltung verwaltung, IDialogDienst dialoge)
    {
        _verwaltung = verwaltung;
        _dialoge = dialoge;
        Suchtext = "";
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Entleiher))]
    public partial string Suchtext { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UmschaltenText))]
    [NotifyCanExecuteChangedFor(nameof(AktivUmschaltenCommand))]
    public partial Entleiher? AusgewaehlterEntleiher { get; set; }

    public string UmschaltenText => AusgewaehlterEntleiher is { Aktiv: false } ? "Aktivieren" : "Deaktivieren";

    public IReadOnlyList<Entleiher> Entleiher => _alleEntleiher
        .Where(e => Passt(e.Name) || Passt(e.Kontakt))
        .ToList();

    [RelayCommand]
    private void NeuerEntleiher()
    {
        if (_dialoge.Zeigen(new EntleiherDialogViewModel(_verwaltung)))
        {
            Aktualisieren();
        }
    }

    [RelayCommand]
    private void Bearbeiten(Entleiher person)
    {
        if (_dialoge.Zeigen(new EntleiherDialogViewModel(_verwaltung, person)))
        {
            Aktualisieren();
        }
    }

    [RelayCommand(CanExecute = nameof(IstEntleiherAusgewaehlt))]
    private void AktivUmschalten()
    {
        var person = AusgewaehlterEntleiher!;
        try
        {
            if (person.Aktiv)
            {
                _verwaltung.Deaktivieren(person);
            }
            else
            {
                _verwaltung.Aktivieren(person);
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

    private bool IstEntleiherAusgewaehlt() => AusgewaehlterEntleiher is not null;

    public void Aktualisieren()
    {
        _alleEntleiher = _verwaltung.AlleLaden();
        OnPropertyChanged(nameof(Entleiher));
    }

    private bool Passt(string? feld) =>
        feld is not null && feld.Contains(Suchtext.Trim(), StringComparison.CurrentCultureIgnoreCase);
}
