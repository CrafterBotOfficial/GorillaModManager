using Avalonia.Controls;
using Avalonia.Interactivity;
using GorillaModManager.ViewModels;
using MsBox.Avalonia.ViewModels.Commands;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace GorillaModManager.Views
{
    public partial class ModBrowser : UserControl
    {
        public ModBrowser()
        {
            InitializeComponent();
            DataContext = new ModBrowserViewModel();
            SearchIndex.IsReadOnly = true;
        }

        public void OnPageClick(object sender, RoutedEventArgs e)
        {
            ((ModBrowserViewModel)DataContext).OnPageChanged(((Button)sender).Name);
        }
    }
}
