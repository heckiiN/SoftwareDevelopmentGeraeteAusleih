using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Desktop.ViewModels;

// Eine Zeile der Reservierungsliste: die Reservierung plus die Namen statt der IDs.
public record ReservierungsZeile(Reservierung Reservierung, string Geraet, string Entleiher, bool Ueberfaellig)
{
    // Nur die Übersicht hebt hervor, was heute zurückkommen muss.
    public bool HeuteFaellig { get; init; }

    public string StatusText => Reservierung.Status switch
    {
        _ when Ueberfaellig => "Überfällig",
        _ when HeuteFaellig => "Heute fällig",
        ReservierungsStatus.Zurueckgegeben => "Zurückgegeben",
        var status => status.ToString()
    };
}
