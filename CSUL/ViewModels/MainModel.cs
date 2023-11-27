using CSUL.Views;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace CSUL.ViewModels
{
    /// <summary>
    /// MainWindow的ViewModel
    /// </summary>
    internal class MainModel : BaseViewModel
    {
        public MainModel()
        {
            ViewContents = new()
            {   //View列表
                new PlayView(),
                new MapView(),
                new SaveView(),
                new ModView(),
                new SetView()
            };
            viewContent = ViewContents[0];
            ViewCommand = new RelayCommand((x) =>
            {
                //判断传参x是否符合条件
                if (x is not string text || !int.TryParse(text, out int num)) throw new ArgumentNullException(nameof(x));
                ChangeViewContent(num);
            });
        }

        #region ---公共属性---

        private UserControl viewContent;

        /// <summary>
        /// 当前显示的页面
        /// </summary>
        public UserControl ViewContent
        {
            get => viewContent;
            set
            {
                viewContent = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 页面列表
        /// </summary>
        public List<UserControl> ViewContents { get; }

        public ICommand ViewCommand { get; }

        #endregion ---公共属性---

        #region ---私有方法---

        /// <summary>
        /// 改变当前显示的页面
        /// </summary>
        /// <param name="index">目标页面索引</param>
        private void ChangeViewContent(int index)
        {
            ViewContent = ViewContents[index];
        }

        #endregion ---私有方法---
    }
}