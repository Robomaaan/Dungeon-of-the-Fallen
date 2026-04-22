# ⚔️ DungeonOfTheFallen - WPF Dungeon Crawler

## 📋 Übersicht

**DungeonOfTheFallen** ist ein rundenbasiertes 2D-Dungeon-Crawler-Spiel im Fantasy-Stil, entwickelt als Projektarbeit in einer Woche mit **C# / .NET 8 / WPF**.

Das Ziel: Ein spielbares Dungeon-Adventure mit Gegner-KI, Kampfsystem und Sieg/Niederlage-Bedingungen.

**Visuelle Richtung**: Düsteres Fantasy-Thema, Top-Down 2D, inspiriert durch *Crown Trick* und *Shattered Pixel Dungeon*.

---

## 🛠️ Technologie-Stack

| Komponente | Details |
|---|---|
| **Sprache** | C# (.NET 8) |
| **UI-Framework** | WPF (Windows Presentation Foundation) |
| **Architektur** | MVVM (Model-View-ViewModel), ohne externe Frameworks |
| **Persistenz** | XML-Serialisierung (Mittwoch) |
| **Platform** | Windows Desktop (.NET 8 Windows) |

---

## 📁 Projektstruktur

```
Projektarbeit_Dungeon of the Fallen/
├── DungeonOfTheFallen.Core/               # Domänenlogik & Business Logic
│   ├── Models/                            # Domänenmodelle
│   │   ├── GameState.cs                   # Zentrale Spielzustand-Verwaltung
│   │   ├── Player.cs                      # Spielercharakter (HP, XP, Inventory, Stats)
│   │   ├── Enemy.cs                       # Gegner (Type, Stats, Rewards)
│   │   ├── Tile.cs & DungeonMap.cs        # Dungeon-Grid-Logik
│   │   ├── Item.cs, Potion.cs             # Loot & Inventar
│   │   ├── TileType.cs (Floor/Wall/Exit)  # Tile-Typen
│   │   ├── EnemyType.cs (Goblin/Orc/Boss) # Gegner-Typen
│   │   └── ItemType.cs (Gold/Potion/Key)  # Item-Typen
│   │
│   ├── Services/                          # Game Services
│   │   ├── TurnManager.cs                 # Rundenlogik (später)
│   │   ├── CombatService.cs               # Kampf-Mechaniken (später)
│   │   └── DungeonGenerator.cs            # Map-Generierung (später)
│   │
│   └── Persistence/                       # Repository-Pattern
│       ├── IGameRepository.cs             # Interface (später)
│       └── XmlGameRepository.cs           # XML-Implementation (später)
│
├── Projektarbeit_Dungeon of the Fallen/   # WPF-Präsentation
│   ├── ViewModels/
│   │   ├── ViewModelBase.cs               # ✅ Basis mit INotifyPropertyChanged
│   │   ├── RelayCommand.cs                # ✅ Command-Implementation
│   │   ├── MainViewModel.cs               # ✅ Zentrale ViewModel (GameState Binding)
│   │   ├── TileViewModel.cs               # ✅ Einzeltile-Rendering
│   │   └── PlayerViewModel.cs             # ✅ Player Status Display
│   │
│   ├── MainWindow.xaml & .cs              # ✅ Hauptfenster mit Grid-Layout
│   ├── App.xaml & .cs
│   │
│   ├── Converters/                        # Value Converters (später)
│   └── Styles/                            # Fantasy-Theme Resourcen (später)
│
└── README.md                              # Diese Datei
```

---

## 🎮 Aktueller Entwicklungsstand

### **Donnerstag (Do)** ✅ ABGESCHLOSSEN
- ✅ Solution mit zwei Projekten aufgebaut
- ✅ Core-Klassenbibliothek mit allen Domänenmodellen
- ✅ Enums: `TileType`, `EnemyType`, `ItemType`
- ✅ Modelle: `Player`, `Enemy`, `Tile`, `DungeonMap`, `GameState`, `Item`, `Potion`, `Inventory`
- ✅ Build erfolgreich, App startet
- ✅ README angelegt

### **Freitag (Fr)** ✅ ABGESCHLOSSEN
- ✅ MVVM-Grundgerüst (ViewModelBase, RelayCommand)
- ✅ MainViewModel mit GameState-Integration
- ✅ TileViewModel + PlayerViewModel für UI-Binding
- ✅ MainWindow.xaml mit ItemsControl + UniformGrid (20×20 Grid)
- ✅ Dunkles Fantasy-Farbschema (Hex-Farben)
- ✅ Spieler ('P') sichtbar im Grid
- ✅ Exit ('E') sichtbar
- ✅ Wände ('█') mit Umrandung
- ✅ Player Status Panel (HP, Level, XP, Gold, ATK/DEF)
- ✅ Combat Log Placeholder
- ✅ 0 Fehler, läuft stabil

### **Montag (Mo)** ⏳ GEPLANT
- ⏳ KeyBindings für Bewegung (WASD / Pfeile)
- ⏳ Wand-Kollisionslogik
- ⏳ 1–2 Gegnertypen spawnen
- ⏳ TurnManager schreiben
- ⏳ Gegner-Züge (zuerst zufällig/einfach richtungsbasiert)
- ⏳ Grid nach jedem Zug aktualisieren

### **Dienstag (Di)** ⏳ GEPLANT
- ⏳ CombatService (Nahkampf wenn Nachbarschaft)
- ⏳ Schaden/HP-Verlust/Tod
- ⏳ Loot-System (Gold, Tränke)
- ⏳ XP/Level-Aufstieg
- ⏳ Boss-Gegner
- ⏳ Dungeon-Exit (Gewinn-Bedingung)
- ⏳ Kampflog aktualisieren
- ⏳ MVP komplett spielbar

### **Mittwoch (Mi)** ⏳ GEPLANT
- ⏳ IGameRepository + XmlGameRepository
- ⏳ Save/Load-Funktionalität
- ⏳ UI-Polish
- ⏳ README vervollständigen
- ⏳ Präsentationsvorbereitung

---

## 🚀 MVP-Features (Scope bis Dienstag)

| Feature | Status |
|---|---|
| 20×20 Dungeon-Grid | ✅ |
| Spielerfigur sichtbar | ✅ |
| Spieler-Bewegung | ⏳ Montag |
| Gegner (2–3 Typen) | ⏳ Montag |
| Gegner-KI (einfach) | ⏳ Montag |
| Rundenbasierte Züge | ⏳ Montag |
| Direkter Kampf | ⏳ Dienstag |
| HP/XP/Level-System | ⏳ Dienstag |
| Loot (Gold, Tränke) | ⏳ Dienstag |
| Boss-Gegner | ⏳ Dienstag |
| Dungeon-Ausgang | ⏳ Dienstag |
| Kampflog | ✅ (UI placeholder) |
| Sieg/Niederlage | ⏳ Dienstag |

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
cd "Projektarbeit_Dungeon of the Fallen"

# Build
dotnet build --configuration Debug

# Run
dotnet run --project "Projektarbeit_Dungeon of the Fallen" --configuration Debug
```

Oder einfach in Visual Studio öffnen und F5 drücken.

---

## 🎨 Farbschema (Dungeon-Rendering)

```
Hintergrund:    #0a0a0a (Schwarz)
Panel:          #1a1a1a (Dunkelgrau)
Wand:           #333333 (Grau)
Floor:          #1a1a1a (Dunkelgrau)
Exit:           #FFD700 (Gold) ✨
Spawn:          #00AA00 (Grün) 🌿
Trap:           #AA0000 (Rot)
Healing Room:   #0088FF (Blau)
Text:           #e0e0e0 (Hellgrau)
Titel:          #FFD700 (Gold)
```

---

## 🏗️ MVVM-Architektur

```
MainWindow.xaml
    ↓ (DataContext)
MainViewModel
    ├── GameState (Player, Map, Enemies, CombatLog)
    ├── PlayerViewModel (Status Display)
    └── ObservableCollection<TileViewModel>
         └── [Jede TileViewModel bindet auf Tile-Modell]
             ├── DisplayText (P/E/I/█)
             └── BackgroundColor (je nach TileType)
```

**Datenfluss:**
1. Spieler-Input → Command
2. MainViewModel verarbeitet Logik
3. GameState wird aktualisiert
4. UI-Bindings aktualisieren automatisch
5. Tiles re-rendern sich selbst

---

## 📝 Bekannte Einschränkungen (MVP)

- **Prozeduraler Generator**: Noch nicht implementiert → Hardcoded Test-Map
- **Gegner-KI**: Einfache zufällige Bewegung (Montag)
- **Grafiken/Assets**: Nur Text-Symbole (P, E, I, █) statt Sprites
- **Sound/Musik**: Nicht vorhanden
- **Speichern/Laden**: Erst ab Mittwoch
- **Mobile/Web**: Nur Windows Desktop
- **Mehrere Ebenen**: Nicht im MVP

---

## 🧪 Tests

Derzeit keine Unit-Tests implementiert. MVP-Fokus liegt auf Funktionalität.

---

## 📄 Lizenz

Dieses Projekt ist Lernprojekt einer Projektarbeitswoche. Frei verwendbar für Lehrzwecke.

---

## 👨‍💻 Entwickler

Robo (Projektarbeit Week 1, April 2026)

---

**Letztes Update**: Freitag, 22. April 2026  
**Aktueller Branch**: `main`  
**Build Status**: ✅ Passing

## 💾 Domänenmodell (Donnerstag)

### Kernklassen
```csharp
// Player mit Stats
public class Player
{
    public int HP, MaxHP, Attack, Defense;
    public int XP, Level, Gold;
    public int PositionX, PositionY;
    public Inventory Inventory;
}

// Gegner mit variabler Difficulty
public class Enemy
{
    public EnemyType Type;  // Goblin, Orc, Boss
    public int HP, Attack, Defense;
    public bool IsBoss;
    public int GoldReward, XpReward;
}

// Tile-basiertes Grid
public class Tile
{
    public TileType Type;           // Floor, Wall, Exit, Spawn
    public Enemy? Enemy;
    public Item? Item;
    public bool HasPlayer;
}

// GameState: zentrale Verwaltung
public class GameState
{
    public Player Player;
    public DungeonMap Map;
    public List<Enemy> Enemies;
    public List<string> CombatLog;
    public bool IsGameOver, IsVictory;
}
```

---

## 🛠️ Build & Run

### Bauen
```bash
cd "Projektarbeit_Dungeon of the Fallen"
dotnet build
```

### Ausführen
```bash
cd "Projektarbeit_Dungeon of the Fallen/Projektarbeit_Dungeon of the Fallen"
dotnet run
```

---

## 📝 Projektwochen-Fortschritt

| Tag | Ziel | Status |
|-----|------|--------|
| **Do** | Basis-Setup | ✅ Laufend |
| **Fr** | MVVM + Grid-Rendering | ⏳ Geplant |
| **Mo** | Bewegung + Gegner | ⏳ Geplant |
| **Di** | Kampfsystem + MVP | ⏳ Geplant |
| **Mi** | Save/Load + Polish | ⏳ Geplant |

---

## 🎯 Known Constraints & Design-Entscheidungen

1. **Hardcoded Map statt Procedural Gen**: Simplifiziert die Implementierung, fokussiert auf Spielmechanik
2. **Rundenbasierte Züge statt Echtzeit**: Vereinfacht AI und kollisionserkennung
3. **Brushes statt Sprites**: Keine großen Asset-Anforderungen anfangs
4. **XML statt Datenbank**: Leicht zu speichern/laden ohne externe Dependencies
5. **Kein großes MVVM-Framework**: Eigene ViewModelBase + RelayCommand für Simplizität

---

## 📖 Weitere Informationen

- **Projektname**: DungeonOfTheFallen
- **Team**: Einzelentwicklung (Pair Programming mit KI-Assistent)
- **Zeitrahmen**: Woche vom [Datum]
- **Zielplattform**: Windows Desktop (.NET 8)

---

*Zuletzt aktualisiert: Donnerstag, Projektstart*
