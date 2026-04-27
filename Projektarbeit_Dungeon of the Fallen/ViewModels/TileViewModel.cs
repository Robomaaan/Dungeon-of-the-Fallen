using DungeonOfTheFallen.Core.Models;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    public class TileViewModel : ViewModelBase
    {
        public Tile Model { get; }
        public int X => Model.X;
        public int Y => Model.Y;
        public string DisplayText => GetDisplayText(Model);
        public string BackgroundColor => GetBackgroundForTileType(Model.TileType);

        public TileViewModel(Tile tile)
        {
            ArgumentNullException.ThrowIfNull(tile);
            Model = tile;
        }

        public void UpdateDisplay()
        {
            OnPropertyChanged(nameof(DisplayText), nameof(BackgroundColor));
        }

        private static string GetDisplayText(Tile tile)
        {
            return tile switch
            {
                { HasPlayer: true } => "P",
                { Enemy: not null } => "E",
                { Npc: not null } => "N",
                { Item: KeyItem } => "K",
                { Item: not null } => "I",
                { TileType: TileType.LockedDoor } => "D",
                { TileType: TileType.Puzzle } => "?",
                { TileType: TileType.KeyPedestal } => "⌘",
                { TileType: TileType.Wall } => "█",
                _ => " "
            };
        }

        private static string GetBackgroundForTileType(TileType type)
        {
            return type switch
            {
                TileType.Wall => "#333333",
                TileType.Floor => "#1a1a1a",
                TileType.AshFloor => "#2c2422",
                TileType.SandFloor => "#2d2a1f",
                TileType.CloudFloor => "#e4eefc",
                TileType.CursedFloor => "#241b2f",
                TileType.Exit => "#FFD700",
                TileType.Spawn => "#00AA00",
                TileType.Trap => "#AA0000",
                TileType.HealingRoom => "#0088FF",
                TileType.ThornTrap => "#2f6b2f",
                TileType.CurseTrap => "#6a1b9a",
                TileType.LavaTrap => "#c4471c",
                TileType.SpikeTrap => "#607d8b",
                TileType.DivineTrap => "#d4af37",
                TileType.HealingShrine => "#4c9f70",
                TileType.HealingAltar => "#6f5aa5",
                TileType.HotSpring => "#ff8f66",
                TileType.HealingBubble => "#5ec8ff",
                TileType.LightCircle => "#fff4b0",
                TileType.LockedDoor => "#7b5a30",
                TileType.Puzzle => "#5a4fcf",
                TileType.KeyPedestal => "#346b7a",
                _ => "#1a1a1a"
            };
        }
    }
}
