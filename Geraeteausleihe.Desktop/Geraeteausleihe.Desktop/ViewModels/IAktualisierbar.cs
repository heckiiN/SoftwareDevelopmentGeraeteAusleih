namespace Geraeteausleihe.Desktop.ViewModels;

// Seiten mit Daten laden neu, sobald man sie öffnet. So zeigt z. B. die
// Reservierungsliste nach dem Umbenennen eines Geräts gleich den neuen Namen.
public interface IAktualisierbar
{
    void Aktualisieren();
}
