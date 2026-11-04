using MCItemViewer.ViewModels.Windows;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace MCItemViewer.Views.Windows
{
    /// <summary>
    /// アプリケーションのメインウィンドウ。
    /// <see cref="INavigationWindow"/> を実装し、NavigationView によるページ遷移の起点となる。
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        /// <summary>
        /// このウィンドウに対応する ViewModel
        /// </summary>
        public MainWindowViewModel ViewModel { get; }

        /// <summary>
        /// DI コンテナから依存サービスを受け取って初期化する。
        /// </summary>
        /// <param name="viewModel">メインウィンドウの ViewModel</param>
        /// <param name="navigationViewPageProvider">遷移先のページを DI コンテナから取得するプロバイダー</param>
        public MainWindow(MainWindowViewModel viewModel, INavigationViewPageProvider navigationViewPageProvider)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
            SetPageService(navigationViewPageProvider);
        }

        #region INavigationWindow methods

        /// <summary>
        /// ナビゲーションビューコントロールを返す。
        /// </summary>
        public INavigationView GetNavigation() => RootNavigation;

        /// <summary>
        /// 指定したページ型へ遷移する。
        /// </summary>
        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        /// <summary>
        /// ページプロバイダーをナビゲーションビューに設定する。
        /// </summary>
        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => RootNavigation.SetPageProviderService(navigationViewPageProvider);

        /// <summary>
        /// サービスプロバイダーをナビゲーションビューに設定する。
        /// </summary>
        public void SetServiceProvider(IServiceProvider serviceProvider) => RootNavigation.SetServiceProvider(serviceProvider);

        /// <summary>
        /// ウィンドウを表示する。
        /// </summary>
        public void ShowWindow() => Show();

        /// <summary>
        /// ウィンドウを閉じる。
        /// </summary>
        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// ウィンドウが閉じた後に呼び出される。このウィンドウが唯一のメインウィンドウのため、アプリケーションを終了する。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
