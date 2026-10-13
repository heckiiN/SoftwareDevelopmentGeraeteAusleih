namespace Geraeteausleihe.Core.Domain;

public class Geraet
{
    public int Id { get; set; }
    public string Inventarnummer { get; set; } = "";
    public string Bezeichnung { get; set; } = "";
    public string? Beschreibung { get; set; }
    public bool Aktiv { get; set; } = true;
}
