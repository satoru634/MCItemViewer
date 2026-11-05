using MCItemViewer.Models;
using Wpf.Ui.Appearance;

namespace MCItemViewerTests.Models
{
    /// <summary>
    /// ThemeOption のテスト
    /// </summary>
    public class ThemeOptionTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_SetsKeyAndLabel()
        {
            var option = new ThemeOption(ApplicationTheme.Dark, "ダーク");

            Assert.Equal(ApplicationTheme.Dark, option.Key);
            Assert.Equal("ダーク", option.Label);
        }

        // ── Equals ───────────────────────────────────────────────────────────

        [Fact]
        public void Equals_SameKeyAndLabel_IsTrue()
        {
            Assert.Equal(new ThemeOption(ApplicationTheme.Light, "ライト"), new ThemeOption(ApplicationTheme.Light, "ライト"));
        }
    }
}
