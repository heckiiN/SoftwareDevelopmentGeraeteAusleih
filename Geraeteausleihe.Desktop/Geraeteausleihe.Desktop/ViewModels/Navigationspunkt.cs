using CommunityToolkit.Mvvm.ComponentModel;

namespace Geraeteausleihe.Desktop.ViewModels;

// Symbol ist ein Zeichen aus der Schriftart Segoe MDL2 Assets.
public record Navigationspunkt(string Titel, string Symbol, ObservableObject Seite);
