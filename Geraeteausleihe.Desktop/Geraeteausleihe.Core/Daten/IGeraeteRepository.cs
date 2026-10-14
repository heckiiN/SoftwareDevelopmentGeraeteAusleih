using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Core.Daten;

public interface IGeraeteRepository
{
    IReadOnlyList<Geraet> AlleLaden();
    Geraet? Laden(int id);
    bool InventarnummerVergeben(string inventarnummer, int ausserGeraetId);
    void Hinzufuegen(Geraet geraet);
    void Aktualisieren(Geraet geraet);
}
