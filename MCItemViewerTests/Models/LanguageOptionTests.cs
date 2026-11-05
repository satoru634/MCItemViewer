using MCItemViewer.Models;

namespace MCItemViewerTests.Models
{
    /// <summary>
    /// LanguageOption のテスト
    /// </summary>
    public class LanguageOptionTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_SetsKeyAndLabel()
        {
            var option = new LanguageOption("en", "English");

            Assert.Equal("en", option.Key);
            Assert.Equal("English", option.Label);
        }

        // ── Equals ───────────────────────────────────────────────────────────

        [Fact]
        public void Equals_SameKeyAndLabel_IsTrue()
        {
            Assert.Equal(new LanguageOption("ja", "日本語"), new LanguageOption("ja", "日本語"));
        }
    }
}
