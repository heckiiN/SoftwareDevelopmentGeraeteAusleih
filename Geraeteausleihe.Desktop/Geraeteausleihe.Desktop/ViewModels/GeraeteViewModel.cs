using CommunityToolkit.Mvvm.ComponentModel;
using Geraeteausleihe.Core.Dienste;
using Geraeteausleihe.Core.Domain;

namespace Geraeteausleihe.Desktop.ViewModels;

public partial class GeraeteViewModel : ObservableObject
{
    private readonly IReadOnlyList<Geraet> _alleGeraete;

    public GeraeteViewModel(GeraeteVerwaltung verwaltung)
    {
        _alleGeraete = verwaltung.AlleLaden();
        Suchtext = "";
        NurAktive = true;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Geraete))]
    public partial string Suchtext { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Geraete))]
    public partial bool NurAktive { get; set; }

    public IReadOnlyList<Geraet> Geraete => _alleGeraete
        .Where(g => !NurAktive || g.Aktiv)
        .Where(g => Passt(g.Inventarnummer) || Passt(g.Bezeichnung) || Passt(g.Beschreibung))
        .ToList();

    private bool Passt(string? feld) =>
        feld is not null && feld.Contains(Suchtext.Trim(), StringComparison.CurrentCultureIgnoreCase);
}
