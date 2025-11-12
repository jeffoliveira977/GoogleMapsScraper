using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GoogleMapsScraper.Utils
{
    // Source - https://stackoverflow.com/a
    // Posted by Rohit Vats, modified by community. See post 'Timeline' for change history
    // Retrieved 2025-11-08, License - CC BY-SA 4.0

    // A classe precisa estar acessível pelo seu ViewModel
    public class RelayCommand : ICommand
    {
        // Variáveis Action (métodos) para armazenar os delegados a serem executados
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// Inicializa uma nova instância do RelayCommand.
        /// </summary>
        /// <param name="execute">A lógica de execução.</param>
        /// <param name="canExecute">A lógica de status de execução (opcional).</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            // O comando deve ter uma ação para executar
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Evento acionado quando o status CanExecute muda.
        /// Requer um método no ViewModel para ser chamado (por exemplo, CommandManager.InvalidateRequerySuggested()).
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Define se o comando pode ser executado.
        /// </summary>
        /// <param name="parameter">O parâmetro de dados passado pela View (XAML).</param>
        /// <returns>True se o comando pode ser executado; caso contrário, false.</returns>
        public bool CanExecute(object? parameter)
        {
            // Retorna o resultado de _canExecute, ou True por padrão.
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Executa a lógica do comando.
        /// </summary>
        /// <param name="parameter">O parâmetro de dados passado pela View (XAML).</param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
