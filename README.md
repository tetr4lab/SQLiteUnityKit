# SQLiteUnityKit を下敷きにしたライブラリ
- 数多あるSQLiteUnityKitの改修のひとつです。
- [リポジトリ](https://github.com/tetr4lab/SQLiteUnityKit) (GitHub)

### 特徴
- 日本語が使えるようにしました。
- トランザクション処理に対応しました。
- バインド変数に対応しました。
- 既存DBを上書きしないようにしました。(かといって、マージもしません。)
- オープンやクローズ、リソースの開放などといった低レベル処理は隠蔽して、抽象化レベルの高い処理だけを表に出すようにしました。
- できるだけ例外は内部で捉えて、リソースの未解放を避けて動き続けるように務めました。

# 前提
### 環境
- Unity 2021.3.30f1 (LTS), 2022.3.8f1
- SQlite 3.41.0

### SQLite
- SQLiteは、SQLのサブセットが使えるスタンドアローンなデータベース管理システムです。
    - Windows、macOS、Android、iOSなどに対応しています。
- [公式サイト](https://www.sqlite.org/index.html)

#### 更新手順
- [UnityでSQLiteをAndroid(64bit対応)向けに導入する](https://qiita.com/tetr4lab/items/729008c94daaff82833e) (Qiita)

### SQLiteUnityKit (fork元)
- SQLiteUnityKitは、UnityからSQLiteを使用するためのフレームワークです。
- [リポジトリ](https://github.com/Busta117/SQLiteUnityKit) (GitHub)

# 導入と概要
- リポジトリから`Assets`をプロジェクトへ導入してください。

### 概要
- `Assets/Plugins/sqlite3/`
    - 各プラットフォーム向けのSQLiteプラグインです。(iOSはOS側でサポートがあります。)
- `Assets/Scripts/`
    - `SQLiteUnity.cs`
        - 必須部分です。
    - "SQLiteUnityUtility.cs"
        - 拡張ユーティリティクラスです。お好みでどうぞ。
        - トランザクションでも擬似的なバインドが使えるようになっています。
    - "Test.cs"
        - デモ用スクリプトです。
- `Assets/Prefabs/Console.prefab`
    - デモ用プレハブです。
- `Assets/Scenes/SQLite_Test.unity`
    - デモ用シーンです。

### 最新プラグインへの更新
- [公式サイトのダウンロード](https://www.sqlite.org/download.html)から最新版を取ってきて`Assets/Plugins`へ導入してください。
- Androidについては、[こちらの記事(Qiita)](https://qiita.com/tetr4lab/items/729008c94daaff82833e)を参考にしてください。

# 基本的な使い方
  - データベース
    - `public class SQLite : IDisposable`
    - 新規生成 (初期化クエリ) (既にあれば単に使う、元があればコピーして使う)
      - `public SQLite (string dbName, string query = null)`
    - 単文を実行
      - `public void ExecuteNonQuery (string query, SQLiteRow param = null)`
    - 単文を実行して結果を返す
      - `public SQLiteTable ExecuteQuery (string query, SQLiteRow param = null)`
    - 単文の変数を差し替えながら順に実行
      - `public void ExecuteNonQuery (string query, SQLiteTable param)`
      - 同じSQL文を、パラメータを変えながら繰り返し実行します。
    - 複文を一括実行し、誤りがあれば巻き戻す
      - `public bool TransactionQueries<T> (T query) where T : IEnumerable<string>`
      - `public bool TransactionQueries (string query)`
      - 複数行を配列やリストで渡すか、単一文字列として渡すか、という違いです。
      - 冒頭と末尾に`BEGIN`,`COMMIT`が勝手に付きます。
  - 行列データ
    - `public class SQLiteTable`
    - クエリで返されるデータで、列の定義と行データの集合です。
  - 行データ / バインドパラメータ
    - `public class SQLiteRow : Dictionary<string, object>`
    - 1行分のデータで、列データの集合です。バインドパラメータを渡すときにも使います。
  - 拡張バインド (トランザクション用)
    - `public static string SQLiteBind (this string query, SQLiteRow param)`
    - sqliteの外側で行われる文字列ベースのバインドです。

# その他
  - ご指摘やご提案、あるいはご質問などを歓迎します。
    - 常識的なことも理解していないので、何か間違えているような場合はご助言いただけると助かります。
  - より詳しい使い方については、使用例を記事にしたいと考えています。
