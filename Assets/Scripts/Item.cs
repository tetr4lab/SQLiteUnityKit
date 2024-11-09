using Tetr4lab.UI;
using UnityEngine;
using UnityEngine.UI;
using Tetr4lab.SQLiteUnity;

/// <summary>論理アイテム</summary>
public class Item : SQLiteRow, IInfiniteScrollItem {

    /// <summary>スクロール方向の位置 (実態へ反映)</summary>
    public float Position { get; set; }

    /// <summary>スクロール方向のサイズ (物理項目から反映)</summary>
    public float Size { get; set; }

	/// <summary>物理への反映が必要になった</summary>
	public bool Dirty { get; set; }

	/// <summary>DBへの反映が必要になった</summary>
	public bool DbDirty { get; set; }

    /// <summary>タイトル</summary>
    public string Title {
		get => this ["Title"] as string;
		set {
			if (Title != value) {
				this ["Title"] = value;
				Dirty = true;
				ScrollTest.UpdateDatabase (this);
			}
		}
	}

    /// <summary>説明</summary>
    public string Description {
		get => this ["Description"] as string;
		set {
			if (Description != value) {
				this ["Description"] = value;
				Dirty = true;
				ScrollTest.UpdateDatabase (this);
			}
		}
	}

    /// <summary>アイコン</summary>
    public Sprite Icon {
        get => _icon;
		set {
			if (_icon != value) {
				_icon = value;
				ScrollTest.UpdateDatabase (this);
			}
		}
	}
    private Sprite _icon;

    /// <summary>チェックボックスのラベル</summary>
    public string Label {
		get => $"{this ["Label"]}{this ["Number"]}";
		set {
			if (Label != value) {
				this ["Label"] = value;
				Dirty = true;
				ScrollTest.UpdateDatabase (this);
			}
		}
	}

    /// <summary>チェックボックスの状態</summary>
    public bool Check {
		get {
			return bool.TryParse ( this ["Check"] as string, out var result) ? result : false;
		}
		set {
			if (Check != value) {
				this ["Check"] = value.ToString ();
				Dirty = true;
				ScrollTest.UpdateDatabase (this);
			}
		}
	}

	/// <summary>縦サイズ</summary>
	public float Height {
		get => (float) ((this ["Height"] as double?) ?? 0f);
		set {
			if (Height != value) {
				this ["Height"] = (double) value;
				Dirty = true;
				ScrollTest.UpdateDatabase (this);
			}
		}
	}

	/// <summary>横サイズ</summary>
	public float Width {
		get => (float) ((this ["Width"] as double?) ?? 0f);
		set {
			if (Width != value) {
				this ["Width"] = (double) value;
				Dirty = true;
				ScrollTest.UpdateDatabase (this);
			}
		}
	}

	/// <summary>実体を生成</summary>
	/// <returns>生成したオブジェクトにアタッチされているコンポーネントを返す</returns>
	public InfiniteScrollItemComponentBase Create (InfiniteScrollRect scrollRect, int index) => ItemComponent.Create (scrollRect, index);


    /// <summary>文字列化</summary>
    public override string ToString () => $"{base.ToString ()}({Position}, {Size}, {Dirty})";

}

public class Table : SQLiteTable<Item> {

	public Table () : base () { }

	/// <summary>列一覧からの生成</summary>
	public Table (params ColumnDefinition [] columns) : base (columns) { }

}
