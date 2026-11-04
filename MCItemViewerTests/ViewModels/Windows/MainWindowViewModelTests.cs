using System.ComponentModel;
using System.IO;
using MCBlockViewer.Models;
using MCItemViewer.ViewModels.Pages;
using MCItemViewer.ViewModels.Windows;
using MCItemViewerTests.Helpers;
using MCToolsCommonLib.Common;

namespace MCItemViewerTests.ViewModels.Windows
{
    /// <summary>
    /// MainWindowViewModel（ナビゲーションのホストとなるメインウィンドウの ViewModel）のテスト
    /// </summary>
    /// <remarks>
    /// 一時フォルダの設定ファイルを読み込んだ Setting&lt;ToolConfig&gt; を渡して生成する。
    /// 実環境の .minecraft を参照しないよう、設定ファイルには存在しない Minecraft のパスを書き込んでおく。
    /// </remarks>
    public class MainWindowViewModelTests : IDisposable
    {
        private readonly TempDirectory _temp = new TempDirectory();

        private readonly string _settingPath;

        public MainWindowViewModelTests()
        {
            _settingPath = _temp.Combine("tool_settings.json");
        }

        public void Dispose()
        {
            _temp.Dispose();
        }

        /// <summary>
        /// 設定ファイルを作成する。
        /// </summary>
        private void WriteSetting(Action<ToolConfig>? configure = null)
        {
            var config = new ToolConfig
            {
                VanillaMinecraftPath = _temp.Combine("not_exists"),
                MinecraftVersion = "",
                OutputBasePath = _temp.Path,
            };
            configure?.Invoke(config);
            CommonLib.WriteJson(_settingPath, config);
        }

        /// <summary>
        /// 保存された設定ファイルを読み込む。
        /// </summary>
        private ToolConfig ReadSetting()
        {
            return CommonLib.ReadJson<ToolConfig>(_settingPath);
        }

        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_ApplicationTitle_IsMCItemViewer()
        {
            WriteSetting();

            var vm = new MainWindowViewModel(new Setting<ToolConfig>(_settingPath));

            Assert.Equal("MCItemViewer", vm.ApplicationTitle);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_IsPaneOpen_RestoredFromSetting(bool isPaneOpen)
        {
            WriteSetting(c => c.WindowSetting.IsPaneOpen = isPaneOpen);

            var vm = new MainWindowViewModel(new Setting<ToolConfig>(_settingPath));

            Assert.Equal(isPaneOpen, vm.IsPaneOpen);
        }

        [Fact]
        public void Constructor_SettingWithoutWindowSetting_PaneIsClosed()
        {
            // WindowSetting 追加前の設定ファイル（キーが無い）でも、既定値（閉じた状態）になる
            File.WriteAllText(_settingPath, """{ "BackgroundColor": "#123456" }""");

            var vm = new MainWindowViewModel(new Setting<ToolConfig>(_settingPath));

            Assert.False(vm.IsPaneOpen);
        }

        // ── IsPaneOpen ───────────────────────────────────────────────────────

        [Fact]
        public void IsPaneOpen_Changed_SavesToSettingFile()
        {
            WriteSetting(c => c.WindowSetting.IsPaneOpen = false);
            var setting = new Setting<ToolConfig>(_settingPath);
            var vm = new MainWindowViewModel(setting);

            vm.IsPaneOpen = true;

            Assert.True(setting.Data.WindowSetting.IsPaneOpen);
            Assert.True(ReadSetting().WindowSetting.IsPaneOpen);
        }

        [Fact]
        public void IsPaneOpen_Changed_RaisesPropertyChanged()
        {
            WriteSetting(c => c.WindowSetting.IsPaneOpen = false);
            var vm = new MainWindowViewModel(new Setting<ToolConfig>(_settingPath));
            var changed = new List<string?>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.IsPaneOpen = true;

            Assert.Contains(nameof(MainWindowViewModel.IsPaneOpen), changed);
        }

        [Fact]
        public void IsPaneOpen_Changed_KeepsOtherSettings()
        {
            WriteSetting(c =>
            {
                c.BackgroundColor = "#FF00FF";
                c.WindowSetting.IsPaneOpen = false;
            });
            var vm = new MainWindowViewModel(new Setting<ToolConfig>(_settingPath));

            vm.IsPaneOpen = true;

            Assert.Equal("#FF00FF", ReadSetting().BackgroundColor);
        }

        // ── MainPageViewModel との設定の共有 ─────────────────────────────────

        [Fact]
        public void SharedSetting_MainPageSaves_KeepsPaneState()
        {
            // DI では同じ Setting<ToolConfig> を共有するため、メインページ側の保存でペインの開閉状態が失われない
            WriteSetting(c => c.WindowSetting.IsPaneOpen = false);
            var setting = new Setting<ToolConfig>(_settingPath);
            var windowVm = new MainWindowViewModel(setting);
            var pageVm = new MainPageViewModel(setting);

            windowVm.IsPaneOpen = true;
            pageVm.VanillaMinecraftPath = _temp.Combine("other_minecraft");

            ToolConfig saved = ReadSetting();
            Assert.True(saved.WindowSetting.IsPaneOpen);
            Assert.Equal(_temp.Combine("other_minecraft"), saved.VanillaMinecraftPath);
        }
    }
}
