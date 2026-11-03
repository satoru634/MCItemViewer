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
│   └── submodules/MCLibs/    サブモジュール（MCLibs）
└── MCItemViewerTests/        xUnit v3 のユニットテスト（MCItemViewer.sln に含まれる）
    ├── Helpers/              テスト用ヘルパー（一時フォルダ、ダミーの .minecraft の作成）
    ├── Models/               ToolConfig のテスト
    └── ViewModels/           MainWindowViewModel のテスト
```

## アプリ本体（このリポジトリ）

単一ウィンドウ構成。MVVM は CommunityToolkit.Mvvm（`[ObservableProperty]`、`[RelayCommand]`）、UI は WPF-UI（`FluentWindow`、`AutoSuggestBox`、ダークテーマ）を使用しています。

- `ViewModels/MainWindowViewModel.cs` は `Setting<ToolConfig>`、バージョン/名前空間/出力サイズのリスト、`BlockModelLoarder`、`ModelRender` を保持します。Minecraft のパスやバージョンを変更すると、ローダーは破棄されて再生成されます。設定は変更のたびに即座に保存されます。
- `Views/MainWindow.xaml.cs` はイベントの配線が中心です。アイテムID とアイテム名の AutoSuggestBox の相互同期と、サイズ/アイテム/背景色が変わっていなければ再描画をスキップする判定は、ViewModel の `TryResolveItem`（入力元は `ItemInputSource`）、`NeedsRedraw`、`SaveDrawnState` に切り出してあり、code-behind はそれらを呼んで結果をコントロールに反映するだけです。
- ViewModel は XAML 内で直接生成されます（DataContext に `<vm:MainWindowViewModel/>`、既定のコンストラクタを使用）。設定ファイルのパスを受け取る `internal` コンストラクタはテスト用です。`App.xaml.cs` の Generic Host / DI はテンプレートの残骸で使われていません（`ConfigureServices` は例外を投げ、`OnStartup` もフックされていない）。新しいコードを `App.GetService` 経由にしないでください。

## PNG 出力の流れ（分かりにくい点）

`Viewport3DX` は2つあります。画面表示用の `BlockView` と、出力専用の `ImageRender`（高さ 0 の行に配置）です。

1. `BeginOutputPngCommand` がビューポートを選択した出力サイズにリサイズし、`_isOutputPng` を立てて描画する。
2. `ImageRender.OnRendered` は複数回発火するため、code-behind で 100ms の `DispatcherTimer` を使って間引く。
3. タイマーの Tick で `OutputPng` が呼ばれ、`ModelRender.Output` でビューポートをキャプチャする。
4. `NotifyArea` に3秒間通知を表示する。
