namespace Geraeteausleihe.Core.Domain;

public class Entleiher
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Kontakt { get; set; }
    public bool Aktiv { get; set; } = true;
}
