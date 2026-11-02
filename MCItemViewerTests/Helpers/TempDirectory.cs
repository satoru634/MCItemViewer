using System.IO;

namespace MCItemViewerTests.Helpers
{
    /// <summary>
    /// テスト用の一時ディレクトリ。Dispose 時に配下ごと削除する。
    /// </summary>
    public sealed class TempDirectory : IDisposable
    {
        /// <summary>
        /// 一時ディレクトリのパス
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// 一時ディレクトリを作成する。
        /// </summary>
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MCItemViewerTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        /// <summary>
        /// 一時ディレクトリ配下のパスを生成する（ディレクトリは作成しない）。
        /// </summary>
        /// <param name="relativePaths">一時ディレクトリからの相対パス</param>
        /// <returns>結合したパス</returns>
        public string Combine(params string[] relativePaths)
        {
            return System.IO.Path.Combine([Path, .. relativePaths]);
        }

        /// <summary>
        /// 一時ディレクトリを削除する。
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
