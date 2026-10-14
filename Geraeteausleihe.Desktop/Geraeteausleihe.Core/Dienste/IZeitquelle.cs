namespace Geraeteausleihe.Core.Dienste;

// Austauschbar, damit Tests mit einer festen Uhrzeit arbeiten können.
public interface IZeitquelle
{
    DateTime Jetzt { get; }
}

public class Systemzeit : IZeitquelle
{
    public DateTime Jetzt => DateTime.Now;
}
