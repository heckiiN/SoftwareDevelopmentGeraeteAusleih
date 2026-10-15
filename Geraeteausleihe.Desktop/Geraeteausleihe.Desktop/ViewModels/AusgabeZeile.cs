using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Desktop.ViewModels;

// Hindernis ist gesetzt, wenn eine Reservierung gerade nicht ausgegeben werden kann.
public record AusgabeZeile(
    Reservierung Reservierung, string Geraet, string Entleiher, bool Ueberfaellig, string? Hindernis)
    : ReservierungsZeile(Reservierung, Geraet, Entleiher, Ueberfaellig)
{
    public string Zeitraum => $"{Reservierung.Beginn:dd.MM.yyyy HH:mm} – {Reservierung.Ende:dd.MM.yyyy HH:mm}";
    public bool IstAusgegeben => Reservierung.Status == ReservierungsStatus.Ausgegeben;
    public bool IstReserviert => Reservierung.Status == ReservierungsStatus.Reserviert;
    public bool KannAusgegebenWerden => IstReserviert && Hindernis is null;
}
