using MCItemViewer.Views.Pages;
using MCItemViewer.Views.Windows;
using Microsoft.Extensions.Hosting;
using System.Windows;
using Wpf.Ui;

namespace MCItemViewer.Services
{
    /// <summary>
    /// アプリケーションのホステッドサービス。
    /// ホストの起動時にメインウィンドウを表示し、メインページへ遷移する。
    /// </summary>
    public class ApplicationHostService : IHostedService
    {
        /// <summary>
        /// DI コンテナのサービスプロバイダー
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// DI コンテナからサービスプロバイダーを受け取って初期化する。
        /// </summary>
        /// <param name="serviceProvider">サービスプロバイダー</param>
        public ApplicationHostService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
        /// メインウィンドウがまだ表示されていなければ生成して表示し、メインページへ遷移する。
        /// </summary>
        private async Task HandleActivationAsync()
        {
            if (!Application.Current.Windows.OfType<MainWindow>().Any())
            {
                INavigationWindow navigationWindow = (INavigationWindow)_serviceProvider.GetService(typeof(INavigationWindow))!;
                navigationWindow.ShowWindow();
                navigationWindow.Navigate(typeof(MainPage));
            }

            await Task.CompletedTask;
        }
    }
}
