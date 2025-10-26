using System.Windows.Input;

namespace ConwayDesk
{
    /// <summary>
    /// Represents a command that can be executed in the context of this 
    /// application, with the ability to determine whether the command can 
    /// execute based on a specified condition.
    /// </summary>
    /// <remarks>The <see cref="RelayCommand"/> class is commonly used in MVVM 
    /// applications to bind commands to UI elements. It allows the execution 
    /// logic and the condition for execution to be specified via delegates.
    /// </remarks>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}