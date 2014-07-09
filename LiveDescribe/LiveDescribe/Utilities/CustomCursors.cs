using System;
using System.Windows;
using System.Windows.Input;

namespace LiveDescribe.Utilities
{
    public static class CustomCursors
    {
        public static void CreateCustomCursors()
        {
            var cursfile = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Cursors/grab.cur"));
            GrabCursor = new Cursor(cursfile.Stream);
            cursfile = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Cursors/grabbing.cur"));
            GrabbingCursor = new Cursor(cursfile.Stream);
        }

        public static Cursor GrabbingCursor { get; private set; }
        public static Cursor GrabCursor { get; private set; }
    }
}
