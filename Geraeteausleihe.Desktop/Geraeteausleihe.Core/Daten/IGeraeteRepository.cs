using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Core.Daten;

public interface IGeraeteRepository
{
    IReadOnlyList<Geraet> AlleLaden();
    bool InventarnummerVergeben(string inventarnummer, int ausserGeraetId);
    void Hinzufuegen(Geraet geraet);
    void Aktualisieren(Geraet geraet);
}
