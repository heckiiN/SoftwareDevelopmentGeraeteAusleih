using System.Windows;
using System.Windows.Input;
using Geraeteausleihe.Desktop.ViewModels;

namespace Geraeteausleihe.Desktop.Views;

public partial class DialogFenster : Window
{
    public DialogFenster(DialogViewModel dialog)
    {
        InitializeComponent();
        DataContext = dialog;
        dialog.Schliessen += gespeichert => DialogResult = gespeichert;
    }

    // Ohne Titelleiste lässt sich das Fenster sonst nicht verschieben.
    private void Verschieben(object sender, MouseButtonEventArgs e) => DragMove();
}
