using System.Windows;
using Geraeteausleihe.Desktop.ViewModels;
using Geraeteausleihe.Desktop.Views;

namespace Geraeteausleihe.Desktop.Dienste;

public class DialogDienst : IDialogDienst
{
    public bool Zeigen(DialogViewModel dialog)
    {
        var besitzer = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            ?? Application.Current.MainWindow;
        var fenster = new DialogFenster(dialog) { Owner = besitzer };
        return fenster.ShowDialog() == true;
    }

    public void Hinweis(string meldung)
    {
        MessageBox.Show(Application.Current.MainWindow, meldung, "Geräteausleihe",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public bool Bestaetigen(string frage)
    {
        return MessageBox.Show(Application.Current.MainWindow, frage, "Geräteausleihe",
            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
    }
}
