using DungeonOfTheFallen.Core.Models;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    /// <summary>
    /// ViewModel für eine einzelne Tile im Dungeon-Grid
    /// </summary>
    public class TileViewModel : ViewModelBase
    {
        private Tile _model;
        private string _displayText;
        private string _backgroundColor;

        public Tile Model => _model;
        public int X => _model.X;
        public int Y => _model.Y;

        public string DisplayText
        {
            get => _displayText;
            set => SetProperty(ref _displayText, value);
        }

        public string BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }

        public TileViewModel(Tile tile)
        {
            _model = tile;
            _displayText = "";
            _backgroundColor = GetBackgroundForTileType(tile.TileType);
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            // Farbe aktualisieren
            BackgroundColor = GetBackgroundForTileType(_model.TileType);

            // Anzeige aktualisieren
            if (_model.HasPlayer)
            {
                DisplayText = "P";
            }
            else if (_model.Enemy != null)
            {
                DisplayText = "E";
            }
            else if (_model.Item != null)
            {
                DisplayText = "I";
            }
            else
            {
                DisplayText = _model.TileType == TileType.Wall ? "█" : " ";
            }
        }

        private string GetBackgroundForTileType(TileType type)
        {
            return type switch
            {
                TileType.Wall => "#333333",      // Dunkelgrau
                TileType.Floor => "#1a1a1a",      // Fast schwarz
                TileType.Exit => "#FFD700",        // Gold
                TileType.Spawn => "#00AA00",       // Grün
                TileType.Trap => "#AA0000",        // Rot
                TileType.HealingRoom => "#0088FF", // Blau
                _ => "#1a1a1a"
            };
        }
    }
}
