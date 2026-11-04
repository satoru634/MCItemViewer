namespace MCItemViewer.ViewModels
{
    /// <summary>
    /// 表示するアイテムを決める入力元
    /// </summary>
    public enum ItemInputSource
    {
        /// <summary>
        /// 入力なし（モデルローダーで選択中のアイテムを使う）
        /// </summary>
        None,

        /// <summary>
        /// アイテムID の入力欄
        /// </summary>
        ItemID,

        /// <summary>
        /// アイテム名の入力欄
        /// </summary>
        ItemName,
    }
}
