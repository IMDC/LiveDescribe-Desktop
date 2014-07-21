using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LiveDescribe.Utilities
{
    public static class CustomResources
    {
        public static void LoadResources()
        {
            var cursfile = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Cursors/grab.cur"));
            GrabCursor = new Cursor(cursfile.Stream);
            cursfile = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Cursors/grabbing.cur"));
            GrabbingCursor = new Cursor(cursfile.Stream);
            
            Play = new Image();
            Play.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/play.png"));

            Pause = new Image();
            Pause.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/pause.png"));

            Mute = new Image();
            Mute.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/mute.png"));

            UnMute = new Image();
            UnMute.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/unmute.png"));
        }

        public static Cursor GrabbingCursor { get; private set; }
        public static Cursor GrabCursor { get; private set; }
        public static Image Play { get; private set; }
        public static Image Pause { get; private set; }
        public static Image Mute { get; private set; }
        public static Image UnMute { get; private set; }
    }
}
