using MCBlockViewer.Models;
using MCItemViewer.Views.Pages;
using MCItemViewer.Views.Windows;
using MCToolsCommonLib.Common;
using Microsoft.Extensions.Hosting;
using System.Windows;
using Wpf.Ui;

namespace MCItemViewer.Services
{
    /// <summary>
    /// アプリケーションのホステッドサービス。
    /// ホストの起動時に保存されているテーマを適用してからメインウィンドウを表示し、メインページへ遷移する。
    /// </summary>
    public class ApplicationHostService : IHostedService
    {
        /// <summary>
        /// DI コンテナのサービスプロバイダー
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// ツールの設定
        /// </summary>
        private readonly Setting<ToolConfig> _config;

        /// <summary>
        /// テーマを適用するサービス
        /// </summary>
        private readonly IThemeService _themeService;

        /// <summary>
        /// DI コンテナからサービスプロバイダー・設定・テーマサービスを受け取って初期化する。
        /// </summary>
        /// <param name="serviceProvider">サービスプロバイダー</param>
        /// <param name="config">ツールの設定</param>
        /// <param name="themeService">テーマを適用するサービス</param>
        public ApplicationHostService(IServiceProvider serviceProvider, Setting<ToolConfig> config, IThemeService themeService)
        {
            _serviceProvider = serviceProvider;
            _config = config;
            _themeService = themeService;
        }

        /// <summary>
        /// ホストの起動時に呼び出され、メインウィンドウを表示する。
        /// </summary>
        /// <param name="cancellationToken">起動処理の中断を通知するトークン</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await HandleActivationAsync();
        }

        /// <summary>
        /// ホストの終了時に呼び出される。
        /// </summary>
        /// <param name="cancellationToken">終了処理の中断を通知するトークン</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// メインウィンドウがまだ表示されていなければ、保存されているテーマを適用してから生成・表示し、メインページへ遷移する。
        /// </summary>
        private async Task HandleActivationAsync()
        {
            if (!Application.Current.Windows.OfType<MainWindow>().Any())
            {
                _themeService.SetTheme(_config.Data.WindowSetting.Theme);

                INavigationWindow navigationWindow = (INavigationWindow)_serviceProvider.GetService(typeof(INavigationWindow))!;
                navigationWindow.ShowWindow();
                navigationWindow.Navigate(typeof(MainPage));
            }

            await Task.CompletedTask;
        }
    }
}
