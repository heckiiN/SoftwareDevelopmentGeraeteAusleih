using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Core.Daten;

public interface IReservierungsRepository
{
    IReadOnlyList<Reservierung> AlleLaden();
    IReadOnlyList<Reservierung> BlockierendeLaden(int geraetId);

    // Prüft und speichert in einer Transaktion. Liefert die blockierende Reservierung,
    // oder null, wenn gespeichert wurde.
    Reservierung? AnlegenWennFrei(Reservierung neu);
}
