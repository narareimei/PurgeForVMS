


using System.Reflection;

namespace TestProject1
{
    [TestClass]
    public sealed class TestArguments
    {
        // ValidateAndNormalizePaths 用 MethodInfo (ref string, ref string)
        private MethodInfo? _miValidateAndNormalizePaths;

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
            var refString = typeof(string).MakeByRefType();
            _miValidateAndNormalizePaths = purgeType.GetMethod(
                "ValidateAndNormalizePaths",
                flags,
                binder: null,
                types: new Type[] { refString, refString },
                modifiers: null
            );

            if (_miValidateAndNormalizePaths == null)
            {
                Assert.Fail("ValidateAndNormalizePaths が見つかりません：名前／シグネチャを確認してください。");
            }
        }

        // 各テスト実行後に呼ばれる（インスタンスメソッド）
        [TestCleanup]
        public void TestCleanup()
        {
            // 今は特になし
        }


        [TestMethod]
        [TestCategory("引数チェック")]
        // 引数チェックのテスト
        public void TestMethod_arg_empty()
        {
            bool ret = false;

            // 値が指定されているか
            {
                object?[] args = new object?[] { "", @".\from\SRC.DIR" };
                var result = _miValidateAndNormalizePaths!.Invoke(null, args);
                Assert.IsInstanceOfType(result, typeof(bool));
                ret = (bool)result;
                Assert.IsTrue(ret == false);
            }

            {
                object?[] args = new object?[] { "", "" };
                var result = _miValidateAndNormalizePaths!.Invoke(null, args);
                Assert.IsInstanceOfType(result, typeof(bool));
                ret = (bool)result;
                Assert.IsTrue(ret == false);
            }

            {
                object?[] args = new object?[] { @".\from\SRC.DIR", "" };
                var result = _miValidateAndNormalizePaths!.Invoke(null, args);
                Assert.IsInstanceOfType(result, typeof(bool));
                ret = (bool)result;
                Assert.IsTrue(ret == false);
            }

            // 両引数にパスが指定された場合、次のチェックへ進むのでここでは除外する
            //ret = Purge.Purge.ValidateAndNormalizePaths(@"..\from\SRC.DIR", @"..\from\SRC.DIR");
            //Assert.IsTrue(ret == false);
        }


        [TestMethod]
        [TestCategory("引数チェック")]
        // 引数チェックのテスト
        public void TestMethod_arg_duplicate()
        {
            bool ret = false;

            // 同じパスが指定されていないか
            var result = _miValidateAndNormalizePaths!.Invoke(null, [@".\from\SRC.DIR", @".\from\SRC.DIR"]);
            Assert.IsInstanceOfType(result, typeof(bool));
            ret = (bool)result;
            Assert.IsTrue(ret == false);

            result = _miValidateAndNormalizePaths!.Invoke(null, [@".\from\src.DIR", @".\from\SRC.DIR"]);
            Assert.IsInstanceOfType(result, typeof(bool));
            ret = (bool)result;
            Assert.IsTrue(ret == false);

            result = _miValidateAndNormalizePaths!.Invoke(null, [@".\from\SRC.DIR\", @".\from\SRC.DIR"]);
            Assert.IsInstanceOfType(result, typeof(bool));
            ret = (bool)result;
            Assert.IsTrue(ret == false);

            result = _miValidateAndNormalizePaths!.Invoke(null, [@".\from\SRC.DIR\", @".\from\SRC.DIR\"]);
            Assert.IsInstanceOfType(result, typeof(bool));
            ret = (bool)result;
            Assert.IsTrue(ret == false);


            // frompPathとtoPathにパス重複は許容されないこと（お互いに包含すると問題を引き起こす）
            result = _miValidateAndNormalizePaths!.Invoke(null, [@".\from\", @".\from\SRC.DIR"]);
            Assert.IsInstanceOfType(result, typeof(bool));
            ret = (bool)result;
            Assert.IsTrue(ret == false);

            result = _miValidateAndNormalizePaths!.Invoke(null, [@".\from\SRC.DIR", @".\from\"]);
            Assert.IsInstanceOfType(result, typeof(bool));
            ret = (bool)result;
            Assert.IsTrue(ret == false);
        }


        [TestMethod]
        [TestCategory("引数チェック")]
        public void testMethod_arg_exists()
        {
            bool ret = false;

            var result = _miValidateAndNormalizePaths!.Invoke(null, [@".\from\XXX", @".\to\SRC.DIR\"]);
            Assert.IsInstanceOfType(result, typeof(bool));
            ret = (bool)result;
            Assert.IsTrue(ret == false);

            result = _miValidateAndNormalizePaths!.Invoke(null, [@".\from\SRC.DIR", @".\to\SRC.DIR\XXX"]);
            Assert.IsInstanceOfType(result, typeof(bool));
            ret = (bool)result;
            Assert.IsTrue(ret == false);

            result = _miValidateAndNormalizePaths!.Invoke(null, [@".\from\", @".\to\"]);
            Assert.IsInstanceOfType(result, typeof(bool));
            ret = (bool)result;
            Assert.IsTrue(ret == true);
        }
    }
}
