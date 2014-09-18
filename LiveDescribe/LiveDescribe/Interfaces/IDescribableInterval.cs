using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace LiveDescribe.Interfaces
{
    /// <summary>
    /// Represents a period of time that can be described in some way
    /// </summary>
    public interface IDescribableInterval : INotifyPropertyChanged
    {
        bool IsSelected { set; get; }
        bool LockedInPlace { set; get; }
        double X { set; get; }
        double Y { set; get; }
        double Height { set; get; }
        double Width { set; get; }
        double StartInVideo { set; get; }
        double EndInVideo { set; get; }
        double Duration { get; }
        Color Colour { set; get; }
        string Text { set; get; }
        string Title { set; get; }

        event EventHandler DeleteRequested;
        event EventHandler<MouseEventArgs> MouseDown;
        event EventHandler<MouseEventArgs> MouseUp;
        event EventHandler<MouseEventArgs> MouseMove;
        event EventHandler NavigateToRequested;

        void SetStartAndEndInVideo(double startInVideo, double endInVideo);
        void MoveInterval(double startInVideo);
        void SetColour();
    }
}
