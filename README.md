# Geräteausleihe

Windows-Desktop-Anwendung, mit der ein Verein seine Geräte (z. B. Beamer, Leinwände, Lautsprecher) an Mitglieder verleiht. Sie verwaltet Geräte und Entleiher, nimmt Reservierungen ohne Doppelbelegung an, erfasst Ausgabe und Rückgabe und zeigt überfällige Ausleihen an.

Entstanden im IU-Kurs DLBSEPPSD01_D „Projekt: Software Development“.

## Voraussetzungen

- Windows 10 oder 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Einrichtung

```sh
git clone https://github.com/heckiiN/SoftwareDevelopmentGeraeteAusleih.git
cd SoftwareDevelopmentGeraeteAusleih/Geraeteausleihe.Desktop
dotnet build Geraeteausleihe.Desktop.slnx
```

## Anwendung starten

```sh
dotnet run --project Geraeteausleihe.Desktop/Geraeteausleihe.Desktop.csproj
```

Beim ersten Start wird eine leere Datenbank angelegt unter `%LOCALAPPDATA%\Geraeteausleihe\geraeteausleihe.db`. Zum Zurücksetzen die Anwendung schließen und diese Datei löschen.

## Tests ausführen

Im Ordner `Geraeteausleihe.Desktop`:

```sh
dotnet test --solution Geraeteausleihe.Desktop.slnx
```

Die Tests laufen auf Microsoft.Testing.Platform (eingestellt in `global.json`). Integrationstests verwenden jeweils eine eigene temporäre SQLite-Datei, die Datenbank der Anwendung bleibt unberührt.

## Aufbau

| Projekt | Inhalt |
|---|---|
| `Geraeteausleihe.Core` | Fachlogik und Datenzugriff, ohne Oberfläche |
| `Geraeteausleihe.Desktop` | WPF-Oberfläche nach MVVM |
| `Geraeteausleihe.Tests` | Unit- und Integrationstests |

## Verwendete Bibliotheken

| Bibliothek | Version | Zweck |
|---|---|---|
| .NET / WPF | 10.0 | Laufzeit und Oberfläche |
| Microsoft.Data.Sqlite | 10.0.12 | Zugriff auf die SQLite-Datenbank |
| CommunityToolkit.Mvvm | 8.4.2 | Hilfen für ViewModels und Befehle |
| xunit.v3 | 4.0.2 | Testframework |
