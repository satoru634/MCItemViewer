using System.IO;
using System.Windows;
using MCBlockViewer.Models;
using MCItemViewerTests.Helpers;
using MCToolsCommonLib.Common;
using Wpf.Ui.Appearance;

namespace MCItemViewerTests.Models
{
    /// <summary>
    /// ToolConfig のテスト
    /// </summary>
    /// <remarks>
    /// ToolConfig・WindowSettingData の名前空間は MCBlockViewer.Models（姉妹プロジェクトからの持ち越し）だが、
    /// テストはフォルダ構成に合わせて MCItemViewerTests.Models に置く。
    /// </remarks>
    public class ToolConfigTests : IDisposable
    {
        private readonly TempDirectory _temp = new TempDirectory();

        public void Dispose()
        {
            _temp.Dispose();
        }

        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_VanillaMinecraftPath_IsAppDataMinecraft()
        {
            var config = new ToolConfig();

            string expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
            Assert.Equal(expected, config.VanillaMinecraftPath);
        }

        [Fact]
        public void Constructor_MinecraftVersion_IsFirstVersionSince113OrEmpty()
        {
            var config = new ToolConfig();

            // 実行環境の .minecraft に依存するため、同じ規則で求めた値と比較する
            var versions = CommonLib.GetMinecraftProgramFiles(config.VanillaMinecraftPath, "1.13");
            Assert.Equal(versions.Count > 0 ? versions[0] : "", config.MinecraftVersion);
        }

        [Fact]
        public void Constructor_InitializesOtherDefaults()
        {
            var config = new ToolConfig();

            Assert.Equal("", config.ModMinecraftPath);
            Assert.Equal("", config.PrevItemID);
            Assert.Equal("", config.PrevItemName);
            Assert.Equal(Directory.GetCurrentDirectory(), config.OutputBasePath);
            Assert.Equal("#000000", config.BackgroundColor);
            Assert.Equal([16, 32, 64, 128, 256, 512, 1024], config.OutputSizeList);
            Assert.Equal("ja", config.Language);
        }

        [Fact]
        public void Constructor_WindowSetting_IsDefaultWindowSettingData()
        {
            var config = new ToolConfig();

            Assert.NotNull(config.WindowSetting);
            Assert.Equal(new Point(100, 100), config.WindowSetting.WindowPos);
            Assert.Equal(new Size(1000, 560), config.WindowSetting.WindowSize);
            Assert.False(config.WindowSetting.IsPaneOpen);
            Assert.Equal(ApplicationTheme.Dark, config.WindowSetting.Theme);
        }

        // ── 設定ファイルとの読み書き ───────────────────────────────────────────

        [Fact]
        public void Setting_SaveThenLoad_RoundTripsAllProperties()
        {
            string path = _temp.Combine("tool_settings.json");
            var setting = new Setting<ToolConfig>(path, onLoad: false);
            setting.Data.VanillaMinecraftPath = @"C:\mc";
            setting.Data.ModMinecraftPath = @"C:\mods";
            setting.Data.MinecraftVersion = "1.20.1";
            setting.Data.PrevItemID = "minecraft:stone";
            setting.Data.PrevItemName = "石";
            setting.Data.OutputBasePath = @"C:\out";
            setting.Data.BackgroundColor = "#FF00FF";
            setting.Data.OutputSizeList = [32, 64];
            setting.Data.WindowSetting.WindowPos = new Point(20, 30);
            setting.Data.WindowSetting.WindowSize = new Size(1200, 700);
            setting.Data.WindowSetting.IsPaneOpen = true;
            setting.Data.WindowSetting.Theme = ApplicationTheme.Light;
            setting.Data.Language = "en";

            setting.Save();
            var loaded = new Setting<ToolConfig>(path).Data;

            Assert.Equal(@"C:\mc", loaded.VanillaMinecraftPath);
            Assert.Equal(@"C:\mods", loaded.ModMinecraftPath);
            Assert.Equal("1.20.1", loaded.MinecraftVersion);
            Assert.Equal("minecraft:stone", loaded.PrevItemID);
            Assert.Equal("石", loaded.PrevItemName);
            Assert.Equal(@"C:\out", loaded.OutputBasePath);
            Assert.Equal("#FF00FF", loaded.BackgroundColor);
            Assert.Equal([32, 64], loaded.OutputSizeList);
            Assert.Equal(new Point(20, 30), loaded.WindowSetting.WindowPos);
            Assert.Equal(new Size(1200, 700), loaded.WindowSetting.WindowSize);
            Assert.True(loaded.WindowSetting.IsPaneOpen);
            Assert.Equal(ApplicationTheme.Light, loaded.WindowSetting.Theme);
            Assert.Equal("en", loaded.Language);
        }

        [Fact]
        public void Setting_LoadPartialFile_KeepsDefaultsForMissingKeys()
        {
            string path = _temp.Combine("partial.json");
            File.WriteAllText(path, """{ "BackgroundColor": "#123456" }""");

            var loaded = new Setting<ToolConfig>(path).Data;

            Assert.Equal("#123456", loaded.BackgroundColor);
            Assert.Equal("", loaded.PrevItemName);
            Assert.Equal([16, 32, 64, 128, 256, 512, 1024], loaded.OutputSizeList);
            Assert.False(loaded.WindowSetting.IsPaneOpen);
            Assert.Equal(ApplicationTheme.Dark, loaded.WindowSetting.Theme);
            Assert.Equal("ja", loaded.Language);
        }

        [Fact]
        public void Setting_Save_WritesThemeAsName()
        {
            string path = _temp.Combine("theme.json");
            var setting = new Setting<ToolConfig>(path, onLoad: false);
            setting.Data.WindowSetting.Theme = ApplicationTheme.Light;

            setting.Save();

            // 数値（2）ではなく名前で保存される
            Assert.Contains("\"Theme\": \"Light\"", File.ReadAllText(path));
        }

        [Theory]
        [InlineData("\"Dark\"", ApplicationTheme.Dark)]
        [InlineData("\"Light\"", ApplicationTheme.Light)]
        [InlineData("2", ApplicationTheme.Light)]
        public void Setting_Load_ReadsThemeFromNameOrNumber(string themeJson, ApplicationTheme expected)
        {
            string path = _temp.Combine("theme.json");
            File.WriteAllText(path, $$"""{ "WindowSetting": { "Theme": {{themeJson}} } }""");

            var loaded = new Setting<ToolConfig>(path).Data;

            Assert.Equal(expected, loaded.WindowSetting.Theme);
        }
    }

    /// <summary>
    /// WindowSettingData のテスト
    /// </summary>
    public class WindowSettingDataTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_SetsDefaultPositionAndSize()
        {
            var data = new WindowSettingData();

            Assert.Equal(new Point(100, 100), data.WindowPos);
            Assert.Equal(new Size(1000, 560), data.WindowSize);
        }

        [Fact]
        public void Constructor_IsPaneOpen_IsFalse()
        {
            var data = new WindowSettingData();

            Assert.False(data.IsPaneOpen);
        }

        [Fact]
        public void Constructor_Theme_IsDark()
        {
            var data = new WindowSettingData();

            Assert.Equal(ApplicationTheme.Dark, data.Theme);
        }
    }
}
