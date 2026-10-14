using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Core.Daten;

public interface IReservierungsRepository
{
    IReadOnlyList<Reservierung> AlleLaden();
    Reservierung? Laden(int id);
    IReadOnlyList<Reservierung> BlockierendeLaden(int geraetId);
    bool GeraetHatOffene(int geraetId);
    bool EntleiherHatOffene(int entleiherId);

    // Prüft und speichert in einer Transaktion. Liefert die blockierende Reservierung,
    // oder null, wenn gespeichert wurde.
    Reservierung? AnlegenWennFrei(Reservierung neu);

    void StatusSpeichern(Reservierung reservierung);
}
