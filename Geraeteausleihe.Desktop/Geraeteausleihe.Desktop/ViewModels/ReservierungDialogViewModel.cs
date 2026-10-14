using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;
using Geraeteausleihe.Desktop.Dienste;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class ReservierungDialogViewModel : DialogViewModel
{
    private const string Zeitformat = "dd.MM.yyyy, HH:mm";
    private static readonly string[] Eingabeformate = ["d.M.yyyy, H:mm", "d.M.yyyy H:mm"];
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    private readonly ReservierungsVerwaltung _verwaltung;
    private readonly EntleiherVerwaltung _entleiherVerwaltung;
    private readonly IDialogDienst _dialoge;

    public ReservierungDialogViewModel(ReservierungsVerwaltung verwaltung, GeraeteVerwaltung geraete,
        EntleiherVerwaltung entleiher, IDialogDienst dialoge)
    {
        _verwaltung = verwaltung;
        _entleiherVerwaltung = entleiher;
        _dialoge = dialoge;
        Geraete = geraete.AlleLaden().Where(g => g.Aktiv).OrderBy(g => g.Bezeichnung).ToList();
        Entleiher = AktiveEntleiher();

        var morgen = DateTime.Today.AddDays(1);
        Beginn = morgen.AddHours(9).ToString(Zeitformat, Deutsch);
        Ende = morgen.AddHours(17).ToString(Zeitformat, Deutsch);
    }

    public override string Titel => "Neue Reservierung anlegen";

    // Deaktivierte Geräte und Entleiher dürfen nicht reserviert werden, deshalb fehlen sie hier.
    public IReadOnlyList<Geraet> Geraete { get; }

    [ObservableProperty]
    public partial IReadOnlyList<Entleiher> Entleiher { get; set; }

    [ObservableProperty]
    public partial Geraet? AusgewaehltesGeraet { get; set; }

    [ObservableProperty]
    public partial Entleiher? AusgewaehlterEntleiher { get; set; }

    [ObservableProperty]
    public partial string Beginn { get; set; }

    [ObservableProperty]
    public partial string Ende { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IstVerfuegbar))]
    public partial string? Verfuegbarkeitsmeldung { get; set; }

    public bool IstVerfuegbar => Verfuegbarkeitsmeldung is not null;

    // Eine alte Meldung passt nicht mehr, sobald sich eine Eingabe ändert.
    partial void OnAusgewaehltesGeraetChanged(Geraet? value) => MeldungenLoeschen();
    partial void OnAusgewaehlterEntleiherChanged(Entleiher? value) => MeldungenLoeschen();
    partial void OnBeginnChanged(string value) => MeldungenLoeschen();
    partial void OnEndeChanged(string value) => MeldungenLoeschen();

    // Der neue Entleiher wird sofort gespeichert. Bricht man danach die Reservierung ab,
    // bleibt er trotzdem in den Stammdaten.
    [RelayCommand]
    private void NeuerEntleiher()
    {
        var dialog = new EntleiherDialogViewModel(_entleiherVerwaltung) { SpeichernText = "Speichern und auswählen" };
        if (_dialoge.Zeigen(dialog) && dialog.Gespeichert is { } neu)
        {
            Entleiher = AktiveEntleiher();
            AusgewaehlterEntleiher = Entleiher.Single(e => e.Id == neu.Id);
        }
    }

    [RelayCommand]
    private void VerfuegbarkeitPruefen()
    {
        MeldungenLoeschen();
        if (AusgewaehltesGeraet is null)
        {
            Fehlermeldung = "Bitte ein Gerät auswählen.";
            return;
        }
        if (!ZeitraumLesen(out var beginn, out var ende))
        {
            return;
        }

        var geraetId = AusgewaehltesGeraet.Id;
        Reservierung? konflikt = null;
        if (!Versuchen(() => konflikt = _verwaltung.VerfuegbarkeitPruefen(geraetId, beginn, ende)))
        {
            return;
        }

        if (konflikt is null)
        {
            Verfuegbarkeitsmeldung = "Der gewählte Zeitraum ist verfügbar.";
        }
        else
        {
            Fehlermeldung = ReservierungsVerwaltung.BelegtMeldung(konflikt);
        }
    }

    [RelayCommand]
    private void Reservieren()
    {
        MeldungenLoeschen();
        if (AusgewaehltesGeraet is null || AusgewaehlterEntleiher is null)
        {
            Fehlermeldung = "Bitte Gerät und Entleiher auswählen.";
            return;
        }
        if (!ZeitraumLesen(out var beginn, out var ende))
        {
            return;
        }

        var geraetId = AusgewaehltesGeraet.Id;
        var entleiherId = AusgewaehlterEntleiher.Id;
        if (Versuchen(() => _verwaltung.Reservieren(geraetId, entleiherId, beginn, ende)))
        {
            Beenden(true);
        }
    }

    private IReadOnlyList<Entleiher> AktiveEntleiher() =>
        _entleiherVerwaltung.AlleLaden().Where(e => e.Aktiv).ToList();

    private bool ZeitraumLesen(out DateTime beginn, out DateTime ende)
    {
        ende = default;
        if (!Lesen(Beginn, out beginn))
        {
            Fehlermeldung = "Bitte den Beginn im Format TT.MM.JJJJ, HH:MM angeben.";
            return false;
        }
        if (!Lesen(Ende, out ende))
        {
            Fehlermeldung = "Bitte das Ende im Format TT.MM.JJJJ, HH:MM angeben.";
            return false;
        }
        return true;
    }

    private static bool Lesen(string text, out DateTime zeitpunkt) =>
        DateTime.TryParseExact(text.Trim(), Eingabeformate, Deutsch, DateTimeStyles.None, out zeitpunkt);

    private void MeldungenLoeschen()
    {
        Fehlermeldung = null;
        Verfuegbarkeitsmeldung = null;
    }
}
