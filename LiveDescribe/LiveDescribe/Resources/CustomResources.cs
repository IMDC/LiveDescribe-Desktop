using NAudio.Wave;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LiveDescribe.Resources
{
    public static class CustomResources
    {
        public static void LoadResources()
        {
            LoadCursors();
            LoadIcons();
        }

        private static void LoadIcons()
        {
            Play = new Image {Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/play.png"))};

            Pause = new Image {Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/pause.png"))};

            Mute = new Image {Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/mute.png"))};

            UnMute = new Image {Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/unmute.png"))};

            Record = new Image {Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/record.png"))};

            StopRecord = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/stop.png"))
            };

            BeginSpace = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/beginspace.png"))
            };

            EndSpace = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/endspace.png"))
            };
        }

        private static void LoadCursors()
        {
            var cursfile = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Cursors/grab.cur"));
            GrabCursor = new Cursor(cursfile.Stream);
            cursfile = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Cursors/grabbing.cur"));
            GrabbingCursor = new Cursor(cursfile.Stream);
        }

        public static Cursor GrabbingCursor { get; private set; }
        public static Cursor GrabCursor { get; private set; }
        public static Image Play { get; private set; }
        public static Image Pause { get; private set; }
        public static Image Mute { get; private set; }
        public static Image UnMute { get; private set; }
        public static Image Record { get; private set; }
        public static Image StopRecord { get; private set; }
        public static Image BeginSpace { get; private set; }
        public static Image EndSpace { get; private set; }

        /* Note: Make sure to set the "Copy to Output Folder" when using sound effects.
         */
        public static WaveOut Beep
        {
            get
            {
                var readerBeep = new WaveFileReader("Resources/SoundEffects/beep.wav");
                var beep = new WaveOut();
                beep.Init(readerBeep);
                return beep;
            }
        }

        public static WaveOut LongBeep
        {
            get
            {
                var readerEndBeep = new WaveFileReader("../../Resources/SoundEffects/finished-beep.wav");
                var longbeep = new WaveOut();
                longbeep.Init(readerEndBeep);
                return longbeep;
            }
        }
    }
}
