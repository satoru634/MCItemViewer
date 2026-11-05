using MCItemViewer.ViewModels;
using MCItemViewer.ViewModels.Pages;
using MCModelRenderer.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace MCItemViewer.Views.Pages
{
    /// <summary>
    /// メインページ（アイテムの表示と PNG 出力）の View。
    /// DataContext に自身を設定し、XAML からは ViewModel プロパティ経由で <see cref="MainPageViewModel"/> にアクセスする。
    /// </summary>
    public partial class MainPage : INavigableView<MainPageViewModel>
    {
        /// <summary>
        /// このページに対応する ViewModel
        /// </summary>
        public MainPageViewModel ViewModel { get; }

        /// <summary>
        /// レンダリング完了を待つためのタイマー
        /// </summary>
        private readonly DispatcherTimer _renderCompleteTimer;

        /// <summary>
        /// InitializeComponent が完了したかどうか。
        /// InitializeComponent 中はバインディングの反映で各イベントが発火するが、コントロールがまだ揃っていないため処理しない。
        /// （旧 MainWindow では、この間 ViewModel が null で処理を飛ばしていたのと同じ扱い）
        /// </summary>
        private bool _isComponentInitialized = false;

        /// <summary>
        /// DI コンテナから ViewModel を受け取って初期化する。
        /// </summary>
        /// <param name="viewModel">メインページの ViewModel</param>
        public MainPage(MainPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            _renderCompleteTimer = new DispatcherTimer();
            _renderCompleteTimer.Tick += RenderCompleteTimer_Tick;

            InitializeComponent();
            _isComponentInitialized = true;
        }

        /// <summary>
        /// ページが読み込まれたときに呼び出されるイベントハンドラ。
        /// ナビゲーションで再表示されたときも呼び出されるが、サイズやアイテムが変わっていなければ再描画しない。
        /// 設定ページで言語が切り替えられていた場合は、アイテム名をその言語で読み込み直してから再描画する。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyItemLanguage();
            UpdateBlockModel();
            return;
        }

        /// <summary>
        /// バージョンリストの選択が変更されたときに呼び出されるイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VersionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if ((_isComponentInitialized == false) || (combo.Items.Count == 0))
            {
                return;
            }

            ViewModel.ChangeVersion();
            return;
        }

        /// <summary>
        /// 名前空間の選択が変更されたときに呼び出されるイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NameSpace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if ((_isComponentInitialized == false) || (combo.Items.Count == 0))
            {
                return;
            }

            ViewModel.ChangeNameSpace();
            return;
        }

        /// <summary>
        /// AutoSuggestBoxのテキストが変更されたときに呼び出されるイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = (string)args.SelectedItem;
            UpdateBlockModel(ToInputSource(sender));

            return;
        }

        /// <summary>
        /// AutoSuggestBox上でキーが押されたときに呼び出されるイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SuggestBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Enterキー以外のキーが押された場合は何もしない
            if (e.Key != Key.Enter)
            {
                return;
            }

            // Enterキーが押された場合は、アイテムを更新
            AutoSuggestBox suggestBox = (AutoSuggestBox)sender;
            UpdateBlockModel(ToInputSource(suggestBox));

            return;
        }

        /// <summary>
        /// AutoSuggestBoxがフォーカスを失ったときに呼び出されるイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SuggestBox_LostFocus(object sender, RoutedEventArgs e)
        {
            AutoSuggestBox suggestBox = (AutoSuggestBox)sender;
            UpdateBlockModel(ToInputSource(suggestBox));

            return;
        }

        /// <summary>
        /// BackgroundColorのテキストが変更されたときに呼び出されるイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 色のフォーマットが正しくない場合は何もしない
           if (ColorConverter.CheckColorFormat(BackgroundColor.Text) == false)
            {
                return;
            }

            UpdateBlockModel();
            return;
        }

        /// <summary>
        /// BlockViewAreaのサイズが変更されたときに呼び出されるイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BlockViewArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBlockModel();
            return;
        }

        /// <summary>
        /// AutoSuggestBox から入力元を求める
        /// </summary>
        /// <param name="suggestBox">AutoSuggestBox</param>
        /// <returns>入力元</returns>
        private static ItemInputSource ToInputSource(AutoSuggestBox suggestBox)
        {
            return suggestBox.Name switch
            {
                "SuggestItemID" => ItemInputSource.ItemID,
                "SuggestItemName" => ItemInputSource.ItemName,
                _ => ItemInputSource.None,
            };
        }

        /// <summary>
        /// ブロックモデルを更新
        /// </summary>
        /// <param name="source">入力元</param>
        private void UpdateBlockModel(ItemInputSource source = ItemInputSource.None)
        {
            // InitializeComponent が完了していない場合は何もしない
            if (_isComponentInitialized == false)
            {
                return;
            }

            // 入力からアイテムIDとアイテム名を求め、SuggestBoxを更新
            if (ViewModel.TryResolveItem(source, SuggestItemID.Text, SuggestItemName.Text, out string itemID, out string itemName) == false)
            {
                // アイテムID・アイテム名が無効な場合は何もしない
                return;
            }

            SuggestItemID.Text = itemID;
            SuggestItemName.Text = itemName;

            // サイズやアイテムIDが変更されていない場合は何もしない
            int width = (int)BlockViewArea.ActualWidth;
            int height = (int)BlockViewArea.ActualHeight;
            if (ViewModel.NeedsRedraw(itemID, width, height, BackgroundColor.Text) == false)
            {
                return;
            }

            // BlockViewを更新
            ViewModel.UpdateBlockModel(itemName, width, height, BackgroundColor.Text, BlockView);

            // 描画した内容を記録
            ViewModel.SaveDrawnState(width, height, BackgroundColor.Text);
            return;
        }

        /// <summary>
        /// ImageRenderの描画が完了したときに呼び出されるイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageRender_OnRendered(object sender, EventArgs e)
        {
            // レンダリングが完了するまで複数回呼び出される可能性があるため、タイマーをリセット
            _renderCompleteTimer.Interval = TimeSpan.FromMilliseconds(100);
            _renderCompleteTimer.Start();
            return;
        }

        /// <summary>
        /// 出力タイマーのTickイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenderCompleteTimer_Tick(object? sender, EventArgs e)
        {
            // 出力処理を実行
            var task = ViewModel.OutputPng(ImageRender, NotifyArea);
            _renderCompleteTimer.Stop();
            return;
        }
    }
}
