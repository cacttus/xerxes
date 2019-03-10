using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BuildGui
{
    public class UiBrushes
    {

        public static Brush Brush_OkColor_Output = Brushes.LightGreen;
        public static Brush Brush_WarnColor_Output = Brushes.Khaki;// Lighter than basic yellow
        public static Brush Brush_FailColor_Output = Brushes.PeachPuff;//Ligher than basic Red
        public static Brush Brush_Foreground_Output = Brushes.Black;
        public static Brush Brush_BuildColor_Window = Brushes.AliceBlue;

        public static Brush Brush_OkColor_Window = Brushes.LightGreen;
        public static Brush Brush_WarnColor_Window = Brushes.Yellow;
        public static Brush Brush_FailColor_Window = Brushes.Red;

    }
}
