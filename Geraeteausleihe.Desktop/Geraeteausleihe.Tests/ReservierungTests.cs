using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Tests;

// T5 und T6: Statusübergänge ohne Datenbank. Gebucht ist immer der 01.11.2026 von 09:00 bis 12:00.
public class ReservierungTests
{
    private static DateTime Um(int stunde, int minute = 0) => new(2026, 11, 1, stunde, minute, 0);

    private static Reservierung Gebucht(ReservierungsStatus status = ReservierungsStatus.Reserviert) =>
        new() { Id = 1, GeraetId = 1, Beginn = Um(9), Ende = Um(12), Status = status };

    [Fact]
    public void Reservierte_Reservierung_wird_storniert()
    {
        var reservierung = new Reservierung { Status = ReservierungsStatus.Reserviert };

        reservierung.Stornieren();

        Assert.Equal(ReservierungsStatus.Storniert, reservierung.Status);
    }

    [Theory]
    [InlineData(ReservierungsStatus.Ausgegeben)]
    [InlineData(ReservierungsStatus.Zurueckgegeben)]
    [InlineData(ReservierungsStatus.Storniert)]
    public void Nur_reservierte_Vorgaenge_lassen_sich_stornieren(ReservierungsStatus status)
    {
        var reservierung = new Reservierung { Status = status };

        Assert.Throws<FachlicherFehler>(reservierung.Stornieren);
        Assert.Equal(status, reservierung.Status);
    }

    [Fact]
    public void Ausgabe_und_Rueckgabe_setzen_Status_und_Zeitpunkt()
    {
        var reservierung = Gebucht();

        reservierung.Ausgeben(Um(9, 30));
        reservierung.Zurueckgeben(Um(12, 15));

        Assert.Equal(ReservierungsStatus.Zurueckgegeben, reservierung.Status);
        Assert.Equal(Um(9, 30), reservierung.Ausgabezeitpunkt);
        Assert.Equal(Um(12, 15), reservierung.Rueckgabezeitpunkt);
    }

    [Theory]
    [InlineData(ReservierungsStatus.Ausgegeben)]
    [InlineData(ReservierungsStatus.Zurueckgegeben)]
    [InlineData(ReservierungsStatus.Storniert)]
    public void Ausgabe_nur_bei_reservierten_Vorgaengen(ReservierungsStatus status)
    {
        var reservierung = Gebucht(status);

        Assert.Throws<FachlicherFehler>(() => reservierung.Ausgeben(Um(10)));
        Assert.Equal(status, reservierung.Status);
    }

    [Theory]
    [InlineData(ReservierungsStatus.Reserviert)]
    [InlineData(ReservierungsStatus.Zurueckgegeben)]
    [InlineData(ReservierungsStatus.Storniert)]
    public void Rueckgabe_nur_bei_ausgegebenen_Vorgaengen(ReservierungsStatus status)
    {
        var reservierung = Gebucht(status);

        Assert.Throws<FachlicherFehler>(() => reservierung.Zurueckgeben(Um(10)));
        Assert.Equal(status, reservierung.Status);
    }

    [Theory]
    [InlineData(9, 0)]
    [InlineData(11, 59)]
    public void Ausgabe_ist_vom_Beginn_bis_vor_dem_Ende_moeglich(int stunde, int minute)
    {
        var reservierung = Gebucht();

        reservierung.Ausgeben(Um(stunde, minute));

        Assert.Equal(ReservierungsStatus.Ausgegeben, reservierung.Status);
    }

    [Theory]
    [InlineData(8, 59)]
    [InlineData(12, 0)]
    [InlineData(14, 0)]
    public void Ausgabe_ausserhalb_des_Zeitraums_wird_abgelehnt(int stunde, int minute)
    {
        var reservierung = Gebucht();

        Assert.Throws<FachlicherFehler>(() => reservierung.Ausgeben(Um(stunde, minute)));
        Assert.Equal(ReservierungsStatus.Reserviert, reservierung.Status);
    }

    [Fact]
    public void Noch_ausgegebenes_Geraet_blockiert_die_Folgeausgabe()
    {
        var vorher = new Reservierung
        {
            Id = 7, GeraetId = 1, Beginn = Um(6), Ende = Um(9), Status = ReservierungsStatus.Ausgegeben
        };

        Assert.Same(vorher, Verfuegbarkeit.FindeNochAusgegebene([vorher], Gebucht()));

        vorher.Zurueckgeben(Um(10));
        Assert.Null(Verfuegbarkeit.FindeNochAusgegebene([vorher], Gebucht()));
    }
}
