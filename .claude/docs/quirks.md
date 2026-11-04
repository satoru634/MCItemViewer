# 注意すべき癖・既知の制限

- `App` と `Models/ToolConfig` は名前空間 `MCBlockViewer`（姉妹プロジェクトからの持ち越し）を使っており、Views/ViewModels は `MCItemViewer` です。テストはフォルダ構成に合わせ、`ToolConfig` のテストも `MCItemViewerTests.Models` に置いています。
- 言語は `ja_jp`、名前空間は `minecraft` に固定されています。`ModMinecraftPath` は設定と UI に存在しますが、MOD アイテムはまだ未対応です。
- ブロックエンティティ（チェスト、ベッドなど）は描画されません。
- 1.21.4 以降ではバイオームの色が反映されない場合があります。
- `MainPage` には `ScrollViewer.CanContentScroll="False"` を指定しています。WPF-UI の `NavigationView` は既定でページをスクロール領域に入れるため、指定しないとページの高さが確定せず、3D ビュー（`BlockViewArea`）のサイズが決まりません。
- `MainPage` の code-behind は `_isComponentInitialized` が立つまで（`InitializeComponent` 中）、バインディングの反映で発火する `SelectionChanged`・`TextChanged` などを処理しません。この間はコントロールが揃っておらず、またバージョン変更処理（jar の再読み込み）が無駄に走るためです。
