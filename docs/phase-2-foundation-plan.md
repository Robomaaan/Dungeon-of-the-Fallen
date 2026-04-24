# Phase 2 Foundation Plan

## Ziel

Die Combat-, Entity- und Save-Architektur so vorbereiten, dass spätere Features kontrolliert ausgebaut werden können, ohne Logik weiter zwischen ViewModels und Core zu verteilen.

## Umgesetzte Foundation

### 1. Combat-Service-Zentrierung
- `CombatService` verarbeitet jetzt komplette Combat-Turns
- `CombatTurnResult` transportiert Ergebnisdaten zurück an die UI
- Würfel-/Schadenslogik liegt im Core statt verstreut im ViewModel

### 2. Erweiterbare Entity-Basis
- `CombatantStats` als vorbereitete gemeinsame Stats-Struktur für `Player` und `Enemy`
- Bestehende Property-Zugriffe bleiben kompatibel über Wrapper

### 3. Save-Versionierung
- `SaveConstants.CurrentSaveVersion = 2`
- `SaveData` speichert jetzt zusätzlich Positionen und Gegnerzustand
- `SaveDataMapper` kapselt Save/Load-Mapping aus dem ViewModel heraus

### 4. Spawn-/Factory-Richtung
- `EnemyFactory` erzeugt Standard-Gegner zentral

## Empfohlene Commit-Reihenfolge

1. `phase2/foundation-combat-core`
   - Combat-Result-Modelle
   - CombatService-Refactor

2. `phase2/foundation-entity-stats`
   - CombatantStats
   - Player/Enemy-Umstellung mit Wrapper-Kompatibilität

3. `phase2/foundation-save-v2`
   - Save-Versionierung
   - SaveDataMapper
   - Load/Save-Anpassungen

4. `phase2/foundation-docs`
   - README / technische Dokumentation / Architekturplan

## Nächste saubere Ausbaustufen

- `PlayerProgressionService`
- `LootService`
- `EnemySpawnService`
- echte Combat-Action-Typen für spätere Skills / Specials / Defend / Crits
- Save-Migrationen für Version 1 → 2

## Offene Punkte nach lokaler Abnahme

- `RestartGame()` wurde auf vollständige Neuinitialisierung von `GameState`, `TurnManager` und `PlayerViewModel` umgestellt, damit Phase-2-Stats/Save-Zustand nicht aus einem halb zurückgesetzten Objekt weiterlaufen.
- `LoadGame()` setzt jetzt `IsGameOver`/`IsVictory` vor dem Apply zurück und protokolliert die geladene Save-Version explizit.
- Ein echter Build-/Compile-Check steht weiterhin aus.

## Harte Grenze aktuell

Die Runtime hier hat kein `dotnet`, daher konnte kein echter Build-/Compile-Check ausgeführt werden.
