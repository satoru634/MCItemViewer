# 注意すべき癖・既知の制限

- `App` と `Models/ToolConfig` は名前空間 `MCBlockViewer`（姉妹プロジェクトからの持ち越し）を使っており、Views/ViewModels は `MCItemViewer` です。後から追加した `Models/ThemeOption`・`Models/LanguageOption` は `MCItemViewer.Models` です。テストはフォルダ構成に合わせ、`ToolConfig` のテストも `MCItemViewerTests.Models` に置いています。
- 言語は `ja_jp`、名前空間は `minecraft` に固定されています。`ModMinecraftPath` は設定と UI に存在しますが、MOD アイテムはまだ未対応です。
- ブロックエンティティ（チェスト、ベッドなど）は描画されません。
- 1.21.4 以降ではバイオームの色が反映されない場合があります。
- `MainPage` には `ScrollViewer.CanContentScroll="False"` を指定しています。WPF-UI の `NavigationView` は既定でページをスクロール領域に入れるため、指定しないとページの高さが確定せず、3D ビュー（`BlockViewArea`）のサイズが決まりません。
- `MainPage` の code-behind は `_isComponentInitialized` が立つまで（`InitializeComponent` 中）、バインディングの反映で発火する `SelectionChanged`・`TextChanged` などを処理しません。この間はコントロールが揃っておらず、またバージョン変更処理（jar の再読み込み）が無駄に走るためです。
- テーマの適用には WPF-UI の `ThemeService` ではなく `Services/AppThemeService` を使います。`ThemeService.SetTheme` は Mica 背景を前提に適用してウィンドウ背景を透過にするため、Mica を使わない MainWindow では実行中に切り替えると背景が黒く抜けます。`AppThemeService` は `WindowBackdropType.None` で適用し、単色背景を保ちます。
- `ToolConfig.WindowSetting.Theme` は `JsonStringEnumConverter` により `"Light"` / `"Dark"` の名前で保存します（数値でも読み込めます）。`ApplicationTheme` の数値は Unknown=0、Dark=1、Light=2 で、見た目と逆に感じやすいため名前にしています。
- `ToolConfig.Language`（設定ページの言語）は保存のみで、UI の英語化はまだです。Minecraft のアイテム名の言語（`ja_jp` 固定）とも連動していません。
