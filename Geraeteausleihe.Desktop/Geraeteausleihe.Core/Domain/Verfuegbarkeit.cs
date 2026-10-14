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

    // Wird ein Gerät zu spät zurückgegeben, ist es physisch noch nicht da.
    // Dann blockiert es die nächste Ausgabe, auch wenn deren Zeitraum schon begonnen hat.
    public static Reservierung? FindeNochAusgegebene(IEnumerable<Reservierung> bestehende, Reservierung naechste)
    {
        return bestehende.FirstOrDefault(r =>
            r.GeraetId == naechste.GeraetId && r.Id != naechste.Id && r.Status == ReservierungsStatus.Ausgegeben);
    }
}
