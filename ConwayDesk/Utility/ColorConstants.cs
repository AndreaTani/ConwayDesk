using System.Windows.Media;

namespace ConwayDesk
{
    public class ColorConstants
    {
        public static readonly SolidColorBrush MediumGray = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
        public static readonly SolidColorBrush DarkGrey = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
        public static readonly SolidColorBrush Orange = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FC6A03"));
        public static readonly SolidColorBrush White = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));

        public static readonly SolidColorBrush ActiveCell = Orange;
        public static readonly SolidColorBrush InactiveCell = DarkGrey;
        public static readonly SolidColorBrush WindowBackground = MediumGray;
        public static readonly SolidColorBrush HeaderBackground = DarkGrey;
        public static readonly SolidColorBrush GameAreaBackground = DarkGrey;
        public static readonly SolidColorBrush CellBorder = MediumGray;

        public static readonly SolidColorBrush ForegroundTextBrush = White;
    }
}
