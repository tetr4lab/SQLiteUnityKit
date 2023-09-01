using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

/*
 * please don't use this code for sell a asset
 * user for free 
 * developed by Poya  @  http://gamesforsoul.com/
 * BLOB support by Jonathan Derrough @ http://jderrough.blogspot.com/
 * Modify and structure by Santiago Bustamante @ busta117@gmail.com
 * Android compatibility by Thomas Olsen @ olsen.thomas@gmail.com
 *
 * */
/* * modified by tetr4lab. @ http://tetr4lab.nyanta.jp/
 * */
namespace SQLiteUnity {

	/// <summary>プラグインアクセス</summary>
	internal static class SQLiteEntry {
		[DllImport ("sqlite3", EntryPoint = "sqlite3_open")] public static extern SQLiteResultCode sqlite3_open (string filename, out IntPtr db);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_close")] public static extern SQLiteResultCode sqlite3_close (IntPtr db);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_prepare_v2")] public static extern SQLiteResultCode sqlite3_prepare_v2 (IntPtr db, string zSql, int nByte, out IntPtr ppStmpt, IntPtr pzTail);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_step")] public static extern SQLiteResultCode sqlite3_step (IntPtr statement);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_finalize")] public static extern SQLiteResultCode sqlite3_finalize (IntPtr statement);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_column_count")] public static extern int sqlite3_column_count (IntPtr statement);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_column_name")] public static extern IntPtr sqlite3_column_name (IntPtr statement, int iCol);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_column_type")] public static extern SQLiteColumnType sqlite3_column_type (IntPtr statement, int iCol);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_column_int")] public static extern int sqlite3_column_int (IntPtr statement, int iCol);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_column_text")] public static extern IntPtr sqlite3_column_text (IntPtr statement, int iCol);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_column_double")] public static extern double sqlite3_column_double (IntPtr statement, int iCol);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_column_blob")] public static extern IntPtr sqlite3_column_blob (IntPtr statement, int iCol);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_column_bytes")] public static extern int sqlite3_column_bytes (IntPtr statement, int iCol);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_exec")] public static extern SQLiteResultCode sqlite3_exec (IntPtr db, string sql, IntPtr callback, IntPtr args, out IntPtr errorMessage);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_bind_parameter_index")] public static extern int sqlite3_bind_parameter_index (IntPtr statement, string key);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_bind_int")] public static extern SQLiteResultCode sqlite3_bind_int (IntPtr statement, int index, int val);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_bind_text")] public static extern SQLiteResultCode sqlite3_bind_text (IntPtr statement, int index, byte [] value, int length, IntPtr freeType);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_bind_blob")] public static extern SQLiteResultCode sqlite3_bind_blob (IntPtr statement, int index, byte [] value, int length, IntPtr freeType);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_bind_double")] public static extern SQLiteResultCode sqlite3_bind_double (IntPtr statement, int index, double value);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_bind_null")] public static extern SQLiteResultCode sqlite3_bind_null (IntPtr statement, int index);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_free")] public static extern SQLiteResultCode sqlite3_free (IntPtr memory);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_errmsg")] public static extern IntPtr sqlite3_errmsg (IntPtr db);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_errcode")] public static extern SQLiteResultCode sqlite3_errcode (IntPtr db);
		[DllImport ("sqlite3", EntryPoint = "sqlite3_extended_errcode")] public static extern SQLiteResultCode sqlite3_extended_errcode (IntPtr db);
	}

	/// <summary>SQLiteハンドラ</summary>
	public class SQLite : IDisposable {

		/// <summary>コネクションがある</summary>
		public bool IsOpen {
			get => _ptrSQLiteDB != IntPtr.Zero;
			private set { if (!value) { _ptrSQLiteDB = IntPtr.Zero; } }
		}
		/// <summary>DBハンドル</summary>
		private IntPtr _ptrSQLiteDB;
		/// <summary>DBファイルパス</summary>
		private string _pathDB;

		/// <summary>新規生成 (初期化クエリ) (既にあれば単に使う、元があればコピーして使う)</summary>
		public SQLite (string dbName, string query = null, string path = null, bool force = false) {
			_ptrSQLiteDB = IntPtr.Zero;
			_pathDB = System.IO.Path.Combine (path ?? Application.persistentDataPath, dbName);
			if (!force && System.IO.File.Exists (_pathDB)) { // 既存
				return;
			} else { // 複製
				string sourcePath = System.IO.Path.Combine (Application.streamingAssetsPath, dbName);
				if (sourcePath.Contains ("://")) { // Android
					UnityWebRequest www = UnityWebRequest.Get (sourcePath);
					www.SendWebRequest ();
					while (www.result == UnityWebRequest.Result.InProgress) { }
					if (www.result == UnityWebRequest.Result.Success) {
						System.IO.File.WriteAllBytes (_pathDB, www.downloadHandler.data);
						return;
					}
				} else if (System.IO.File.Exists (sourcePath)) { // Mac, Windows, Iphone
					System.IO.File.Copy (sourcePath, _pathDB, true);
					return;
				}
			}
			if (string.IsNullOrEmpty (query)) {
				throw new ArgumentNullException ("no query");
			}
			TransactionQueries (query); // 新規
		}

		/// <summary>破棄</summary>
		public void Dispose () {
			if (IsOpen) { Close (); }
		}

		/// <summary>DBを開く</summary>
		private void Open () {
			if (!IsOpen) {
				var result = SQLiteEntry.sqlite3_open (_pathDB, out _ptrSQLiteDB);
				if (result != SQLiteResultCode.SQLITE_OK) {
					IsOpen = false;
					throw new SQLiteException ($"Could not open database file: {_pathDB} {result}");
				}
			}
		}

		/// <summary>DBを閉じる</summary>
		private void Close () {
			if (IsOpen) {
				SQLiteEntry.sqlite3_close (_ptrSQLiteDB);
				IsOpen = false;
			}
		}

		/// <summary>結果の不要なステートメントを実行する</summary>
		private void ExecuteNonQuery (Statement statement) {
			if (!IsOpen) { return; }
			var result = SQLiteEntry.sqlite3_step (statement.Pointer);
			if (result != SQLiteResultCode.SQLITE_DONE) {
				throw new SQLiteException ($"Could not execute SQL statement. {result}");
			}
		}

		/// <summary>ステートメントを実行して結果行列を取得</summary>
		private SQLiteTable ExecuteQuery (Statement statement) {
			if (!IsOpen) { return null; }
			var pointer = statement.Pointer;
			var dataTable = new SQLiteTable ();
			// 列の生成
			int columnCount = SQLiteEntry.sqlite3_column_count (pointer);
			for (int i = 0; i < columnCount; i++) {
				string columnName = Marshal.PtrToStringAnsi (SQLiteEntry.sqlite3_column_name (pointer, i));
				dataTable.AddColumn (columnName, SQLiteColumnType.SQLITE_UNKNOWN);
			}
			// 行の生成
			object [] row = new object [columnCount];
			while (SQLiteEntry.sqlite3_step (pointer) == SQLiteResultCode.SQLITE_ROW) {
				for (int i = 0; i < columnCount; i++) {
					if (dataTable.Columns [i].Type == SQLiteColumnType.SQLITE_UNKNOWN || dataTable.Columns [i].Type == SQLiteColumnType.SQLITE_NULL) {
						dataTable.Columns [i].Type = SQLiteEntry.sqlite3_column_type (pointer, i);
					}
					switch (dataTable.Columns [i].Type) {
						case SQLiteColumnType.SQLITE_INTEGER:
							row [i] = SQLiteEntry.sqlite3_column_int (pointer, i);
							break;
						case SQLiteColumnType.SQLITE_TEXT:
							IntPtr text = SQLiteEntry.sqlite3_column_text (pointer, i);
							row [i] = Marshal.PtrToStringAnsi (text);
							break;
						case SQLiteColumnType.SQLITE_FLOAT:
							row [i] = SQLiteEntry.sqlite3_column_double (pointer, i);
							break;
						case SQLiteColumnType.SQLITE_BLOB:
							IntPtr blob = SQLiteEntry.sqlite3_column_blob (pointer, i);
							int size = SQLiteEntry.sqlite3_column_bytes (pointer, i);
							byte [] data = new byte [size];
							Marshal.Copy (blob, data, 0, size);
							row [i] = data;
							break;
						case SQLiteColumnType.SQLITE_NULL:
							row [i] = null;
							break;
						default:
							row [i] = null;
							break;
					}
				}
				dataTable.AddRow (row);
			}
			return dataTable;
		}

		/// <summary>トランザクション実行</summary>
		private void ExecuteTransaction (string query) {
			if (!IsOpen) { return; }
			IntPtr errorMessage;
			var result = SQLiteEntry.sqlite3_exec (_ptrSQLiteDB, query, IntPtr.Zero, IntPtr.Zero, out errorMessage);
			if (result != SQLiteResultCode.SQLITE_OK || errorMessage != IntPtr.Zero) {
				var str = $"Could not execute SQL statement. {result} '{errorMessage}' {SQLiteEntry.sqlite3_extended_errcode (_ptrSQLiteDB)}";
				SQLiteEntry.sqlite3_free (errorMessage);
				throw new SQLiteException (str);
			}
		}

		#region HighLevelAPI

		/// <summary>単文の変数を差し替えながら順に実行</summary>
		public void ExecuteNonQuery (string query, SQLiteTable param) {
			foreach (SQLiteRow row in param) { ExecuteNonQuery (query, row); }
		}

		/// <summary>単文を実行</summary>
		public void ExecuteNonQuery (string query, SQLiteRow param = null) {
			var close = !IsOpen; // 元の状態
			Open ();
			try {
				using (Statement statement = new Statement (this, query, param)) {
					if (statement.Pointer != IntPtr.Zero) {
						ExecuteNonQuery (statement);
					}
				}
			}
			catch (SQLiteException e) {
				Debug.LogError ($"SQLite: Can't ExecuteNonQuery {e}");
			}
			finally {
				if (close) { Close (); } // 元に戻す
			}
		}

		/// <summary>単文を実行して結果を返す</summary>
		public SQLiteTable ExecuteQuery (string query, SQLiteRow param = null) {
			SQLiteTable result = null;
			var close = !IsOpen; // 元の状態
			Open ();
			try {
				using (Statement statement = new Statement (this, query, param)) {
					if (statement.Pointer != IntPtr.Zero) {
						result = ExecuteQuery (statement);
					}
				}
			}
			catch (SQLiteException e) {
				Debug.LogError ($"SQLite: Can't ExecuteQuery {e}");
			}
			finally {
				if (close) { Close (); } // 元に戻す
			}
			return result;
		}

		/// <summary>複文を一括実行し、誤りがあれば巻き戻す</summary>
		public bool TransactionQueries<T> (T query) where T : IEnumerable<string> => TransactionQueries (string.Join ("\n", query));

		/// <summary>複文を一括実行し、誤りがあれば巻き戻す</summary>
		public bool TransactionQueries (string query) {
			var close = !IsOpen; // 元の状態
			try {
				Open ();
				ExecuteTransaction ($"BEGIN TRANSACTION;\n{query};\nCOMMIT;");
				return true;
			}
			catch (SQLiteException e) {
				ExecuteTransaction ("ROLLBACK;");
				Debug.LogError ($"SQLite: Can't Transaction, rollbacked {e}");
				return false;
			}
			finally {
				if (close) { Close (); } // 元に戻す
			}
		}

		#endregion

		/// <summary>SQLステートメント</summary>
		private class Statement : IDisposable {
			private SQLite _database;
			public IntPtr Pointer { get { return pointer; } }
			private IntPtr pointer;

			/// <summary>SQLステートメントの生成</summary>
			public Statement (SQLite database, string query, SQLiteRow param = null) {
                Statement statement = this;
                statement._database = database;
				pointer = IntPtr.Zero;
				if (_database != null && !database.IsOpen) {
					throw new SQLiteException ("SQLite database is not open.");
				}
				if (SQLiteEntry.sqlite3_prepare_v2 (_database._ptrSQLiteDB, query, System.Text.Encoding.GetEncoding ("UTF-8").GetByteCount (query), out pointer, IntPtr.Zero) != SQLiteResultCode.SQLITE_OK) {
					IntPtr errorMsg = SQLiteEntry.sqlite3_errmsg (_database._ptrSQLiteDB);
					throw new SQLiteException (Marshal.PtrToStringAnsi (errorMsg));
				}
				if (param != null) {
					BindParameter (param);
				}
			}

			/// <summary>破棄</summary>
			public void Dispose () {
				if (_database != null && pointer != IntPtr.Zero) {
					var result = SQLiteEntry.sqlite3_finalize (pointer);
					if (result != SQLiteResultCode.SQLITE_OK) {
						throw new SQLiteException ($"Could not finalize SQL statement. {result} {SQLiteEntry.sqlite3_extended_errcode (_database._ptrSQLiteDB)}");
					}
				}
			}

			/// <summary>ステートメントにSQL引数をバインドする 必要なら':'が補われる</summary>
			private void BindParameter (SQLiteRow param) {
				if (param != null) {
					foreach (string key in param.Keys) {
						object val = param [key];
						string name = (key [0] == ':' || key [0] == '@' || key [0] == '$') ? key : $":{key}";
						if (val == null) {
							SQLiteEntry.sqlite3_bind_null (pointer, SQLiteEntry.sqlite3_bind_parameter_index (pointer, name));
						} else if (val is string) {
							SQLiteEntry.sqlite3_bind_text (pointer, SQLiteEntry.sqlite3_bind_parameter_index (pointer, name), System.Text.Encoding.UTF8.GetBytes ((string) val), System.Text.Encoding.GetEncoding ("UTF-8").GetByteCount ((string) val), new IntPtr (-1));
						} else if (val is byte []) {
							SQLiteEntry.sqlite3_bind_text (pointer, SQLiteEntry.sqlite3_bind_parameter_index (pointer, name), (byte []) val, ((byte []) val).Length, new IntPtr (-1));
						} else if (val is float) {
							SQLiteEntry.sqlite3_bind_double (pointer, SQLiteEntry.sqlite3_bind_parameter_index (pointer, name), (double) (float) val);
						} else if (val is double) {
							SQLiteEntry.sqlite3_bind_double (pointer, SQLiteEntry.sqlite3_bind_parameter_index (pointer, name), (double) val);
						} else if (val.IsInt32 ()) {
							SQLiteEntry.sqlite3_bind_int (pointer, SQLiteEntry.sqlite3_bind_parameter_index (pointer, name), (int) val);
						} else { // その他の型
							if (val.Equals (val.GetType ().GetDefaultValue ())) { // デフォルト値ならNULL
								SQLiteEntry.sqlite3_bind_null (pointer, SQLiteEntry.sqlite3_bind_parameter_index (pointer, name));
							} else { // 既定の文字列化
								val = (val is DateTime) ? ((DateTime)val).ToString ("yyyy-MM-dd HH:mm:ss")  : val.ToString (); // 日時の文字列化書式を制御
								SQLiteEntry.sqlite3_bind_text (pointer, SQLiteEntry.sqlite3_bind_parameter_index (pointer, name), System.Text.Encoding.UTF8.GetBytes ((string) val), System.Text.Encoding.GetEncoding ("UTF-8").GetByteCount ((string) val), new IntPtr (-1));
							}
						}
					}
				}
			}

		}

	}

	/// <summary>列型</summary>
	public enum SQLiteColumnType {
		SQLITE_UNKNOWN = 0,
		SQLITE_INTEGER = 1,
		SQLITE_FLOAT = 2,
		SQLITE_TEXT = 3,
		SQLITE_BLOB = 4,
		SQLITE_NULL = 5,
	}

	/// <summary>結果コード</summary>
	public enum SQLiteResultCode {
		SQLITE_ABORT = 4,
		SQLITE_AUTH = 23,
		SQLITE_BUSY = 5,
		SQLITE_CANTOPEN = 14,
		SQLITE_CONSTRAINT = 19,
		SQLITE_CORRUPT = 11,
		SQLITE_DONE = 101,
		SQLITE_EMPTY = 16,
		SQLITE_ERROR = 1,
		SQLITE_FORMAT = 24,
		SQLITE_FULL = 13,
		SQLITE_INTERNAL = 2,
		SQLITE_INTERRUPT = 9,
		SQLITE_IOERR = 10,
		SQLITE_LOCKED = 6,
		SQLITE_MISMATCH = 20,
		SQLITE_MISUSE = 21,
		SQLITE_NOLFS = 22,
		SQLITE_NOMEM = 7,
		SQLITE_NOTADB = 26,
		SQLITE_NOTFOUND = 12,
		SQLITE_NOTICE = 27,
		SQLITE_OK = 0,
		SQLITE_PERM = 3,
		SQLITE_PROTOCOL = 15,
		SQLITE_RANGE = 25,
		SQLITE_READONLY = 8,
		SQLITE_ROW = 100,
		SQLITE_SCHEMA = 17,
		SQLITE_TOOBIG = 18,
		SQLITE_WARNING = 28,
		SQLITE_ABORT_ROLLBACK = 516,
		SQLITE_BUSY_RECOVERY = 261,
		SQLITE_BUSY_SNAPSHOT = 517,
		SQLITE_CANTOPEN_CONVPATH = 1038,
		SQLITE_CANTOPEN_DIRTYWAL = 1294,
		SQLITE_CANTOPEN_FULLPATH = 782,
		SQLITE_CANTOPEN_ISDIR = 526,
		SQLITE_CANTOPEN_NOTEMPDIR = 270,
		SQLITE_CONSTRAINT_CHECK = 275,
		SQLITE_CONSTRAINT_COMMITHOOK = 531,
		SQLITE_CONSTRAINT_FOREIGNKEY = 787,
		SQLITE_CONSTRAINT_FUNCTION = 1043,
		SQLITE_CONSTRAINT_NOTNULL = 1299,
		SQLITE_CONSTRAINT_PRIMARYKEY = 1555,
		SQLITE_CONSTRAINT_ROWID = 2579,
		SQLITE_CONSTRAINT_TRIGGER = 1811,
		SQLITE_CONSTRAINT_UNIQUE = 2067,
		SQLITE_CONSTRAINT_VTAB = 2323,
		SQLITE_CORRUPT_SEQUENCE = 523,
		SQLITE_CORRUPT_VTAB = 267,
		SQLITE_ERROR_MISSING_COLLSEQ = 257,
		SQLITE_ERROR_RETRY = 513,
		SQLITE_ERROR_SNAPSHOT = 769,
		SQLITE_IOERR_ACCESS = 3338,
		SQLITE_IOERR_BLOCKED = 2826,
		SQLITE_IOERR_CHECKRESERVEDLOCK = 3594,
		SQLITE_IOERR_CLOSE = 4106,
		SQLITE_IOERR_CONVPATH = 6666,
		SQLITE_IOERR_DELETE = 2570,
		SQLITE_IOERR_DELETE_NOENT = 5898,
		SQLITE_IOERR_DIR_CLOSE = 4362,
		SQLITE_IOERR_DIR_FSYNC = 1290,
		SQLITE_IOERR_FSTAT = 1802,
		SQLITE_IOERR_FSYNC = 1034,
		SQLITE_IOERR_GETTEMPPATH = 6410,
		SQLITE_IOERR_LOCK = 3850,
		SQLITE_IOERR_MMAP = 6154,
		SQLITE_IOERR_NOMEM = 3082,
		SQLITE_IOERR_RDLOCK = 2314,
		SQLITE_IOERR_READ = 266,
		SQLITE_IOERR_SEEK = 5642,
		SQLITE_IOERR_SHMLOCK = 5130,
		SQLITE_IOERR_SHMMAP = 5386,
		SQLITE_IOERR_SHMOPEN = 4618,
		SQLITE_IOERR_SHMSIZE = 4874,
		SQLITE_IOERR_SHORT_READ = 522,
		SQLITE_IOERR_TRUNCATE = 1546,
		SQLITE_IOERR_UNLOCK = 2058,
		SQLITE_IOERR_WRITE = 778,
		SQLITE_LOCKED_SHAREDCACHE = 262,
		SQLITE_LOCKED_VTAB = 518,
		SQLITE_NOTICE_RECOVER_ROLLBACK = 539,
		SQLITE_NOTICE_RECOVER_WAL = 283,
		SQLITE_OK_LOAD_PERMANENTLY = 256,
		SQLITE_READONLY_CANTINIT = 1288,
		SQLITE_READONLY_CANTLOCK = 520,
		SQLITE_READONLY_DBMOVED = 1032,
		SQLITE_READONLY_DIRECTORY = 1544,
		SQLITE_READONLY_RECOVERY = 264,
		SQLITE_READONLY_ROLLBACK = 776,
		SQLITE_WARNING_AUTOINDEX = 284,
	}

	/// <summary>例外</summary>
	public class SQLiteException : Exception {
		public SQLiteException (string message) : base (message) { }
	}

	/// <summary>列の定義</summary>
	public class ColumnDefinition {

		/// <summary>列名</summary>
		public string Name;

		/// <summary>型</summary>
		public SQLiteColumnType Type;

		/// <summary>要素を指定して生成</summary>
		public ColumnDefinition (string name, SQLiteColumnType type) {
			Name = name;
			Type = type;
		}

		/// <summary>文字列化</summary>
		public override string ToString () => $"{Name} {ColumnTypeName [Type]}";

		/// <summary>SQLite列型をSQL型名に変換</summary>
		public static readonly Dictionary<SQLiteColumnType, string> ColumnTypeName = new Dictionary<SQLiteColumnType, string> {
			{ SQLiteColumnType.SQLITE_UNKNOWN,  "" },
			{ SQLiteColumnType.SQLITE_INTEGER,  "INTEGER" },
			{ SQLiteColumnType.SQLITE_FLOAT,    "REAL" },
			{ SQLiteColumnType.SQLITE_TEXT,     "TEXT" },
			{ SQLiteColumnType.SQLITE_BLOB,     "BLOB" },
			{ SQLiteColumnType.SQLITE_NULL,     "" },
		};
	}

	/// <summary>行のデータ / バインドパラメータ</summary>
	public class SQLiteRow : Dictionary<string, object> {

		#region Static
		/// <summary>行がnullまたは空</summary>
		public static bool IsNullOrEmpty (SQLiteRow row) => (row == null || row.Count <= 0);
		#endregion

		/// <summary>列にアクセスするインデクサ (列名)</summary>
		public new object this [string columnName] {
			get => ContainsKey (columnName) ? base [columnName] : null;
			set { if (ContainsKey (columnName)) { base [columnName] = value; } }
		}

		/// <summary>要素の連結</summary>
		public virtual SQLiteRow AddRange (SQLiteRow addition) {
			foreach (var item in addition) { if (!ContainsKey (item.Key)) { Add (item.Key, item.Value); } }
			return this;
		}

		/// <summary>文字列化</summary>
		public override string ToString () {
			var keyval = new List<string> { };
			foreach (var key in Keys) {
				keyval.Add ($"{{ {key}, {_toString (this [key])} }}");
			}
			return $"{{ {(string.Join (", ", keyval))} }}";

			string _toString (object val) {
				if (val == null) {
					return "null";
				} else if (val is string || val is byte []) {
					return $"\"{val}\"";
				} else {
					return val.ToString ();
				}
			}
		}
	}

	/// <summary>テーブルのデータ</summary>
	public class SQLiteTable {

		/// <summary>列定義</summary>
		public List<ColumnDefinition> Columns { get; protected set; }

		/// <summary>行</summary>
		public List<SQLiteRow> Rows { get; protected set; }

		#region Static
		/// <summary>テーブルがnullまたは空</summary>
		public static bool IsNullOrEmpty (SQLiteTable table) => (table == null || table.Rows.Count <= 0);
		#endregion

		/// <summary>空の生成</summary>
		public SQLiteTable () {
			Columns = new List<ColumnDefinition> { };
			Rows = new List<SQLiteRow> { };
		}

		/// <summary>列一覧からの生成</summary>
		public SQLiteTable (params ColumnDefinition [] columns) : this () {
			for (var i = 0; i < columns.Length; i++) {
				Columns.Add (columns [i]);
			}
		}

		/// <summary>先頭行</summary>
		public SQLiteRow Top => (Rows.Count > 0) ? Rows [0] : null;

		// 行にアクセスするインデクサ
		public SQLiteRow this [int index] => (index >= 0 && index < Rows.Count) ? Rows [index] : null;

		// セルにアクセスするインデクサ (行番号と列番号)
		public object this [int rowIndex, int columnIndex] {
			get => (rowIndex >= 0 && rowIndex < Rows.Count && columnIndex >= 0 && columnIndex < Columns.Count) ? Rows [rowIndex] [Columns [columnIndex].Name] : null;
			set {
				if (rowIndex >= 0 && rowIndex < Rows.Count && columnIndex >= 0 && columnIndex < Columns.Count) {
					Rows [rowIndex] [Columns [columnIndex].Name] = value;
				}
			}
		}

		// セルにアクセスするインデクサ (行番号と列名)
		public object this [int rowIndex, string columnName] {
			get => (rowIndex >= 0 && rowIndex < Rows.Count ) ? Rows [rowIndex] [columnName] : null;
			set {
				if (rowIndex >= 0 && rowIndex < Rows.Count) {
					Rows [rowIndex] [columnName] = value;
				}
			}
		}

		/// <summary>コレクション</summary>
		public IEnumerator GetEnumerator () {
			for (var i = 0; i < Rows.Count; i++) {
				yield return Rows [i];
			}
		}

		/// <summary>値を指定して行を加える</summary>
		public void AddRow (object [] values) {
			if (values.Length != Columns.Count) {
				throw new IndexOutOfRangeException ("The number of values in the row must match the number of column");
			}
			var row = new SQLiteRow ();
			for (int i = 0; i < values.Length; i++) {
				row.Add (Columns [i].Name, values [i]);
			}
			Rows.Add (row);
		}

		/// <summary>名前と型を指定して列を加える</summary>
		public void AddColumn (string name, SQLiteColumnType type, object [] values = null) {
			AddColumn (new ColumnDefinition (name, type), values);
		}

		/// <summary>列の定義を指定して列を加える</summary>
		public void AddColumn (ColumnDefinition column, object [] values = null) {
			if (Columns.Exists (col => col.Name == column.Name)) {
				throw new ArgumentException ($"The column name is already exist. '{column.Name}'");
			}
			Columns.Add (column);
			if (values != null) {
				if (values.Length != Rows.Count) {
					throw new IndexOutOfRangeException ("The number of values in the table must match the number of row");
				}
				for (var i = 0; i < Rows.Count; i++) {
					Rows [i].Add (column.Name, values [i]);
				}
			}
		}

		/// <summary>文字列化</summary>
		public override string ToString () => $"({string.Join (", ", Columns.ConvertAll (column => column.ToString ()))})\n{string.Join ("\n", Rows.ConvertAll (row => row.ToString ()))}";

	}

	/// <summary>SQLiteユーティリティ</summary>
	public static partial class SQLiteUtility {

		/// <summary>型のデフォルト値を得る</summary>
		public static object GetDefaultValue (this Type type) => type.IsValueType ? Activator.CreateInstance (type) : null;

		/// <summary>32bitまでの整数型か判定</summary>
		public static bool IsInt32<T> (this T val) => (val is int || val is uint || val is short || val is ushort  || val is byte || val is sbyte);

		/// <summary>テーブルがnullまたは空</summary>
		public static bool IsNullOrEmpty (this SQLiteTable table) => SQLiteTable.IsNullOrEmpty (table);

		/// <summary>行がnullまたは空</summary>
		public static bool IsNullOrEmpty (this SQLiteRow row) => SQLiteRow.IsNullOrEmpty (row);

	}

}