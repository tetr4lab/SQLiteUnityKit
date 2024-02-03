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
- このライブラリは直にSQLを使います。O/Rマッパーが必要な場合は、[SQLite-net](https://github.com/praeclarum/sqlite-net) (GitHub) や、[Microsoft.EntityFrameworkCore.Sqlite](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite/) (nuget) などをご検討ください。

# 前提
### 環境
- Unity 2022.3.19f1 (LTS)
- SQlite 3.41.0
- Target PF: Windows、macOS、Android、iOS
 
### SQLite
- SQLiteは、SQLのサブセットが使えるスタンドアローンなデータベース管理システムです。
- [公式サイト](https://www.sqlite.org/index.html)
- [SQLite3のためのSQLリファレンス](https://qiita.com/tetr4lab/items/691ceeb528d6144547c8) (Qiita)、([Zenn](https://zenn.dev/tetr4lab/articles/42c5e0ccc9b750))

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
    - `SQLiteUnityUtility.cs`
        - 拡張ユーティリティクラスです。必要に応じてお使いください。
        - トランザクションでも擬似的なバインドが使えるようになっています。
    - `Startup.cs`
        - デモ用のローダです。
    - `SQLiteTest.cs`
        - デモ用スクリプトです。
        - Test 1: ゲームっぽい関係性のあるデータ
        - Test 2: シンプルで多めのデータ
        - Test 3: [SQL学習型じゃんけん](https://qiita.com/tetr4lab/items/656f8f9d3ea68bbe76ec) (Qiita)
    - `ScrollTest.cs`
        - デモ用スクリプトです。多数のデータをスクロールさせます。
        - [仮想スクロールレクト](https://github.com/tetr4lab/InfiniteScroll) (GitHub)
- `Assets/Resources/Prefabs/Canvas1.prefab`
    - デモ用プレハブです。
- `Assets/Resources/Prefabs/Canvas2.prefab`
    - デモ用プレハブです。
- `Assets/Resources/Prefabs/Item.prefab`
    - `Canvas2`で使用します。
- `Assets/Scenes/SQLite_Test.unity`
    - デモ用シーンです。

### 最新プラグインへの更新
- [公式サイトのダウンロード](https://www.sqlite.org/download.html)から最新版を取ってきて`Assets/Plugins`へ導入してください。
- Androidについては、[こちらの記事(Qiita)](https://qiita.com/tetr4lab/items/729008c94daaff82833e)を参考にしてください。

# 基本的な使い方
  - データベース
    - `public class SQLite<TTable, TRow> : IDisposable where TTable : SQLiteTable<TRow>, new () where TRow : SQLiteRow, new ()`
      - `SQLiteTable<SQLiteRow>`、または、その派生クラスを使います。
    - 新規生成
      - `public SQLite (string dbName, string query = null, string path = null, bool force = false)`
      - 初期化クエリを指定します。
      - データベースファイルが既存なら何もせずそのまま使います。
      - `Application.streamingAssetsPath`に同盟ファイルがあればコピーして使います。
    - 単文を実行
      - `public void ExecuteNonQuery (string query, TRow param = null)`
    - 単文を実行して結果を返す
      - `public TTable ExecuteQuery (string query, TRow param = null)`
    - 単文の変数を差し替えながら順に実行
      - `public void ExecuteNonQuery (string query, TTable param)`
      - 同じSQL文を、パラメータを変えながら繰り返し実行します。
    - 複文を一括実行し、誤りがあれば巻き戻す
      - `public bool TransactionQueries (IEnumerable<string> query)`
      - `public bool TransactionQueries (string query)`
      - 複数行を配列やリストで渡すか、単一文字列として渡すか、という違いです。
      - 冒頭と末尾に`BEGIN`,`COMMIT`が勝手に付きます。
  - 基底行列データ
    - `public class SQLiteTable<TRow> where TRow : SQLiteRow, new ()`
      - `SQLiteRow`、または、その派生クラスを使います。
    - クエリで返されるデータで、列の定義と行データの集合です。
  - 基底行データ / バインドパラメータ
    - `public class SQLiteRow : Dictionary<string, object>`
    - 1行分のデータで、列データの集合です。バインドパラメータを渡すときにも使います。
  - 拡張バインド (トランザクション用)
    - `public static string SQLiteBind (this string query, SQLiteRow param)`
    - sqliteの外側で行われる文字列ベースのバインド(単なる文字列置換)です。

# その他
  - ご指摘やご提案、あるいはご質問などを歓迎します。
    - 常識的なことも理解していないので、何か間違えているような場合はご助言いただけると助かります。
  - より詳しい使い方については、使用例を記事にしたいと考えています。
