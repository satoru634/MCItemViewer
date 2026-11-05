# 注意すべき癖・既知の制限

- `App` と `Models/ToolConfig` は名前空間 `MCBlockViewer`（姉妹プロジェクトからの持ち越し）を使っており、Views/ViewModels は `MCItemViewer` です。後から追加した `Models/ThemeOption`・`Models/LanguageOption` は `MCItemViewer.Models` です。テストはフォルダ構成に合わせ、`ToolConfig` のテストも `MCItemViewerTests.Models` に置いています。
- 名前空間は `minecraft` に固定されています。`ModMinecraftPath` は設定と UI に存在しますが、MOD アイテムはまだ未対応です。
- ブロックエンティティ（チェスト、ベッドなど）は描画されません。
- 1.21.4 以降ではバイオームの色が反映されない場合があります。
- `MainPage` には `ScrollViewer.CanContentScroll="False"` を指定しています。WPF-UI の `NavigationView` は既定でページをスクロール領域に入れるため、指定しないとページの高さが確定せず、3D ビュー（`BlockViewArea`）のサイズが決まりません。
- `MainPage` の code-behind は `_isComponentInitialized` が立つまで（`InitializeComponent` 中）、バインディングの反映で発火する `SelectionChanged`・`TextChanged` などを処理しません。この間はコントロールが揃っておらず、またバージョン変更処理（jar の再読み込み）が無駄に走るためです。
- テーマの適用には WPF-UI の `ThemeService` ではなく `Services/AppThemeService` を使います。`ThemeService.SetTheme` は Mica 背景を前提に適用してウィンドウ背景を透過にするため、Mica を使わない MainWindow では実行中に切り替えると背景が黒く抜けます。`AppThemeService` は `WindowBackdropType.None` で適用し、単色背景を保ちます。
- `ToolConfig.WindowSetting.Theme` は `JsonStringEnumConverter` により `"Light"` / `"Dark"` の名前で保存します（数値でも読み込めます）。`ApplicationTheme` の数値は Unknown=0、Dark=1、Light=2 で、見た目と逆に感じやすいため名前にしています。
- 設定ページで表示言語を切り替えたとき、画面の文言は即座に切り替わりますが、メインページのアイテム名はメインページを再表示したとき（`Page_Loaded`）に切り替わります。アイテム名はメインページにしか表示されないためです。
- `PrevItemName`（前回のアイテム名）は保存時の言語の名前です。言語を切り替えた後は、次にアイテムを描画したときに新しい言語の名前で保存し直されます。
- 英語の文言はサテライトアセンブリ（出力フォルダの `en/MCItemViewer.resources.dll`）になります。Costura.Fody は exe に埋め込まないため、配布時は `en` フォルダも一緒に配置してください。無い場合は日本語で表示されます。
