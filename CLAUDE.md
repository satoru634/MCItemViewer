# CLAUDE.md

このファイルは、Claude Code（claude.ai/code）がこのリポジトリのコードを扱う際のガイダンスを提供します。

## 概要

MCItemViewer は Windows 専用の WPF デスクトップアプリ（.NET 8、x64）です。Minecraft Java Edition（1.13 以降）のアイテムモデルを、ゲームを起動せずにバニラクライアントの `.jar` から直接描画し、PNG として出力できます。UI 文字列・コードコメント・README はすべて日本語なので、新たに追加するコメントや UI 文字列も日本語で書いてください。

## ビルド / 実行

ソリューションはリポジトリ直下ではなく、内側の `MCItemViewer/` ディレクトリにあります。

```bash
git submodule update --init --recursive   # ビルドには MCLibs が必要
dotnet build MCItemViewer/MCItemViewer.sln
dotnet run --project MCItemViewer/MCItemViewer.csproj
```

- テストや lint は設定されていません。
- Costura.Fody（`FodyWeavers.xml`）により、依存ライブラリは出力 exe に埋め込まれます。
- 実行時、アプリは**カレントディレクトリ**の `tool_settings.json`（gitignore 対象）を読み書きします。何かを表示するには、バージョンがインストール済みの有効な `.minecraft` ディレクトリ（デフォルトは `%APPDATA%\.minecraft`）が必要です。

## アーキテクチャ

### 2リポジトリ構成
Minecraft 固有の処理のほとんどは、git サブモジュール `MCItemViewer/submodules/MCLibs`（別リポジトリ `satoru634/MCLibs`。同じ作者の他ツールと共用）にあります。csproj は `submodules\**` をコンパイル対象から除外し、代わりにその中の2つのプロジェクトを `ProjectReference` で参照しています。

- **MCToolsCommonLib** — 汎用ヘルパー群。`JarLoader`（クライアント jar から assets/lang/JSON/テクスチャを読み込む）、`Setting<T>`（JSON 設定の永続化）、`ItemBaseViewModel<T>`（ComboBox バインド用のリスト＋選択インデックス）、`NotifyInfo`、`CommonLib.GetMinecraftProgramFiles/GetMinecraftProgramFilePath`（バージョン検出）。
- **MCModelRenderer** — `BlockModelLoarder`（綴りはこのまま。`JarLoader` を継承し、アイテムID ↔ ローカライズ名の解決と、ブロック/アイテムモデル＋テクスチャの読み込みを行う）、`ModelRender`（HelixToolkit SharpDX のシーンを構築して `Viewport3DX` に描画し、`Output()` で `<名前空間>_<ID>_x<サイズ>.png` を書き出す）、`ColorConverter`。

モデルの読み込みや描画の挙動を変える場合、修正先は通常このリポジトリではなく MCLibs です。MCLibs 側でコミットするとこちらのサブモジュール参照が更新されるので、それは別途こちらでコミットする必要があります。

### アプリ本体（このリポジトリ）
単一ウィンドウ構成。MVVM は CommunityToolkit.Mvvm（`[ObservableProperty]`、`[RelayCommand]`）、UI は WPF-UI（`FluentWindow`、`AutoSuggestBox`、ダークテーマ）を使用しています。

- `ViewModels/MainWindowViewModel.cs` は `Setting<ToolConfig>`、バージョン/名前空間/出力サイズのリスト、`BlockModelLoarder`、`ModelRender` を保持します。Minecraft のパスやバージョンを変更すると、ローダーは破棄されて再生成されます。設定は変更のたびに即座に保存されます。
- `Views/MainWindow.xaml.cs` には単なる配線ではなく実質的なロジックがあります。アイテムID とアイテム名の AutoSuggestBox をローダー経由で相互に同期させ、サイズ/アイテム/背景色が変わっていなければ再描画をスキップします。
- ViewModel は XAML 内で直接生成されます（DataContext に `<vm:MainWindowViewModel/>`）。`App.xaml.cs` の Generic Host / DI はテンプレートの残骸で使われていません（`ConfigureServices` は例外を投げ、`OnStartup` もフックされていない）。新しいコードを `App.GetService` 経由にしないでください。

### PNG 出力の流れ（分かりにくい点）
`Viewport3DX` は2つあります。画面表示用の `BlockView` と、出力専用の `ImageRender`（高さ 0 の行に配置）です。`BeginOutputPngCommand` がビューポートを選択した出力サイズにリサイズし、`_isOutputPng` を立てて描画します。`ImageRender.OnRendered` は複数回発火するため、code-behind で 100ms の `DispatcherTimer` を使って間引いています。タイマーの Tick で `OutputPng` が呼ばれ、`ModelRender.Output` でビューポートをキャプチャし、`NotifyArea` に3秒間通知を表示します。

### 注意すべき癖
- `App` と `Models/ToolConfig` は名前空間 `MCBlockViewer`（姉妹プロジェクトからの持ち越し）を使っており、Views/ViewModels は `MCItemViewer` です。
- 言語は `ja_jp`、名前空間は `minecraft` に固定されています。`ModMinecraftPath` は設定と UI に存在しますが、MOD アイテムはまだ未対応です。
- ブロックエンティティ（チェスト、ベッドなど）は描画されず、1.21.4 以降ではバイオームの色が反映されない場合があります。
