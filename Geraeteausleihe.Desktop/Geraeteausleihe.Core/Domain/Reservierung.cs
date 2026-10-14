namespace Geraeteausleihe.Core.Domain;

public class Reservierung
{
    public int Id { get; set; }
    public int GeraetId { get; set; }
    public int EntleiherId { get; set; }
    public DateTime Beginn { get; set; }
    public DateTime Ende { get; set; }
    public ReservierungsStatus Status { get; set; } = ReservierungsStatus.Reserviert;
    public DateTime? Ausgabezeitpunkt { get; set; }
    public DateTime? Rueckgabezeitpunkt { get; set; }

    // Stornierte und zurückgegebene Vorgänge geben ihren Zeitraum wieder frei.
    public bool BlockiertZeitraum => Status is ReservierungsStatus.Reserviert or ReservierungsStatus.Ausgegeben;

    // Das Ende zählt nicht mehr zum Zeitraum: Endet eine Reservierung um 12:00,
    // darf die nächste um 12:00 beginnen.
    public bool UeberschneidetSich(DateTime beginn, DateTime ende) => beginn < Ende && ende > Beginn;

    // Ein ausgegebenes Gerät ist schon beim Entleiher und muss zurückgegeben werden.
    public void Stornieren()
    {
        switch (Status)
        {
            case ReservierungsStatus.Reserviert:
                Status = ReservierungsStatus.Storniert;
                break;
            case ReservierungsStatus.Ausgegeben:
                throw new FachlicherFehler("Ein ausgegebenes Gerät kann nicht storniert werden. Es muss zurückgegeben werden.");
            case ReservierungsStatus.Storniert:
                throw new FachlicherFehler("Die Reservierung ist bereits storniert.");
            default:
                throw new FachlicherFehler("Eine abgeschlossene Ausleihe kann nicht storniert werden.");
        }
    }

    // Ausgabe nur innerhalb des gebuchten Zeitraums: frühestens zum Beginn, spätestens kurz vor dem Ende.
    public void Ausgeben(DateTime jetzt)
    {
        switch (Status)
        {
            case ReservierungsStatus.Ausgegeben:
                throw new FachlicherFehler("Das Gerät ist bereits ausgegeben.");
            case ReservierungsStatus.Storniert:
                throw new FachlicherFehler("Eine stornierte Reservierung kann nicht ausgegeben werden.");
            case ReservierungsStatus.Zurueckgegeben:
                throw new FachlicherFehler("Diese Ausleihe ist bereits abgeschlossen.");
        }
        if (jetzt < Beginn)
        {
            throw new FachlicherFehler($"Die Ausgabe ist erst ab {Beginn:dd.MM.yyyy HH:mm} möglich.");
        }
        if (jetzt >= Ende)
        {
            throw new FachlicherFehler("Der Reservierungszeitraum ist bereits abgelaufen.");
        }

        Status = ReservierungsStatus.Ausgegeben;
        Ausgabezeitpunkt = jetzt;
    }

    // Die Rückgabe ist jederzeit möglich, auch verspätet.
    public void Zurueckgeben(DateTime jetzt)
    {
        switch (Status)
        {
            case ReservierungsStatus.Reserviert:
                throw new FachlicherFehler("Das Gerät wurde noch nicht ausgegeben.");
            case ReservierungsStatus.Zurueckgegeben:
                throw new FachlicherFehler("Das Gerät wurde bereits zurückgegeben.");
            case ReservierungsStatus.Storniert:
                throw new FachlicherFehler("Eine stornierte Reservierung kann nicht zurückgegeben werden.");
        }

        Status = ReservierungsStatus.Zurueckgegeben;
        Rueckgabezeitpunkt = jetzt;
    }
}
