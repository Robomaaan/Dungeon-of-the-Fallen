# Local Build Checklist

## Ziel

Die erste lokale Build-Runde so effizient wie möglich durchführen und typische Foundation-Refactor-Probleme schnell eingrenzen.

## 1. Build starten

```bash
dotnet build "Projektarbeit_Dungeon of the Fallen.sln"
```

## 2. Typische erste Fehlerquellen prüfen

### Namespaces / Imports
- fehlt `using DungeonOfTheFallen.Core.Models;`
- fehlt `using DungeonOfTheFallen.Core.Services;`
- Konflikte durch neue Typen wie:
  - `CombatTurnResult`
  - `CombatActionType`
  - `CombatantStats`
  - `SaveConstants`

### WPF / Binding-Seite
- `CombatViewModel` Property-Namen stimmen mit XAML-Bindings überein
- `MainViewModel` Commands unverändert erreichbar
- `PlayerViewModel` greift weiter korrekt auf `Player`-Properties zu

### Save-/Load-Seite
- `SaveData` serialisiert `EnemySaveData` sauber
- bestehende alte Savefiles können evtl. inkompatibel sein, da jetzt V2 vorliegt

## 3. Danach gezielt starten

```bash
dotnet run --project "Projektarbeit_Dungeon of the Fallen/Projektarbeit_Dungeon of the Fallen.csproj"
```

## 4. Manuelle Smoke Tests

### Kampf
- Gegner betreten
- Angriff ausführen
- Potion im Kampf verwenden
- Sieg gegen normalen Gegner
- Niederlagefall prüfen

### Save / Load
- speichern
- laden
- Spielerposition korrekt
- Gegnerpositionen korrekt
- besiegte Gegner bleiben entfernt

### Player-Progress
- XP nach Kampf steigt
- Level-Up erhöht Stats
- Boss-Loot gibt Trank

## 5. Empfohlene Fix-Reihenfolge bei Fehlern

1. Compile-Fehler im Core
2. Compile-Fehler in ViewModels
3. XAML-/Binding-Probleme
4. Save-/Load-Verhalten
5. Combat-Flow-Feinheiten

## Harte bekannte Grenze

Diese Checklist basiert auf statischer Vorarbeit. Ein echter Build konnte in der aktuellen Runtime nicht ausgeführt werden, weil `dotnet` hier fehlt.
