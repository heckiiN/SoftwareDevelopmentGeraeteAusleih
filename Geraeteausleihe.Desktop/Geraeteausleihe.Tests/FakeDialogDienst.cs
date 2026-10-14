using Geraeteausleihe.Desktop.Dienste;
using Geraeteausleihe.Desktop.ViewModels;

namespace Geraeteausleihe.Tests;

// Ersetzt die echten Fenster: Bedienen spielt nach, was der Benutzer im Dialog tut.
public sealed class FakeDialogDienst : IDialogDienst
{
    public Action<DialogViewModel> Bedienen { get; set; } = dialog => dialog.AbbrechenCommand.Execute(null);

    public bool Zeigen(DialogViewModel dialog)
    {
        var gespeichert = false;
        dialog.Schliessen += ergebnis => gespeichert = ergebnis;
        Bedienen(dialog);
        return gespeichert;
    }

    public void Hinweis(string meldung)
    {
    }

    public bool Bestaetigen(string frage) => true;
}
