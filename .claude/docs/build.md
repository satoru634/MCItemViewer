# ビルド / 実行

ソリューションはリポジトリ直下ではなく、内側の `MCItemViewer/` ディレクトリにあります。

```bash
git submodule update --init --recursive   # ビルドには MCLibs が必要
dotnet build MCItemViewer/MCItemViewer.sln
dotnet run --project MCItemViewer/MCItemViewer.csproj
dotnet test MCItemViewer/MCItemViewer.sln
```

## テスト

- テストプロジェクトはリポジトリ直下の `MCItemViewerTests/`（xUnit v3: `xunit.v3`）です。ソリューション（`MCItemViewer/MCItemViewer.sln`）に含まれ、`MCItemViewer.csproj` を `ProjectReference` で参照しています。
  - `MCItemViewer/` 配下に置くと、本体の csproj がテストコードまでコンパイル対象にしてしまうため、リポジトリ直下に置いています。
- `dotnet test`（VSTest）からテストを検出・実行するため、`xunit.runner.visualstudio`（3.x）も参照しています。外すと「使用できるテストはありません」となり、テストが1件も実行されません。
- MCLibs のテストはサブモジュール側（`submodules/MCLibs/MCLibsTests`）にあり、このソリューションには含みません。MCLibs 側で `dotnet test MCLibs.sln` を実行してください。
- テストファイルは `MCItemViewerTests/<フォルダ>/<クラス名>Tests.cs` に置きます（例: `MCItemViewerTests/ViewModels/Pages/MainPageViewModelTests.cs`）。名前空間は `MCItemViewerTests.<フォルダ>`（例: `MCItemViewerTests.ViewModels.Pages`）です。
- テストメソッド名は `<メソッド名>_<条件>_<期待結果>`（英語）とし、メソッドごとに `// ── <メソッド名> ───` の区切りコメントを入れます。
- `AssemblyInfo.cs` の `InternalsVisibleTo("MCItemViewerTests")` により、`internal` なクラスもテストから参照できます。
- ViewModel は DI 用のコンストラクタで `Setting<ToolConfig>` を受け取ります。テストでは一時フォルダの設定ファイルを読み込んだ `Setting<ToolConfig>` を渡して生成します（アプリ本体のようにカレントディレクトリの `tool_settings.json` や実環境の `.minecraft` は使わない）。
- `IThemeService` を受け取る ViewModel（`SettingPageViewModel`）には、テーマを実際には適用せず呼び出しを記録する `Helpers/FakeThemeService` を渡します。
- 表示言語（`LocalizationManager.Instance`）はプロセス全体で共有されるため、表示言語を切り替える・表示言語に依存するテストクラスには `[Collection("Culture")]`（並列実行しない）を付け、`Helpers/CultureScope` で言語を切り替えてテスト後に元へ戻します。
- テストでは一時フォルダにダミーの `.minecraft` を作ります（`Helpers/TestMinecraft`）。アプリは言語を `ja_jp` に固定しているため、jar に加えて、アセットインデックスと `assets/objects` 配下の日本語の言語ファイルも作成します。
- 次のものはテスト対象外です。
  - `Viewport3DX`（STA スレッドと GPU）が必要な描画処理（`UpdateBlockModel`、`BeginOutputPng`）
  - ダイアログを表示する `OpenFolder`
  - View（`MainWindow`、`MainPage`、`SettingPage`）と `App`・`ApplicationHostService`・`AppThemeService`。STA スレッドと WPF の `Application` が必要なためです。ロジックは ViewModel に切り出してテストしており、code-behind にはコントロールとの値の受け渡しとイベントの配線だけを残しています。

## 補足

- Costura.Fody（`FodyWeavers.xml`）により、依存ライブラリは出力 exe に埋め込まれます。
- 実行時、アプリは**カレントディレクトリ**の `tool_settings.json`（gitignore 対象）を読み書きします。
- 何かを表示するには、バージョンがインストール済みの有効な `.minecraft` ディレクトリ（デフォルトは `%APPDATA%\.minecraft`）が必要です。
