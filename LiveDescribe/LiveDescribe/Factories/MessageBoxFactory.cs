using LiveDescribe.Resources.UiStrings;
using System.Windows;

namespace LiveDescribe.Factories
{
    /// <summary>
    /// Contains specific message boxes
    /// </summary>
    public static class MessageBoxFactory
    {
        /// <summary>
        /// Creates and shows a MessageBox meant to display an error message.
        /// </summary>
        /// <param name="errorMessage">Error message to display.</param>
        /// <returns>The option the user selected on the MessageBox.</returns>
        public static MessageBoxResult ShowError(string errorMessage)
        {
            return MessageBox.Show(errorMessage, UiStrings.MessageBox_Title_Error, MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        /// <summary>
        /// Creates and shows a MessageBox meant to warn the user about something, and ask for
        /// confirmation to do it. IE "Do you want to save this project before exiting?"
        /// </summary>
        /// <param name="warningMessage"></param>
        /// <returns></returns>
        public static MessageBoxResult ShowWarningQuestion(string warningMessage)
        {
            return MessageBox.Show(warningMessage, UiStrings.MessageBox_Title_ConfirmationWarning,
                MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        }
    }
}
