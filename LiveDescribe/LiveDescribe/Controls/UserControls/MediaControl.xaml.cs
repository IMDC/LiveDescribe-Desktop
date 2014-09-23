using System.Windows.Controls;
using System.Windows.Input;

namespace LiveDescribe.Controls.UserControls
{
    /// <summary>
    /// Interaction logic for MediaControl.xaml
    /// </summary>
    public partial class MediaControl : UserControl
    {
        public MediaControl()
        {
            InitializeComponent();

            //Make the command manager requery to update the video controls (Play, Pause, etc)
            VideoMedia.MediaOpened += (sender, args) => CommandManager.InvalidateRequerySuggested();
        }
    }
}
