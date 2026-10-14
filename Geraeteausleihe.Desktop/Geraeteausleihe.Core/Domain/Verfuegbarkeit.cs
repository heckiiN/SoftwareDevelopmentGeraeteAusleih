namespace Geraeteausleihe.Core.Domain;

public static class Verfuegbarkeit
{
    // Liefert die Reservierung, die den gewünschten Zeitraum blockiert, oder null, wenn er frei ist.
    public static Reservierung? FindeKonflikt(
        IEnumerable<Reservierung> bestehende, int geraetId, DateTime beginn, DateTime ende)
    {
        return bestehende.FirstOrDefault(r =>
            r.GeraetId == geraetId && r.BlockiertZeitraum && r.UeberschneidetSich(beginn, ende));
    }
}
