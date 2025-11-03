using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Purge
{
     public class Purge
    {
        #region // プロパティ


        #endregion


        #region // クラス構築

        // コンストラクタ
        public Purge()
        {
        }


        // デストラクタ
        ~Purge()
        {
        }

        #endregion


        #region // 公開メソッド

        public static bool PurgeDirectoryTree(string fromPath, string toPath)
        {
            bool ret = ValidateAndNormalizePaths(ref fromPath, ref toPath);
            if (!ret)
            {
                return false;
            }

            // 出力先ディレクトリの空確認
            {
                var toDirInfo = new DirectoryInfo(toPath);
                if (toDirInfo.GetFiles().Length > 0 ||
                     toDirInfo.GetDirectories().Length > 0)
                {
                    // パスエラー：メッセージ出力
                    Console.WriteLine("パスエラー：出力先 にファイルまたはディレクトリが存在します。");
                    return false;
                }
            }
            // 最新リビジョン分のみをコピーする Purge 処理呼び出し
            return PurgeDirectoryRecursive(fromPath, toPath);

        }
        #endregion



        #region // 非公開メソッド
        // 引数チェック（正規化も行う）
        private static bool ValidateAndNormalizePaths(ref string fromPath, ref string toPath)
        {
            // 値が指定されているか
            if (String.IsNullOrEmpty(fromPath) == true ||
                    String.IsNullOrEmpty(toPath) == true)
            {
                // 引数エラー：メッセージ出力
                Console.WriteLine("引数エラー：パスが指定されていません。");
                return false;
            }

            // 同じパスが指定されていないか
            {
                // パス末尾の区切りを付与（DirectorySeparatorChar を使う）
                if (!Path.EndsInDirectorySeparator(fromPath))
                {
                    fromPath += Path.DirectorySeparatorChar;
                }
                if (!Path.EndsInDirectorySeparator(toPath))
                {
                    toPath += Path.DirectorySeparatorChar;
                }

                if (String.Compare(fromPath, toPath, false) == 0)
                {
                    // 引数エラー：メッセージ出力
                    Console.WriteLine("引数エラー：同じパスが指定されています。");
                    return false;
                }

                if (fromPath.StartsWith(toPath, StringComparison.OrdinalIgnoreCase) == true ||
                        toPath.StartsWith(fromPath, StringComparison.OrdinalIgnoreCase) == true)
                {
                    // 引数エラー：メッセージ出力
                    Console.WriteLine("引数エラー：パスが重複しています。");
                    return false;
                }
            }

            // 指定されたパスがファイルシステムに存在するか
            if (Directory.Exists(fromPath) == false ||
                    Directory.Exists(toPath) == false)
            {
                // パスエラー：メッセージ出力
                Console.WriteLine("パスエラー：指定されたパスが存在しません。");
                return false;
            }

            // 指摘された先がディレクトリかどうか
            {
                var fromAttr = File.GetAttributes(fromPath);
                if (fromAttr.HasFlag(FileAttributes.Directory) == false)
                {
                    // パスエラー：メッセージ出力
                    Console.WriteLine("パスエラー：入力元 はディレクトリではありません。");
                    return false;
                }
                var toAttr = File.GetAttributes(toPath);
                if (toAttr.HasFlag(FileAttributes.Directory) == false)
                {
                    // パスエラー：メッセージ出力
                    Console.WriteLine("パスエラー：出力先 はディレクトリではありません。");
                    return false;
                }
            }

            return true;
        }

        // Purge処理本体（１ディレクトリ分）
        private static bool PurgeDirectoryRecursive(string fromPath, string toPath)
        {
            var ret = false;

            // ファイルリビジョン収集
            var revisionInfo = new Dictionary<string, int>();
            var directories = new List<string>();
            if (!CollectLatestRevisions(fromPath, revisionInfo, directories))
                return false;

            // Purge処理
            foreach( var file in revisionInfo)
            {
                var srcFileName = file.Key + ";" + String.Format("{0:D}",file.Value);

                File.Copy( Path.Combine( fromPath, srcFileName), Path.Combine( toPath, file.Key), true );
                Console.WriteLine("CopyTo : " + Path.Combine(toPath, file.Key));
            }

            // ディレクトリ処理
            foreach (var dir in directories)
            {
                // dir は既にフルパスで返されるので、そのまま使う
                var toDir = Path.Combine(toPath, dir);
                if( toDir.EndsWith(".DIR", StringComparison.OrdinalIgnoreCase) == true)
                {
                    toDir = toDir.Substring(0, toDir.Length - 4);
                }
                
                // 出力先にディレクトリを作成する
                Directory.CreateDirectory(toDir);
                Console.WriteLine("Create : " + toDir);

                // 再帰呼び出し
                ret = PurgeDirectoryRecursive(Path.Combine(fromPath, dir), toDir);
                if(!ret)
                {
                    Console.WriteLine("Purge処理エラー： " + Path.Combine(fromPath, dir));
                    return false;
                }
            }
            return true;
        }

        // 各ファイルの最終リビジョンを収集する
        private static bool CollectLatestRevisions(string fromPath, Dictionary<string, int> revisionInfo, List<string> directories)
        {
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = false,
                IgnoreInaccessible = true
            };

            // ファイルとディレクトリ両方を列挙
            foreach (var entry in Directory.EnumerateFileSystemEntries(fromPath, "*", options))
            {
                FileAttributes attr;
                try
                {
                    attr = File.GetAttributes(entry);
                }
                catch (Exception ex)
                {
                    // アクセス権等で取得できない場合はログしてスキップ
                    Console.WriteLine($"列挙エラー: {entry} -> {ex.Message}");
                    continue;
                }

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    // fromPath に対する相対パスで保存（再帰呼び出し時に Path.Combine(fromPath, relative) で使える）
                    var relative = Path.GetRelativePath(fromPath, entry);

                    // Normalize to avoid "." or empty
                    if (string.IsNullOrEmpty(relative) || relative == ".")
                        relative = string.Empty;
                    
                    if (!string.IsNullOrEmpty(relative))
                        directories.Add(relative);
                }
                else
                {
                    // ファイルのリビジョン情報を取得する
                    string name = "";
                    int rev = -1;

                    if (TryParseRevisionFromFileName(Path.GetFileName(entry), out name, out rev) == false)
                    {
                        // 書式エラーはログしてスキップ
                        Console.WriteLine("書式エラー：リビジョンを取得できません。　" + entry);
                        continue;
                    }

                    // リビジョン情報を保持する（大文字小文字を無視したい場合は辞書を作る側で設定）
                    if (!revisionInfo.TryGetValue(name, out var exist) || exist < rev)
                    {
                        revisionInfo[name] = rev;
                    }
                }
            }

            return true;
        }

        // ファイル名から「名称」と「リビジョン値」を抽出するヘルパー
        // サポート例:
        //   "Name;123" -> name="Name", rev=123
        private static readonly Regex _revRegex = new Regex(@"^(?<name>.+?);(?<rev>\d+)$", RegexOptions.Compiled);

        private static bool TryParseRevisionFromFileName(string fileName, out string name, out int revision)
        {
            name = string.Empty;
            revision = -1;

            var m = _revRegex.Match(fileName);
            if (!m.Success)
                return false;

            name = m.Groups["name"].Value.Trim();
            if (!int.TryParse(m.Groups["rev"].Value, out revision))
                return false;

            return true;
        }
        #endregion
    }
}

