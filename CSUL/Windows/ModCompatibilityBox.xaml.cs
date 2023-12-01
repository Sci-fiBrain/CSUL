using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CSUL.Windows
{
    /// <summary>
    /// ModCompatibilityBox.xaml 的交互逻辑
    /// </summary>
    public partial class ModCompatibilityBox : Window
    {
        private class ItemData
        {
            public int Index { get; set; }
            public string Name { get; set; } = default!;
            public string Version { get; set; } = default!;
        }

        public static void ShowBox(Dictionary<int, (string, string)> allData,
            List<int> passIndex, List<int> wrongIndex, List<int> unknowIndex)
        {
            ModCompatibilityBox box = new(allData, passIndex, wrongIndex, unknowIndex);
            box.Show();
        }

        public ModCompatibilityBox(Dictionary<int, (string name, string version)> allData,
            List<int> passIndex, List<int> wrongIndex, List<int> unknowIndex)
        {
            InitializeComponent();
            allLabel.Content = allData.Count;
            passLabel.Content = passIndex.Count;
            wrongLabel.Content = wrongIndex.Count;
            unknowLabel.Content = unknowIndex.Count;
            modData = new List<ItemData>[4]
            {
                allData.Select(x => new ItemData{Index = x.Key + 1, Name = x.Value.name, Version = x.Value.version}).ToList(),
                passIndex.Select(x => new ItemData{Index = x + 1, Name = allData[x].name, Version = allData[x].version}).ToList(),
                wrongIndex.Select(x => new ItemData { Index = x + 1, Name = allData[x].name, Version = allData[x].version}).ToList(),
                unknowIndex.Select(x => new ItemData { Index = x + 1, Name = allData[x].name, Version = allData[x].version }).ToList()
            };
            listView.ItemsSource = modData[0];
        }

        #region ---私有字段---

        private readonly List<ItemData>[] modData;

        #endregion ---私有字段---

        #region ---私有方法---

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ChangeToAll(object sender, MouseButtonEventArgs e)
        {
            listView.ItemsSource = modData[0];
        }

        private void ChangeToPass(object sender, MouseButtonEventArgs e)
        {
            listView.ItemsSource = modData[1];
        }

        private void ChangeToWrong(object sender, MouseButtonEventArgs e)
        {
            listView.ItemsSource = modData[2];
        }

        private void ChangeToUnknow(object sender, MouseButtonEventArgs e)
        {
            listView.ItemsSource = modData[3];
        }

        #endregion ---私有方法---
    }
}