using CSUL.Models;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSUL.ViewModels
{
    /// <summary>
    /// ViewModel的基类
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
#pragma warning disable CA1822

        /// <summary>
        /// 获取当前CSUL版本
        /// </summary>
        public Version? CsulVersion { get => Assembly.GetExecutingAssembly().GetName().Version; }

#pragma warning restore CA1822

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