using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfExp.MainPage
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || (parameter is T t && _canExecute(t));
        }

        public void Execute(object parameter)
        {
            if (parameter == null)
            {
                Debug.WriteLine("CommandParameter null veya geçersiz.");
                return;
            }

            if (parameter is T t)
            {
                _execute(t);
            }
            else if (typeof(T) == typeof(int) && int.TryParse(parameter.ToString(), out var intParam))
            {
                _execute((T)(object)intParam); // Tür dönüşümü
            }
            else
            {
                throw new ArgumentException($"Invalid parameter type: {parameter.GetType()}");
            }
        }


        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }




}
