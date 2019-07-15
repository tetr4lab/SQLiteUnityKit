using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SQLiteUnity;

namespace SQLiteTest {

	public class Test : MonoBehaviour {

		// DB
		public static SQLite Database;

		// 出力先
		public static Text Console;

		// 準備
		private void Awake () {
			Console = GetComponentInChildren<Text> ();
		}

		// テスト
		private void Start () {
			// 開始宣言
			Console.text = "SQLiteUnity Test Start\n\n";
			Debug.Log ("Start");

			// DB接続とテスト (初回は生成)
			using (Database = new SQLite ("SQLiteTest.db", Creation)) {

				// 初回のみ
				if (SQLiteTable.IsNullOrEmpty (Party.GetTable ())) {
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
				foreach (SQLiteRow row in Party.GetTable ()) {
					Console.text += $"{new Party (row.GetColumn (Guid.Parse, "PUID"))}\n=====\n";
				}
				this.DumpTable ("パーティ", Party.GetTable ());
				this.DumpTable ("メンバー", Member.GetTable ());
				this.DumpTable ("キャラクタ", Character.GetTable ());
			}

			// 保存場所の報告
			Console.text += $"\npath {Application.persistentDataPath}";
			Debug.Log (Application.persistentDataPath);
		}

		// テーブルの出力
		void DumpTable (string subject, SQLiteTable table) {
			if (!string.IsNullOrEmpty (subject)) { Console.text += $"--- Dump {subject} ---\n"; }
			if (SQLiteTable.IsNullOrEmpty (table)) {
				Console.text += "ERROR\n";
			} else {
				for (var row = 0; row < table.Rows.Count; row++) {
					Console.text += $"#{row + 1}/{table.Rows.Count}\n";
					foreach (ColumnDefinition column in table.Columns) {
						if (table [row, column.Name] != null) {
							Console.text += $"{column.Name} {ColumnDefinition.ColumnTypeName [column.Type]}: {table [row, column.Name]}\n";
						}
					}
				}
			}
			Console.text += "===\n";
			Debug.Log (table);
		}
		void DumpTable (string subject, SQLiteRow row) {
			if (!string.IsNullOrEmpty (subject)) { Console.text += $"--- Dump {subject} ---\n"; }
			if (SQLiteRow.IsNullOrEmpty (row)) {
				Console.text += "ERROR\n";
			} else {
				foreach (string name in row.Keys) {
					if (row [name] != null) {
						Console.text += $"{name}: {row [name]}\n";
					}
				}
			}
			Console.text += "===\n";
			Debug.Log (row);
		}

		#region Creation
		private const string Creation = @"
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
		#endregion

	}


	// キャラ
	public class Character {
		public Guid CUID; // ID
		public string Name; // 名前

		protected Character (Guid cuid, string name) {
			this.CUID = cuid;
			this.Name = name;
		}

		// 全データ取得
		public static SQLiteTable GetTable () {
			return Test.Database.ExecuteQuery ("SELECT * FROM [RichCharacter];");
		}

		// IDを指定して既存のキャラを生成
		public Character (Guid cuid) {
			var table = Test.Database.ExecuteQuery ("SELECT * FROM [Character] WHERE [CUID]=:CUID;", new SQLiteRow { { "CUID", cuid }, });
			this.CUID = cuid;
			this.Name = table.Top.GetColumn ("Name");
		}

		// 名前を与えて新しいキャラを作る
		public static Character NewCharacter (string name) {
			var character = new Character (Guid.NewGuid (), name);
			Test.Database.ExecuteNonQuery ("INSERT INTO [Character]([CUID], [Name]) VALUES(:CUID, :Name);", new SQLiteRow {
				{ "CUID", character.CUID },
				{ "Name", character.Name },
			});
			return character;
		}
	}

	// クラス
	public enum JobClass {
		Sponger,
		Wizard,
		Fighter,
		Cleric,
		Archer,
		Lancer,
	}

	// パーティメンバー
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

		// 全データ取得
		public static SQLiteTable GetTable (Guid puid = default (Guid)) {
			if (puid == default (Guid)) {
				return Test.Database.ExecuteQuery ("SELECT * FROM [RichMember];");
			} else {
				return Test.Database.ExecuteQuery ("SELECT * FROM [RichMember] WHERE [PUID]=:PUID;", new SQLiteRow { { "PUID", puid }, });
			}
		}

		// パーティIDと番号を指定して既存のメンバーを生成
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

		// キャラとクラスを指定して新しいメンバーを作る
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

	// パーティ
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

		// 全データ取得
		public static SQLiteTable GetTable () {
			return Test.Database.ExecuteQuery ("SELECT * FROM [RichParty];");
		}

		// IDを指定して既存のパーティを生成
		public Party (Guid puid) {
			var table = Test.Database.ExecuteQuery ("SELECT * FROM [RichParty] WHERE [PUID]=:PUID;", new SQLiteRow { { "PUID", puid }, });
			this.PUID = puid;
			this.Title = table.Top.GetColumn ("Title");
			this.Message = table.Top.GetColumn ("Message");
			var orders = (new List<string> (table.Top.GetColumn ("Members").Split (new [] { ',' }))).ConvertAll (str => int.Parse (str));
			orders.Sort ();
			this.Members = orders.ConvertAll (order => new Member (this.PUID, order));
		}

		// タイトルを与えて新しいパーティを作る
		public static Party NewParty (string title, string message = "") {
			var party = new Party (Guid.NewGuid (), title, message);
			Test.Database.ExecuteNonQuery ("INSERT INTO [Party]([PUID], [Title], [Message]) VALUES(:PUID, :Title, :Message);", new SQLiteRow {
				{ "PUID", party.PUID },
				{ "Title", party.Title },
				{ "Message", party.Message },
			});
			return party;
		}

		// パーティにメンバーを加える
		public void Add (Character character, JobClass jobclass, string message) {
			var newMember = Member.NewMember (this.PUID, this.Members.Count, character, jobclass, message);
			this.Members.Add (newMember);
		}

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