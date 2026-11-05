using System.ComponentModel;
using System.Globalization;
using MCItemViewer.Resources;

namespace MCItemViewer.Helpers
{
    /// <summary>
    /// GUI の表示文言を Strings.resx（既定: 日本語）/ Strings.en.resx（英語）から解決するシングルトン。
    /// XAML から <c>{Binding Source={x:Static helpers:LocalizationManager.Instance}, Path=[キー]}</c>
    /// のようにインデクサーへバインドすることで、<see cref="CurrentCulture"/> の変更時に全画面の文言が即座に切り替わる。
    /// </summary>
    public sealed class LocalizationManager : INotifyPropertyChanged
    {
        /// <summary>
        /// アプリ全体で共有するシングルトンインスタンス
        /// </summary>
        public static LocalizationManager Instance { get; } = new();

        /// <summary>
        /// 既定の言語コード（対応していない言語コードの代わりに使う）
        /// </summary>
        public const string DefaultLanguage = "ja";

        /// <summary>
        /// 対応している言語コードの一覧
        /// </summary>
        public static IReadOnlyList<string> SupportedLanguages { get; } = ["ja", "en"];

        /// <summary>
        /// 言語コードが対応している言語ならそのまま、対応していなければ既定の言語コードを返す。
        /// </summary>
        /// <param name="language">言語コード</param>
        /// <returns>対応している言語コード</returns>
        public static string NormalizeLanguage(string? language)
        {
            return language != null && SupportedLanguages.Contains(language) ? language : DefaultLanguage;
        }

        /// <summary>
        /// 表示言語の言語コードから、Minecraft の言語ファイルの言語コード（アイテム名の読み込みに使う）を求める。
        /// 対応していない言語コードは既定の言語（日本語）として扱う。
        /// </summary>
        /// <param name="language">表示言語の言語コード（"ja" / "en"）</param>
        /// <returns>Minecraft の言語コード（"ja_jp" / "en_us"）</returns>
        public static string ToMinecraftLanguageCode(string? language)
        {
            return NormalizeLanguage(language) switch
            {
                "en" => "en_us",
                _ => "ja_jp",
            };
        }

        /// <summary>
        /// 現在の表示言語
        /// </summary>
        private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;

        private LocalizationManager() { }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 現在の表示言語。設定すると <see cref="CultureInfo.CurrentUICulture"/> /
        /// <see cref="CultureInfo.DefaultThreadCurrentUICulture"/> も更新し、インデクサーと本プロパティの変更を通知する。
        /// </summary>
        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture.Equals(value))
                {
                    return;
                }

                _currentCulture = value;
                CultureInfo.CurrentUICulture = value;
                CultureInfo.DefaultThreadCurrentUICulture = value;

                // WPF のインデクサーバインディングの規約に従い "Item[]" で変更を通知する
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
            }
        }

        /// <summary>
        /// 指定したキーの表示文言を現在の言語で取得する。キーが存在しない場合はキーそのものを返す。
        /// </summary>
        /// <param name="key">リソースのキー</param>
        public string this[string key] => Strings.ResourceManager.GetString(key, _currentCulture) ?? key;

        /// <summary>
        /// 指定したキーの表示文言を書式文字列として、引数を埋め込んだ文字列を取得する。
        /// </summary>
        /// <param name="key">リソースのキー</param>
        /// <param name="args">埋め込む値</param>
        public string Format(string key, params object[] args) => string.Format(_currentCulture, this[key], args);
    }
}
