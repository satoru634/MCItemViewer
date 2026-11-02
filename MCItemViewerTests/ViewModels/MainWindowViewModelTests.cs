using System.ComponentModel;
using System.IO;
using MCBlockViewer.Models;
using MCItemViewer.ViewModels;
using MCItemViewerTests.Helpers;
using MCToolsCommonLib.Common;

namespace MCItemViewerTests.ViewModels
{
    /// <summary>
    /// MainWindowViewModel のテスト
    /// </summary>
    /// <remarks>
    /// 一時フォルダにダミーの .minecraft と設定ファイルを作り、設定ファイルのパスを指定するコンストラクタで生成する。
    /// Viewport3DX（WPF の STA スレッドと GPU）が必要な描画処理（UpdateBlockModel、BeginOutputPng）と、
    /// ダイアログを表示する OpenFolder はテスト対象外とする。
    /// </remarks>
    public class MainWindowViewModelTests : IDisposable
    {
        private readonly TempDirectory _temp = new TempDirectory();

        private readonly string _minecraftPath;

        private readonly string _settingPath;

        private readonly string _outputPath;

        public MainWindowViewModelTests()
        {
            _minecraftPath = _temp.Combine(".minecraft");
            _settingPath = _temp.Combine("tool_settings.json");
            _outputPath = _temp.Combine("output");
            Directory.CreateDirectory(_outputPath);

            // 1.13 より古いバージョンは一覧に出ない
            TestMinecraft.CreateEmptyVersion(_minecraftPath, "1.12.2");
            TestMinecraft.CreateVersion(_minecraftPath, "1.20.1", new Dictionary<string, string>
            {
                ["block.minecraft.stone"] = "石",
                ["item.minecraft.apple"] = "リンゴ",
            });
            TestMinecraft.CreateVersion(_minecraftPath, "1.20.4", new Dictionary<string, string>
            {
                ["block.minecraft.stone"] = "石",
                ["item.minecraft.apple"] = "リンゴ",
                ["item.minecraft.copper_ingot"] = "銅インゴット",
            });
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
                VanillaMinecraftPath = _minecraftPath,
                ModMinecraftPath = "",
                MinecraftVersion = "1.20.1",
                PrevItemID = "",
                PrevItemName = "",
                OutputBasePath = _outputPath,
                BackgroundColor = "#336699",
                OutputSizeList = [16, 32, 64],
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
        /// 選択中の項目を取得する。
        /// </summary>
        private static T Selected<T>(ItemBaseViewModel<T> list)
        {
            return list.ItemList[list.SelectedIndex];
        }

        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_LoadsPathsAndColorFromSetting()
        {
            WriteSetting(c => c.ModMinecraftPath = @"C:\mods");

            var vm = new MainWindowViewModel(_settingPath);

            Assert.Equal(_minecraftPath, vm.VanillaMinecraftPath);
            Assert.Equal(@"C:\mods", vm.ModMinecraftPath);
            Assert.Equal(_outputPath, vm.OutputBasePath);
            Assert.Equal("#336699", vm.BackgroundColor);
        }

        [Fact]
        public void Constructor_NotifyData_IsEmpty()
        {
            WriteSetting();

            var vm = new MainWindowViewModel(_settingPath);

            Assert.Equal("", vm.NotifyData.Message);
        }

        [Fact]
        public void Constructor_VersionList_ContainsOnly113OrLaterAndSelectsConfiguredVersion()
        {
            WriteSetting(c => c.MinecraftVersion = "1.20.4");

            var vm = new MainWindowViewModel(_settingPath);

            Assert.Equal(["1.20.1", "1.20.4"], vm.VersionList.ItemList.OrderBy(v => v));
            Assert.Equal("1.20.4", Selected(vm.VersionList));
            Assert.True(vm.VersionList.Enable);
        }

        [Fact]
        public void Constructor_ConfiguredVersionEmpty_SelectsFirstVersion()
        {
            WriteSetting(c => c.MinecraftVersion = "");

            var vm = new MainWindowViewModel(_settingPath);

            Assert.Equal(0, vm.VersionList.SelectedIndex);
        }

        [Fact]
        public void Constructor_VanillaPathNotExists_HasNoVersionsAndNoItems()
        {
            WriteSetting(c => c.VanillaMinecraftPath = _temp.Combine("not_exists"));

            var vm = new MainWindowViewModel(_settingPath);

            Assert.Empty(vm.VersionList.ItemList);
            Assert.Equal(-1, vm.VersionList.SelectedIndex);
            Assert.Empty(vm.ModelLoader.ItemIDList);
            Assert.False(vm.ModelLoader.ModelEnabled);
        }

        [Fact]
        public void Constructor_ConfiguredVersionNotInstalled_HasNoItems()
        {
            WriteSetting(c => c.MinecraftVersion = "1.19");

            var vm = new MainWindowViewModel(_settingPath);

            Assert.Empty(vm.ModelLoader.ItemIDList);
        }

        [Fact]
        public void Constructor_NameSpaceList_IsMinecraftOnly()
        {
            WriteSetting();

            var vm = new MainWindowViewModel(_settingPath);

            Assert.Equal(["minecraft"], vm.NameSpaceList.ItemList);
            Assert.Equal("minecraft", Selected(vm.NameSpaceList));
        }

        [Fact]
        public void Constructor_OutputSizeList_UsesSettingAndSelects16()
        {
            WriteSetting(c => c.OutputSizeList = [32, 16, 64]);

            var vm = new MainWindowViewModel(_settingPath);

            Assert.Equal([32, 16, 64], vm.OutputSizeList.ItemList);
            Assert.Equal(16, Selected(vm.OutputSizeList));
        }

        [Fact]
        public void Constructor_OutputSizeListWithout16_SelectsFirst()
        {
            WriteSetting(c => c.OutputSizeList = [64, 128]);

            var vm = new MainWindowViewModel(_settingPath);

            Assert.Equal(64, Selected(vm.OutputSizeList));
        }

        [Fact]
        public void Constructor_OutputSizeList_IsCopyOfSetting()
        {
            WriteSetting();

            var vm = new MainWindowViewModel(_settingPath);
            vm.OutputSizeList.Add(2048);

            Assert.Equal([16, 32, 64], ReadSetting().OutputSizeList);
        }

        [Fact]
        public void Constructor_ModelLoader_UsesJapaneseNamesOfConfiguredVersion()
        {
            WriteSetting();

            var vm = new MainWindowViewModel(_settingPath);

            Assert.Equal(["minecraft:apple", "minecraft:stone"], vm.ModelLoader.ItemIDList);
            Assert.Equal(["リンゴ", "石"], vm.ModelLoader.ItemNameList);
        }

        [Fact]
        public void Constructor_PrevItemNameExists_LoadsModel()
        {
            WriteSetting(c => c.PrevItemName = "石");

            var vm = new MainWindowViewModel(_settingPath);

            Assert.Equal("minecraft:stone", vm.ModelLoader.ItemID);
            Assert.Equal("石", vm.ModelLoader.ItemName);
            Assert.Equal(["assets/minecraft/models/item/stone.json"], vm.ModelLoader.Model.ModelFilePath);
        }

        [Fact]
        public void Constructor_PrevItemNameNotExists_DoesNotLoadModel()
        {
            WriteSetting(c => c.PrevItemName = "存在しないアイテム");

            var vm = new MainWindowViewModel(_settingPath);

            Assert.Empty(vm.ModelLoader.Model.ModelFilePath);
            Assert.Equal("minecraft:apple", vm.ModelLoader.ItemID);   // 先頭のアイテムが選択されたまま
        }

        [Fact]
        public void Constructor_BlockRender_IsCreated()
        {
            WriteSetting();

            var vm = new MainWindowViewModel(_settingPath);

            Assert.NotNull(vm.BlockRender);
        }

        // ── ChangeVersion ─────────────────────────────────────────────────────

        [Fact]
        public void ChangeVersion_SelectedAnotherVersion_SavesSettingAndReloadsItems()
        {
            WriteSetting();
            var vm = new MainWindowViewModel(_settingPath);
            vm.VersionList.SelectedIndex = vm.VersionList.ItemList.IndexOf("1.20.4");

            vm.ChangeVersion();

            Assert.Equal("1.20.4", ReadSetting().MinecraftVersion);
            Assert.Contains("minecraft:copper_ingot", vm.ModelLoader.ItemIDList);
        }

        [Fact]
        public void ChangeVersion_DisposesPreviousLoaderAndRaisesPropertyChanged()
        {
            WriteSetting();
            var vm = new MainWindowViewModel(_settingPath);
            var oldLoader = vm.ModelLoader;
            var changed = new List<string?>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => changed.Add(e.PropertyName);
            vm.VersionList.SelectedIndex = vm.VersionList.ItemList.IndexOf("1.20.4");

            vm.ChangeVersion();

            Assert.NotSame(oldLoader, vm.ModelLoader);
            Assert.Empty(oldLoader.ItemIDList);
            Assert.Contains(nameof(MainWindowViewModel.ModelLoader), changed);
        }

        [Fact]
        public void ChangeVersion_KeepsPreviousItemSelection()
        {
            WriteSetting(c => c.PrevItemName = "リンゴ");
            var vm = new MainWindowViewModel(_settingPath);
            vm.VersionList.SelectedIndex = vm.VersionList.ItemList.IndexOf("1.20.4");

            vm.ChangeVersion();

            Assert.Equal("minecraft:apple", vm.ModelLoader.ItemID);
            Assert.NotEmpty(vm.ModelLoader.Model.ModelFilePath);
        }

        [Fact]
        public void ChangeVersion_NoSelection_DoesNothing()
        {
            WriteSetting();
            var vm = new MainWindowViewModel(_settingPath);
            var oldLoader = vm.ModelLoader;
            vm.VersionList.SelectedIndex = -1;

            vm.ChangeVersion();

            Assert.Same(oldLoader, vm.ModelLoader);
            Assert.Equal("1.20.1", ReadSetting().MinecraftVersion);
        }

        // ── ChangeNameSpace ───────────────────────────────────────────────────

        [Fact]
        public void ChangeNameSpace_RecreatesLoader()
        {
            WriteSetting();
            var vm = new MainWindowViewModel(_settingPath);
            var oldLoader = vm.ModelLoader;

            vm.ChangeNameSpace();

            Assert.NotSame(oldLoader, vm.ModelLoader);
            Assert.Equal(oldLoader.ItemIDList, vm.ModelLoader.ItemIDList);
        }

        // ── VanillaMinecraftPath の変更 ───────────────────────────────────────

        [Fact]
        public void VanillaMinecraftPath_Changed_SavesSetting()
        {
            WriteSetting();
            var vm = new MainWindowViewModel(_settingPath);
            string newPath = _temp.Combine("other_minecraft");

            vm.VanillaMinecraftPath = newPath;

            Assert.Equal(newPath, ReadSetting().VanillaMinecraftPath);
        }

        [Fact]
        public void VanillaMinecraftPath_ChangedToPathWithVersions_ReloadsVersionsAndItems()
        {
            string otherPath = _temp.Combine("other_minecraft");
            TestMinecraft.CreateVersion(otherPath, "1.20.1", new Dictionary<string, string> { ["item.minecraft.bread"] = "パン" });
            WriteSetting();
            var vm = new MainWindowViewModel(_settingPath);

            vm.VanillaMinecraftPath = otherPath;

            Assert.Equal(["1.20.1"], vm.VersionList.ItemList);
            Assert.Equal(["minecraft:bread"], vm.ModelLoader.ItemIDList);
        }

        [Fact]
        public void VanillaMinecraftPath_ChangedToMissingPath_ClearsVersionsAndItems()
        {
            WriteSetting();
            var vm = new MainWindowViewModel(_settingPath);
            var changed = new List<string?>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.VanillaMinecraftPath = _temp.Combine("not_exists");

            Assert.Empty(vm.VersionList.ItemList);
            Assert.Empty(vm.ModelLoader.ItemIDList);
            Assert.Contains(nameof(MainWindowViewModel.VersionList), changed);
            Assert.Contains(nameof(MainWindowViewModel.ModelLoader), changed);
        }

        // ── OutputPng ─────────────────────────────────────────────────────────

        [Fact]
        public async Task OutputPng_OutputNotStarted_DoesNothing()
        {
            WriteSetting(c => c.PrevItemName = "石");
            var vm = new MainWindowViewModel(_settingPath);

            // BeginOutputPng を経由していないため、ビューポートやパネルに触れずに終了する
            await vm.OutputPng(null!, null!);

            Assert.Empty(Directory.GetFiles(_outputPath));
            Assert.Equal("", vm.NotifyData.Message);
        }

        [Fact]
        public async Task OutputPng_NoModelLoaded_DoesNothing()
        {
            WriteSetting();
            var vm = new MainWindowViewModel(_settingPath);

            await vm.OutputPng(null!, null!);

            Assert.Empty(Directory.GetFiles(_outputPath));
        }
    }
}
