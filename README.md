# Dungeon of the Fallen

Dungeon of the Fallen ist ein WPF-/C#-Dungeon-Crawler auf .NET 8. Der aktuelle Stand ist ein spielbarer MVP mit klar getrenntem Dungeon-Flow, sicherer Zwischenphase nach Kämpfen, zentralem Balancing und Debug-Hotkeys für Entwicklungs- und Testzwecke.

## Projektüberblick

- WPF-Oberfläche mit MVVM-Struktur
- Zwei Hauptprojekte: Core-Logik und WPF-Präsentation
- Hauptmenü, Klassenauswahl, Run-Start, Dungeon-Erkundung, Kampf, Loot, Save/Load
- Mehrere Ebenen mit sauberem Übergang zwischen Exploration, Kampf und Belohnungsphase
- Floor 1 ist bewusst so balanciert, dass Ebene 2 realistisch erreichbar ist

## Spielprozess

1. Im Hauptmenü wird ein neuer Run gestartet oder ein Save geladen.
2. In der Klassenauswahl wird die Spielerklasse festgelegt.
3. Der Run beginnt auf Ebene 1 in der Explorationsphase.
4. Der Spieler bewegt sich durch den Dungeon, sammelt Loot ein, öffnet Räume und löst Tile-Effekte aus.
5. Beim Kontakt mit einem Gegner wechselt das Spiel in den Kampfbeginn und öffnet das Kampffenster.
6. Im Kampf laufen die Phasen Spielerzug, Würfelauflösung, Gegnerzug, Sieg oder Niederlage.
7. Nach einem Sieg folgt keine harte Fortsetzung, sondern eine sichere Post-Combat-Zwischenphase.
8. In dieser Zwischenphase kann der Spieler gefahrlos heilen, bevor er mit Weiter erneut loszieht.
9. Sobald die Ebene gesäubert ist, wechselt das Spiel in `LevelComplete`.
10. Mit Weiter wird die nächste Ebene geladen und der Run läuft in `Exploration` weiter.
11. Bei Niederlage oder Run-Abbruch endet der Run sauber und das Spiel kann ins Hauptmenü zurückkehren.

## Zustandsmodell

### Dungeon-Flow

Die globale Laufsteuerung arbeitet mit diesen Phasen:

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

Das Kampffenster verwendet zusätzlich eine interne Kampfsteuerung:

| Phase | Bedeutung |
|---|---|
| `PlayerTurn` | Spieleraktion wählen |
| `BothRolling` | Würfelanimation läuft |
| `EnemyTurn` | Gegner reagiert im Kampf |
| `Victory` | Gegner besiegt |
| `Defeat` | Spieler besiegt |

## Wichtige Regeln

- Ein Gegner darf nur dann angreifen, wenn der aktive Flow wirklich einen Gegnerzug vorsieht.
- Tränke sind eine freie Sicherheitsaktion und lösen keinen direkten Gegnerangriff aus.
- Loot einsammeln, Menüaktionen, Save/Load oder Run-Abbruch lösen keinen automatischen EnemyTurn aus.
- Nach einem Sieg entsteht zuerst die sichere Post-Combat-Phase, erst danach geht es bewusst weiter.
- Der Übergang zur nächsten Ebene passiert nur über eine explizite Continue-Aktion.

## Kampf- und Tranklogik

- Der Kampf protokolliert Würfelwurf, Boni, Trefferstatus, Schaden und HP-Änderungen.
- Tränke schreiben eigene Logeinträge, zum Beispiel:
  - `[Trank] Spieler heilt 12 HP.`
  - `[Trank] Kein Gegnerzug ausgelöst.`
  - `[Trank] HP bereits voll. Kein Trank verbraucht.`
  - `[Trank] Kein Trank verfügbar.`
- Bei voller HP wird kein Trank verbraucht.
- Ein Trank verursacht weder Würfelwurf noch gegnerischen Gegenschaden.

## Balancing von Ebene 1

Die erste Ebene wurde bewusst entschärft, damit normale Runs nicht regelmäßig vor Ebene 2 enden.

- Starttränke: mindestens 2
- Ebene 1: 2 normale Gegner plus Boss
- Gegner-HP und Gegner-Schaden auf Ebene 1 reduziert
- Boss-Schaden auf Ebene 1 reduziert
- Nach Kämpfen gibt es eine zentrale Heilung von rund 15 Prozent der MaxHP, begrenzt auf einen sinnvollen Bereich
- Beim Ebenenwechsel gibt es eine zusätzliche sichere Heilung
- Belohnungen wie Feldtränke und Boss-Tränke stützen den Run zusätzlich

Die zentralen Werte liegen in `DungeonOfTheFallen.Core/Services/GameBalance.cs`.

## Debug-Hotkeys

Die folgenden Tasten sind nur in `DEBUG` aktiv:

- `F9` = Spieler vollständig heilen
- `F10` = aktuellen Kampf sofort gewinnen
- `F11` = direkt zur nächsten Ebene wechseln
- `F12` = Godmode an- oder ausschalten

Jede Debug-Aktion erzeugt einen Logeintrag. Im Debug-Build kann `F9` als Entwickler-Hotkey die normale Ladebelegung überlagern.

## Steuerung

- `WASD` oder Pfeiltasten = bewegen
- `P` = Trank benutzen
- `F5` = speichern
- Laden über die UI-Aktion im Hauptmenü bzw. das Laden im Spiel
- Weiter-Button = sichere Zwischenphase verlassen oder nächste Ebene starten
- Hauptmenü- und Run-Abbruch-Buttons = Run kontrolliert verlassen

## Build & Run

Voraussetzungen:

- .NET 8 SDK
- Windows 10/11
- Visual Studio 2022 oder ein kompatibler Editor

Build:

```bash
dotnet build "Projektarbeit_Dungeon of the Fallen.sln"
```

Run:

```bash
dotnet run --project "Projektarbeit_Dungeon of the Fallen/Projektarbeit_Dungeon of the Fallen.csproj"
```

## Projektstruktur

```text
.
├── DungeonOfTheFallen.Core/              # Domänenlogik, Kampf, Balancing, Persistenz
├── Projektarbeit_Dungeon of the Fallen/   # WPF-Oberfläche, Fenster, ViewModels
├── docs/                                  # Entwicklungsnotizen und Prozessdokumentation
├── README.md
└── .gitignore
```

Die lokalen Arbeitsordner `.claude/` und `Projektschritte/` sind per `.gitignore` ausgeschlossen und gehören nicht in den normalen Commit-Verlauf.

## Tests und Status

- Der aktuelle Build wurde erfolgreich ausgeführt.
- Es sind keine automatisierten Unit-Tests im Repository hinterlegt.
- Die Hauptprüfung ist aktuell der saubere Build plus manuelle Spieltests.

## Lizenz

Dieses Projekt ist ein Lern- und Projektarbeitsprojekt und kann für Lehrzwecke verwendet werden.

## Entwickler

Robo

## Letztes Update

29. April 2026
