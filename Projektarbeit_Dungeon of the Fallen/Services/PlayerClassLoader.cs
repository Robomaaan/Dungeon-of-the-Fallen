using System.Diagnostics;
using System.IO;
using System.Text.Json;
using DungeonOfTheFallen.Core.Models;

namespace Projektarbeit_Dungeon_of_the_Fallen.Services
{
    public static class PlayerClassLoader
    {
        private const string JsonFile = "Data/player_classes.json";

        private static readonly Dictionary<string, PlayerClass> IdToEnum = new(StringComparer.OrdinalIgnoreCase)
        {
            ["warrior"]  = PlayerClass.Warrior,
            ["rogue"]    = PlayerClass.Rogue,
            ["mage"]     = PlayerClass.Mage,
            ["cleric"]   = PlayerClass.Cleric,
            ["ranger"]   = PlayerClass.Ranger,
            ["assassin"] = PlayerClass.Assassin,
        };

        public static List<(PlayerClassDefinition Def, PlayerClass Enum)> Load()
        {
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, JsonFile);
            Debug.WriteLine($"[ClassSelection] Loading class data from {JsonFile}");

            List<PlayerClassDefinition>? defs = null;
            try
            {
                if (File.Exists(fullPath))
                {
                    var json = File.ReadAllText(fullPath);
                    defs = JsonSerializer.Deserialize<List<PlayerClassDefinition>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    Debug.WriteLine($"[Assets] Missing file: {JsonFile}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ClassSelection] JSON load failed: {ex.Message}");
            }

            if (defs != null && defs.Count > 0)
            {
                Debug.WriteLine($"[ClassSelection] Loaded classes: {defs.Count}");
                return defs
                    .Where(d => IdToEnum.ContainsKey(d.Id))
                    .Select(d => (NormalizePortrait(d), IdToEnum[d.Id]))
                    .ToList();
            }

            Debug.WriteLine("[ClassSelection] Using fallback class data");
            return BuildFallback();
        }

        private static List<(PlayerClassDefinition, PlayerClass)> BuildFallback()
        {
            return new List<(PlayerClassDefinition, PlayerClass)>
            {
                (new PlayerClassDefinition
                {
                    Id = "warrior", DisplayName = "Krieger",
                    PortraitImage = "/Assets/Characters/Player/player_idle_down_00.png",
                    ShortDescription = "Meister des Schwertkampfes",
                    HoverDescription = "Gepanzerter Frontkämpfer mit Schildhieb.",
                    Specials = new List<string> { "Schildhieb: +2 Trefferwurf, +1W6 Wuchtschaden, +2 RK für den Gegenzug" },
                    BaseStats = new ClassBaseStats { Health = 70, Damage = 12, Defense = 15, Speed = 5, Magic = 0 }
                }, PlayerClass.Warrior),
                (new PlayerClassDefinition
                {
                    Id = "rogue", DisplayName = "Schurke",
                    PortraitImage = "/Assets/Characters/Player/player_idle_down_00.png",
                    ShortDescription = "Präziser Angreifer aus dem Schatten",
                    HoverDescription = "Schnell und tödlich mit Degen und Hinterhalt.",
                    Specials = new List<string> { "Hinterhalt: +3 Trefferwurf, +2W4 Stichschaden" },
                    BaseStats = new ClassBaseStats { Health = 52, Damage = 14, Defense = 13, Speed = 8, Magic = 1 }
                }, PlayerClass.Rogue),
                (new PlayerClassDefinition
                {
                    Id = "mage", DisplayName = "Magier",
                    PortraitImage = "/Assets/Characters/Player/player_idle_down_00.png",
                    ShortDescription = "Meister arkaner Zerstörung",
                    HoverDescription = "Glaskanone mit arkanem W10-Angriff.",
                    Specials = new List<string> { "Arkaner Stoß: +1W8 Arkanschaden, +6 LP" },
                    BaseStats = new ClassBaseStats { Health = 44, Damage = 16, Defense = 11, Speed = 6, Magic = 10 }
                }, PlayerClass.Mage),
                (new PlayerClassDefinition
                {
                    Id = "cleric", DisplayName = "Kleriker",
                    PortraitImage = "/Assets/Characters/Player/player_idle_down_00.png",
                    ShortDescription = "Geweihter Kämpfer des Lichts",
                    HoverDescription = "Robust und heilig mit starkem Heilgebet.",
                    Specials = new List<string> { "Lichtgebet: +12 LP, +1W6 Strahlenschaden, +1 RK" },
                    BaseStats = new ClassBaseStats { Health = 58, Damage = 9, Defense = 14, Speed = 5, Magic = 6 }
                }, PlayerClass.Cleric),
                (new PlayerClassDefinition
                {
                    Id = "ranger", DisplayName = "Waldläufer",
                    PortraitImage = "/Assets/Characters/Player/player_idle_down_00.png",
                    ShortDescription = "Beständiger Jäger mit Langbogen",
                    HoverDescription = "Verlässlicher Fernkämpfer mit Pfeilhagel.",
                    Specials = new List<string> { "Pfeilhagel: +2 Trefferwurf, +1W8 Stichschaden" },
                    BaseStats = new ClassBaseStats { Health = 56, Damage = 12, Defense = 13, Speed = 8, Magic = 2 }
                }, PlayerClass.Ranger),
                (new PlayerClassDefinition
                {
                    Id = "assassin", DisplayName = "Assassine",
                    PortraitImage = "/Assets/Characters/Player/player_idle_down_00.png",
                    ShortDescription = "Hochrisiko-Schadensspitze aus dem Dunkeln",
                    HoverDescription = "Höchster Schaden – aber auch höchstes Risiko.",
                    Specials = new List<string> { "Hinrichtung: +4 Trefferwurf, +2W6 Nekrotenschaden" },
                    BaseStats = new ClassBaseStats { Health = 48, Damage = 18, Defense = 12, Speed = 9, Magic = 3 }
                }, PlayerClass.Assassin),
            };
        }

        private static PlayerClassDefinition NormalizePortrait(PlayerClassDefinition def)
        {
            def.PortraitImage = "/Assets/Characters/Player/player_idle_down_00.png";
            return def;
        }
    }
}
