using Geraeteausleihe.Core.Dienste;

namespace Geraeteausleihe.Tests;

public sealed class FesteZeit(DateTime jetzt) : IZeitquelle
{
    public DateTime Jetzt { get; set; } = jetzt;
}
