# 注意すべき癖・既知の制限

- `App` と `Models/ToolConfig` は名前空間 `MCBlockViewer`（姉妹プロジェクトからの持ち越し）を使っており、Views/ViewModels は `MCItemViewer` です。
- 言語は `ja_jp`、名前空間は `minecraft` に固定されています。`ModMinecraftPath` は設定と UI に存在しますが、MOD アイテムはまだ未対応です。
- ブロックエンティティ（チェスト、ベッドなど）は描画されません。
- 1.21.4 以降ではバイオームの色が反映されない場合があります。
