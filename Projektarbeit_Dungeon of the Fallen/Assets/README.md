# 2.5D Sprite Assets Guide

## Übersicht

Dieses Verzeichnis enthält alle Sprite-Assets für die 2.5D-Rendering-Engine des Dungeon-Spiels.

**Format**: PNG mit transparentem Hintergrund  
**Größe (Tiles)**: 64×48 Pixel  
**Größe (Charaktere)**: 64×96 Pixel  
**Größe (Items)**: 32×32 Pixel  

## Verzeichnisstruktur

```
Assets/
├── Backgrounds/          # Menü- und Dungeon-Hintergründe
├── Tiles/
│   └── StoneDungeon/
│       ├── Floors/       # Bodenplatten
│       ├── Walls/        # Wandblöcke
│       └── Special/      # Türen, Treppen, Fallen, Rätsel
├── Characters/
│   ├── Player/           # Spieler-Sprites (Idle, Walk, Attack)
│   ├── Goblin/           # Goblin-Gegner
│   ├── Orc/              # Orc-Gegner
│   └── Boss/             # Boss-Charaktere
├── Items/                # Gold, Tränke, Schlüssel
├── Props/                # Dekorative Objekte (Fackeln, Säulen, etc.)
├── FX/                   # Effekte (Slash, Hit, Heal)
├── Dice/                 # Würfelgraphiken
└── UI/                   # Buttons, Panels, Icons
```

## Asset-Namenskonventionen

- **Tiles**: `{type}_{variant}_{id}.png`
  - Beispiel: `floor_stone_01.png`, `wall_back_01.png`, `trap_spikes_01.png`

- **Characters**: `{character}_{action}_{direction}_{frame}.png`
  - Beispiel: `player_idle_down_00.png`, `goblin_attack_down_00.png`

- **Items**: `{item}_{variant}.png`
  - Beispiel: `potion_red_01.png`, `gold_pile_01.png`, `key_01.png`

- **Effects**: `{effect}_{variant}.png`
  - Beispiel: `slash_01.png`, `heal_fx_01.png`

## Asset-Registry

Die `AssetRegistry`-Klasse in `Services/AssetRegistry.cs` verwaltet alle Asset-Pfade. Sie bietet folgende Methoden:

- `GetFloorAsset(Tile)` - Bodenplatte für einen Tile-Typ
- `GetWallAsset(Tile, DungeonMap)` - Wand-Sprite
- `GetEnemyAsset(Enemy)` - Gegner-Sprite
- `GetPlayerAsset(Player)` - Spieler-Sprite
- `GetItemAsset(Tile)` - Item-Icon
- `GetSpecialTileAsset(Tile)` - Spezielle Tile-Graphiken

## WPF-Integration

Assets werden zur Laufzeit als WPF-Ressourcen geladen:

```csharp
var assetPath = "/Assets/Characters/Player/player_idle_down_00.png";
```

In XAML:

```xaml
<Image Source="{Binding AssetPath}" 
       RenderOptions.BitmapScalingMode="NearestNeighbor" />
```

## Fallback-Verhalten

Falls ein Asset fehlt:
1. Die Methoden der `AssetRegistry` geben `string.Empty` zurück
2. Das XAML-Binding ignoriert leere Strings
3. Das Spiel crasht nicht, sondern zeigt ein leeres Feld an

## Performance-Tipps

- **Pixel-Art scharf halten**: Immer `RenderOptions.BitmapScalingMode="NearestNeighbor"` verwenden
- **Größen konsistent**: Alle Tiles 64×48, alle Charaktere 64×96
- **Transparenz**: PNG mit Alpha-Channel für korrekte Überlagerung

## Animation vorbereitet

Die `RenderObjectViewModel`-Klasse hat Platz für Animationen:

```csharp
public bool IsAnimated { get; set; }
public int FrameIndex { get; set; }
```

Dies ermöglicht später die Implementierung von Idle/Walk/Attack-Animationen durch Frame-Wechsel.

## Nächste Schritte

1. **Asset-Generation**: Sprite Forge nutzen, um PNGs zu generieren
2. **Batch-Ordnung**: Bilder in die entsprechenden Unterordner sortieren
3. **Namengebung**: Konventionen folgen
4. **Testing**: Spiel starten und visuelle Ausgabe prüfen
5. **Optimierung**: Bei Bedarf Farben/Größen anpassen

---

Für Fragen: Siehe `RebuildRenderObjects()` in `MainViewModel.cs`
