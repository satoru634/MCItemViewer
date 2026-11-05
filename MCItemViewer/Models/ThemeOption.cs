using Wpf.Ui.Appearance;

namespace MCItemViewer.Models
{
    /// <summary>
    /// テーマ選択コンボボックスの1項目（テーマ＋表示名）
    /// </summary>
    /// <param name="Key">テーマ</param>
    /// <param name="Label">表示名</param>
    public record ThemeOption(ApplicationTheme Key, string Label);
}
