namespace MCItemViewerTests.Helpers
{
    /// <summary>
    /// <see cref="MCItemViewer.Helpers.LocalizationManager.Instance"/> の表示言語を書き換える（または表示言語に依存する）テストのコレクション。
    /// 表示言語はプロセス全体で共有される静的状態のため、並列実行すると互いに干渉する。
    /// このコレクションに所属するテストクラスは互いに並列実行されない。
    /// </summary>
    [CollectionDefinition("Culture", DisableParallelization = true)]
    public class CultureCollection
    {
    }
}
