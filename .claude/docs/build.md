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
- テストファイルは `MCItemViewerTests/<フォルダ>/<クラス名>Tests.cs` に置きます（例: `MCItemViewerTests/ViewModels/MainWindowViewModelTests.cs`）。名前空間は `MCItemViewerTests.<フォルダ>` です。
- テストメソッド名は `<メソッド名>_<条件>_<期待結果>`（英語）とし、メソッドごとに `// ── <メソッド名> ───` の区切りコメントを入れます。
- `MainWindowViewModel` は `internal` のため、`AssemblyInfo.cs` の `InternalsVisibleTo("MCItemViewerTests")` でテストから参照しています。
- `MainWindowViewModel` のテストは、設定ファイルのパスを受け取る `internal` コンストラクタで生成します。既定のコンストラクタはカレントディレクトリの `tool_settings.json` と実環境の `.minecraft` を使うため、テストでは使いません。
- テストでは一時フォルダにダミーの `.minecraft` を作ります（`Helpers/TestMinecraft`）。アプリは言語を `ja_jp` に固定しているため、jar に加えて、アセットインデックスと `assets/objects` 配下の日本語の言語ファイルも作成します。
- 次のものはテスト対象外です。
  - `Viewport3DX`（STA スレッドと GPU）が必要な描画処理（`UpdateBlockModel`、`BeginOutputPng`）
  - ダイアログを表示する `OpenFolder`
  - code-behind（`MainWindow.xaml.cs`）

## 補足

- Costura.Fody（`FodyWeavers.xml`）により、依存ライブラリは出力 exe に埋め込まれます。
- 実行時、アプリは**カレントディレクトリ**の `tool_settings.json`（gitignore 対象）を読み書きします。
- 何かを表示するには、バージョンがインストール済みの有効な `.minecraft` ディレクトリ（デフォルトは `%APPDATA%\.minecraft`）が必要です。
