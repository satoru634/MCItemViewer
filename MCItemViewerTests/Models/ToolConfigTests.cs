using System.IO;
using System.Windows;
using MCBlockViewer.Models;
using MCItemViewerTests.Helpers;
using MCToolsCommonLib.Common;

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
            Assert.Equal(new Size(1000, 500), data.WindowSize);
        }
    }
}
