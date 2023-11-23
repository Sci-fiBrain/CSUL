using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CSUL.ViewModels
{
    /// <summary>
    /// Command调用类
    /// </summary>
    class RelayCommand : ICommand
    {
        //命令执行时要调用的方法
        private readonly Action<object?> action;
        //判断是否能调用方法的方法
        private readonly Func<object?,bool>? canExecute;

        /// <summary>
        /// 实例化<see cref="RelayCommand"/实例>
        /// </summary>
        /// <param name="action">命令调用时要调用的方法</param>
        /// <param name="canExecute">判断是否能调用方法的方法</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RelayCommand(Action<object?> action, Func<object?, bool>? canExecute = null)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => action is not null || canExecute?.Invoke(parameter) is true;

        public void Execute(object? parameter) => action?.Invoke(parameter);
    }
}
