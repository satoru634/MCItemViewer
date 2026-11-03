using MCItemViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using Wpf.Ui.Controls;
using System.Diagnostics;
using HelixToolkit.Wpf.SharpDX.Controls;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Input;
using MCModelRenderer.Utils;

namespace MCItemViewer.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        /// <summary>
        /// MainWindowViewModelのインスタンス
        /// </summary>
        private MainWindowViewModel _viewModel;

        /// <summary>
        /// レンダリング完了を待つためのタイマー
        /// </summary>
        private DispatcherTimer _renderCompleteTimer;

        /// <summary>
        /// MainWindowのコンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = (MainWindowViewModel)DataContext;

            _renderCompleteTimer = new DispatcherTimer();
            _renderCompleteTimer.Tick += RenderCompleteTimer_Tick;
        }

        /// <summary>
        /// ウィンドウが読み込まれたときに呼び出されるイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
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
            if ((_viewModel == null) || (combo.Items.Count == 0))
            {
                return;
            }

            _viewModel.ChangeVersion();
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
            if ((_viewModel == null) || (combo.Items.Count == 0))
            {
                return;
            }

            _viewModel.ChangeNameSpace();
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
            // ViewModelがnullの場合は何もしない
            if (_viewModel == null)
            {
                return;
            }

            // 入力からアイテムIDとアイテム名を求め、SuggestBoxを更新
            if (_viewModel.TryResolveItem(source, SuggestItemID.Text, SuggestItemName.Text, out string itemID, out string itemName) == false)
            {
                // アイテムID・アイテム名が無効な場合は何もしない
                return;
            }

            SuggestItemID.Text = itemID;
            SuggestItemName.Text = itemName;

            // サイズやアイテムIDが変更されていない場合は何もしない
            int width = (int)BlockViewArea.ActualWidth;
            int height = (int)BlockViewArea.ActualHeight;
            if (_viewModel.NeedsRedraw(itemID, width, height, BackgroundColor.Text) == false)
            {
                return;
            }

            // BlockViewを更新
            _viewModel.UpdateBlockModel(itemName, width, height, BackgroundColor.Text, BlockView);

            // 描画した内容を記録
            _viewModel.SaveDrawnState(width, height, BackgroundColor.Text);
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
            // ViewModelがnullの場合は何もしない
            if (_viewModel == null)
            {
                return;
            }

            // 出力処理を実行
            var task = _viewModel.OutputPng(ImageRender, NotifyArea);
            _renderCompleteTimer.Stop();
            return;
        }
    }
}
