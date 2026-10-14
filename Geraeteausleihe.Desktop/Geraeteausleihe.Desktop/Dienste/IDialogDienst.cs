using Geraeteausleihe.Desktop.ViewModels;

namespace Geraeteausleihe.Desktop.Dienste;

// ViewModels öffnen Dialoge nur über diese Schnittstelle und kennen so keine Fenster.
public interface IDialogDienst
{
    bool Zeigen(DialogViewModel dialog);
}
