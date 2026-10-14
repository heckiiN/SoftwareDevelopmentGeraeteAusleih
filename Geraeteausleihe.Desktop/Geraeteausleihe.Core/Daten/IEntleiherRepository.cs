using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Core.Daten;

public interface IEntleiherRepository
{
    IReadOnlyList<Entleiher> AlleLaden();
    void Hinzufuegen(Entleiher entleiher);
    void Aktualisieren(Entleiher entleiher);
}
