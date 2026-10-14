using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Tests;

// T2 bis T4: reine Unit-Tests ohne Datenbank. Bestehend ist immer Gerät 1 am 01.11.2026 von 09:00 bis 12:00.
public class VerfuegbarkeitTests
{
    private const int Beamer = 1;
    private const int Leinwand = 2;

    private static DateTime Um(int stunde) => new(2026, 11, 1, stunde, 0, 0);

    private static List<Reservierung> Bestehend(ReservierungsStatus status = ReservierungsStatus.Reserviert) =>
    [
        new() { Id = 7, GeraetId = Beamer, Beginn = Um(9), Ende = Um(12), Status = status }
    ];

    [Fact]
    public void Freier_Zeitraum_ist_verfuegbar()
    {
        Assert.Null(Verfuegbarkeit.FindeKonflikt(Bestehend(), Beamer, Um(14), Um(16)));
    }

    [Theory]
    [InlineData(8, 10)]   // überschneidet den Anfang
    [InlineData(11, 13)]  // überschneidet das Ende
    [InlineData(8, 13)]   // überdeckt vollständig
    [InlineData(10, 11)]  // liegt innerhalb
    [InlineData(9, 12)]   // identisch
    public void Ueberschneidung_wird_erkannt(int beginn, int ende)
    {
        var konflikt = Verfuegbarkeit.FindeKonflikt(Bestehend(), Beamer, Um(beginn), Um(ende));

        Assert.NotNull(konflikt);
        Assert.Equal(7, konflikt.Id);
    }

    [Theory]
    [InlineData(6, 9)]
    [InlineData(12, 15)]
    public void Direkt_angrenzende_Zeitraeume_sind_erlaubt(int beginn, int ende)
    {
        Assert.Null(Verfuegbarkeit.FindeKonflikt(Bestehend(), Beamer, Um(beginn), Um(ende)));
    }

    [Fact]
    public void Anderes_Geraet_ist_im_selben_Zeitraum_verfuegbar()
    {
        Assert.Null(Verfuegbarkeit.FindeKonflikt(Bestehend(), Leinwand, Um(9), Um(12)));
    }

    [Theory]
    [InlineData(ReservierungsStatus.Storniert)]
    [InlineData(ReservierungsStatus.Zurueckgegeben)]
    public void Abgeschlossene_Vorgaenge_blockieren_nicht(ReservierungsStatus status)
    {
        Assert.Null(Verfuegbarkeit.FindeKonflikt(Bestehend(status), Beamer, Um(9), Um(12)));
    }

    [Fact]
    public void Ausgegebenes_Geraet_blockiert_den_Zeitraum()
    {
        Assert.NotNull(Verfuegbarkeit.FindeKonflikt(Bestehend(ReservierungsStatus.Ausgegeben), Beamer, Um(10), Um(11)));
    }
}
