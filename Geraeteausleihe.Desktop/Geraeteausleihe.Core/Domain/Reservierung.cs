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
}
