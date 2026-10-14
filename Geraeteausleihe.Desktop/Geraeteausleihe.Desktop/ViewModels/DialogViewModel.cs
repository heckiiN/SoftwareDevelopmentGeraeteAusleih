using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Geraeteausleihe.Desktop.ViewModels;

public abstract partial class DialogViewModel : ObservableObject
{
    // true, wenn der Dialog mit Speichern beendet wurde
    public event Action<bool>? Schliessen;

    public abstract string Titel { get; }

    protected void Beenden(bool gespeichert) => Schliessen?.Invoke(gespeichert);

    [RelayCommand]
    private void Abbrechen() => Beenden(false);
}
