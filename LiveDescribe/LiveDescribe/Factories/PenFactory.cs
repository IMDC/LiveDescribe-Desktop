using System.Windows.Media;

namespace LiveDescribe.Factories
{
    public class PenFactory
    {
        public static Pen LinePen(Brush lineBrush)
        {
            var pen = new Pen(lineBrush, 1);
            pen.Freeze();
            return pen;
        }
    }
}
