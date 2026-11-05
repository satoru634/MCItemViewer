using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MCItemViewer.Services
{
    /// <summary>
    /// アプリケーション用のテーマサービス。
    /// WPF-UI の <see cref="ThemeService"/> は Mica 背景を前提にテーマを適用し、ウィンドウの背景を透過にする。
    /// MainWindow は Mica を使わない単色背景のため、実行中にテーマを切り替えると背景が黒く抜けてしまう。
    /// そこで背景効果なし（<see cref="WindowBackdropType.None"/>）で適用し、単色背景のまま切り替える。
    /// </summary>
    public class AppThemeService : ThemeService
    {
        /// <summary>
        /// テーマを背景効果なしで適用する。
        /// </summary>
        /// <param name="applicationTheme">適用するテーマ</param>
        /// <returns>常に true</returns>
        public override bool SetTheme(ApplicationTheme applicationTheme)
        {
            ApplicationThemeManager.Apply(applicationTheme, WindowBackdropType.None);
            return true;
        }
    }
}
