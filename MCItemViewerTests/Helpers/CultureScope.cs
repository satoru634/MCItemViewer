using System.Globalization;
using MCItemViewer.Helpers;

namespace MCItemViewerTests.Helpers
{
    /// <summary>
    /// テスト中だけ <see cref="LocalizationManager.Instance"/> の表示言語を切り替え、Dispose 時に元の言語へ戻す。
    /// </summary>
    /// <remarks>
    /// 使用するテストクラスには <c>[Collection("Culture")]</c> を付け、並列実行させないこと。
    /// </remarks>
    public sealed class CultureScope : IDisposable
    {
        /// <summary>
        /// 切り替え前の表示言語
        /// </summary>
        private readonly CultureInfo _original;

        /// <summary>
        /// 切り替え前の既定のスレッドの UI カルチャ
        /// </summary>
        private readonly CultureInfo? _originalDefaultThread;

        /// <summary>
        /// 切り替え前の現在のスレッドの UI カルチャ
        /// </summary>
        private readonly CultureInfo _originalCurrentUI;

        /// <summary>
        /// 表示言語を指定した言語に切り替える。
        /// </summary>
        /// <param name="cultureName">切り替える言語（"ja"、"en" など）</param>
        public CultureScope(string cultureName)
        {
            _original = LocalizationManager.Instance.CurrentCulture;
            _originalDefaultThread = CultureInfo.DefaultThreadCurrentUICulture;
            _originalCurrentUI = CultureInfo.CurrentUICulture;
            LocalizationManager.Instance.CurrentCulture = new CultureInfo(cultureName);
        }

        /// <summary>
        /// 表示言語を元に戻す。
        /// </summary>
        public void Dispose()
        {
            LocalizationManager.Instance.CurrentCulture = _original;
            CultureInfo.DefaultThreadCurrentUICulture = _originalDefaultThread;
            CultureInfo.CurrentUICulture = _originalCurrentUI;
        }
    }
}
