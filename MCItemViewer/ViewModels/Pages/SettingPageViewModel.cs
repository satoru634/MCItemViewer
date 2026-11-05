using CommunityToolkit.Mvvm.ComponentModel;
using MCBlockViewer.Models;
using MCItemViewer.Models;
using MCToolsCommonLib.Common;
using Wpf.Ui;
using Wpf.Ui.Appearance;

namespace MCItemViewer.ViewModels.Pages
{
    /// <summary>
    /// 設定ページの ViewModel。テーマと表示言語の選択・保存を担当する。
    /// </summary>
    /// <remarks>
    /// 表示言語は設定ファイルへの保存のみで、UI への反映（英語化）は未対応。
    /// </remarks>
    public partial class SettingPageViewModel : ObservableObject
    {
        /// <summary>
        /// 既定のテーマ（保存値が選択肢にない場合に使う）
        /// </summary>
        internal const ApplicationTheme DefaultTheme = ApplicationTheme.Dark;

        /// <summary>
        /// 既定の言語コード（保存値が選択肢にない場合に使う）
        /// </summary>
        internal const string DefaultLanguage = "ja";

        /// <summary>
        /// ツールの設定（アプリ全体で共有するシングルトン）
        /// </summary>
        private readonly Setting<ToolConfig> _config;

        /// <summary>
        /// テーマを適用するサービス
        /// </summary>
        private readonly IThemeService _themeService;

        /// <summary>
        /// テーマ選択コンボボックスに表示する選択肢
        /// </summary>
        public List<ThemeOption> ThemeList { get; } =
        [
            new(ApplicationTheme.Light, "ライト"),
            new(ApplicationTheme.Dark, "ダーク"),
        ];

        /// <summary>
        /// 言語選択コンボボックスに表示する選択肢
        /// </summary>
        public List<LanguageOption> LanguageList { get; } =
        [
            new("ja", "日本語"),
            new("en", "English"),
        ];

        /// <summary>
        /// 選択中のテーマ。変更すると即時に適用し、設定を保存する。
        /// </summary>
        [ObservableProperty]
        private ApplicationTheme _selectedTheme;

        /// <summary>
        /// 選択中の言語コード。変更すると設定を保存する。
        /// </summary>
        [ObservableProperty]
        private string _selectedLanguage;

        /// <summary>
        /// コンストラクタ。DI コンテナから設定とテーマサービスを受け取り、保存されているテーマと言語を復元する。
        /// 保存値が選択肢にない場合は既定値（ダーク / 日本語）にする。
        /// </summary>
        /// <param name="config">ツールの設定</param>
        /// <param name="themeService">テーマを適用するサービス</param>
        public SettingPageViewModel(Setting<ToolConfig> config, IThemeService themeService)
        {
            _config = config;
            _themeService = themeService;

            ApplicationTheme theme = _config.Data.WindowSetting.Theme;
            _selectedTheme = ThemeList.Any(t => t.Key == theme) ? theme : DefaultTheme;

            string language = _config.Data.Language;
            _selectedLanguage = LanguageList.Any(l => l.Key == language) ? language : DefaultLanguage;
        }

        /// <summary>
        /// テーマが変更されたときに呼び出され、テーマを適用して設定を保存する。
        /// </summary>
        /// <param name="value">変更後のテーマ</param>
        partial void OnSelectedThemeChanged(ApplicationTheme value)
        {
            _themeService.SetTheme(value);

            _config.Data.WindowSetting.Theme = value;
            _config.Save();
            return;
        }

        /// <summary>
        /// 言語が変更されたときに呼び出され、設定を保存する。
        /// </summary>
        /// <param name="value">変更後の言語コード</param>
        partial void OnSelectedLanguageChanged(string value)
        {
            _config.Data.Language = value;
            _config.Save();
            return;
        }
    }
}
