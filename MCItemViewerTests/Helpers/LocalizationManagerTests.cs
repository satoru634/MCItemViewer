using System.ComponentModel;
using System.Globalization;
using MCItemViewer.Helpers;

namespace MCItemViewerTests.Helpers
{
    /// <summary>
    /// LocalizationManager のテスト
    /// </summary>
    /// <remarks>
    /// LocalizationManager.Instance はプロセス全体で共有されるため、CultureScope で表示言語を切り替えて元に戻す。
    /// </remarks>
    [Collection("Culture")]
    public class LocalizationManagerTests
    {
        // ── インデクサー ─────────────────────────────────────────────────────

        [Fact]
        public void Indexer_Japanese_ReturnsJapaneseText()
        {
            using var scope = new CultureScope("ja");

            Assert.Equal("設定", LocalizationManager.Instance["MainWindow_MenuSettings"]);
        }

        [Fact]
        public void Indexer_English_ReturnsEnglishText()
        {
            using var scope = new CultureScope("en");

            Assert.Equal("Settings", LocalizationManager.Instance["MainWindow_MenuSettings"]);
        }

        [Fact]
        public void Indexer_EnglishUS_FallsBackToEnglish()
        {
            using var scope = new CultureScope("en-US");

            Assert.Equal("Settings", LocalizationManager.Instance["MainWindow_MenuSettings"]);
        }

        [Fact]
        public void Indexer_UnsupportedCulture_FallsBackToJapanese()
        {
            // 対応していない言語は既定（日本語）のリソースになる
            using var scope = new CultureScope("fr");

            Assert.Equal("設定", LocalizationManager.Instance["MainWindow_MenuSettings"]);
        }

        [Fact]
        public void Indexer_UnknownKey_ReturnsKeyItself()
        {
            Assert.Equal("NonExistentKey", LocalizationManager.Instance["NonExistentKey"]);
        }

        // ── Format ───────────────────────────────────────────────────────────

        [Fact]
        public void Format_Japanese_EmbedsArguments()
        {
            using var scope = new CultureScope("ja");

            Assert.Equal("PNGを出力しました。: a.png", LocalizationManager.Instance.Format("Main_OutputPngSucceeded", "a.png"));
        }

        [Fact]
        public void Format_English_EmbedsArguments()
        {
            using var scope = new CultureScope("en");

            Assert.Equal("Exported PNG: a.png", LocalizationManager.Instance.Format("Main_OutputPngSucceeded", "a.png"));
        }

        // ── CurrentCulture ───────────────────────────────────────────────────

        [Fact]
        public void CurrentCulture_Set_UpdatesCurrentUICulture()
        {
            using var scope = new CultureScope("en");

            Assert.Equal("en", CultureInfo.CurrentUICulture.Name);
            Assert.Equal("en", CultureInfo.DefaultThreadCurrentUICulture?.Name);
        }

        [Fact]
        public void CurrentCulture_SetDifferentValue_RaisesPropertyChanged()
        {
            using var scope = new CultureScope("ja");
            var changed = new List<string?>();
            PropertyChangedEventHandler handler = (_, e) => changed.Add(e.PropertyName);
            LocalizationManager.Instance.PropertyChanged += handler;
            try
            {
                LocalizationManager.Instance.CurrentCulture = new CultureInfo("en");
            }
            finally
            {
                LocalizationManager.Instance.PropertyChanged -= handler;
            }

            Assert.Equal(["Item[]", nameof(LocalizationManager.CurrentCulture)], changed);
        }

        [Fact]
        public void CurrentCulture_SetSameValue_DoesNotRaisePropertyChanged()
        {
            using var scope = new CultureScope("ja");
            var changed = new List<string?>();
            PropertyChangedEventHandler handler = (_, e) => changed.Add(e.PropertyName);
            LocalizationManager.Instance.PropertyChanged += handler;
            try
            {
                LocalizationManager.Instance.CurrentCulture = new CultureInfo("ja");
            }
            finally
            {
                LocalizationManager.Instance.PropertyChanged -= handler;
            }

            Assert.Empty(changed);
        }

        // ── NormalizeLanguage ────────────────────────────────────────────────

        [Fact]
        public void SupportedLanguages_IsJapaneseAndEnglish()
        {
            Assert.Equal(["ja", "en"], LocalizationManager.SupportedLanguages);
        }

        [Theory]
        [InlineData("ja")]
        [InlineData("en")]
        public void NormalizeLanguage_Supported_ReturnsAsIs(string language)
        {
            Assert.Equal(language, LocalizationManager.NormalizeLanguage(language));
        }

        [Theory]
        [InlineData("fr")]
        [InlineData("en-US")]
        [InlineData("")]
        [InlineData(null)]
        public void NormalizeLanguage_Unsupported_ReturnsJapanese(string? language)
        {
            Assert.Equal("ja", LocalizationManager.NormalizeLanguage(language));
        }
    }
}
