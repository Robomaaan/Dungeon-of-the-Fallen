# ⚔️ DungeonOfTheFallen - WPF Dungeon Crawler

## 📋 Übersicht

**DungeonOfTheFallen** ist ein rundenbasiertes 2D-Dungeon-Crawler-Spiel im Fantasy-Stil, entwickelt mit **C# / .NET 8 / WPF**.

Der aktuelle Stand ist ein spielbarer MVP mit Hauptmenü, Klassenauswahl, Dungeon-Erkundung, Kampffenster, Loot, Tränken, Fallen, Heilungsraum, Save/Load, sicherer Zwischenphase nach Kämpfen und sauberem Levelwechsel.

**Visuelle Richtung**: Düsteres Fantasy-Thema, Top-Down 2D, inspiriert durch *Crown Trick* und *Shattered Pixel Dungeon*.

---

## 🛠️ Technologie-Stack

| Komponente | Details |
|---|---|
| **Sprache** | C# (.NET 8) |
| **UI-Framework** | WPF (Windows Presentation Foundation) |
| **Architektur** | MVVM (Model-View-ViewModel), ohne externes Framework |
| **Persistenz** | XML-Serialisierung über `SaveData` + `XmlGameRepository` |
| **Plattform** | Windows Desktop (.NET 8 Windows) |

---

## 🎮 Aktueller Spielprozess

1. Im Hauptmenü wird ein neuer Run gestartet oder ein Save geladen.
2. In der Klassenauswahl wird die Spielerklasse festgelegt.
3. Der Run beginnt auf Ebene 1 in der **Exploration**.
4. Der Spieler bewegt sich durch den Dungeon, sammelt Loot ein, öffnet Räume und löst Tile-Effekte aus.
5. Beim Kontakt mit einem Gegner wechselt das Spiel in **CombatStart** und öffnet das Kampffenster.
6. Im Kampf laufen die Phasen **PlayerTurn**, **BothRolling**, **EnemyTurn**, **Victory** oder **Defeat**.
7. Nach einem Sieg folgt keine harte Fortsetzung, sondern eine sichere **PostCombat**-Zwischenphase.
8. In dieser Zwischenphase kann der Spieler gefahrlos heilen, bevor er mit **Weiter** erneut loszieht.
9. Sobald die Ebene gesäubert ist, wechselt das Spiel in **LevelComplete**.
10. Mit **Weiter** wird die nächste Ebene geladen und der Run läuft wieder in **Exploration** weiter.
11. Bei Niederlage oder Run-Abbruch endet der Run sauber und das Spiel kehrt kontrolliert ins Hauptmenü zurück.

---

## 🧭 Zustandsmodell

### Dungeon-Flow

| Phase | Bedeutung |
|---|---|
| `Exploration` | Normales Bewegen und Erkunden |
| `CombatStart` | Kampf wird gestartet |
| `PlayerTurn` | Spieler darf handeln |
| `EnemyTurn` | Gegnerzug ist ausdrücklich vorgesehen |
| `PostCombat` | Sichere Zwischenphase nach einem Sieg |
| `LevelComplete` | Ebene abgeschlossen, Weiter zur nächsten Ebene |
| `GameOver` | Run beendet |

### Kampf-Flow

| Phase | Bedeutung |
|---|---|
| `PlayerTurn` | Spieleraktion wählen |
| `BothRolling` | Würfelanimation läuft |
| `EnemyTurn` | Gegner reagiert im Kampf |
| `Victory` | Gegner besiegt |
| `Defeat` | Spieler besiegt |

**Wichtig:** Ein EnemyTurn wird nur dann ausgelöst, wenn der aktive Flow wirklich einen Gegnerzug vorsieht.

---

## 📁 Projektstruktur

```text
.
├── DungeonOfTheFallen.Core/               # Domänenlogik, Kampf, Balancing, Persistenz
│   ├── Models/                            # Spielzustände und Datenmodelle
│   ├── Services/                          # Kampf, Gegner, Balancing, Turn-Logik
│   └── Persistence/                       # XML Save/Load
│
├── Projektarbeit_Dungeon of the Fallen/   # WPF-Präsentation
│   ├── ViewModels/                        # MainViewModel, CombatViewModel, PlayerViewModel
│   ├── MainMenuWindow.xaml & .cs          # Hauptmenü
│   ├── ClassSelectionWindow.xaml & .cs    # Klassenauswahl
│   ├── MainWindow.xaml & .cs              # Hauptspiel
│   ├── CombatWindow.xaml & .cs            # Kampfansicht
│   └── App.xaml & .cs
│
├── Projektarbeit_Dungeon of the Fallen.sln
└── README.md
```

Die lokalen Arbeitsordner `.claude/` und `Projektschritte/` sind per `.gitignore` ausgeschlossen und gehören nicht in den normalen Commit-Verlauf.

---

## 🚀 Aktueller Entwicklungsstand

### Wichtige Gameplay-Anpassungen

- ✅ Saubere Trennung von Exploration, CombatStart, PlayerTurn, EnemyTurn, PostCombat, LevelComplete und GameOver
- ✅ Trankbenutzung löst keinen Gegnerzug mehr aus
- ✅ Nach Kämpfen gibt es eine sichere Zwischenphase
- ✅ Ebene 1 ist entschärft und Ebene 2 realistisch erreichbar
- ✅ Debug-Hotkeys für Testläufe eingebaut
- ✅ Hauptfenster und Spielansicht sind wieder geschlossen und nicht transparent
- ✅ DragMove-Fehler in Menü- und Auswahlfenstern abgesichert

### Architektur-Fundament

- zentralere Kampfverarbeitung über `CombatService`
- `CombatTurnResult` als Rückgabeobjekt für UI, Logs und Animation
- gemeinsame Phasensteuerung über `GameFlowPhase`
- zentrale Balancing-Konfiguration über `GameBalance`
- `EnemyFactory` für wiederverwendbare Gegnerwerte
- `EnemySpawnService` für biomebasierte Gegnerroster
- Save-System mit sauberem Mapping über `SaveDataMapper`

### Jüngste Optimierungen

- Floor 1: 2 normale Gegner plus Boss
- Floor 1: reduzierte Gegner- und Boss-Schadenswerte
- Starttränke: mindestens 2
- Post-Combat-Heilung: kleiner, konfigurierbarer Puffer
- Übergang zur nächsten Ebene: sicher und bewusst

---

## 🚀 MVP-Features

| Feature | Status |
|---|---|
| 20x20 Dungeon-Grid | ✅ |
| Spielerfigur sichtbar | ✅ |
| Spieler-Bewegung | ✅ |
| Gegner (biomabhängige Gegner- und Bossliste) | ✅ |
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

Oder die Solution direkt in Visual Studio öffnen und mit F5 starten.

---

## 🎨 Farbschema (Dungeon-Rendering)

| Bereich | Farbe |
|---|---|
| Hintergrund | `#0a0a0a` |
| Panel | `#1a1a1a` |
| Wand | `#333333` |
| Floor | `#1a1a1a` |
| Exit | `#FFD700` |
| Spawn | `#00AA00` |
| Trap | `#AA0000` |
| Healing Room | `#0088FF` |
| Text | `#e0e0e0` |
| Titel | `#FFD700` |

---

## 🏗️ MVVM-Architektur

```text
App.xaml.cs
    ↓
MainMenuWindow.xaml
    ↓ (Neues Spiel / Laden)
ClassSelectionWindow.xaml
    ↓ (Klasse wählen)
MainWindow.xaml
    ↓ (DataContext)
MainViewModel
    ├── GameState (Player, Map, Enemies, CombatLog, Phase)
    ├── PlayerViewModel (Status Display)
    └── ObservableCollection<TileViewModel>
         └── [Jede TileViewModel bindet auf Tile-Modell]
             ├── DisplayText (P/E/I/█)
             └── BackgroundColor (je nach TileType)

CombatWindow.xaml
    ↓ (DataContext)
CombatViewModel
    ├── Würfelanimation
    ├── Angriff / Potion / Skill
    └── Rückgabe an `MainWindow`
```

**Datenfluss:**

1. Spieler-Input oder Fenster-Event → Command / Event
2. ViewModel verarbeitet die Logik
3. `GameState` wird aktualisiert
4. UI-Bindings aktualisieren automatisch
5. `CombatWindow` meldet das Ergebnis nach der Animation zurück

---

## 🧱 Phase-2 Foundation (Technischer Hintergrund)

Der technische Unterbau ist bewusst modular gehalten und auf spätere Ausbaustufen vorbereitet:

- zentralere Combat-Abwicklung über `CombatService`
- `CombatTurnResult` als Rückgabeobjekt für UI/Animation
- gemeinsame Stats-Basis über `CombatantStats`
- `EnemyFactory` für zentrale Gegnererzeugung
- `EnemySpawnService` für biomebasierte Gegnerroster
- Save-System auf `Version 2` erweitert
- `SaveDataMapper` kapselt Save/Load-Mapping aus dem ViewModel

Aktuelle Gegnerfamilien:

- Goblin
- Spider
- Skeleton
- Orc
- Zombie
- Troll
- Ogre
- Dragon
- DemonLord
- Lich
- Boss

Geplante nächste Ausbaustufen:

- `PlayerProgressionService`
- `LootService`
- `EnemySpawnService`
- Save-Migrationen

---

## 🧠 Debug-Hotkeys

Die folgenden Tasten sind nur in `DEBUG` aktiv:

- `F9` = Spieler vollständig heilen
- `F10` = aktuellen Kampf sofort gewinnen
- `F11` = direkt zur nächsten Ebene wechseln
- `F12` = Godmode an- oder ausschalten

Jede Debug-Aktion erzeugt einen Logeintrag. Im Debug-Build kann `F9` als Entwickler-Hotkey die normale Laden-Belegung überlagern.

---

## 🎯 Balancing Ebene 1

Die erste Ebene wurde bewusst entschärft, damit normale Runs nicht regelmäßig vor Ebene 2 enden.

| Wert | Anpassung |
|---|---|
| Start-Tränke | mindestens 2 |
| Normale Gegner | 2 statt einer zu hohen Menge |
| Gegner-Schaden | reduziert |
| Boss-Schaden | reduziert |
| Nach Kampf | sichere Heilung als Puffer |
| Ebenenwechsel | zusätzliche sichere Heilung |
| Loot | Feldtränke und Boss-Tränke als Puffer |

Die zentralen Werte liegen in `DungeonOfTheFallen.Core/Services/GameBalance.cs`.

---

## 📝 Bekannte Einschränkungen

- **Prozeduraler Generator**: Noch nicht implementiert, aktuell wird eine feste Test-Map verwendet
- **Sound/Musik**: Nicht vorhanden
- **Automatisierte Tests**: Keine Unit-Tests im Repository hinterlegt
- **Mehrere Ebenen**: Bereits im Flow angelegt, Inhalt kann aber noch weiter ausgebaut werden

---

## 🧪 Tests

- Der aktuelle Build wurde erfolgreich ausgeführt.
- Die Kampf- und Tranklogik wurde so umgebaut, dass Tränke keinen Gegnerzug mehr auslösen.
- Die UI wurde auf geschlossene Darstellung und saubere Sichtbarkeit geprüft.
- Für tiefergehende Spielbalance ist ein manueller Playthrough weiterhin sinnvoll.

---

## 📄 Lizenz

Dieses Projekt ist ein Lernprojekt einer Projektarbeitswoche. Frei verwendbar für Lehrzwecke.

---

## 👨‍💻 Entwickler

Robo (Projektarbeit, April 2026)

---

**Letztes Update**: 29. April 2026  
**Aktueller Branch**: `main`  
**Build Status**: ✅ Passing
