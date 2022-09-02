using System.Windows.Media;
using System.Windows.Shapes;

namespace TetrisClient.rectangle
{
    internal class CustomRectangle
    {
        public Rectangle Rectangle { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public SolidColorBrush Color { get; private set; }

        public CustomRectangle(SolidColorBrush color, int y, int x )
        {
            this.Rectangle = new Rectangle()
            {
                Width = 25, // Breedte van een 'cell' in de Grid
                Height = 25, // Hoogte van een 'cell' in de Grid
                Stroke = Brushes.Black, // De rand
                StrokeThickness = 1, // Dikte van de rand
                Fill = color, // Achtergrondkleur
            };
            this.X = x;
            this.Y = y;
            this.Color = color;
        }
    }
}
