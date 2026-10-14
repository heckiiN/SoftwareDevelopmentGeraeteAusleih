using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Tests;

// T5: Statusübergänge ohne Datenbank
public class ReservierungTests
{
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
}
