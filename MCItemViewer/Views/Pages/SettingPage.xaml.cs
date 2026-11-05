using MCItemViewer.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace MCItemViewer.Views.Pages
{
    /// <summary>
    /// 設定ページ（テーマと表示言語）の View。
    /// DataContext に自身を設定し、XAML からは ViewModel プロパティ経由で <see cref="SettingPageViewModel"/> にアクセスする。
    /// </summary>
    public partial class SettingPage : INavigableView<SettingPageViewModel>
    {
        /// <summary>
        /// このページに対応する ViewModel
        /// </summary>
        public SettingPageViewModel ViewModel { get; }

        /// <summary>
        /// DI コンテナから ViewModel を受け取って初期化する。
        /// </summary>
        /// <param name="viewModel">設定ページの ViewModel</param>
        public SettingPage(SettingPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
