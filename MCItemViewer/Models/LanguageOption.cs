namespace MCItemViewer.Models
{
    /// <summary>
    /// 言語選択コンボボックスの1項目（言語コード＋表示名）。
    /// 表示名は翻訳せず、常にその言語自身の呼称（"日本語" / "English"）を表示する。
    /// </summary>
    /// <param name="Key">言語コード（"ja" / "en"）</param>
    /// <param name="Label">表示名</param>
    public record LanguageOption(string Key, string Label);
}
