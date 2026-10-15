using Geraeteausleihe.Core.Daten;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Core.Dienste;

public class ReservierungsVerwaltung(
    IReservierungsRepository reservierungen,
    IGeraeteRepository geraete,
    IEntleiherRepository entleiher,
    IZeitquelle zeit)
{
    public IReadOnlyList<Reservierung> AlleLaden() => reservierungen.AlleLaden();

    public bool IstUeberfaellig(Reservierung reservierung) => reservierung.IstUeberfaellig(zeit.Jetzt);

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

    public void Stornieren(int reservierungId)
    {
        var reservierung = FrischLaden(reservierungId);
        reservierung.Stornieren();
        reservierungen.StatusSpeichern(reservierung);
    }

    // Für die Ausgabeliste: Grund, warum die Reservierung gerade nicht ausgegeben werden kann, oder null.
    public string? AusgabeHindernis(Reservierung reservierung, IEnumerable<Reservierung> bestehende)
    {
        var nochAusgegeben = Verfuegbarkeit.FindeNochAusgegebene(bestehende, reservierung);
        if (nochAusgegeben is not null)
        {
            var name = entleiher.Laden(nochAusgegeben.EntleiherId)?.Name;
            return $"Das Gerät ist noch bei {name} (Rückgabe war bis {nochAusgegeben.Ende:dd.MM.yyyy HH:mm} geplant).";
        }

        return reservierung.AusgabeHindernis(zeit.Jetzt);
    }

    public void Ausgeben(int reservierungId)
    {
        var reservierung = FrischLaden(reservierungId);

        var hindernis = AusgabeHindernis(reservierung, reservierungen.BlockierendeLaden(reservierung.GeraetId));
        if (hindernis is not null)
        {
            throw new FachlicherFehler(hindernis);
        }

        reservierung.Ausgeben(zeit.Jetzt);
        reservierungen.StatusSpeichern(reservierung);
    }

    public void Zurueckgeben(int reservierungId)
    {
        var reservierung = FrischLaden(reservierungId);
        reservierung.Zurueckgeben(zeit.Jetzt);
        reservierungen.StatusSpeichern(reservierung);
    }

    public static string BelegtMeldung(Reservierung konflikt) =>
        $"Das Gerät ist von {konflikt.Beginn:dd.MM.yyyy HH:mm} bis {konflikt.Ende:dd.MM.yyyy HH:mm} bereits reserviert.";

    // Statuswechsel gehen immer vom gespeicherten Stand aus, nicht von einer veralteten Zeile in der Liste.
    private Reservierung FrischLaden(int reservierungId) =>
        reservierungen.Laden(reservierungId) ?? throw new FachlicherFehler("Die Reservierung wurde nicht gefunden.");

    private static void ZeitraumPruefen(DateTime beginn, DateTime ende)
    {
        if (ende <= beginn)
        {
            throw new FachlicherFehler("Das Ende muss nach dem Beginn liegen.");
        }
    }
}
