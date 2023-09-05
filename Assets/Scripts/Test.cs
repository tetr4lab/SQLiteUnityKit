using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SQLiteUnity;
using System.IO;
using UnityEngine.XR;

namespace SQLiteTest {

	public class Test : MonoBehaviour {

		/// <summary>DB path 1</summary>
		private const string DbPath1 = "SQLiteTest.db";

		/// <summary>DB path 2</summary>
		private const string DbPath2 = "SQLiteTest2.db";

		/// <summary>DB path 3</summary>
		private const string DbPath3 = "SQLiteTest3.db";

		/// <summary>DB</summary>
		public static SQLite<SQLiteTable<SQLiteRow>, SQLiteRow> Database;

		/// <summary>出力先</summary>
		public static Text Console;

		/// <summary>ボタン</summary>
		private static List<Button> Buttons;

		/// <summary>準備</summary>
		private void Awake () {
			Console = transform.GetChild (1).GetComponentInChildren<Text> ();
			var buttons = transform.GetChild (0).GetComponentsInChildren<Button> ();
			if (buttons != null) {
				Buttons = new List<Button> (buttons);
				foreach (var button in buttons) {
					switch (button.name) {
						case "DropButton":
							button.onClick.AddListener (() => {
								var path1 = Path.Combine (Application.persistentDataPath, DbPath1);
								var path2 = Path.Combine (Application.persistentDataPath, DbPath2);
								var path3 = Path.Combine (Application.persistentDataPath, DbPath3);
								System.IO.File.Delete (path1);
								System.IO.File.Delete (path2);
								System.IO.File.Delete (path3);
								Console.text = string.Join("\n", new [] {
									$"{DbPath1}: {(System.IO.File.Exists (path1) ? "<color=red>Exists</color>" : "<color=aqua>Droped</color>")}",
									$"{DbPath2}: {(System.IO.File.Exists (path2) ? "<color=red>Exists</color>" : "<color=aqua>Droped</color>")}",
									$"{DbPath3}: {(System.IO.File.Exists (path3) ? "<color=red>Exists</color>" : "<color=aqua>Droped</color>")}",
								});
							});
							break;
						case "Test1Button":
							button.onClick.AddListener (() => DoTest (1, DbPath1, _creationSql));
							break;
						case "Test2Button":
							button.onClick.AddListener (() => DoTest (2, DbPath2, _creationSql2));
							break;
						case "GooButton":
							button.onClick.AddListener (() => DoTest (31, DbPath3, _creationSql3));
							break;
						case "ChokiButton":
							button.onClick.AddListener (() => DoTest (32, DbPath3, _creationSql3));
							break;
						case "ParButton":
							button.onClick.AddListener (() => DoTest (33, DbPath3, _creationSql3));
							break;
					}
				}
			}
		}

		/// <summary>開始</summary>
		private void Start () {
			DoTest (1, DbPath1, _creationSql);
		}

		/// <summary>テスト</summary>
		private void DoTest (int number, string path, string sql) {
			// 開始宣言
			Console.text = $"SQLiteUnity Test [{number}] Start {DateTime.Now}\n{Path.Combine (Application.persistentDataPath, path)}\n\n";
			Debug.Log ($"Start [{number}]");
			// DB接続とテスト (初回は生成)
			using (Database = new SQLite<SQLiteTable<SQLiteRow>, SQLiteRow> (path, sql)) {
				// バージョン確認
				DumpTable ("バージョン", Database.ExecuteQuery ("select sqlite_version();"));
				switch (number) {
					case 1:
						// 初回のみ
						if (Party.GetTable ().IsNullOrEmpty ()) {
							// 適当にキャラを生成
							var characters = new List<string> { "太郎", "花子", "二郎", "三郎" }.ConvertAll (name => Character.NewCharacter (name));

							// 適当にパーティを生成してメンバーを追加
							var P1 = Party.NewParty ("不滅の旅団", "ガンガンいこうぜ!");
							P1.Add (characters [0], JobClass.Fighter, "俺がリーダーだぜ!");
							P1.Add (characters [1], JobClass.Cleric, "紅一点");
							P1.Add (characters [2], JobClass.Archer, "…");
							var P2 = Party.NewParty ("肉球", "まったり");
							P2.Add (characters [3], JobClass.Fighter, "のんびりやりましょう");
							P2.Add (characters [2], JobClass.Cleric, "…");
							P2.Add (characters [1], JobClass.Archer, "よろしくお願いします");
						}
						// 状況の取得と表示
						Console.text += $"<color=lightblue><size=24>--- パーティ ---</size></color>\n";
						foreach (SQLiteRow row in Party.GetTable ()) {
							Console.text += $"{new Party (row.GetColumn (Guid.Parse, "PUID"))}\n<color=lightblue>=====</color>\n";
						}
						DumpTable ("パーティ", Party.GetTable ());
						DumpTable ("メンバー", Member.GetTable ());
						DumpTable ("キャラクタ", Character.GetTable ());
						break;
					case 2:
						// 生成数
						var COUNT = 1000;
						// 既存数
						var count = Database.ExecuteQuery ("SELECT count(Serial) AS count FROM Test") [0].GetColumn ("count", 0);
						// パラメータの型を生成
						var paramTable = new SQLiteTable<SQLiteRow> (new [] {
							new ColumnDefinition ("Serial", SQLiteColumnType.SQLITE_TEXT),
							new ColumnDefinition ("Guid", SQLiteColumnType.SQLITE_TEXT),
						});
						// パラメータを生成
						for (var i = 0; i < COUNT; i++) {
							paramTable.AddRow (new object [] { count + i, Guid.NewGuid (), });
						}
						// 日時
						var startTime = DateTime.Now;
						// パラメータを差し替えながらレコードを生成
						Database.ExecuteNonQuery ("INSERT OR REPLACE INTO Test ([Serial], [Guid]) VALUES(:Serial, :Guid);", paramTable);
						// 生成に要した時間
						Console.text += $"Duration {DateTime.Now - startTime} / {COUNT} + {count}\n";
						// 結果
						DumpTable ("末尾100行", Database.ExecuteQuery ("SELECT * FROM Test LIMIT 100 OFFSET :offset;", new SQLiteRow { { "offset", count + COUNT - 100 }, }));
						break;
					case 31:
					case 32:
					case 33:
						DumpTable ("PCの手", Database.ExecuteQuery ("SELECT name FROM janken_decision;"));
						Database.ExecuteNonQuery ("INSERT INTO janken_log ([hand]) VALUES (:hand);", new SQLiteRow { { "hand", number - 30 } });
						DumpTable ("次のPCの手", Database.ExecuteQuery ("SELECT name FROM janken_decision;"));
						DumpTable ("人の手のログ", Database.ExecuteQuery ("SELECT * FROM janken_log"));
						DumpTable ("人の手の頻度", Database.ExecuteQuery ("SELECT * FROM janken_frq"));
						break;
				}
			}
		}

		/// <summary>テーブルの出力</summary>
		void DumpTable (string subject, SQLiteTable<SQLiteRow> table) {
			if (!string.IsNullOrEmpty (subject)) { Console.text += $"<color=aqua><size=24>--- Table {subject} ---</size></color>\n"; }
			if (table.IsNullOrEmpty ()) {
				Console.text += "<color=red>ERROR</color>\n";
			} else {
				for (var row = 0; row < table.Rows.Count; row++) {
					Console.text += $"<color=aqua>#{row + 1}/{table.Rows.Count}</color>\n";
					foreach (ColumnDefinition column in table.Columns) {
						if (table [row, column.Name] != null) {
							Console.text += $"<color=orange>{column.Name}</color> {ColumnDefinition.ColumnTypeName [column.Type]}: {table [row, column.Name]}\n";
						}
					}
				}
			}
			Console.text += "<color=aqua>===</color>\n";
			Debug.Log (table);
		}
		void DumpTable (string subject, SQLiteRow row) {
			if (!string.IsNullOrEmpty (subject)) { Console.text += $"<color=aqua><size=24>--- Row {subject} ---</size></color>\n"; }
			if (row.IsNullOrEmpty ()) {
				Console.text += "<color=red>ERROR</color>\n";
			} else {
				foreach (string name in row.Keys) {
					if (row [name] != null) {
						Console.text += $"<color=orange>{name}:</color> {row [name]}\n";
					}
				}
			}
			Console.text += "<color=aqua>===</color>\n";
			Debug.Log (row);
		}

		private const string _creationSql = @"
CREATE TABLE [Character] ( -- キャラ
		[CUID] TEXT NOT NULL PRIMARY KEY CHECK ([CUID] GLOB '[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'), -- キャラID
		[Name] TEXT, -- 名前
		[Created] TEXT CHECK([Created] GLOB '[0-9][0-9][0-9][0-9]-[0-1][0-9]-[0-3][0-9] [0-2][0-9]:[0-5][0-9]:[0-5][0-9]'), -- 作成日時
		[Modified] TEXT CHECK([Modified] GLOB '[0-9][0-9][0-9][0-9]-[0-1][0-9]-[0-3][0-9] [0-2][0-9]:[0-5][0-9]:[0-5][0-9]') -- 更新日時
		);
CREATE TABLE [Party] ( -- パーティ
		[PUID] TEXT NOT NULL PRIMARY KEY CHECK ([PUID] GLOB '[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'), -- パーティID
		[Title] TEXT, -- タイトル
		[Message] TEXT, -- メッセージ
		[Created] TEXT CHECK([Created] GLOB '[0-9][0-9][0-9][0-9]-[0-1][0-9]-[0-3][0-9] [0-2][0-9]:[0-5][0-9]:[0-5][0-9]'), -- 作成日時
		[Modified] TEXT CHECK([Modified] GLOB '[0-9][0-9][0-9][0-9]-[0-1][0-9]-[0-3][0-9] [0-2][0-9]:[0-5][0-9]:[0-5][0-9]') -- 更新日時
		);
CREATE TABLE [Member] ( -- パーティメンバー
		[PUID] TEXT NOT NULL CHECK ([PUID] GLOB '[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'), -- パーティID
		[Order] INTEGER NOT NULL, -- パーティ内番号
		[CUID] TEXT CHECK ([CUID] GLOB '[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]-[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'), -- キャラID
		[Class] TEXT, -- クラス
		[Message] TEXT, -- メッセージ
		[Created] TEXT CHECK([Created] GLOB '[0-9][0-9][0-9][0-9]-[0-1][0-9]-[0-3][0-9] [0-2][0-9]:[0-5][0-9]:[0-5][0-9]'), -- 作成日時
		[Modified] TEXT CHECK([Modified] GLOB '[0-9][0-9][0-9][0-9]-[0-1][0-9]-[0-3][0-9] [0-2][0-9]:[0-5][0-9]:[0-5][0-9]'), -- 更新日時
		PRIMARY KEY( [PUID], [Order] ), -- パーティ内ポジションはユニーク
		UNIQUE( [PUID], [CUID] ) -- 多重参加は不許可
		);
CREATE VIEW [RichCharacter] AS -- キャラクタ
		SELECT 
			[Character].[CUID] AS [CUID],
			[Character].[Name] AS [Name],
			[Character].[Created] AS [Created],
			[Character].[Modified] AS [Modified],
			COUNT ([Member].[PUID]) AS [LinkCount]
		FROM [Character]
			LEFT OUTER JOIN [Member] ON [Member].[CUID] = [Character].[CUID]
		GROUP BY [Character].[CUID];
CREATE VIEW [RichMember] AS -- パーティメンバー
		SELECT 
			[Member].[PUID] AS [PUID],
			[Member].[Order] AS [Order],
			[Member].[CUID] AS [CUID],
			[Character].[Name] AS [Nmae],
			[Member].[Class] AS [Class],
			[Member].[Message] AS [Message],
			[Party].[Title] AS [PartyTitle],
			[Party].[Message] AS [PartyMessage],
			[Member].[Created] AS [Created],
			[Member].[Modified] AS [Modified]
		FROM [Member]
			INNER JOIN [Character] ON [Member].[CUID] = [Character].[CUID]
			LEFT OUTER JOIN [Party] ON [Member].[PUID] = [Party].[PUID]
		GROUP BY [Member].[PUID], [Member].[Order];
CREATE VIEW [RichParty] AS -- パーティ
		SELECT 
			[Party].[PUID] AS [PUID],
			[Party].[Title] AS [Title],
			[Party].[Message] AS [Message],
			[Party].[Created] AS [Created],
			[Party].[Modified] AS [Modified],
			GROUP_CONCAT([Member].[Order], ',') AS [Members]
		FROM [Party]
			LEFT OUTER JOIN [Member] ON [Member].[PUID] = [Party].[PUID]
			LEFT OUTER JOIN [Character] ON [Character].[CUID] = [Member].[CUID]
		GROUP BY [Party].[PUID];
CREATE TRIGGER [InsertCharacter] AFTER INSERT ON [Character] FOR EACH ROW
		BEGIN
		UPDATE [Character] SET [Created] = CURRENT_TIMESTAMP WHERE ROWID == NEW.ROWID;
		END;
CREATE TRIGGER [UpdateCharacter] AFTER UPDATE ON [Character] FOR EACH ROW
		BEGIN
		UPDATE [Character] SET [Modified] = CURRENT_TIMESTAMP WHERE ROWID == NEW.ROWID;
		END;
CREATE TRIGGER [InsertMember] AFTER INSERT ON [Member] FOR EACH ROW
		BEGIN
		UPDATE [Member] SET [Created] = CURRENT_TIMESTAMP WHERE ROWID == NEW.ROWID;
		END;
CREATE TRIGGER [UpdateMember] AFTER UPDATE ON [Member] FOR EACH ROW
		BEGIN
		UPDATE [Member] SET [Modified] = CURRENT_TIMESTAMP WHERE ROWID == NEW.ROWID;
		END;
CREATE TRIGGER [InsertParty] AFTER INSERT ON [Party] FOR EACH ROW
		BEGIN
		UPDATE [Party] SET [Created] = CURRENT_TIMESTAMP WHERE ROWID == NEW.ROWID;
		END;
CREATE TRIGGER [UpdateParty] AFTER UPDATE ON [Party] FOR EACH ROW
		BEGIN
		UPDATE [Party] SET [Modified] = CURRENT_TIMESTAMP WHERE ROWID == NEW.ROWID;
		END;
";

		private const string _creationSql2 = @"
CREATE TABLE IF NOT EXISTS Test (
[Serial] INTEGER NOT NULL PRIMARY KEY, -- ID
[Guid] TEXT, -- 名前
[Created] TEXT CHECK([Created] GLOB '[0-9][0-9][0-9][0-9]-[0-1][0-9]-[0-3][0-9] [0-2][0-9]:[0-5][0-9]:[0-5][0-9]'), -- 作成日時
[Modified] TEXT CHECK([Modified] GLOB '[0-9][0-9][0-9][0-9]-[0-1][0-9]-[0-3][0-9] [0-2][0-9]:[0-5][0-9]:[0-5][0-9]') -- 更新日時
);";

		private const string _creationSql3 = @"
-- 手の呼び名
DROP TABLE IF EXISTS janken_names;
CREATE TABLE IF NOT EXISTS janken_names (
    hand INTEGER UNIQUE NOT NULL CHECK (hand >= 1 AND hand <= 3), 
    name TEXT UNIQUE NOT NULL
);
INSERT INTO janken_names VALUES
    (1, 'Goo'),
    (2, 'Choki'),
    (3, 'Par')
;

-- 相手の手のログ
DROP TABLE IF EXISTS janken_log;
CREATE TABLE IF NOT EXISTS janken_log (
    number INTEGER PRIMARY KEY AUTOINCREMENT,
    hand INTEGER NOT NULL CHECK (hand >= 1 AND hand <= 3)
);
INSERT INTO janken_log (hand) VALUES 
    (abs(random() % 3) + 1),
    (abs(random() % 3) + 1),
    (abs(random() % 3) + 1),
    (abs(random() % 3) + 1),
    (abs(random() % 3) + 1)
;

-- 相手の手の頻度
DROP TABLE IF EXISTS janken_frq;
CREATE TABLE IF NOT EXISTS janken_frq (
    hand1 INTEGER NOT NULL CHECK (hand1 >= 1 AND hand1 <= 3), 
    hand2 INTEGER NOT NULL CHECK (hand2 >= 1 AND hand2 <= 3), 
    hand3 INTEGER NOT NULL CHECK (hand3 >= 1 AND hand3 <= 3), 
    hand4 INTEGER NOT NULL CHECK (hand4 >= 1 AND hand4 <= 3), 
    frequency INTEGER DEFAULT (abs(random() % 3))
);
WITH RECURSIVE tmp (h1, h2, h3, h4) AS (
    SELECT 1, 1, 1, 1
    UNION ALL
    SELECT 
        iif(h1 < 3, h1 + 1, 1), 
        iif(h1 >= 3, iif(h2 < 3, h2 + 1, 1), h2), 
        iif(h1 >= 3 AND h2 >= 3, iif(h3 < 3, h3 + 1, 1), h3), 
        iif(h1 >= 3 AND h2 >= 3 AND h3 >= 3, h4 + 1, h4) 
    FROM tmp WHERE h1 < 3 or h2 < 3 or h3 < 3 or h4 < 3
)
INSERT INTO janken_frq (hand1, hand2, hand3, hand4) SELECT * FROM tmp;

-- 相手の手を記録したときの頻度記録処理
DROP TRIGGER IF EXISTS janken_logging;
CREATE TRIGGER IF NOT EXISTS janken_logging AFTER INSERT ON janken_log BEGIN
    UPDATE janken_frq SET frequency = frequency + 1 
    FROM (
        SELECT 
               max(hand) OVER (ORDER BY number DESC ROWS CURRENT ROW)AS h1,
               max(hand) OVER (ORDER BY number DESC ROWS BETWEEN 1 FOLLOWING AND 1 FOLLOWING) AS h2,
               max(hand) OVER (ORDER BY number DESC ROWS BETWEEN 2 FOLLOWING AND 2 FOLLOWING) AS h3,
               max(hand) OVER (ORDER BY number DESC ROWS BETWEEN 3 FOLLOWING AND 3 FOLLOWING) AS h4
        FROM janken_log LIMIT 1
    )
    WHERE hand1 = h1 AND hand2 = h2 AND hand3 = h3 AND hand4 = h4;
    DELETE FROM janken_log WHERE number NOT IN (
        SELECT number FROM janken_log ORDER BY number DESC LIMIT 5
    );
END;

-- 相手の手を予測してそれに勝つ自分の手を決める
DROP VIEW IF EXISTS janken_decision;
CREATE VIEW IF NOT EXISTS janken_decision AS 
    SELECT name, hand FROM (
        SELECT iif(janken_frq.hand1 <= 1, 3, hand1 - 1) AS hand FROM janken_frq, (
            SELECT 
                max(hand) OVER (ORDER BY number DESC ROWS CURRENT ROW) AS h2,
                max(hand) OVER (ORDER BY number DESC ROWS BETWEEN 1 FOLLOWING AND 1 FOLLOWING) AS h3,
                max(hand) OVER (ORDER BY number DESC ROWS BETWEEN 2 FOLLOWING AND 2 FOLLOWING) AS h4
            FROM janken_log LIMIT 1
        )
        WHERE hand2 = h2 AND hand3 = h3 AND hand4 = h4
        ORDER BY frequency DESC
        LIMIT 1
    ) NATURAL JOIN janken_names
;
";
	}

	/// <summary>キャラ</summary>
	public class Character {
		public Guid CUID; // ID
		public string Name; // 名前

		protected Character (Guid cuid, string name) {
			this.CUID = cuid;
			this.Name = name;
		}

		/// <summary>全データ取得</summary>
		public static SQLiteTable<SQLiteRow> GetTable () {
			return Test.Database.ExecuteQuery ("SELECT * FROM [RichCharacter];");
		}

		/// <summary>IDを指定して既存のキャラを生成</summary>
		public Character (Guid cuid) {
			var table = Test.Database.ExecuteQuery ("SELECT * FROM [Character] WHERE [CUID]=:CUID;", new SQLiteRow { { "CUID", cuid }, });
			this.CUID = cuid;
			this.Name = table.Top.GetColumn ("Name");
		}

		/// <summary>名前を与えて新しいキャラを作る</summary>
		public static Character NewCharacter (string name) {
			var character = new Character (Guid.NewGuid (), name);
			Test.Database.ExecuteNonQuery ("INSERT INTO [Character]([CUID], [Name]) VALUES(:CUID, :Name);", new SQLiteRow {
				{ "CUID", character.CUID },
				{ "Name", character.Name },
			});
			return character;
		}
	}

	/// <summary>クラス</summary>
	public enum JobClass {
		Sponger,
		Wizard,
		Fighter,
		Cleric,
		Archer,
		Lancer,
	}

	/// <summary>パーティメンバー</summary>
	public class Member {
		public int Order;
		public Character Character;
		public JobClass JobClass;
		public string Message;

		protected Member (int order, Character character, JobClass jobclass, string message) {
			this.Order = order;
			this.Character = character;
			this.JobClass = jobclass;
			this.Message = message;
		}

		/// <summary>全データ取得</summary>
		public static SQLiteTable<SQLiteRow> GetTable (Guid puid = default (Guid)) {
			if (puid == default (Guid)) {
				return Test.Database.ExecuteQuery ("SELECT * FROM [RichMember];");
			} else {
				return Test.Database.ExecuteQuery ("SELECT * FROM [RichMember] WHERE [PUID]=:PUID;", new SQLiteRow { { "PUID", puid }, });
			}
		}

		/// <summary>パーティIDと番号を指定して既存のメンバーを生成</summary>
		public Member (Guid puid, int order) {
			var table = Test.Database.ExecuteQuery ("SELECT * FROM [Member] WHERE [PUID]=:PUID AND [Order]=:Order;", new SQLiteRow {
				{ "PUID", puid },
				{ "Order", order },
			});
			this.Order = order;
			this.Character = new Character (table.Top.GetColumn (Guid.Parse, "CUID"));
			this.JobClass = table.Top.GetColumn ("Class", JobClass.Sponger);
			this.Message = table.Top.GetColumn ("Message");
		}

		/// <summary>キャラとクラスを指定して新しいメンバーを作る</summary>
		public static Member NewMember (Guid puid, int order, Character character, JobClass jobclass, string message = "") {
			var member = new Member (order, character, jobclass, message);
			Test.Database.ExecuteNonQuery ("INSERT INTO [Member]([PUID], [Order], [CUID], [Class], [Message]) VALUES(:PUID, :Order, :CUID, :Class, :Message);", new SQLiteRow {
				{ "PUID", puid },
				{ "Order", member.Order },
				{ "CUID", member.Character.CUID },
				{ "Class", member.JobClass },
				{ "Message", member.Message },
			});
			return member;
		}
	}

	/// <summary>パーティ</summary>
	public class Party {
		public Guid PUID;
		public string Title;
		public string Message;
		public Member this [int order] { get { return this.Members.Find (member => member.Order == order); } }
		protected List<Member> Members;

		protected Party (Guid puid, string title, string message) {
			this.PUID = puid;
			this.Title = title;
			this.Message = message;
			this.Members = new List<Member> { };
		}

		/// <summary>全データ取得</summary>
		public static SQLiteTable<SQLiteRow> GetTable () {
			return Test.Database.ExecuteQuery ("SELECT * FROM [RichParty];");
		}

		/// <summary>IDを指定して既存のパーティを生成</summary>
		public Party (Guid puid) {
			var table = Test.Database.ExecuteQuery ("SELECT * FROM [RichParty] WHERE [PUID]=:PUID;", new SQLiteRow { { "PUID", puid }, });
			this.PUID = puid;
			this.Title = table.Top.GetColumn ("Title");
			this.Message = table.Top.GetColumn ("Message");
			var orders = (new List<string> (table.Top.GetColumn ("Members").Split (new [] { ',' }))).ConvertAll (str => int.Parse (str));
			orders.Sort ();
			this.Members = orders.ConvertAll (order => new Member (this.PUID, order));
		}

		/// <summary>タイトルを与えて新しいパーティを作る</summary>
		public static Party NewParty (string title, string message = "") {
			var party = new Party (Guid.NewGuid (), title, message);
			Test.Database.ExecuteNonQuery ("INSERT INTO [Party]([PUID], [Title], [Message]) VALUES(:PUID, :Title, :Message);", new SQLiteRow {
				{ "PUID", party.PUID },
				{ "Title", party.Title },
				{ "Message", party.Message },
			});
			return party;
		}

		/// <summary>パーティにメンバーを加える</summary>
		public void Add (Character character, JobClass jobclass, string message) {
			var newMember = Member.NewMember (this.PUID, this.Members.Count, character, jobclass, message);
			this.Members.Add (newMember);
		}

		/// <summary>文字列化</summary>
		public override string ToString () {
			var str = new List<string> { };
			str.Add ($"パーティ[{this.Title}]「{this.Message}」");
			foreach (var member in this.Members) {
				str.Add ($"{member.Order}:[{member.Character.Name}]({member.JobClass})「{member.Message}」");
			}
			return string.Join ("\n", str);
		}
	}

}