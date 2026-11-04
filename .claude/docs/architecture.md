# アーキテクチャ

## 2リポジトリ構成

Minecraft 固有の処理のほとんどは、git サブモジュール `MCItemViewer/submodules/MCLibs`（別リポジトリ `satoru634/MCLibs`。同じ作者の他ツールと共用）にあります。csproj は `submodules\**` をコンパイル対象から除外し、代わりにその中の2つのプロジェクトを `ProjectReference` で参照しています。

- **MCToolsCommonLib** — 汎用ヘルパー群。
  - `JarLoader`: クライアント jar から assets/lang/JSON/テクスチャを読み込む
  - `Setting<T>`: JSON 設定の永続化
  - `ItemBaseViewModel<T>`: ComboBox バインド用のリスト＋選択インデックス
  - `NotifyInfo`
  - `CommonLib.GetMinecraftProgramFiles` / `GetMinecraftProgramFilePath`: バージョン検出
- **MCModelRenderer**
  - `BlockModelLoarder`（綴りはこのまま）: `JarLoader` を継承し、アイテムID ↔ ローカライズ名の解決と、ブロック/アイテムモデル＋テクスチャの読み込みを行う
  - `ModelRender`: HelixToolkit SharpDX のシーンを構築して `Viewport3DX` に描画し、`Output()` で `<名前空間>_<ID>_x<サイズ>.png` を書き出す
  - `ColorConverter`

モデルの読み込みや描画の挙動を変える場合、修正先は通常このリポジトリではなく MCLibs です。MCLibs 側でコミットするとこちらのサブモジュール参照が更新されるので、それは別途こちらでコミットする必要があります。

## リポジトリ構成

```
MCItemViewer/                 リポジトリ直下
├── MCItemViewer/             アプリ本体（MCItemViewer.sln と MCItemViewer.csproj）
│   ├── Models/               ToolConfig（設定データ）
│   ├── Services/             ApplicationHostService（起動時にメインウィンドウを表示）
│   ├── ViewModels/
│   │   ├── Pages/            MainPageViewModel
│   │   └── Windows/          MainWindowViewModel
│   ├── Views/
│   │   ├── Pages/            MainPage（アイテムの表示と PNG 出力）
│   │   └── Windows/          MainWindow（NavigationView のホスト）
│   └── submodules/MCLibs/    サブモジュール（MCLibs）
└── MCItemViewerTests/        xUnit v3 のユニットテスト（MCItemViewer.sln に含まれる）
    ├── Helpers/              テスト用ヘルパー（一時フォルダ、ダミーの .minecraft の作成）
    ├── Models/               ToolConfig のテスト
    └── ViewModels/
        ├── Pages/            MainPageViewModel のテスト
        └── Windows/          MainWindowViewModel のテスト
```

## アプリ本体（このリポジトリ）

WPF-UI の `NavigationView` によるページ遷移構成です（参考: ComfyUIRunWorkflow）。MVVM は CommunityToolkit.Mvvm（`[ObservableProperty]`、`[RelayCommand]`）、UI は WPF-UI（`FluentWindow`、`NavigationView`、`AutoSuggestBox`、ダークテーマ）を使用しています。

### 起動と DI

- `App.xaml.cs` で .NET Generic Host を構築し、`OnStartup` でホストを開始します（`App.xaml` に `StartupUri` はありません）。
- DI コンテナには次をシングルトンで登録しています。新しいページを追加するときは、ページと ViewModel の両方をここに登録してください。
  - `Setting<ToolConfig>`（カレントディレクトリの `tool_settings.json`。全 ViewModel で同じインスタンスを共有する）
  - `AddNavigationViewPageProvider()`（WPF-UI.DependencyInjection。遷移先のページを DI コンテナから取得する）
  - `ApplicationHostService`（ホステッドサービス。起動時に `INavigationWindow` を取得して表示し、`MainPage` へ遷移する）
  - `INavigationWindow` → `MainWindow`、`MainWindowViewModel`
  - `MainPage`、`MainPageViewModel`
- View は `INavigableView<TViewModel>` / `INavigationWindow` を実装し、コンストラクタで ViewModel を受け取って `ViewModel` プロパティに保持し、`DataContext = this` とします。XAML のバインディングは `{Binding ViewModel.Xxx}` の形になります。

### ウィンドウ（`Views/Windows/MainWindow`）

- `NavigationView` のホストです。ナビゲーション項目は XAML（`NavigationView.MenuItems`）で定義しています。`NavigationViewItem` は STA スレッドでしか生成できず、ViewModel に置くとテストできないためです。
- `MainWindowViewModel` はタイトルと、ペインの開閉状態（`IsPaneOpen`）を持ちます。開閉状態は `ToolConfig.WindowSetting.IsPaneOpen` から復元し、変更のたびに保存します。

### メインページ（`Views/Pages/MainPage`）

- `ViewModels/Pages/MainPageViewModel.cs` は `Setting<ToolConfig>`、バージョン/名前空間/出力サイズのリスト、`BlockModelLoarder`、`ModelRender` を保持します。Minecraft のパスやバージョンを変更すると、ローダーは破棄されて再生成されます。設定は変更のたびに即座に保存されます。
- `Views/Pages/MainPage.xaml.cs` はイベントの配線が中心です。アイテムID とアイテム名の AutoSuggestBox の相互同期と、サイズ/アイテム/背景色が変わっていなければ再描画をスキップする判定は、ViewModel の `TryResolveItem`（入力元は `ItemInputSource`）、`NeedsRedraw`、`SaveDrawnState` に切り出してあり、code-behind はそれらを呼んで結果をコントロールに反映するだけです。
- ページはシングルトンのため、別ページから戻ってきても同じインスタンスが再表示されます（`Loaded` は再度発火するが、サイズ等が変わっていなければ再描画しない）。

## PNG 出力の流れ（分かりにくい点）

`Viewport3DX` は2つあります。画面表示用の `BlockView` と、出力専用の `ImageRender`（高さ 0 の行に配置）です。

1. `BeginOutputPngCommand` がビューポートを選択した出力サイズにリサイズし、`_isOutputPng` を立てて描画する。
2. `ImageRender.OnRendered` は複数回発火するため、code-behind で 100ms の `DispatcherTimer` を使って間引く。
3. タイマーの Tick で `OutputPng` が呼ばれ、`ModelRender.Output` でビューポートをキャプチャする。
4. `NotifyArea` に3秒間通知を表示する。
