using System.ComponentModel;
using MCItemViewer.Helpers;
using Wpf.Ui.Appearance;

namespace MCItemViewer.Models
{
    /// <summary>
    /// テーマ選択コンボボックスの1項目（テーマ＋表示名）。
    /// 表示名は <see cref="LocalizationManager"/> から現在の言語で取得し、言語が切り替わると <see cref="Label"/> の変更を通知する。
    /// </summary>
    public sealed class ThemeOption : INotifyPropertyChanged
    {
        /// <summary>
        /// テーマ
        /// </summary>
        public ApplicationTheme Key { get; }

        /// <summary>
        /// 表示名のリソースキー
        /// </summary>
        public string LabelKey { get; }

        /// <summary>
        /// 現在の言語での表示名
        /// </summary>
        public string Label => LocalizationManager.Instance[LabelKey];

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// テーマと表示名のリソースキーを指定して初期化する。
        /// </summary>
        /// <param name="key">テーマ</param>
        /// <param name="labelKey">表示名のリソースキー</param>
        public ThemeOption(ApplicationTheme key, string labelKey)
        {
            Key = key;
            LabelKey = labelKey;

            // 選択肢は ViewModel（シングルトン）と同じだけ生存するため、購読は解除しない
            LocalizationManager.Instance.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(LocalizationManager.CurrentCulture))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Label)));
                }
            };
        }
    }
}
