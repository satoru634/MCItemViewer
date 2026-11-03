# CLAUDE.md

MCItemViewer は、Minecraft Java Edition（1.13 以降）のアイテムモデルをクライアント `.jar` から直接描画し PNG 出力する、Windows 専用 WPF アプリ（.NET 8、x64）です。コメント・UI 文字列・ドキュメントは日本語で書いてください。

## 開発ルール

- ファイルの変更や追加を行う前に、作業ブランチを切ること。
- クラスを追加・変更したら、対応するユニットテストを追加すること（テストフレームワーク: xUnit v3、配置先: `MCItemViewerTests/<同じ名前空間>/`）。
- ユニットテストがパスするまで次の実装に進まないこと。
- 指示があるまでコミットしないこと。
- `作業ブランチを切る -> 実装 -> テスト実装`を1タスクとする。
- コミットは1タスク毎に行い、その都度コミットしてよいか確認すること。
- ファイルやディレクトリ構成を変更した場合は、CLAUDE.md および `.claude` 配下に記載の該当箇所も変更する。
- プルリクマージ後は、作業ブランチをローカル・リモート共に削除し、master ブランチを最新にする。

## ビルド / テスト

```bash
git submodule update --init --recursive
dotnet build MCItemViewer/MCItemViewer.sln
dotnet test MCItemViewer/MCItemViewer.sln
```

## 詳細ドキュメント

作業内容に応じて必要なものを参照してください。

- [.claude/docs/build.md](.claude/docs/build.md) — ビルド・実行手順、実行時に必要な環境
- [.claude/docs/architecture.md](.claude/docs/architecture.md) — MCLibs サブモジュールとの役割分担、アプリ本体の構成、PNG 出力の流れ
- [.claude/docs/quirks.md](.claude/docs/quirks.md) — 名前空間の不一致などの癖、既知の制限

モデルの読み込み・描画の挙動を変える場合、修正先は通常このリポジトリではなくサブモジュール MCLibs です（詳細は architecture.md）。
