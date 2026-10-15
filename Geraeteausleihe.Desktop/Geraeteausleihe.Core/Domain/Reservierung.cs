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

    // Überfällig wird nie gespeichert, sondern immer aus Status und Uhrzeit berechnet.
    // Genau zum Ende ist eine Ausleihe noch nicht überfällig.
    public bool IstUeberfaellig(DateTime jetzt) => Status == ReservierungsStatus.Ausgegeben && jetzt > Ende;

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
    // Liefert den Grund, warum jetzt nicht ausgegeben werden kann, oder null.
    public string? AusgabeHindernis(DateTime jetzt) => Status switch
    {
        ReservierungsStatus.Ausgegeben => "Das Gerät ist bereits ausgegeben.",
        ReservierungsStatus.Storniert => "Eine stornierte Reservierung kann nicht ausgegeben werden.",
        ReservierungsStatus.Zurueckgegeben => "Diese Ausleihe ist bereits abgeschlossen.",
        _ when jetzt < Beginn => $"Die Ausgabe ist erst ab {Beginn:dd.MM.yyyy HH:mm} möglich.",
        _ when jetzt >= Ende => "Der Reservierungszeitraum ist bereits abgelaufen.",
        _ => null
    };

    public void Ausgeben(DateTime jetzt)
    {
        if (AusgabeHindernis(jetzt) is { } grund)
        {
            throw new FachlicherFehler(grund);
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
