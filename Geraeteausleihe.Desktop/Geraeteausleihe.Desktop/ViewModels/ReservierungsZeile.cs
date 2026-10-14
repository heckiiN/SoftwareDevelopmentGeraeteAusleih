using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Desktop.ViewModels;

// Eine Zeile der Reservierungsliste: die Reservierung plus die Namen statt der IDs.
public record ReservierungsZeile(Reservierung Reservierung, string Geraet, string Entleiher)
{
    public string StatusText => Reservierung.Status switch
    {
        ReservierungsStatus.Zurueckgegeben => "Zurückgegeben",
        var status => status.ToString()
    };
}
