using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tetr4lab.UI;

/// <summary>物理アイテム</summary>
public class ItemComponent : InfiniteScrollItemComponentBase {

    /// <summary>プレハブ</summary>
    private static GameObject prefab = null;

    /// <summary>生成</summary>
    public static new ItemComponent Create (InfiniteScrollRect scrollRect, int index) {
        if (prefab == null) {
            prefab = Resources.Load<GameObject> ("Prefabs/Item");
        }
        var obj = Instantiate (prefab, scrollRect.content);
        var component = obj.GetComponent<ItemComponent> () ?? obj.AddComponent<ItemComponent> ();
        component.ScrollRect = scrollRect;
        component.Index = index;
        component.Initialize ();
        return component;
    }

	/// <summary>論理項目の反映中</summary>
	private bool isApply;

    /// <summary>初期化</summary>
    protected override void Initialize () {
        var texts = GetComponentsInChildren<Text> ();
        if (titleText == default && texts.Length > 0) {
            titleText = texts [0];
        }
        if (descriptionText == default && texts.Length > 1) {
            descriptionText = texts [1];
        }
        if (checkBoxLabelText == default && texts.Length > 2) {
            checkBoxLabelText = texts [2];
        }
        if (iconImage == default) {
            iconImage = GetComponentInChildren<Image> ();
        }
        if (checkBoxToggle == default) {
            checkBoxToggle = GetComponentInChildren<Toggle> ();
        }
        if (checkBoxToggle) {
            checkBoxToggle.onValueChanged.AddListener (isOn => {
				// 物理から論理に反映 (ループしないように)
				Item ["Check"] = isOn.ToString ();
				if (!isApply) {
					// 物理操作起点ならDBに反映
					ScrollTest.UpdateDatabase (Item);
				}
			});
        }
    }

    /// <summary>論理項目のコンテンツを反映</summary>
    protected override void Apply () {
		isApply = true;
		titleText.text = Item.Title;
		descriptionText.text = Item.Description;
        iconImage.sprite = Item.Icon;
        checkBoxLabelText.text = Item.Label;
		checkBoxToggle.isOn = Item.Check;
        base.Apply ();
		// サイズに反映
		if (!ScrollRect.vertical && Item.Width > 0) {
			RectTransform.sizeDelta = new Vector2 (Item.Width, RectTransform.sizeDelta.y);
		}
		if (ScrollRect.vertical && Item.Height > 0) {
			RectTransform.sizeDelta = new Vector2 (RectTransform.sizeDelta.x, Item.Height);
		}
		isApply = false;
	}

    /// <summary>リンク中の論理項目</summary>
    public new Item Item => ScrollRect [_index] as Item;

    /// <summary>タイトルテキスト</summary>
    [SerializeField]
    private Text titleText = default;

    /// <summary>説明テキスト</summary>
    [SerializeField]
    private Text descriptionText = default;

    /// <summary>アイコンイメージ</summary>
    [SerializeField]
    private Image iconImage = default;

    /// <summary>チェックボックスのトグル</summary>
    [SerializeField]
    private Toggle checkBoxToggle = default;

    /// <summary>チェックボックスのラベルテキスト</summary>
    [SerializeField]
    private Text checkBoxLabelText = default;

}
