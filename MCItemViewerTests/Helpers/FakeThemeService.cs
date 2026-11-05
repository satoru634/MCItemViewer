using System.Windows.Media;
using Wpf.Ui;
using Wpf.Ui.Appearance;

namespace MCItemViewerTests.Helpers
{
    /// <summary>
    /// テスト用の IThemeService。実際にはテーマを適用せず、SetTheme の呼び出しを記録する。
    /// </summary>
    /// <remarks>
    /// 本物の ThemeService は WPF の Application とリソースを操作するため、テストでは使わない。
    /// </remarks>
    public sealed class FakeThemeService : IThemeService
    {
        /// <summary>
        /// SetTheme に渡されたテーマの履歴
        /// </summary>
        public List<ApplicationTheme> SetThemeCalls { get; } = new List<ApplicationTheme>();

        /// <summary>
        /// 現在のテーマ（最後に SetTheme されたテーマ）
        /// </summary>
        public ApplicationTheme CurrentTheme { get; private set; } = ApplicationTheme.Unknown;

        public ApplicationTheme GetTheme() => CurrentTheme;

        public SystemTheme GetNativeSystemTheme() => SystemTheme.Unknown;

        public ApplicationTheme GetSystemTheme() => ApplicationTheme.Unknown;

        public bool SetTheme(ApplicationTheme applicationTheme)
        {
            SetThemeCalls.Add(applicationTheme);
            CurrentTheme = applicationTheme;
            return true;
        }

        public bool SetSystemAccent() => true;

        public bool SetAccent(Color accentColor) => true;

        public bool SetAccent(SolidColorBrush accentSolidBrush) => true;
    }
}
