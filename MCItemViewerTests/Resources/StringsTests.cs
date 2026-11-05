using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using MCItemViewer.Resources;

namespace MCItemViewerTests.Resources
{
    /// <summary>
    /// Strings.resx（日本語）と Strings.en.resx（英語）の整合性のテスト
    /// </summary>
    public class StringsTests
    {
        /// <summary>
        /// 指定した言語のリソースだけ（親へのフォールバックなし）をキーと値の辞書で取得する。
        /// </summary>
        private static Dictionary<string, string> ReadResources(CultureInfo culture)
        {
            ResourceSet set = Strings.ResourceManager.GetResourceSet(culture, true, false)!;
            return set.Cast<DictionaryEntry>().ToDictionary(e => (string)e.Key, e => (string)e.Value!);
        }

        /// <summary>
        /// 書式文字列に含まれるプレースホルダー（{0} など）を取得する。
        /// </summary>
        private static string[] Placeholders(string format)
        {
            return Regex.Matches(format, @"\{\d+\}").Select(m => m.Value).Order().ToArray();
        }

        [Fact]
        public void Japanese_HasResources()
        {
            Assert.NotEmpty(ReadResources(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void English_HasSameKeysAsJapanese()
        {
            Dictionary<string, string> ja = ReadResources(CultureInfo.InvariantCulture);
            Dictionary<string, string> en = ReadResources(new CultureInfo("en"));

            Assert.Equal(ja.Keys.Order(), en.Keys.Order());
        }

        [Fact]
        public void AllValues_AreNotEmpty()
        {
            Dictionary<string, string> ja = ReadResources(CultureInfo.InvariantCulture);
            Dictionary<string, string> en = ReadResources(new CultureInfo("en"));

            Assert.All(ja, e => Assert.False(string.IsNullOrWhiteSpace(e.Value), e.Key));
            Assert.All(en, e => Assert.False(string.IsNullOrWhiteSpace(e.Value), e.Key));
        }

        [Fact]
        public void English_HasSamePlaceholdersAsJapanese()
        {
            Dictionary<string, string> ja = ReadResources(CultureInfo.InvariantCulture);
            Dictionary<string, string> en = ReadResources(new CultureInfo("en"));

            Assert.All(ja, e => Assert.Equal(Placeholders(e.Value), Placeholders(en[e.Key])));
        }
    }
}
