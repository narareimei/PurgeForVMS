using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    [TestClass]
    public sealed class TestLatestRevision
    {
        // 初期化時に取得する MethodInfo
        private MethodInfo? _miCollectLatestRevisions;


        // 各テスト実行前に呼ばれる（インスタンスメソッド）
        [TestInitialize]
        public void TestInitialize()
        {
            // テストデータのあるディレクトリをカレントディレクトリとする
            Directory.SetCurrentDirectory(@"C:\Develop\05_OpenVMS\Purgeツール作成\PurgeForVMS\TestData");


            // テスト対象のprivateメソッドのMethodInfoを取得する
            var purgeType = typeof(Purge.Purge);

            // private static bool CollectLatestRevisions(string, Dictionary<string,int>)
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            _miCollectLatestRevisions = purgeType.GetMethod(
                "CollectLatestRevisions",
                flags,
                binder: null,
                types: new Type[] { typeof(string), typeof(Dictionary<string, int>), typeof(List<string>) },
                modifiers: null
            );

            if (_miCollectLatestRevisions == null)
            {
                Assert.Fail("CollectLatestRevisions が見つかりません：名前／シグネチャを確認してください。");
            }
        }

        // 各テスト実行後に呼ばれる（インスタンスメソッド）
        [TestCleanup]
        public void TestCleanup()
        {
            // 今は特になし
        }

        [TestMethod]
        [TestCategory("最新リビジョン取得")]
        public void TestMethod_latestRevision_01()
        {
            var revisionInfo = new Dictionary<string, int>();
            var directories = new List<string>();

            // 正常終了の確認
            var result = _miCollectLatestRevisions!.Invoke(null, [ @".\from", revisionInfo, directories ]);
            Assert.IsTrue(result is bool);
            Assert.IsTrue((bool)result);

            // 収集結果の確認
            Assert.IsTrue(revisionInfo.Count == 0);
            Assert.IsTrue(directories.Count == 2);
        }

        [TestMethod]
        [TestCategory("最新リビジョン取得")]
        public void TestMethod_latestRevision_02()
        {
            var revisionInfo = new Dictionary<string, int>();
            var directories = new List<string>();

            // 正常終了の確認
            var result = _miCollectLatestRevisions!.Invoke(null, [@".\from\SRC.DIR", revisionInfo, directories]);
            Assert.IsTrue(result is bool);
            Assert.IsTrue((bool)result);

            // 収集結果の確認
            Assert.IsTrue(revisionInfo.Count == 2);
            Assert.IsTrue(directories.Count == 1);
        }
    }
}
