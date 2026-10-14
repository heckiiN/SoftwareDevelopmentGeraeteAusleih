using Geraeteausleihe.Core.Daten;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Core.Dienste;

public class EntleiherVerwaltung(IEntleiherRepository entleiher, IReservierungsRepository reservierungen)
{
    public IReadOnlyList<Entleiher> AlleLaden() => entleiher.AlleLaden();

    public void Speichern(Entleiher person)
    {
        person.Name = person.Name.Trim();
        person.Kontakt = string.IsNullOrWhiteSpace(person.Kontakt) ? null : person.Kontakt.Trim();

        if (person.Name.Length == 0)
        {
            throw new FachlicherFehler("Bitte einen Namen eingeben.");
        }

        if (person.Id == 0)
        {
            entleiher.Hinzufuegen(person);
        }
        else
        {
            entleiher.Aktualisieren(person);
        }
    }

    public void Deaktivieren(Entleiher person)
    {
        if (reservierungen.EntleiherHatOffene(person.Id))
        {
            throw new FachlicherFehler(
                "Der Entleiher hat noch offene Reservierungen. Bitte diese zuerst abschließen oder stornieren.");
        }

        person.Aktiv = false;
        entleiher.Aktualisieren(person);
    }

    public void Aktivieren(Entleiher person)
    {
        person.Aktiv = true;
        entleiher.Aktualisieren(person);
    }
}
