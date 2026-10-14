using CommunityToolkit.Mvvm.ComponentModel;

namespace Geraeteausleihe.Desktop.ViewModels;

// Steht für Seiten, die noch nicht umgesetzt sind.
public class PlatzhalterViewModel(string titel) : ObservableObject
{
    public string Titel { get; } = titel;
}
