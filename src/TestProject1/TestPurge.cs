using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    [TestClass]
    public sealed class TestPurge
    {
        // PurgeDirectoryRecursive 用 MethodInfo (ref string, ref string)
        private MethodInfo? _miPurgeDirectoryRecursive;


        // 各テスト実行前に呼ばれる（インスタンスメソッド）
        [TestInitialize]
        public void TestInitialize()
        {
            // テストデータのあるディレクトリをカレントディレクトリとする
            Directory.SetCurrentDirectory(@"C:\Develop\05_OpenVMS\Purgeツール作成\PurgeForVMS\TestData");

            // テスト対象の型
            var purgeType = typeof(Purge.Purge);

            // private static bool ValidateAndNormalizePaths(ref string, ref string)
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var refString = typeof(string);
            _miPurgeDirectoryRecursive = purgeType.GetMethod(
                "PurgeDirectoryRecursive",
                flags,
                binder: null,
                types: new Type[] { refString, refString },
                modifiers: null
            );

            if (_miPurgeDirectoryRecursive == null)
            {
                Assert.Fail("PurgeDirectoryRecursive が見つかりません：名前／シグネチャを確認してください。");
            }
        }

        // 各テスト実行後に呼ばれる（インスタンスメソッド）
        [TestCleanup]
        public void TestCleanup()
        {
            // 今は特になし
        }

        [TestMethod]
        [TestCategory("Purge本体")]
        public void TestMethod_purgeDirectoryRecursive()
        {
            bool ret = false;

            // ディク取り作成の確認
            {
                object?[] args = new object?[] { @".\from\", @".\to\" };
                var result = _miPurgeDirectoryRecursive!.Invoke(null, args);
                Assert.IsInstanceOfType(result, typeof(bool));
                ret = (bool)result;
                Assert.IsTrue(ret == true);
                Assert.IsTrue(Directory.Exists(@".\to\INC"));
                Assert.IsTrue(Directory.Exists(@".\to\SRC"));
                Assert.IsTrue(Directory.Exists(@".\to\SRC\Sub"));
            }
        }

        [TestMethod]
        [TestCategory("Purge本体")]
        public void TestMethod_Go()
        {
            bool ret = false;


            // ひとまずカラにしておく
            ClearDirectory(@".\to\");

            // テスト対象実行
            ret = Purge.Purge.PurgeDirectoryTree(@".\from\", @".\to\");
            Assert.IsTrue(ret);
        }



        public static void ClearDirectory(string targetDirectory)
        {
            if (!Directory.Exists(targetDirectory))
            {
                // フォルダが存在しない場合は何もしない
                return;
            }

            // 1. フォルダ内の全ファイルを削除
            string[] files = Directory.GetFiles(targetDirectory);
            foreach (string file in files)
            {
                // 読み取り専用ファイルなどがある場合に備えて属性をクリアしてから削除
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            // 2. フォルダ内の全サブディレクトリを削除（再帰的に中身も削除）
            string[] directories = Directory.GetDirectories(targetDirectory);
            foreach (string dir in directories)
            {
                // 第二引数に 'true' を指定して、サブディレクトリとその中身も全て一括削除
                Directory.Delete(dir, true);
            }
        }


    }
}