using System.Windows.Input;

namespace LiveDescribe.Extensions
{
    public static class CommandExtensions
    {
        /// <summary>
        /// Wrapper for ICommand.CanExecute method. Calls the CaneExecute method with a null parameter
        /// </summary>
        /// <param name="command">Command to execute.</param>
        public static bool CanExecute(this ICommand command)
        {
            return command.CanExecute(null);
        }

        /// <summary>
        /// Wrapper for ICommand.Execute method. Calls the execute method with a null parameter
        /// </summary>
        /// <param name="command">Command to execute.</param>
        public static void Execute(this ICommand command)
        {
            command.Execute(null);
        }

        /// <summary>
        /// Will call the Execute method for a command, iff the command can execute.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        public static void ExecuteIfCan(this ICommand command)
        {
            if (command.CanExecute())
                command.Execute();
        }
    }
}
