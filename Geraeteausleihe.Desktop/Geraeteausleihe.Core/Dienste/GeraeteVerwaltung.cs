using Geraeteausleihe.Core.Daten;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Core.Dienste;

public class GeraeteVerwaltung(IGeraeteRepository geraete, IReservierungsRepository reservierungen)
{
    public IReadOnlyList<Geraet> AlleLaden() => geraete.AlleLaden();

    public void Speichern(Geraet geraet)
    {
        geraet.Inventarnummer = geraet.Inventarnummer.Trim();
        geraet.Bezeichnung = geraet.Bezeichnung.Trim();
        geraet.Beschreibung = string.IsNullOrWhiteSpace(geraet.Beschreibung) ? null : geraet.Beschreibung.Trim();

        if (geraet.Inventarnummer.Length == 0)
        {
            throw new FachlicherFehler("Bitte eine Inventarnummer eingeben.");
        }
        if (geraet.Bezeichnung.Length == 0)
        {
            throw new FachlicherFehler("Bitte eine Bezeichnung eingeben.");
        }
        if (geraete.InventarnummerVergeben(geraet.Inventarnummer, geraet.Id))
        {
            throw new FachlicherFehler($"Die Inventarnummer {geraet.Inventarnummer} ist bereits vergeben.");
        }

        if (geraet.Id == 0)
        {
            geraete.Hinzufuegen(geraet);
        }
        else
        {
            geraete.Aktualisieren(geraet);
        }
    }

    // Geräte werden nie gelöscht, damit die Ausleihhistorie erhalten bleibt.
    public void Deaktivieren(Geraet geraet)
    {
        if (reservierungen.GeraetHatOffene(geraet.Id))
        {
            throw new FachlicherFehler(
                "Das Gerät hat noch offene Reservierungen. Bitte diese zuerst abschließen oder stornieren.");
        }

        geraet.Aktiv = false;
        geraete.Aktualisieren(geraet);
    }

    public void Aktivieren(Geraet geraet)
    {
        geraet.Aktiv = true;
        geraete.Aktualisieren(geraet);
    }
}
