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
                { Item: not null } => "I",
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
                TileType.Exit => "#FFD700",
                TileType.Spawn => "#00AA00",
                TileType.Trap => "#AA0000",
                TileType.HealingRoom => "#0088FF",
                _ => "#1a1a1a"
            };
        }
    }
}
