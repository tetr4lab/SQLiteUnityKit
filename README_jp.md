# SQLiteUnityKit を下敷きにしたライブラリ
___

## 変更点
  * 日本語が使えるようにしました。
  * トランザクション処理に対応しました。
  * バインド変数に対応しました。
  * オープンやクローズ、リソースの開放などといった低レベル処理は隠蔽して、抽象化レベルの高い処理だけを表に出すようにしました。
  * できるだけ例外は内部で捉えて、リソースの未解放を避けて動き続けるように務めました。

## 必要な環境
  * テストは、Unity2017.4と2018.2のWindowsとAndroidのみで行いました。
  * C# 6 の文字列補完(string interpolation)を使用しています。
    * PlayerSettingsでScripting Runtime Versionを4.xに設定してください。

## 導入の仕方
  * 最新プラグインの導入
    * 公式サイトから最新版を取ってきます。
    * パッケージ内のプラグイン名に合わせてDllImportの引数を書き換えるか、逆にAARパッケージ内のプラグイン名を変更します。
      * なお、パッケージから取り出して直置きにすると、ビルドの際にUnityの同名チェックに引っかかります。
  * "SQLiteUnity.cs"
    * 必須部分です。
  * "SQLiteUnityUtility.cs"
    * 拡張ユーティリティクラスです。お好みでどうぞ。
    * トランザクションでも擬似的なバインドが使えるようになっています。
  * "Test.cs"
     * 必要ありません。

## 基本的な使い方
  * データベース
    * public class SQLite : IDisposable
    * 新規生成 (初期化クエリ) (既にあれば単に使う、元があればコピーして使う)
      * public SQLite (string dbName, string query = null)
    * 単文を実行
      * public void ExecuteNonQuery (string query, SQLiteRow param = null)
    * 単文を実行して結果を返す
      * public SQLiteTable ExecuteQuery (string query, SQLiteRow param = null)
    * 単文の変数を差し替えながら順に実行
      * public void ExecuteNonQuery (string query, SQLiteTable param)
      * 同じSQL文を、パラメータを変えながら繰り返し実行します。
    * 複文を一括実行し、誤りがあれば巻き戻す
      * public bool TransactionQueries<T> (T query) where T : IEnumerable<string>
      * public bool TransactionQueries (string query)
      * 複数行を配列やリストで渡すか、単一文字列として渡すか、という違いです。
      * 冒頭と末尾に'BEGIN','COMMIT'が勝手に付きます。
  * 行列データ
    * public class SQLiteTable
    * クエリで返されるデータで、列の定義と行データの集合です。
  * 行データ / バインドパラメータ
    * public class SQLiteRow : Dictionary<string, object>
    * 1行分のデータで、列データの集合です。バインドパラメータを渡すときにも使います。
  * 拡張バインド (トランザクション用)
    * public static string SQLiteBind (this string query, SQLiteRow param)
    * sqliteの外側で行われる文字列ベースのバインドです。

## その他
  * ご指摘やご提案を歓迎します。
    * 常識的なことも理解していないので、何か間違えているような場合はご助言いただけると助かります。
    * 英文は書けないので、英語でいただいたメッセージにも日本語でお返しします。何卒ご容赦ください。
  * より詳しい使い方については、別途説明を書きたいと考えています。
