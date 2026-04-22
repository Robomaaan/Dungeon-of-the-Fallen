# DungeonOfTheFallen - WPF Dungeon Crawler

## 📋 Übersicht

**DungeonOfTheFallen** ist ein kleines 2D-Dungeon-Crawler-Spiel im Fantasy-Stil, entwickelt als Projektarbeit mit **C#**, **.NET 8** und **WPF**.

Das Spiel bietet rundenbasierte Gameplay-Mechaniken auf einem Dungeon-Grid mit:
- Spielerfigur mit Levelsystem
- Gegner mit unterschiedlichen Typen
- Direkter Kampf auf dem Grid
- Loot-System (Gold, Tränke)
- Boss-Gegner
- Sieg/Niederlage-Bedingungen

**Visuelle Richtung**: Düsteres Fantasy-Theme, Top-Down 2D, inspiriertdurch Games wie *Crown Trick* und *Shattered Pixel Dungeon*.

---

## 🛠️ Technologie-Stack

- **Sprache**: C# (.NET 8)
- **UI-Framework**: WPF (Windows Presentation Foundation)
- **Architektur**: MVVM (Model-View-ViewModel)
- **Persistenz**: XML-Serialisierung (geplant für Mittwoch)

---

## 📁 Projektstruktur

```
DungeonOfTheFallen/
├── DungeonOfTheFallen.Core/                 # Domänenlogik & Services
│   ├── Models/                              # Domänenmodelle
│   │   ├── GameState.cs                     # Spielzustand
│   │   ├── Player.cs                        # Spielercharakter
│   │   ├── Enemy.cs                         # Gegnertypen
│   │   ├── Tile.cs & DungeonMap.cs          # Dungeon-Grid-Logik
│   │   ├── Item.cs, Potion.cs, Inventory.cs # Loot & Inventar
│   │   └── *Type.cs Enums                   # Typdefinitionen
│   ├── Services/                            # Services (später gefüllt)
│   └── Persistence/                         # Repository-Pattern (später)
│
└── DungeonOfTheFallen.WPF/                  # WPF-Präsentation
    ├── MainWindow.xaml & .cs
    ├── App.xaml & .cs
    ├── ViewModels/                          # MVVM ViewModels (Freitag)
    ├── Views/                               # XAML-Views (Freitag)
    ├── Converters/                          # Value Converters (später)
    └── Styles/                              # Fantasy-Theme (später)
```

---

## 🎮 Architektur (Kurzübersicht)

### **Donnerstag (Do)** – Basis-Setup ✅ (in Bearbeitung)
1. ✅ Solution mit zwei Projekten aufgebaut
2. ✅ Core-Klassenbibliothek mit Domänenmodellen
3. ✅ Basis-Enums: `TileType`, `EnemyType`, `ItemType`
4. ✅ Modelle: `Player`, `Enemy`, `Tile`, `DungeonMap`, `GameState`, `Item`, `Potion`, `Inventory`
5. ✅ Build erfolgreich, App compilierbar

**Nächste Phasen:**
- **Freitag (Fr)**: MVVM-Grundgerüst + Grid-Rendering
- **Montag (Mo)**: Spielerbewegung + Gegner-AI + TurnManager
- **Dienstag (Di)**: Kampfsystem + Loot + Boss + MVP-fertig
- **Mittwoch (Mi)**: XML-Persistenz + Polish + README

---

## 🚀 Features (MVP-Scope)

**MUSS bis Dienstag funktionieren:**
- ✅ Dungeon-Grid (10x10)
- ⏳ Spielerfigur sichtbar & beweglich
- ⏳ Gegner (2–3 Typen)
- ⏳ Rundenbasierte Züge
- ⏳ Direkter Kampf auf dem Grid
- ⏳ HP/XP/Level-System
- ⏳ Loot (Gold, Tränke)
- ⏳ Boss-Gegner
- ⏳ Dungeon-Ausgang
- ⏳ Kampflog
- ⏳ Sieg/Niederlage-Bedingungen

**MUSS bis Mittwoch:**
- ⏳ XML-Save/Load
- ⏳ UI-Polish
- ⏳ Präsentierbar sein

**OPTIONAL (Bonus, nur wenn MVP stabil):**
- Animationen
- Partikeleffekte
- Mehrere Dungeon-Ebenen
- Equipment-System
- Erweiterte Gegner-KI
- Sound

---

## 💾 Domänenmodell (Dönnerstag)

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
