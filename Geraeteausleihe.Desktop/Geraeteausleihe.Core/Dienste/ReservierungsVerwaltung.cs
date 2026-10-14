using Geraeteausleihe.Core.Daten;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Core.Dienste;

public class ReservierungsVerwaltung(
    IReservierungsRepository reservierungen,
    IGeraeteRepository geraete,
    IEntleiherRepository entleiher)
{
    public IReadOnlyList<Reservierung> AlleLaden() => reservierungen.AlleLaden();

    // Für „Verfügbarkeit prüfen“: liefert die blockierende Reservierung oder null.
    public Reservierung? VerfuegbarkeitPruefen(int geraetId, DateTime beginn, DateTime ende)
    {
        ZeitraumPruefen(beginn, ende);
        return Verfuegbarkeit.FindeKonflikt(reservierungen.BlockierendeLaden(geraetId), geraetId, beginn, ende);
    }

    public Reservierung Reservieren(int geraetId, int entleiherId, DateTime beginn, DateTime ende)
    {
        ZeitraumPruefen(beginn, ende);

        if (geraete.Laden(geraetId) is not { Aktiv: true })
        {
            throw new FachlicherFehler("Das Gerät ist deaktiviert und kann nicht reserviert werden.");
        }
        if (entleiher.Laden(entleiherId) is not { Aktiv: true })
        {
            throw new FachlicherFehler("Der Entleiher ist deaktiviert und kann nicht reservieren.");
        }

        // Auch wenn vorher „Verfügbarkeit prüfen“ geklickt wurde, wird hier erneut geprüft,
        // denn inzwischen kann jemand anderes reserviert haben.
        var neu = new Reservierung { GeraetId = geraetId, EntleiherId = entleiherId, Beginn = beginn, Ende = ende };
        var konflikt = reservierungen.AnlegenWennFrei(neu);
        if (konflikt is not null)
        {
            throw new FachlicherFehler(BelegtMeldung(konflikt));
        }

        return neu;
    }

    public static string BelegtMeldung(Reservierung konflikt) =>
        $"Das Gerät ist von {konflikt.Beginn:dd.MM.yyyy HH:mm} bis {konflikt.Ende:dd.MM.yyyy HH:mm} bereits reserviert.";

    private static void ZeitraumPruefen(DateTime beginn, DateTime ende)
    {
        if (ende <= beginn)
        {
            throw new FachlicherFehler("Das Ende muss nach dem Beginn liegen.");
        }
    }
}
