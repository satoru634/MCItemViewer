using MCBlockViewer.Models;
using MCItemViewer.Services;
using MCItemViewer.ViewModels.Pages;
using MCItemViewer.ViewModels.Windows;
using MCItemViewer.Views.Pages;
using MCItemViewer.Views.Windows;
using MCToolsCommonLib.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;

namespace MCBlockViewer
{
    /// <summary>
    /// アプリケーションのエントリポイント。
    /// .NET Generic Host を用いて DI コンテナを構築し、ホストのライフサイクルを管理する。
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// .NET Generic Host（DI・設定・ライフサイクル管理を提供する）
        /// </summary>
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(AppContext.BaseDirectory); })
            .ConfigureServices((context, services) =>
            {
                // 設定ファイル（カレントディレクトリの tool_settings.json）を各 ViewModel で共有する。
                // 読み込みは最初に要求されたとき（メインウィンドウの生成時）に行う
                services.AddSingleton(_ => new Setting<ToolConfig>(Path.Combine(Directory.GetCurrentDirectory(), "tool_settings.json")));

                // NavigationView が遷移先のページを DI コンテナから取得するためのプロバイダー
                services.AddNavigationViewPageProvider();

                // アプリ起動時にメインウィンドウを表示するホステッドサービス
                services.AddHostedService<ApplicationHostService>();

                // メインウィンドウ（ナビゲーションのホスト）
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();

                // 各ページと ViewModel
                services.AddSingleton<MainPage>();
                services.AddSingleton<MainPageViewModel>();
            }).Build();

        /// <summary>
        /// アプリケーションの起動時に呼び出され、ホストを開始する。
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();
        }

        /// <summary>
        /// アプリケーションの終了時に呼び出され、ホストを停止して破棄する。
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// ハンドルされない例外が発生したときに呼び出される。
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // 必要に応じてここでエラーダイアログ表示やログ記録を行う
        }
    }
}
