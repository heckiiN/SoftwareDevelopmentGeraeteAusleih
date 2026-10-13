using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Core.Daten;

public interface IGeraeteRepository
{
    IReadOnlyList<Geraet> AlleLaden();
    void Hinzufuegen(Geraet geraet);
    void Aktualisieren(Geraet geraet);
}
