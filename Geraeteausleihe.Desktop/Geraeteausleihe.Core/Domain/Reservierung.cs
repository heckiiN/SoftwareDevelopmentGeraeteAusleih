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
}
