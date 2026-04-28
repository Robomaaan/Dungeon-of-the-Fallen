using DungeonOfTheFallen.Core.Models;

namespace Projektarbeit_Dungeon_of_the_Fallen.Services
{
    /// <summary>
    /// Central registry for asset paths based on game entities.
    /// Provides safe lookups with fallbacks for missing assets.
    /// </summary>
    public class AssetRegistry
    {
        /// <summary>
        /// Fallback asset shown when an asset is missing or cannot be found.
        /// Should be a simple colored/textured placeholder PNG that's visually distinct.
        /// </summary>
        private const string MissingAssetPath = "/Assets/system/missing_asset.png";

        /// <summary>
        /// Gets the floor tile asset for a given tile.
        /// Returns the missing asset fallback if the tile is null.
        /// </summary>
        public string GetFloorAsset(Tile tile)
        {
            if (tile == null) return MissingAssetPath;

            return tile.TileType switch
            {
                TileType.Floor => "/Assets/Tiles/StoneDungeon/Floors/floor_stone_01.png",
                TileType.AshFloor => "/Assets/Tiles/StoneDungeon/Floors/floor_ash_01.png",
                TileType.SandFloor => "/Assets/Tiles/StoneDungeon/Floors/floor_sand_01.png",
                TileType.CloudFloor => "/Assets/Tiles/StoneDungeon/Floors/floor_cloud_01.png",
                TileType.CursedFloor => "/Assets/Tiles/StoneDungeon/Floors/floor_cursed_01.png",
                TileType.Exit => "/Assets/Tiles/StoneDungeon/Special/exit_stairs_01.png",
                TileType.Spawn => "/Assets/Tiles/StoneDungeon/Floors/floor_stone_01.png",
                TileType.Trap => "/Assets/Tiles/StoneDungeon/Floors/floor_stone_01.png",
                TileType.HealingRoom => "/Assets/Tiles/StoneDungeon/Floors/floor_stone_01.png",
                TileType.ThornTrap => "/Assets/Tiles/StoneDungeon/Special/trap_spikes_01.png",
                TileType.CurseTrap => "/Assets/Tiles/StoneDungeon/Special/trap_magic_01.png",
                TileType.LavaTrap => "/Assets/Tiles/StoneDungeon/Special/trap_spikes_01.png",
                TileType.SpikeTrap => "/Assets/Tiles/StoneDungeon/Special/trap_spikes_01.png",
                TileType.DivineTrap => "/Assets/Tiles/StoneDungeon/Floors/floor_stone_01.png",
                TileType.HealingShrine => "/Assets/Tiles/StoneDungeon/Special/healing_shrine_01.png",
                TileType.HealingAltar => "/Assets/Tiles/StoneDungeon/Special/healing_shrine_01.png",
                TileType.HotSpring => "/Assets/Tiles/StoneDungeon/Special/healing_shrine_01.png",
                TileType.HealingBubble => "/Assets/Tiles/StoneDungeon/Special/healing_shrine_01.png",
                TileType.LightCircle => "/Assets/Tiles/StoneDungeon/Floors/floor_stone_01.png",
                TileType.LockedDoor => "/Assets/Tiles/StoneDungeon/Special/locked_door_01.png",
                TileType.Puzzle => "/Assets/Tiles/StoneDungeon/Special/puzzle_tile_01.png",
                TileType.KeyPedestal => "/Assets/Tiles/StoneDungeon/Special/key_pedestal_01.png",
                TileType.Wall => "/Assets/Tiles/StoneDungeon/Floors/floor_stone_01.png",
                _ => MissingAssetPath
            };
        }

        /// <summary>
        /// Gets the wall asset for a given tile, choosing the correct orientation based on neighbors.
        /// </summary>
        public string GetWallAsset(Tile tile, DungeonMap? map = null)
        {
            if (tile?.TileType != TileType.Wall)
                return MissingAssetPath;

            if (map == null)
                return "/Assets/Tiles/StoneDungeon/Walls/wall_back_01.png";

            var floorN = !IsWallOrNull(map.GetTile(tile.X, tile.Y - 1));
            var floorS = !IsWallOrNull(map.GetTile(tile.X, tile.Y + 1));
            var floorW = !IsWallOrNull(map.GetTile(tile.X - 1, tile.Y));
            var floorE = !IsWallOrNull(map.GetTile(tile.X + 1, tile.Y));

            // Corners: two open sides → pick the matching corner sprite
            if (floorS && floorE) return "/Assets/Tiles/StoneDungeon/Walls/wall_corner_tl_01.png";
            if (floorS && floorW) return "/Assets/Tiles/StoneDungeon/Walls/wall_corner_tr_01.png";
            if (floorN && floorE) return "/Assets/Tiles/StoneDungeon/Walls/wall_corner_bl_01.png";
            if (floorN && floorW) return "/Assets/Tiles/StoneDungeon/Walls/wall_corner_br_01.png";

            // Straight walls
            if (floorS) return "/Assets/Tiles/StoneDungeon/Walls/wall_back_01.png";
            if (floorN) return "/Assets/Tiles/StoneDungeon/Walls/wall_front_01.png";
            if (floorE) return "/Assets/Tiles/StoneDungeon/Walls/wall_left_01.png";
            if (floorW) return "/Assets/Tiles/StoneDungeon/Walls/wall_right_01.png";

            return "/Assets/Tiles/StoneDungeon/Walls/wall_back_01.png";
        }

        private static bool IsWallOrNull(Tile? t) =>
            t == null || t.TileType == TileType.Wall;

        /// <summary>
        /// Gets the asset for an entity on a tile (Enemy or NPC).
        /// </summary>
        public string GetEntityAsset(Tile tile)
        {
            if (tile == null) return MissingAssetPath;

            if (tile.Enemy != null)
                return GetEnemyAsset(tile.Enemy);

            if (tile.Npc != null)
                return GetNpcAsset(tile.Npc);

            return MissingAssetPath;
        }

        /// <summary>
        /// Gets the asset for an item on a tile.
        /// </summary>
        public string GetItemAsset(Tile tile)
        {
            if (tile?.Item == null) return MissingAssetPath;

            if (tile.Item is KeyItem)
                return "/Assets/Items/key_01.png";

            return tile.Item.ItemType switch
            {
                ItemType.Gold => "/Assets/Items/gold_pile_01.png",
                ItemType.Potion => "/Assets/Items/potion_red_01.png",
                _ => MissingAssetPath
            };
        }

        /// <summary>
        /// Gets the asset for special tile types (doors, traps, healing, puzzles).
        /// </summary>
        public string GetSpecialTileAsset(Tile tile)
        {
            if (tile == null) return MissingAssetPath;

            return tile.TileType switch
            {
                TileType.HealingRoom => "/Assets/Tiles/StoneDungeon/Special/fountain.png",
                TileType.LockedDoor => "/Assets/Tiles/StoneDungeon/Special/locked_door_01.png",
                TileType.Exit => "/Assets/Tiles/StoneDungeon/Special/exit_stairs_01.png",
                TileType.ThornTrap => "/Assets/Tiles/StoneDungeon/Special/trap_spikes_01.png",
                TileType.CurseTrap => "/Assets/Tiles/StoneDungeon/Special/trap_magic_01.png",
                TileType.LavaTrap => "/Assets/Tiles/StoneDungeon/Special/trap_spikes_01.png",
                TileType.SpikeTrap => "/Assets/Tiles/StoneDungeon/Special/trap_spikes_01.png",
                TileType.DivineTrap => "/Assets/Tiles/StoneDungeon/Special/trap_spikes_01.png",
                TileType.HealingShrine => "/Assets/Tiles/StoneDungeon/Special/fountain.png",
                TileType.HealingAltar => "/Assets/Tiles/StoneDungeon/Special/fountain.png",
                TileType.HotSpring => "/Assets/Tiles/StoneDungeon/Special/fountain.png",
                TileType.HealingBubble => "/Assets/Tiles/StoneDungeon/Special/fountain.png",
                TileType.Puzzle => "/Assets/Tiles/StoneDungeon/Special/puzzle_tile_01.png",
                TileType.KeyPedestal => "/Assets/Tiles/StoneDungeon/Special/key_pedestal_01.png",
                _ => MissingAssetPath
            };
        }

        /// <summary>
        /// Gets the asset for the player character.
        /// </summary>
        public string GetPlayerAsset(Player? player)
        {
            if (player == null) return MissingAssetPath;
            return "/Assets/Characters/Player/player_idle_down_00.png";
        }

        /// <summary>
        /// Gets the asset for an enemy. Each enemy type has its own planned path.
        /// Missing files fall back to missing_asset.png via FallbackImageConverter.
        /// </summary>
        public string GetEnemyAsset(Enemy? enemy)
        {
            if (enemy == null) return MissingAssetPath;

            return enemy.EnemyType switch
            {
                EnemyType.Goblin    => "/Assets/Characters/Goblin/goblin_idle_down_00.png",
                EnemyType.Spider    => "/Assets/Characters/Spider/spider_idle_down_00.png",
                EnemyType.Skeleton  => "/Assets/Characters/Skeleton/skeleton_idle_down_00.png",
                EnemyType.Orc       => "/Assets/Characters/Orc/orc_idle_down_00.png",
                EnemyType.Zombie    => "/Assets/Characters/Zombie/zombie_idle_down_00.png",
                EnemyType.Troll     => "/Assets/Characters/Troll/troll_idle_down_00.png",
                EnemyType.Ogre      => "/Assets/Characters/Boss/boss_orc_idle_down_00.png",
                EnemyType.Dragon    => "/Assets/Characters/Boss/boss_orc_idle_down_00.png",
                EnemyType.DemonLord => "/Assets/Characters/Boss/boss_orc_idle_down_00.png",
                EnemyType.Lich      => "/Assets/Characters/Boss/boss_orc_idle_down_00.png",
                EnemyType.Boss      => "/Assets/Characters/Boss/boss_orc_idle_down_00.png",
                _                   => MissingAssetPath
            };
        }

        /// <summary>
        /// Gets the asset for an NPC. Each NPC type has its own planned path.
        /// Missing files fall back to missing_asset.png via FallbackImageConverter.
        /// </summary>
        public string GetNpcAsset(Npc? npc)
        {
            if (npc == null) return MissingAssetPath;

            return npc.NpcType switch
            {
                NpcType.Healer      => "/Assets/Characters/NPC/cleric_00001_.png",
                NpcType.Merchant    => "/Assets/Characters/Player/player_idle_down_00.png",
                NpcType.Chronicler  => "/Assets/Characters/Player/player_idle_down_00.png",
                NpcType.Blacksmith  => "/Assets/Characters/Player/player_idle_down_00.png",
                NpcType.Scout       => "/Assets/Characters/NPC/icon_stealth_00001_.png",
                NpcType.Mystic      => "/Assets/Characters/NPC/lichking_idle_down_01.png",
                _                   => MissingAssetPath
            };
        }

        /// <summary>
        /// Gets the asset for a prop (decorative object).
        /// </summary>
        public string GetPropAsset(string propType)
        {
            return propType switch
            {
                "torch" => "/Assets/Props/torch_01.png",
                "pillar" => "/Assets/Props/pillar_01.png",
                "barrel" => "/Assets/Props/barrel_01.png",
                "bones" => "/Assets/Props/bones_01.png",
                "skull" => "/Assets/Props/skull_01.png",
                "banner" => "/Assets/Props/banner_skull_01.png",
                _ => MissingAssetPath
            };
        }

        /// <summary>
        /// Gets the asset for a UI element.
        /// </summary>
        public string GetUiAsset(string uiElement)
        {
            return uiElement switch
            {
                "panel" => "/Assets/UI/panel_dark_01.png",
                "button_normal" => "/Assets/UI/button_normal_01.png",
                "button_hover" => "/Assets/UI/button_hover_01.png",
                "frame_gold" => "/Assets/UI/frame_gold_01.png",
                "icon_hp" => "/Assets/UI/icon_hp_01.png",
                _ => MissingAssetPath
            };
        }

        /// <summary>
        /// Gets the asset for an effect.
        /// </summary>
        public string GetEffectAsset(string effectType)
        {
            return effectType switch
            {
                "slash" => "/Assets/FX/slash_01.png",
                "hit_spark" => "/Assets/FX/hit_spark_01.png",
                "heal" => "/Assets/FX/heal_fx_01.png",
                "magic_hit" => "/Assets/FX/magic_hit_01.png",
                _ => MissingAssetPath
            };
        }

        /// <summary>
        /// Checks if an asset exists (basic check, may not be 100% accurate at runtime).
        /// </summary>
        public bool AssetExists(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
                return false;

            // In a real implementation, this could check the file system or a manifest.
            // For now, we assume all registered paths exist or have fallbacks.
            return true;
        }

        /// <summary>
        /// Returns a safe asset path or fallback if null/empty.
        /// </summary>
        public string GetSafeAssetPath(string? assetPath)
        {
            return string.IsNullOrWhiteSpace(assetPath) ? MissingAssetPath : assetPath;
        }
    }
}
