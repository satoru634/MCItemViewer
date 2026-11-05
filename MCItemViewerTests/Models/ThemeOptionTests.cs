using System.ComponentModel;
using System.Globalization;
using MCItemViewer.Helpers;
using MCItemViewer.Models;
using MCItemViewerTests.Helpers;
using Wpf.Ui.Appearance;

namespace MCItemViewerTests.Models
{
    /// <summary>
    /// ThemeOption のテスト
    /// </summary>
    [Collection("Culture")]
    public class ThemeOptionTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_SetsKeyAndLabelKey()
        {
            var option = new ThemeOption(ApplicationTheme.Dark, "Settings_ThemeDark");

            Assert.Equal(ApplicationTheme.Dark, option.Key);
            Assert.Equal("Settings_ThemeDark", option.LabelKey);
        }

        // ── Label ────────────────────────────────────────────────────────────

        [Theory]
        [InlineData("ja", "ダーク")]
        [InlineData("en", "Dark")]
        public void Label_ReturnsTextInCurrentLanguage(string culture, string expected)
        {
            using var scope = new CultureScope(culture);

            var option = new ThemeOption(ApplicationTheme.Dark, "Settings_ThemeDark");

            Assert.Equal(expected, option.Label);
        }

        [Fact]
        public void Label_LanguageChanged_RaisesPropertyChanged()
        {
            using var scope = new CultureScope("ja");
            var option = new ThemeOption(ApplicationTheme.Light, "Settings_ThemeLight");
            var changed = new List<string?>();
            option.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            LocalizationManager.Instance.CurrentCulture = new CultureInfo("en");

            Assert.Equal([nameof(ThemeOption.Label)], changed);
            Assert.Equal("Light", option.Label);
        }

        [Fact]
        public void Label_SameLanguage_DoesNotRaisePropertyChanged()
        {
            using var scope = new CultureScope("ja");
            var option = new ThemeOption(ApplicationTheme.Light, "Settings_ThemeLight");
            var changed = new List<string?>();
            option.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            LocalizationManager.Instance.CurrentCulture = new CultureInfo("ja");

            Assert.Empty(changed);
        }
    }
}
