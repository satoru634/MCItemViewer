using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using OpenCvSharp;

namespace MCItemViewerTests.Helpers
{
    /// <summary>
    /// テスト用の .minecraft ディレクトリ構成を作成するヘルパー
    /// </summary>
    /// <remarks>
    /// アイテム名は表示言語に応じて en_us.json（jar 内）または ja_jp.json（アセットインデックス経由で参照される assets/objects 配下）から読み込むため、両方を作成する。
    /// 英語名は言語ファイルのキーの末尾（例: "block.minecraft.stone" → "stone"）とする。
    /// </remarks>
    public static class TestMinecraft
    {
        /// <summary>
        /// アセットインデックスの ID（全バージョン共通）
        /// </summary>
        private const string AssetIndexId = "test";

        /// <summary>
        /// 1つのバージョンを作成する。
        /// </summary>
        /// <param name="minecraftPath">.minecraft とみなすディレクトリ</param>
        /// <param name="version">バージョン</param>
        /// <param name="items">アイテム（キー: 言語ファイルのキー、値: 日本語名）。例: "block.minecraft.stone" → "石"</param>
        /// <returns>作成した jar のパス</returns>
        public static string CreateVersion(string minecraftPath, string version, Dictionary<string, string> items)
        {
            string versionDir = Path.Combine(minecraftPath, "versions", version);
            Directory.CreateDirectory(versionDir);

            // jar 本体（英語の言語ファイル・アイテムモデル・テクスチャ）
            string jarPath = Path.Combine(versionDir, $"{version}.jar");
            using (var zip = ZipFile.Open(jarPath, ZipArchiveMode.Create))
            {
                var enUs = items.ToDictionary(item => item.Key, item => item.Key.Split('.').Last());
                AddText(zip, "assets/minecraft/lang/en_us.json", JsonSerializer.Serialize(enUs));

                foreach (string key in items.Keys)
                {
                    string name = key.Split('.').Last();
                    AddText(zip, $"assets/minecraft/models/item/{name}.json",
                        $$"""{ "textures": { "layer0": "minecraft:item/{{name}}" } }""");
                    AddPng(zip, $"assets/minecraft/textures/item/{name}.png");
                }
            }

            // バージョン情報とアセットインデックス、日本語の言語ファイル
            string hash = CreateHash(version);
            File.WriteAllText(Path.Combine(versionDir, $"{version}.json"),
                JsonSerializer.Serialize(new { assetIndex = new { id = $"{AssetIndexId}_{version}" } }));

            string indexDir = Path.Combine(minecraftPath, "assets", "indexes");
            Directory.CreateDirectory(indexDir);
            File.WriteAllText(Path.Combine(indexDir, $"{AssetIndexId}_{version}.json"),
                JsonSerializer.Serialize(new { objects = new Dictionary<string, object> { ["minecraft/lang/ja_jp.json"] = new { hash, size = 0 } } }));

            string objectDir = Path.Combine(minecraftPath, "assets", "objects", hash.Substring(0, 2));
            Directory.CreateDirectory(objectDir);
            File.WriteAllText(Path.Combine(objectDir, hash), JsonSerializer.Serialize(items), Encoding.UTF8);

            return jarPath;
        }

        /// <summary>
        /// 中身のない jar（バージョン一覧の対象になるだけのファイル）を作成する。
        /// </summary>
        /// <param name="minecraftPath">.minecraft とみなすディレクトリ</param>
        /// <param name="version">バージョン</param>
        public static void CreateEmptyVersion(string minecraftPath, string version)
        {
            string versionDir = Path.Combine(minecraftPath, "versions", version);
            Directory.CreateDirectory(versionDir);
            using var zip = ZipFile.Open(Path.Combine(versionDir, $"{version}.jar"), ZipArchiveMode.Create);
        }

        /// <summary>
        /// バージョンごとに異なる 40 桁のハッシュ文字列を生成する。
        /// </summary>
        private static string CreateHash(string version)
        {
            string hex = Convert.ToHexString(Encoding.UTF8.GetBytes(version)).ToLowerInvariant();
            return hex.PadRight(40, '0').Substring(0, 40);
        }

        /// <summary>
        /// zip に文字列のエントリを追加する。
        /// </summary>
        private static void AddText(ZipArchive zip, string entryName, string text)
        {
            using var stream = zip.CreateEntry(entryName).Open();
            byte[] data = Encoding.UTF8.GetBytes(text);
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// zip に 16x16 の白い PNG のエントリを追加する。
        /// </summary>
        private static void AddPng(ZipArchive zip, string entryName)
        {
            using var mat = new Mat(16, 16, MatType.CV_8UC4, new Scalar(255, 255, 255, 255));
            Cv2.ImEncode(".png", mat, out byte[] png);
            using var stream = zip.CreateEntry(entryName).Open();
            stream.Write(png, 0, png.Length);
        }
    }
}
