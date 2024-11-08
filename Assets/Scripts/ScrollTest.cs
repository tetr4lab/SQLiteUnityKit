using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

using Tetr4lab.InfiniteScroll;
using TextAnchor = Tetr4lab.InfiniteScroll.TextAnchor;
using Tetr4lab.SQLiteUnity;
using System.IO;

/// <summary>仮想スクロールサンプルメイン</summary>
public class ScrollTest : MonoBehaviour {

	/// <summary>DBファイル名</summary>
	private const string DbPath = "scroll_test.db";

	/// <summary>シングルトンインスタンス</summary>
	private static ScrollTest instance;

	/// <summary>DBへの反映</summary>
	public static void UpdateDatabase (Item item) => instance?.UpdateDb (item);

	/// <summary>DBへの反映</summary>
	public static void UpdateDatabase () => instance?.UpdateDb ();

	/// <summary>デフォルトのアイテム数</summary>
	private const int NumberOfItems = 1000;

	/// <summary>デフォルトの幅</summary>
	private const int DefaultWidth = 256;

	/// <summary>デフォルトの高さ</summary>
	private const int DefaultHeight = 128;

	/// <summary>仮想スクロール実体</summary>
	[SerializeField]
    private InfiniteScrollRect scroll = default;

    /// <summary>乱数トグル</summary>
    [SerializeField]
    private Toggle randomToggle = default;

    /// <summary>垂直トグル</summary>
    [SerializeField]
    private Toggle verticalToggle = default;

    /// <summary>逆順トグル</summary>
    [SerializeField]
    private Toggle reverseToggle = default;

    [SerializeField]
    private Dropdown alignDropdown = default;

    /// <summary>サイズ制御トグル</summary>
    [SerializeField]
    private Toggle ctrlSizeToggle = default;

    /// <summary>リセットボタン</summary>
    [SerializeField]
    private Button resetButton = default;

    /// <summary>インデックススライダ</summary>
    [SerializeField]
    private Slider indexSlider = default;

    /// <summary>追加ボタン</summary>
    [SerializeField]
    private Button addButton = default;

    /// <summary>追加ボタンのラベル</summary>
    private Text addButtonLabel;

    /// <summary>挿入ボタン</summary>
    [SerializeField]
    private Button insertButton = default;

    /// <summary>挿入ボタンのラベル</summary>
    private Text insertButtonLabel;

    /// <summary>除去ボタン</summary>
    [SerializeField]
    private Button removeButton = default;

    /// <summary>除去ボタンのラベル</summary>
    private Text removeButtonLabel;

    /// <summary>指定除去ボタン</summary>
    [SerializeField]
    private Button removeCheckedButton = default;

    /// <summary>全除去ボタン</summary>
    [SerializeField]
    private Button clearButton = default;

    /// <summary>デバッグ表示</summary>
    [SerializeField]
    private Text debugInfo = default;

    /// <summary>アイテムの数</summary>
    [SerializeField]
    private int numberOfItems = NumberOfItems;

	/// <summary>最初に表示するインデックス</summary>
	[SerializeField]
    private int firstIndex = default;

	/// <summary>クリエイトボタン</summary>
	[SerializeField]
	private Button CreateButton = default;

	/// <summary>ドロップボタン</summary>
	[SerializeField]
	private Button DropButton = default;

	/// <summary>テスト切り替えボタン</summary>
	[SerializeField]
	private Button OtherTestButton = default;

	/// <summary>DB</summary>
	public SQLite<Table, Item> Database;

	/// <summary>シングルトン初期化</summary>
	private void Awake () {
		if (instance != null) {
			Destroy (this);
		} else {
			instance = this;
		}
	}

	/// <summary>
	/// 初期化
	///   コントロールの検出と設定
	/// </summary>
	private void Start () {
		scroll ??= transform.parent.GetComponentInChildren<InfiniteScrollRect> ();
		var toggles = GetComponentsInChildren<Toggle> ();
		randomToggle ??= toggles.GetNth (0);
		verticalToggle ??= toggles.GetNth (1);
		reverseToggle ??= toggles.GetNth (2);
		ctrlSizeToggle ??= toggles.GetNth (3);
		ctrlSizeToggle?.onValueChanged.AddListener (isOn => { if (alignDropdown) { alignDropdown.interactable = !isOn; } });
		alignDropdown ??= GetComponentInChildren<Dropdown> ();
		var buttons = transform.parent.GetComponentsInChildren<Button> ();
		resetButton ??= buttons.GetNth (0);
		resetButton?.onClick.AddListener (OnReset);
		indexSlider ??= GetComponentInChildren<Slider> ();
		addButton ??= buttons.GetNth (1);
		addButtonLabel = addButton?.GetComponentInChildren<Text> ();
		addButton?.onClick.AddListener (() => {
			var time = DateTime.Now;
			// 追加
			AddItems ();
			OnReset ();
			Debug.Log ($"duration {DateTime.Now - time}");
		});
        insertButton ??= buttons.GetNth (2);
        insertButtonLabel = insertButton?.GetComponentInChildren<Text> ();
        insertButton?.onClick.AddListener (() => {
			var time = DateTime.Now;
			// 挿入
			var index = Mathf.RoundToInt (indexSlider.value);
			var serial = ((int) (scroll [index] as Item) ["Serial"]) - 1;
			if (serial >= 0 && (index == 0 || (int) (scroll [index - 1] as Item)? ["Serial"] < serial)) {
				AddItems (serial);
			}
			OnReset ();
            Debug.Log ($"duration {DateTime.Now - time}");
        });
        removeButton ??= buttons.GetNth (3);
        removeButtonLabel = removeButton?.GetComponentInChildren<Text> ();
        removeButton?.onClick.AddListener (() => {
			var time = DateTime.Now;
			// 除去
			var item = scroll [Mathf.RoundToInt (indexSlider.value)] as Item;
			Database.ExecuteNonQuery ("DELETE FROM Test WHERE [Guid] = :Guid;", item);
			OnReset ();
			Debug.Log ($"Removed {item}");
        });
        removeCheckedButton ??= buttons.GetNth (4);
        removeCheckedButton?.onClick.AddListener (() => {
			var time = DateTime.Now;
			// チェックの除去
			Database.ExecuteNonQuery ("DELETE FROM Test WHERE [Check] = 'True';");
			OnReset ();
			Debug.Log ($"Removed Checked duration {DateTime.Now - time}");
        });
        clearButton ??= buttons.GetNth (5);
        clearButton?.onClick.AddListener (() => {
			var time = DateTime.Now;
			// 全除去
			Database.ExecuteNonQuery ("DELETE FROM Test;");
			OnReset ();
			Debug.Log ($"Removed All duration {DateTime.Now - time}");
		});
		CreateButton ??= buttons.GetNth (6);
		var label = CreateButton.GetComponentInChildren<Text> ();
		label.text = $"Create {numberOfItems}";
		CreateButton.onClick.AddListener (() => {
			OnReset ();
			AddItems (numberOfItems: numberOfItems);
			OnReset ();
		});
		DropButton ??= buttons.GetNth (7);
		DropButton.onClick.AddListener (() => {
			if (Database != null) {
				Database.Dispose ();
				Database = null;
			}
			var path = Path.Combine (Application.persistentDataPath, DbPath);
			File.Delete (path);
			scroll.Clear ();
			Debug.Log ($"Dropped Database {path}");
		});
		OtherTestButton ??= buttons.GetNth (8);
		OtherTestButton.onClick.AddListener (() => {
			var prefab = Resources.Load<GameObject> ("Prefabs/Canvas1");
			var obj = Instantiate (prefab, transform.parent.parent);
			obj.transform.SetSiblingIndex (transform.parent.GetSiblingIndex ());
			Destroy (transform.parent.gameObject);
		});
		debugInfo ??= GameObject.Find ("DebugInfo")?.GetComponentInChildren<Text> ();
        // リセットボタンを押す
        OnReset ();
    }

    /// <summary>更新</summary>
    private void Update () {
		if (indexSlider) {
            // 項目数に応じたスライダの制限
            indexSlider.maxValue = scroll.Count > 0 ? scroll.Count - 1 : 0;
        }
        if (addButtonLabel) {
            // 追加ボタンのインデックス
            addButtonLabel.text = $"Add #{scroll.Count}";
        }
		if (addButton) {
			// 追加ボタンの活殺
			addButton.interactable = Database != null;
		}
        var index = Database != null && scroll.Valid ? indexSlider ? Mathf.RoundToInt (indexSlider.value) : (scroll.FirstIndex + scroll.LastIndex) / 2 : 0;
		if (insertButtonLabel) {
			// 挿入ボタンのインデックス
			insertButtonLabel.text = $"Insert #{(index >= 0 ? index : 0)}";
		}
		if (insertButton) {
			// 挿入ボタンの活殺
			insertButton.interactable = scroll.Count > 0;
		}
		if (removeButtonLabel) {
            // 除去ボタンのインデックスと活殺
            removeButtonLabel.text = scroll.Count > 0 ? $"Remove #{index}" : "Remove";
            removeButton.interactable = scroll.Count > 0;
        }
        if (removeCheckedButton) {
            // 指定除去ボタンの活殺
            removeCheckedButton.interactable = scroll.Count > 0;
        }
        if (clearButton) {
            // 全除去ボタンの活殺
            clearButton.interactable = scroll.Count > 0;
        }
		if (debugInfo) {
			// デバッグ情報表示
			index = 0;
			debugInfo.text = $@"viewport: {scroll.viewport.rect.size}
content: {scroll.content.rect.size}
scroll: {scroll.Scroll}
visible: {(scroll.FirstIndex < 0 ? "no items" : $"{scroll.FirstIndex} - {scroll.LastIndex}")} / {scroll.Count}";
		}
    }

    /// <summary>不定サイズ</summary>
    private float RandomSize (bool vertical) => (vertical ? DefaultHeight : DefaultWidth) + Random.Range (0, 5) * 40;

    /// <summary>ドロップダウンからenumを得る変換辞書</summary>
    private static readonly Dictionary<int, TextAnchor> _alignDict = new Dictionary<int, TextAnchor> { { 0, TextAnchor.LowerLeft }, { 1, TextAnchor.MiddleCenter }, { 2, TextAnchor.UpperRight }, };

	/// <summary>DB初期化式</summary>
	private const string _creationSql = @"
CREATE TABLE IF NOT EXISTS Test (
[Serial] INTEGER NOT NULL UNIQUE,
[Guid] TEXT NOT NULL PRIMARY KEY,
[Title] TEXT,
[Description] TEXT,
[Label] TEXT,
[Check] TEXT,
[Width] REAL,
[Height] REAL,
[Created] TEXT CHECK([Created] GLOB '[0-9][0-9][0-9][0-9]-[0-1][0-9]-[0-3][0-9] [0-2][0-9]:[0-5][0-9]:[0-5][0-9]'), -- 作成日時
[Modified] TEXT CHECK([Modified] GLOB '[0-9][0-9][0-9][0-9]-[0-1][0-9]-[0-3][0-9] [0-2][0-9]:[0-5][0-9]:[0-5][0-9]') -- 更新日時
);";

	/// <summary>リセットボタン</summary>
	public void OnReset () {
		var time = DateTime.Now;
		// 設定の取得と反映
		if (numberOfItems <= 0) {
			numberOfItems = NumberOfItems;
		}
		if (firstIndex < 0 || firstIndex >= numberOfItems) {
			firstIndex = 0;
		}
		scroll.horizontal = !(scroll.vertical = verticalToggle.isOn);
#if UNITY_2022_1_OR_NEWER
        scroll.horizontalScrollbar = scroll.horizontalScrollbar;
        scroll.verticalScrollbar = scroll.verticalScrollbar;
#endif
        scroll.reverseArrangement = reverseToggle.isOn;
		scroll.controlChildSize = ctrlSizeToggle.isOn;
		scroll.childAlignment = _alignDict [alignDropdown.value];
		// 旧ハンドルを破棄
		if (Database != null) {
			Database.Dispose ();
		}
		// ハンドル生成
		Database = new SQLite<Table, Item> ("scroll_test.db", _creationSql);
		// テーブル取得とスクロール初期化
		var table = Database.ExecuteQuery ("SELECT row_number() OVER (ORDER BY [Serial]) - 1 as [Number], * FROM Test ORDER BY [Serial];");
		if (table != null && table.Rows.Count > 0) {
			scroll.Initialize (table.Rows, firstIndex);
			Debug.Log ($"Initialized {{{string.Join (", ", scroll.ConvertAll (i => i.ToString ()))}}}");
		} else if (scroll.Valid) {
			scroll.Clear ();
		}
		Debug.Log ($"duration {DateTime.Now - time}");
	}

	/// <summary>項目の追加</summary>
	private void AddItems (int nextSerial = -1, int numberOfItems = 1) {
		var time = DateTime.Now;
		// 次の番号
		var table = Database.ExecuteQuery ("SELECT count([Serial]) as Count, iif(count([Serial]) > 0, max([Serial]) + 1, 0) as Next FROM Test;");
		var count = table [0].GetColumn ("Count", 0);
		if (nextSerial < 0) {
			nextSerial = table [0].GetColumn ("Next", 0);
		}
		// パラメータの型を生成
		var paramTable = new Table (new [] {
			new ColumnDefinition ("Serial", SQLiteColumnType.SQLITE_INTEGER),
			new ColumnDefinition ("Guid", SQLiteColumnType.SQLITE_TEXT),
			new ColumnDefinition ("Title", SQLiteColumnType.SQLITE_TEXT),
			new ColumnDefinition ("Description", SQLiteColumnType.SQLITE_TEXT),
			new ColumnDefinition ("Label", SQLiteColumnType.SQLITE_TEXT),
			new ColumnDefinition ("Width", SQLiteColumnType.SQLITE_FLOAT),
			new ColumnDefinition ("Height", SQLiteColumnType.SQLITE_FLOAT),
		});
		// パラメータを生成
		for (var i = 0; i < numberOfItems; i++) {
			paramTable.AddRow (new object [] {
				nextSerial + i,
				Guid.NewGuid (),
				$"No. {nextSerial + i}",
				$"start of {nextSerial + i}\nend of {nextSerial + i}",
				$"#",
				randomToggle.isOn ? RandomSize (false) : DefaultWidth,
				randomToggle.isOn ? RandomSize (true) : DefaultHeight,
			});
		}
		Debug.Log ($"duration {DateTime.Now - time}");
		time = DateTime.Now;
		// パラメータを差し替えながらレコードを生成
		Database.ExecuteNonQuery ("INSERT OR REPLACE INTO Test ([Serial], [Guid], [Title], [Description], [Label], [Width], [Height]) VALUES(:Serial, :Guid, :Title, :Description, :Label, :Width, :Height);", paramTable);
		Debug.Log ($"AddItems {count} + {numberOfItems} duration {DateTime.Now - time}");
	}

	/// <summary>DBへの反映</summary>
	private void UpdateDb (Item item) {
		Database.ExecuteNonQuery ("UPDATE Test SET ([Title], [Description], [Label], [Check], [Width], [Height]) = (VALUES (:Title, :Description, :Label, :Check, :Width, :Height)) WHERE [Guid] = :Guid;", item);
		item.DbDirty = false;
		Debug.Log ($"Updated DB {{{item}}}");
	}

	/// <summary>DBへの反映</summary>
	private void UpdateDb () {
		var targetItems = scroll.FindAll (item => (item as Item).DbDirty);
		foreach (Item item in targetItems) {
			Database.ExecuteNonQuery ("UPDATE Test SET ([Title], [Description], [Label], [Check], [Width], [Height]) = (VALUES (:Title, :Description, :Label, :Check, :Width, :Height)) WHERE [Guid] = :Guid;", item);
			item.DbDirty = false;
		}
		Debug.Log ($"Updated DB {{{string.Join (", ", targetItems.ConvertAll (i => i.ToString ()))}}}");
	}

}

/// <summary>配列ヘルパー</summary>
public static class ArrayHelper {
    /// <summary>ジェネリック型配列から指定インデックスの要素を安全に取得する</summary>
    /// <typeparam name="T">ジェネリック型(クラス)</typeparam>
    /// <param name="items">ジェネリック型配列</param>
    /// <param name="index">インデックス</param>
    /// <returns>指定されたインデックスが範囲外ならdefault値を返す</returns>
    public static T GetNth<T> (this T [] items, int index) where T : class => index < 0 || index >= items.Length ? default : items [index];
}
