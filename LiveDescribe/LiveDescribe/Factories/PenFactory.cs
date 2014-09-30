using System.Windows.Media;

namespace LiveDescribe.Factories
{
    public class PenFactory
    {
        public static Pen BlackLinePen()
        {
            var pen = new Pen(Brushes.Black, 1);
            pen.Freeze();
            return pen;
        }
    }
}
