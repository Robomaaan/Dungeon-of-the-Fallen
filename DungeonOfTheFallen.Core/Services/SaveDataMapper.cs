using DungeonOfTheFallen.Core.Models;
using DungeonOfTheFallen.Core.Persistence;

namespace DungeonOfTheFallen.Core.Services
{
    public static class SaveDataMapper
    {
        public static SaveData ToSaveData(GameState gameState)
        {
            return new SaveData
            {
                SaveVersion = SaveConstants.CurrentSaveVersion,
                PlayerName = gameState.Player.Name,
                PlayerClass = gameState.Player.PlayerClass,
                CurrentFloor = gameState.CurrentFloor,
                CurrentBiome = gameState.CurrentBiome,
                PlayerPositionX = gameState.Player.PositionX,
                PlayerPositionY = gameState.Player.PositionY,
                PlayerHP = gameState.Player.HP,
                PlayerMaxHP = gameState.Player.MaxHP,
                PlayerAttack = gameState.Player.Attack,
                PlayerDefense = gameState.Player.Defense,
                PlayerXP = gameState.Player.XP,
                PlayerLevel = gameState.Player.Level,
                PlayerGold = gameState.Player.Gold,
                PotionCount = gameState.Player.Inventory.Items.Count(i => i.ItemType == ItemType.Potion),
                PlayerWeaponName = gameState.Player.Weapon.Name,
                PlayerWeaponAttackBonus = gameState.Player.Weapon.AttackBonus,
                PlayerWeaponArmorClassBonus = gameState.Player.Weapon.ArmorClassBonus,
                PlayerWeaponDamageCount = gameState.Player.Weapon.Damage.Count,
                PlayerWeaponDamageDie = gameState.Player.Weapon.Damage.DieSize,
                PlayerWeaponDamageBonus = gameState.Player.Weapon.Damage.Bonus,
                PlayerWeaponDamageType = gameState.Player.Weapon.Damage.DamageType,
                FloorObjectiveTarget = gameState.FloorObjectiveTarget,
                EnemiesDefeatedOnFloor = gameState.EnemiesDefeatedOnFloor,
                BossDefeatedOnFloor = gameState.BossDefeatedOnFloor,
                ExitUnlocked = gameState.ExitUnlocked,
                CollectedKeyIds = gameState.CollectedKeyIds.ToList(),
                Puzzles = gameState.Puzzles.Select(p => new PuzzleSaveData { PuzzleId = p.PuzzleId, Solved = p.Solved }).ToList(),
                Enemies = gameState.Enemies
                    .Where(e => e.IsAlive)
                    .Select(e => new EnemySaveData
                    {
                        Name = e.Name,
                        EnemyType = e.EnemyType,
                        Tier = e.Tier,
                        Role = e.Role,
                        PositionX = e.PositionX,
                        PositionY = e.PositionY,
                        HP = e.HP,
                        MaxHP = e.MaxHP,
                        Attack = e.Attack,
                        Defense = e.Defense,
                        IsBoss = e.IsBoss,
                        GoldReward = e.GoldReward,
                        XpReward = e.XpReward,
                        WeaponName = e.Weapon.Name,
                        WeaponAttackBonus = e.Weapon.AttackBonus,
                        WeaponArmorClassBonus = e.Weapon.ArmorClassBonus,
                        WeaponDamageCount = e.Weapon.Damage.Count,
                        WeaponDamageDie = e.Weapon.Damage.DieSize,
                        WeaponDamageBonus = e.Weapon.Damage.Bonus,
                        WeaponDamageType = e.Weapon.Damage.DamageType
                    })
                    .ToList(),
                Npcs = gameState.Npcs
                    .Select(n => new NpcSaveData
                    {
                        Name = n.Name,
                        NpcType = n.NpcType,
                        PositionX = n.PositionX,
                        PositionY = n.PositionY,
                        HasInteractedOnFloor = n.HasInteractedOnFloor,
                        Greeting = n.Greeting
                    })
                    .ToList()
            };
        }

        public static void ApplyToGameState(GameState gameState, SaveData data)
        {
            gameState.Player.Name = data.PlayerName;
            gameState.Player.PlayerClass = data.PlayerClass;
            gameState.Player.ClassSkill = PlayerClassService.GetProfile(data.PlayerClass).ClassSkill;
            gameState.CurrentFloor = data.CurrentFloor;
            gameState.CurrentBiome = data.CurrentBiome;
            gameState.FloorObjectiveTarget = data.FloorObjectiveTarget;
            gameState.EnemiesDefeatedOnFloor = data.EnemiesDefeatedOnFloor;
            gameState.BossDefeatedOnFloor = data.BossDefeatedOnFloor;
            gameState.ExitUnlocked = data.ExitUnlocked;
            gameState.Player.HP = data.PlayerHP;
            gameState.Player.MaxHP = data.PlayerMaxHP;
            gameState.Player.Attack = data.PlayerAttack;
            gameState.Player.Defense = data.PlayerDefense;
            gameState.Player.XP = data.PlayerXP;
            gameState.Player.Level = data.PlayerLevel;
            gameState.Player.Gold = data.PlayerGold;
            gameState.Player.PositionX = data.PlayerPositionX;
            gameState.Player.PositionY = data.PlayerPositionY;
            gameState.Player.Weapon = new WeaponProfile
            {
                Name = data.PlayerWeaponName,
                AttackBonus = data.PlayerWeaponAttackBonus,
                ArmorClassBonus = data.PlayerWeaponArmorClassBonus,
                Damage = new DamageRoll
                {
                    Count = data.PlayerWeaponDamageCount,
                    DieSize = data.PlayerWeaponDamageDie,
                    Bonus = data.PlayerWeaponDamageBonus,
                    DamageType = data.PlayerWeaponDamageType
                }
            };

            gameState.Player.Inventory.Clear();
            for (var i = 0; i < data.PotionCount; i++)
                gameState.Player.Inventory.Add(new Potion("Health Potion", 25));

            gameState.CollectedKeyIds.Clear();
            foreach (var keyId in data.CollectedKeyIds)
                gameState.AddKey(keyId);

            foreach (var enemy in gameState.Enemies)
            {
                var tile = gameState.Map.GetTile(enemy.PositionX, enemy.PositionY);
                if (tile != null && tile.Enemy == enemy)
                    tile.Enemy = null;
            }

            gameState.Enemies.Clear();
            foreach (var enemyData in data.Enemies)
            {
                var enemy = new Enemy(enemyData.Name, enemyData.EnemyType, enemyData.MaxHP, enemyData.Attack, enemyData.Defense, enemyData.IsBoss)
                {
                    PositionX = enemyData.PositionX,
                    PositionY = enemyData.PositionY,
                    HP = enemyData.HP,
                    GoldReward = enemyData.GoldReward,
                    XpReward = enemyData.XpReward,
                    Tier = enemyData.Tier,
                    Role = enemyData.Role,
                    Weapon = new WeaponProfile
                    {
                        Name = enemyData.WeaponName,
                        AttackBonus = enemyData.WeaponAttackBonus,
                        ArmorClassBonus = enemyData.WeaponArmorClassBonus,
                        Damage = new DamageRoll
                        {
                            Count = enemyData.WeaponDamageCount,
                            DieSize = enemyData.WeaponDamageDie,
                            Bonus = enemyData.WeaponDamageBonus,
                            DamageType = enemyData.WeaponDamageType
                        }
                    }
                };

                gameState.Enemies.Add(enemy);
                var enemyTile = gameState.Map.GetTile(enemy.PositionX, enemy.PositionY);
                if (enemyTile != null)
                    enemyTile.Enemy = enemy;
            }

            foreach (var npc in gameState.Npcs)
            {
                var tile = gameState.Map.GetTile(npc.PositionX, npc.PositionY);
                if (tile != null && tile.Npc == npc)
                    tile.Npc = null;
            }

            gameState.Npcs.Clear();
            foreach (var npcData in data.Npcs)
            {
                var npc = new Npc
                {
                    Name = npcData.Name,
                    NpcType = npcData.NpcType,
                    PositionX = npcData.PositionX,
                    PositionY = npcData.PositionY,
                    HasInteractedOnFloor = npcData.HasInteractedOnFloor,
                    Greeting = npcData.Greeting
                };

                gameState.Npcs.Add(npc);
                var npcTile = gameState.Map.GetTile(npc.PositionX, npc.PositionY);
                if (npcTile != null)
                    tileSetNpc(npcTile, npc);
            }

            foreach (var puzzle in gameState.Puzzles)
            {
                var saved = data.Puzzles.FirstOrDefault(p => p.PuzzleId == puzzle.PuzzleId);
                puzzle.Solved = saved?.Solved ?? puzzle.Solved;
            }

            for (var y = 0; y < gameState.Map.Height; y++)
            for (var x = 0; x < gameState.Map.Width; x++)
            {
                var tile = gameState.Map.GetTile(x, y);
                if (tile != null)
                    tile.HasPlayer = false;
            }

            var playerTile = gameState.Map.GetTile(gameState.Player.PositionX, gameState.Player.PositionY);
            if (playerTile != null)
                playerTile.HasPlayer = true;
        }

        private static void tileSetNpc(Tile tile, Npc npc) => tile.Npc = npc;
    }
}
