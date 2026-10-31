# ビルド / 実行

ソリューションはリポジトリ直下ではなく、内側の `MCItemViewer/` ディレクトリにあります。

```bash
git submodule update --init --recursive   # ビルドには MCLibs が必要
dotnet build MCItemViewer/MCItemViewer.sln
dotnet run --project MCItemViewer/MCItemViewer.csproj
```

## 補足

- テストや lint はまだ設定されていません。
- Costura.Fody（`FodyWeavers.xml`）により、依存ライブラリは出力 exe に埋め込まれます。
- 実行時、アプリは**カレントディレクトリ**の `tool_settings.json`（gitignore 対象）を読み書きします。
- 何かを表示するには、バージョンがインストール済みの有効な `.minecraft` ディレクトリ（デフォルトは `%APPDATA%\.minecraft`）が必要です。
