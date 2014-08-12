using System.Windows.Media;

namespace LiveDescribe.Interfaces
{
    /// <summary>
    /// Represents a period of time that can be described in some way
    /// </summary>
    public interface IDescribableInterval
    {
        bool IsSelected { set; get; }
        double X { set; get; }
        double Y { set; get; }
        double Height { set; get; }
        double Width { set; get; }
        double StartInVideo { set; get; }
        double EndInVideo { set; get; }
        double Duration { get; }
        Color Colour { set; get; }
        string Text { set; get; }
        void SetStartAndEndInVideo(double startInVideo, double endInVideo);
        void SetColour();
    }
}
