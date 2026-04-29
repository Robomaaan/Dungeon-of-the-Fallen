using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    public static class NpcInteractionService
    {
        public static void Interact(GameState gameState, Npc npc)
        {
            if (npc.HasInteractedOnFloor)
            {
                gameState.AddCombatLogEntry($"[NSC] {npc.Name}: Ich habe dir auf dieser Ebene bereits geholfen.");
                return;
            }

            npc.HasInteractedOnFloor = true;
            gameState.AddCombatLogEntry($"[NSC] {npc.Name}: {npc.Greeting}");

            switch (npc.NpcType)
            {
                case NpcType.Healer:
                    var heal = Math.Min(16 + gameState.CurrentFloor * 5, gameState.Player.MaxHP - gameState.Player.HP);
                    if (heal > 0)
                    {
                        gameState.Player.HP += heal;
                        gameState.AddCombatLogEntry($"[NSC] +{heal} LP durch Heilsegen.");
                    }
                    else
                    {
                        gameState.Player.Inventory.Add(new Potion("Gesegneter Trank", 30));
                        gameState.AddCombatLogEntry("[NSC] Deine Kräfte sind voll. Du erhältst stattdessen einen gesegneten Trank.");
                    }
                    break;
                case NpcType.Merchant:
                    if (gameState.Player.Gold >= 40)
                    {
                        gameState.Player.Gold -= 40;
                        gameState.Player.Inventory.Add(new Potion("Händlertrank", 35));
                        gameState.AddCombatLogEntry("[NSC] 40 Gold gegen einen starken Trank getauscht.");
                    }
                    else
                    {
                        gameState.AddCombatLogEntry("[NSC] Sammle mehr Gold, dann öffne ich meine besten Waren.");
                    }
                    break;
                case NpcType.Chronicler:
                    var chroniclerMessages = new List<string>();
                    PlayerProgressionService.GrantXp(gameState.Player, 70 + (gameState.CurrentFloor * 25), chroniclerMessages);
                    gameState.AddCombatLogEntry("[NSC] Der Chronist offenbart Hinweise auf Schlösser und Rätsel dieser Ebene.");
                    foreach (var message in chroniclerMessages)
                        gameState.AddCombatLogEntry(message);
                    break;
                case NpcType.Blacksmith:
                    gameState.Player.Attack += 1;
                    gameState.Player.Defense += 1;
                    gameState.AddCombatLogEntry("[NSC] Die Schmiedin schärft deine Waffe und verstärkt deine Rüstung (+1 Angriff, +1 RK).");
                    break;
                case NpcType.Scout:
                    gameState.AddCombatLogEntry($"[NSC] Noch {gameState.RemainingFloorEnemies} Gegner trennen dich vom geöffneten Ausgang.");
                    var unsolvedPuzzle = gameState.Puzzles.FirstOrDefault(p => !p.Solved);
                    if (unsolvedPuzzle != null)
                        gameState.AddCombatLogEntry($"[NSC] Rätselhinweis: {unsolvedPuzzle.Hint}");
                    break;
                case NpcType.Mystic:
                    var bonusHeal = Math.Min(8 + gameState.CurrentFloor * 3, gameState.Player.MaxHP - gameState.Player.HP);
                    gameState.Player.HP += bonusHeal;
                    gameState.Player.DamageModifiers.Resistances.Add(DamageType.Necrotic);
                    gameState.AddCombatLogEntry($"[NSC] Arkane Segnung: +{bonusHeal} LP und Nekrotik-Resistenz für diesen Lauf.");
                    break;
            }
        }
    }
}
