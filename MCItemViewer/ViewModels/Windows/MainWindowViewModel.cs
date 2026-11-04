using CommunityToolkit.Mvvm.ComponentModel;
using MCBlockViewer.Models;
using MCToolsCommonLib.Common;

namespace MCItemViewer.ViewModels.Windows
{
    /// <summary>
    /// メインウィンドウ（ナビゲーションのホスト）の ViewModel。
    /// タイトルの表示と、ナビゲーションペインの開閉状態の保持・保存を担当する。
    /// </summary>
    /// <remarks>
    /// ナビゲーション項目（NavigationViewItem）は STA スレッドでしか生成できず、ViewModel で持つとテストできないため、
    /// MainWindow.xaml 側で定義している。
    /// </remarks>
    public partial class MainWindowViewModel : ObservableObject
    {
        /// <summary>
        /// ツールの設定（アプリ全体で共有するシングルトン）
        /// </summary>
        private readonly Setting<ToolConfig> _config;

        /// <summary>
        /// タイトルバーに表示するアプリケーション名
        /// </summary>
        [ObservableProperty]
        private string _applicationTitle = "MCItemViewer";

        /// <summary>
        /// ナビゲーションペインが開いているかどうか
        /// </summary>
        [ObservableProperty]
        private bool _isPaneOpen;

        /// <summary>
        /// コンストラクタ。DI コンテナから設定を受け取り、保存されているペインの開閉状態を復元する。
        /// </summary>
        /// <param name="config">ツールの設定</param>
        public MainWindowViewModel(Setting<ToolConfig> config)
        {
            _config = config;
            _isPaneOpen = _config.Data.WindowSetting.IsPaneOpen;
        }

        /// <summary>
        /// ナビゲーションペインの開閉状態が変更されたときに呼び出され、設定を保存する。
        /// </summary>
        /// <param name="value">変更後の開閉状態</param>
        partial void OnIsPaneOpenChanged(bool value)
        {
            _config.Data.WindowSetting.IsPaneOpen = value;
            _config.Save();
            return;
        }
    }
}
