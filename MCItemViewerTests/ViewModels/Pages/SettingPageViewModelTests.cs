using System.ComponentModel;
using System.Globalization;
using System.IO;
using MCBlockViewer.Models;
using MCItemViewer.Helpers;
using MCItemViewer.ViewModels.Pages;
using MCItemViewerTests.Helpers;
using MCToolsCommonLib.Common;
using Wpf.Ui.Appearance;

namespace MCItemViewerTests.ViewModels.Pages
{
    /// <summary>
    /// SettingPageViewModel のテスト
    /// </summary>
    /// <remarks>
    /// 一時フォルダの設定ファイルを読み込んだ Setting&lt;ToolConfig&gt; と、テーマを実際には適用しない FakeThemeService を渡して生成する。
    /// 実環境の .minecraft を参照しないよう、設定ファイルには存在しない Minecraft のパスを書き込んでおく。
    /// 言語の変更は LocalizationManager（プロセス全体で共有）の表示言語を書き換えるため、日本語から始めてテスト後に元へ戻す。
    /// </remarks>
    [Collection("Culture")]
    public class SettingPageViewModelTests : IDisposable
    {
        private readonly TempDirectory _temp = new TempDirectory();

        private readonly CultureScope _culture = new CultureScope("ja");

        private readonly string _settingPath;

        private readonly FakeThemeService _themeService = new FakeThemeService();

        public SettingPageViewModelTests()
        {
            _settingPath = _temp.Combine("tool_settings.json");
        }

        public void Dispose()
        {
            _culture.Dispose();
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

        /// <summary>
        /// 設定ファイルを読み込んで ViewModel を生成する。
        /// </summary>
        private SettingPageViewModel CreateViewModel()
        {
            return new SettingPageViewModel(new Setting<ToolConfig>(_settingPath), _themeService);
        }

        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_ThemeList_IsLightAndDark()
        {
            WriteSetting();

            var vm = CreateViewModel();

            Assert.Equal([ApplicationTheme.Light, ApplicationTheme.Dark], vm.ThemeList.Select(t => t.Key));
            Assert.Equal(["Settings_ThemeLight", "Settings_ThemeDark"], vm.ThemeList.Select(t => t.LabelKey));
            Assert.Equal(["ライト", "ダーク"], vm.ThemeList.Select(t => t.Label));
        }

        [Fact]
        public void Constructor_LanguageList_MatchesSupportedLanguages()
        {
            WriteSetting();

            var vm = CreateViewModel();

            Assert.Equal(LocalizationManager.SupportedLanguages, vm.LanguageList.Select(l => l.Key));
        }

        [Fact]
        public void Constructor_LanguageList_IsJapaneseAndEnglish()
        {
            WriteSetting();

            var vm = CreateViewModel();

            Assert.Equal(["ja", "en"], vm.LanguageList.Select(l => l.Key));
            Assert.Equal(["日本語", "English"], vm.LanguageList.Select(l => l.Label));
        }

        [Theory]
        [InlineData(ApplicationTheme.Light)]
        [InlineData(ApplicationTheme.Dark)]
        public void Constructor_SelectedTheme_RestoredFromSetting(ApplicationTheme theme)
        {
            WriteSetting(c => c.WindowSetting.Theme = theme);

            var vm = CreateViewModel();

            Assert.Equal(theme, vm.SelectedTheme);
        }

        [Theory]
        [InlineData(ApplicationTheme.Unknown)]
        [InlineData(ApplicationTheme.HighContrast)]
        public void Constructor_ThemeNotInList_SelectsDark(ApplicationTheme theme)
        {
            WriteSetting(c => c.WindowSetting.Theme = theme);

            var vm = CreateViewModel();

            Assert.Equal(ApplicationTheme.Dark, vm.SelectedTheme);
        }

        [Theory]
        [InlineData("ja")]
        [InlineData("en")]
        public void Constructor_SelectedLanguage_RestoredFromSetting(string language)
        {
            WriteSetting(c => c.Language = language);

            var vm = CreateViewModel();

            Assert.Equal(language, vm.SelectedLanguage);
        }

        [Theory]
        [InlineData("fr")]
        [InlineData("")]
        public void Constructor_LanguageNotInList_SelectsJapanese(string language)
        {
            WriteSetting(c => c.Language = language);

            var vm = CreateViewModel();

            Assert.Equal("ja", vm.SelectedLanguage);
        }

        [Fact]
        public void Constructor_SettingWithoutThemeAndLanguage_SelectsDarkAndJapanese()
        {
            // テーマ・言語の追加前の設定ファイル（キーが無い）でも、既定値になる
            File.WriteAllText(_settingPath, """{ "BackgroundColor": "#123456" }""");

            var vm = CreateViewModel();

            Assert.Equal(ApplicationTheme.Dark, vm.SelectedTheme);
            Assert.Equal("ja", vm.SelectedLanguage);
        }

        [Fact]
        public void Constructor_DoesNotApplyThemeOrSave()
        {
            // 起動時のテーマ適用は ApplicationHostService が行うため、ViewModel の生成時には適用も保存もしない
            WriteSetting(c => c.WindowSetting.Theme = ApplicationTheme.Light);
            DateTime before = File.GetLastWriteTimeUtc(_settingPath);

            CreateViewModel();

            Assert.Empty(_themeService.SetThemeCalls);
            Assert.Equal(before, File.GetLastWriteTimeUtc(_settingPath));
        }

        [Fact]
        public void Constructor_DoesNotChangeLanguage()
        {
            // 起動時の表示言語の適用は ApplicationHostService が行うため、ViewModel の生成時には切り替えない
            WriteSetting(c => c.Language = "en");

            CreateViewModel();

            Assert.Equal("ja", LocalizationManager.Instance.CurrentCulture.Name);
        }

        // ── SelectedTheme ────────────────────────────────────────────────────

        [Fact]
        public void SelectedTheme_Changed_AppliesTheme()
        {
            WriteSetting(c => c.WindowSetting.Theme = ApplicationTheme.Dark);
            var vm = CreateViewModel();

            vm.SelectedTheme = ApplicationTheme.Light;

            Assert.Equal([ApplicationTheme.Light], _themeService.SetThemeCalls);
        }

        [Fact]
        public void SelectedTheme_Changed_SavesToSettingFile()
        {
            WriteSetting(c => c.WindowSetting.Theme = ApplicationTheme.Dark);
            var vm = CreateViewModel();

            vm.SelectedTheme = ApplicationTheme.Light;

            Assert.Equal(ApplicationTheme.Light, ReadSetting().WindowSetting.Theme);
        }

        [Fact]
        public void SelectedTheme_Changed_RaisesPropertyChanged()
        {
            WriteSetting(c => c.WindowSetting.Theme = ApplicationTheme.Dark);
            var vm = CreateViewModel();
            var changed = new List<string?>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.SelectedTheme = ApplicationTheme.Light;

            Assert.Contains(nameof(SettingPageViewModel.SelectedTheme), changed);
        }

        [Fact]
        public void SelectedTheme_SameValue_DoesNothing()
        {
            WriteSetting(c => c.WindowSetting.Theme = ApplicationTheme.Dark);
            var vm = CreateViewModel();

            vm.SelectedTheme = ApplicationTheme.Dark;

            Assert.Empty(_themeService.SetThemeCalls);
        }

        // ── SelectedLanguage ─────────────────────────────────────────────────

        [Fact]
        public void SelectedLanguage_Changed_SavesToSettingFile()
        {
            WriteSetting(c => c.Language = "ja");
            var vm = CreateViewModel();

            vm.SelectedLanguage = "en";

            Assert.Equal("en", ReadSetting().Language);
        }

        [Theory]
        [InlineData("ja", "en")]
        [InlineData("en", "ja")]
        public void SelectedLanguage_Changed_SwitchesDisplayLanguage(string from, string to)
        {
            WriteSetting(c => c.Language = from);
            LocalizationManager.Instance.CurrentCulture = new CultureInfo(from);
            var vm = CreateViewModel();

            vm.SelectedLanguage = to;

            Assert.Equal(to, LocalizationManager.Instance.CurrentCulture.Name);
        }

        [Fact]
        public void SelectedLanguage_ChangedToEnglish_ThemeLabelsBecomeEnglish()
        {
            WriteSetting(c => c.Language = "ja");
            var vm = CreateViewModel();

            vm.SelectedLanguage = "en";

            Assert.Equal(["Light", "Dark"], vm.ThemeList.Select(t => t.Label));
        }

        [Fact]
        public void SelectedLanguage_Changed_DoesNotApplyTheme()
        {
            WriteSetting(c => c.Language = "ja");
            var vm = CreateViewModel();

            vm.SelectedLanguage = "en";

            Assert.Empty(_themeService.SetThemeCalls);
        }

        [Fact]
        public void SelectedLanguage_Changed_KeepsOtherSettings()
        {
            WriteSetting(c =>
            {
                c.Language = "ja";
                c.BackgroundColor = "#FF00FF";
                c.WindowSetting.Theme = ApplicationTheme.Light;
                c.WindowSetting.IsPaneOpen = true;
            });
            var vm = CreateViewModel();

            vm.SelectedLanguage = "en";

            ToolConfig saved = ReadSetting();
            Assert.Equal("#FF00FF", saved.BackgroundColor);
            Assert.Equal(ApplicationTheme.Light, saved.WindowSetting.Theme);
            Assert.True(saved.WindowSetting.IsPaneOpen);
        }
    }
}
