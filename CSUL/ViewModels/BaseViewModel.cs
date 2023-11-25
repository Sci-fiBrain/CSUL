using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSUL.ViewModels
{
    /// <summary>
    /// ViewModel的基类
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 指示属性发生改变的事件
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 指示属性值发生了改变
        /// </summary>
        /// <param name="propertyName"></param>
        internal void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChangedEventArgs args = new(propertyName);
            PropertyChanged?.Invoke(this, args);
        }
    }
}