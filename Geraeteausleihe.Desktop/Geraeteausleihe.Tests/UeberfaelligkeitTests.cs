using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Tests;

// T7: Überfälligkeit wird aus Status und Uhrzeit berechnet. Gebucht ist der 01.11.2026 von 09:00 bis 12:00.
public class UeberfaelligkeitTests
{
    private static DateTime Um(int stunde, int minute = 0) => new(2026, 11, 1, stunde, minute, 0);

    private static Reservierung Gebucht() => new() { Beginn = Um(9), Ende = Um(12) };

    private static Reservierung Ausgegeben()
    {
        var reservierung = Gebucht();
        reservierung.Ausgeben(Um(9));
        return reservierung;
    }

    [Theory]
    [InlineData(11, 59, false)]
    [InlineData(12, 0, false)]
    [InlineData(12, 1, true)]
    public void Ausgegebene_Ausleihe_ist_erst_nach_dem_Ende_ueberfaellig(int stunde, int minute, bool erwartet)
    {
        Assert.Equal(erwartet, Ausgegeben().IstUeberfaellig(Um(stunde, minute)));
    }

    [Fact]
    public void Nach_der_Rueckgabe_ist_die_Ausleihe_nicht_mehr_ueberfaellig()
    {
        var reservierung = Ausgegeben();

        reservierung.Zurueckgeben(Um(14));

        Assert.False(reservierung.IstUeberfaellig(Um(14)));
    }

    [Fact]
    public void Nie_ausgegebene_Reservierung_ist_nicht_ueberfaellig()
    {
        Assert.False(Gebucht().IstUeberfaellig(Um(14)));
    }
}
