# ⚔️ DungeonOfTheFallen - WPF Dungeon Crawler

## 📋 Übersicht

**DungeonOfTheFallen** ist ein rundenbasiertes 2D-Dungeon-Crawler-Spiel im Fantasy-Stil, entwickelt als Projektarbeit mit **C# / .NET 8 / WPF**.

Der aktuelle Stand ist ein spielbarer MVP mit Hauptmenü, Dungeon-Grid, Bewegung, Gegnern, Kampffenster, Loot, Tränken, Fallen, Heilungsraum, Save/Load und Sieg/Niederlage.

**Visuelle Richtung**: Düsteres Fantasy-Thema, Top-Down 2D, inspiriert durch *Crown Trick* und *Shattered Pixel Dungeon*.

---

## 🛠️ Technologie-Stack

| Komponente | Details |
|---|---|
| **Sprache** | C# (.NET 8) |
| **UI-Framework** | WPF (Windows Presentation Foundation) |
| **Architektur** | MVVM (Model-View-ViewModel), ohne externe Frameworks |
| **Persistenz** | XML-Serialisierung über `SaveData` + `XmlGameRepository` |
| **Plattform** | Windows Desktop (.NET 8 Windows) |

---

## 📁 Projektstruktur

```
Projektarbeit_Dungeon of the Fallen/
├── DungeonOfTheFallen.Core/               # Domänenlogik & Game-Services
│   ├── Models/                            # Domänenmodelle
│   │   ├── GameState.cs                   # Zentrale Spielzustand-Verwaltung
│   │   ├── Player.cs                      # Spielercharakter (HP, XP, Inventory, Stats)
│   │   ├── Enemy.cs                       # Gegner (Typ, Stats, Rewards)
│   │   ├── Tile.cs & DungeonMap.cs        # Dungeon-Grid-Logik
│   │   ├── Item.cs, Potion.cs             # Loot & Inventar
│   │   ├── TileType.cs                    # Floor, Wall, Exit, Spawn, Trap, HealingRoom
│   │   ├── EnemyType.cs                   # Goblin, Orc, Boss
│   │   └── ItemType.cs                    # Gold, Potion, Key
│   │
│   ├── Services/                          # Game Services
│   │   ├── TurnManager.cs                 # Bewegung, Tile-Effekte, Enemy Turns
│   │   └── CombatService.cs               # Wiederverwendbare Kampf-Logik
│   │
│   └── Persistence/                       # Repository-Pattern
│       ├── IGameRepository.cs             # Save/Load-Interface
│       ├── SaveData.cs                    # XML-Save-Daten
│       └── XmlGameRepository.cs           # XML-Implementation
│
├── Projektarbeit_Dungeon of the Fallen/   # WPF-Präsentation
│   ├── ViewModels/
│   │   ├── ViewModelBase.cs               # Basis mit INotifyPropertyChanged
│   │   ├── RelayCommand.cs                # Command-Implementation
│   │   ├── MainViewModel.cs               # Haupt-Game-Flow & Map
│   │   ├── CombatViewModel.cs             # Kampfablauf mit Würfeln
│   │   ├── PlayerViewModel.cs             # Player Status Display
│   │   └── TileViewModel.cs               # Einzeltile-Rendering
│   │
│   ├── MainMenuWindow.xaml & .cs          # Startmenü
│   ├── MainWindow.xaml & .cs              # Hauptspiel
│   ├── CombatWindow.xaml & .cs            # Kampfansicht
│   ├── App.xaml & .cs
│   │
│   └── Converters/
│       ├── BoolToVisibilityConverter.cs
│       └── VictoryTextConverter.cs
│
├── Projektarbeit_Dungeon of the Fallen.sln
└── README.md
```

---

## 🎮 Aktueller Entwicklungsstand

Der aktuelle Branch enthält den kompletten spielbaren MVP.

### **Donnerstag (Do)** ✅ ABGESCHLOSSEN
- ✅ Solution mit zwei Projekten aufgebaut
- ✅ Core-Klassenbibliothek mit allen Domänenmodellen
- ✅ Enums: `TileType`, `EnemyType`, `ItemType`
- ✅ Modelle: `Player`, `Enemy`, `Tile`, `DungeonMap`, `GameState`, `Item`, `Potion`, `Inventory`
- ✅ Build erfolgreich

### **Freitag (Fr)** ✅ ABGESCHLOSSEN
- ✅ MVVM-Grundgerüst (`ViewModelBase`, `RelayCommand`)
- ✅ `MainViewModel`, `TileViewModel` und `PlayerViewModel`
- ✅ Hauptfenster mit `ItemsControl` + `UniformGrid` für das 20x20 Grid
- ✅ Dunkles Fantasy-Farbschema
- ✅ Spielerfigur, Gegner, Items und Wände sichtbar im Grid
- ✅ Statuspanel und Kampflog angebunden

### **Montag (Mo)** ✅ ABGESCHLOSSEN
- ✅ `TurnManager` für Bewegung und Spielzug-Logik
- ✅ Wand-Kollisionslogik
- ✅ Gegner-Spawns
- ✅ Tile-Effekte für Fallen und Heilungsraum
- ✅ Gegner-Züge mit Jagd- und Zufallsverhalten
- ✅ Item-Pickups und Loot

### **Dienstag (Di)** ✅ ABGESCHLOSSEN
- ✅ `CombatWindow` mit Würfelkampf
- ✅ `CombatViewModel` mit Kampfphasen
- ✅ Loot-System, XP und Level-Up
- ✅ Boss-Gegner
- ✅ Sieg- und Niederlage-Bedingungen
- ✅ Kampflog aktualisiert sich live

### **Mittwoch (Mi)** ✅ ABGESCHLOSSEN
- ✅ XML Save/Load
- ✅ `MainMenuWindow`
- ✅ UI-Polish und Bugfixes
- ✅ README an den aktuellen Stand angepasst

---

## 🚀 MVP-Features

| Feature | Status |
|---|---|
| 20x20 Dungeon-Grid | ✅ |
| Spielerfigur sichtbar | ✅ |
| Spieler-Bewegung | ✅ |
| Gegner (Goblins, Orc, Boss) | ✅ |
| Gegner-KI | ✅ |
| Rundenbasierte Züge | ✅ |
| Direkter Kampf | ✅ |
| HP/XP/Level-System | ✅ |
| Loot (Gold, Tränke) | ✅ |
| Boss-Gegner | ✅ |
| Dungeon-Ausgang | ✅ |
| Kampflog | ✅ |
| Sieg/Niederlage | ✅ |
| Save/Load | ✅ |
| Main Menu | ✅ |

---

## 💻 Installation & Build

### Voraussetzungen
- **.NET 8 SDK** installiert
- **Visual Studio 2022** oder **VS Code** mit C#-Support
- **Windows 10/11** (WPF-spezifisch)

### Build & Run

```bash
# Repository klonen
git clone https://github.com/Robomaaan/Dungeon-of-the-Fallen.git
cd "Dungeon-of-the-Fallen"

# Build
dotnet build "Projektarbeit_Dungeon of the Fallen.sln"

# Run
dotnet run --project "Projektarbeit_Dungeon of the Fallen/Projektarbeit_Dungeon of the Fallen.csproj"
```

Oder einfach die Solution in Visual Studio öffnen und F5 drücken.

---

## 🎨 Farbschema (Dungeon-Rendering)

```
Hintergrund:    #0a0a0a (Schwarz)
Panel:          #1a1a1a (Dunkelgrau)
Wand:           #333333 (Grau)
Floor:          #1a1a1a (Dunkelgrau)
Exit:           #FFD700 (Goldenes Ziel-Feld)
Spawn:          #00AA00 (Grün)
Trap:           #AA0000 (Rot)
Healing Room:   #0088FF (Blau)
Text:           #e0e0e0 (Hellgrau)
Titel:          #FFD700 (Gold)
```

---

## 🏗️ MVVM-Architektur

```
App.xaml.cs
    ↓
MainMenuWindow.xaml
    ↓ (Neues Spiel / Laden)
MainWindow.xaml
    ↓ (DataContext)
MainViewModel
    ├── GameState (Player, Map, Enemies, CombatLog)
    ├── PlayerViewModel (Status Display)
    └── ObservableCollection<TileViewModel>
         └── [Jede TileViewModel bindet auf Tile-Modell]
             ├── DisplayText (P/E/I/█)
             └── BackgroundColor (je nach TileType)

CombatWindow.xaml
    ↓ (DataContext)
CombatViewModel
    ├── Würfelanimation
    ├── Angriff / Potion-Zug
    └── Rückgabe an `MainWindow`
```

**Datenfluss:**
1. Spieler-Input oder Fenster-Event → Command / Event
2. ViewModel verarbeitet die Logik
3. `GameState` wird aktualisiert
4. UI-Bindings aktualisieren automatisch
5. `CombatWindow` meldet das Ergebnis nach der Animation zurück

---

## 🧱 Phase-2 Foundation (aktueller Architekturstand)

Der aktuelle Stand wurde intern auf eine erweiterbare Foundation vorbereitet:

- zentralere Combat-Abwicklung über `CombatService`
- `CombatTurnResult` als Rückgabeobjekt für UI/Animation
- gemeinsame Stats-Basis über `CombatantStats`
- `EnemyFactory` für zentrale Gegnererzeugung
- Save-System auf **Version 2** erweitert
- `SaveDataMapper` kapselt Save/Load-Mapping aus dem ViewModel

Geplante nächste Ausbaustufen:
- `PlayerProgressionService`
- `LootService`
- `EnemySpawnService`
- Save-Migrationen

Siehe auch: `docs/phase-2-foundation-plan.md`

## 📝 Bekannte Einschränkungen (MVP)

- **Prozeduraler Generator**: Noch nicht implementiert → Hardcoded Test-Map
- **Sound/Musik**: Nicht vorhanden
- **Mehrere Ebenen**: Nicht im MVP
- **Automatisierte Tests**: Noch keine Unit-Tests vorhanden

---

## 🧪 Tests

Derzeit keine Unit-Tests implementiert. Der Build ist aktuell erfolgreich.

---

## 📄 Lizenz

Dieses Projekt ist Lernprojekt einer Projektarbeitswoche. Frei verwendbar für Lehrzwecke.

---

## 👨‍💻 Entwickler

Robo (Projektarbeit, April 2026)

---

**Letztes Update**: Donnerstag, 23. April 2026  
**Aktueller Branch**: `main`  
**Build Status**: ✅ Passing

## 💾 Domänenmodell (Kurzüberblick)

### Kernklassen
```csharp
public class GameState
{
    public Player Player { get; }
    public DungeonMap Map { get; }
    public List<Enemy> Enemies { get; } = new();
    public List<string> CombatLog { get; } = new();
}

public class TurnManager
{
    // Bewegt den Spieler, löst Tile-Effekte aus und führt Enemy Turns aus
}
```

---

## 🛠️ Build & Run

### Bauen
```bash
dotnet build "Projektarbeit_Dungeon of the Fallen.sln"
```

### Ausführen
```bash
dotnet run --project "Projektarbeit_Dungeon of the Fallen/Projektarbeit_Dungeon of the Fallen.csproj"
```

---

## 📝 Projektwochen-Fortschritt

| Tag | Ziel | Status |
|-----|------|--------|
| **Do** | Basis-Setup und Core-Modelle | ✅ |
| **Fr** | MVVM + Grid-Rendering | ✅ |
| **Mo** | Bewegung + Gegner + Tile-Effekte | ✅ |
| **Di** | Kampfsystem + MVP | ✅ |
| **Mi** | Save/Load + Polish | ✅ |

---

## 🎯 Known Constraints & Design-Entscheidungen

1. **Hardcoded Map statt Procedural Gen**: Simplifiziert die Implementierung und hält den Fokus auf den Spielmechaniken.
2. **Rundenbasierte Züge statt Echtzeit**: Vereinfacht Kampf- und Gegnerlogik.
3. **Brushes statt Sprites**: Keine großen Asset-Anforderungen für den Prototyp.
4. **XML statt Datenbank**: Leicht speicherbar, lesbar und ohne externe Dependencies.
5. **Kein großes MVVM-Framework**: Eigene `ViewModelBase` und `RelayCommand` für eine schlanke Struktur.

---

## 📖 Weitere Informationen

- **Projektname**: `DungeonOfTheFallen`
- **Team**: Einzelentwicklung
- **Unterstützung**: Pair Programming mit KI-Assistent
- **Zeitrahmen**: Projektwoche im April 2026
- **Zielplattform**: Windows Desktop (.NET 8)

---

*Zuletzt aktualisiert: Donnerstag, 23. April 2026*
