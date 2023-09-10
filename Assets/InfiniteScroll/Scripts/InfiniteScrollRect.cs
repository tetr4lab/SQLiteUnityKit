using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InfiniteScroll {

    /// <summary>仮想スクロールレクト</summary>
    [AddComponentMenu ("UI/Infinite Scroll Rect", 38)]
    [DisallowMultipleComponent]
    [RequireComponent (typeof (RectTransform))]
    public class InfiniteScrollRect : ScrollRect {

        /// <summary>項目の外縁</summary>
        [SerializeField]
        public RectOffset padding = new RectOffset ();

        /// <summary>項目の隙間</summary>
        [SerializeField]
        public float spacing = 0;

        /// <summary>項目の配向</summary>
        [SerializeField]
        public TextAnchor childAlignment = TextAnchor.LowerLeft;

        /// <summary>項目の逆順</summary>
        [SerializeField]
        public bool reverseArrangement = false;

        /// <summary>項目の拡縮</summary>
        [SerializeField]
        public bool controlChildSize = false;

        /// <summary>項目の標準サイズ</summary>
        [SerializeField, Range (0f, float.MaxValue)]
        public float standardItemSize = 100f;

        /// <summary>有効</summary>
        public virtual bool Valid => Items != null && Components != null;

        /// <summary>論理項目リストへのアクセス</summary>
        public virtual ReadOnlyCollection<IInfiniteScrollItem> AsReadOnly () => Items.AsReadOnly ();

        /// <summary>論理項目の絞り込み</summary>
        public virtual List<IInfiniteScrollItem> FindAll (Predicate<IInfiniteScrollItem> match) => Items.FindAll (match);

        /// <summary>論理項目リストの変換</summary>
        public virtual List<TOutput> ConvertAll<TOutput> (Converter<IInfiniteScrollItem, TOutput> converter) => Items.ConvertAll (converter);

        /// <summary>論理項目へのアクセス</summary>
        public virtual IInfiniteScrollItem this [int index] => Items [index];

        /// <summary>論理項目数</summary>
        public virtual int Count => Valid ? Items.Count : 0;

        /// <summary>論理項目リスト</summary>
        protected virtual List<IInfiniteScrollItem> Items { get; set; }

        /// <summary>物理項目リスト</summary>
        protected virtual List<InfiniteScrollItemComponentBase> Components { get; set; }

        /// <summary>項目の平均サイズ (0以下の値をセットすると初期化される、初期値は標準サイズ)</summary>
        public virtual float AverageItemSize {
            get => _averageItemSize;
            set {
                if (value <= 0) {
                    _averageItemSize = standardItemSize;
                    _averageItemCount = 0;
                } else {
                    _averageItemSize = (_averageItemSize * _averageItemCount + value) / ++_averageItemCount;
                }
            }
        }
        protected float _averageItemSize;
        protected int _averageItemCount;        

        /// <summary>更新停止</summary>
        public virtual bool LockedUpdate { get; protected set; }

        /// <summary>サイズの更新請求</summary>
        public virtual bool ResizeRequest { get; set; }

        /// <summary>表示中の最初の物理項目の論理インデックス</summary>
        public virtual int FirstIndex { get; protected set; } = -1;

        /// <summary>表示中の最後の物理項目の論理インデックス</summary>
        public virtual int LastIndex { get; protected set; } = -1;

        /// <summary>スクロール方向のビューポートサイズ</summary>
        public virtual float ViewportSize => vertical ? viewport.rect.height : viewport.rect.width;

        /// <summary>前回のビューポートサイズ</summary>
        protected Vector2 lastViewportSize;

        /// <summary>スクロール方向のコンテントサイズ</summary>
        public virtual float ContentSize {
            get => vertical ? content.rect.height : content.rect.width;
            protected set => content.sizeDelta = vertical ? new Vector2 (content.sizeDelta.x, value) : new Vector2 (value, content.sizeDelta.y);
        }

        /// <summary>スクロール方向のスクロール量</summary>
        public virtual float Scroll {
            get => vertical ? verticalNormalizedPosition : horizontalNormalizedPosition;
            protected set {
                if (vertical) {
                    verticalNormalizedPosition = value;
                } else {
                    horizontalNormalizedPosition = value;
                }
            }
        }

        /// <summary>クリア</summary>
        /// <param name="hard">リスト自体を削除する(!= 項目を削除してリストを残す)</param>
        public virtual void Clear (bool hard = false) {
            foreach (RectTransform t in content) {
                Destroy (t.gameObject);
            }
            if (hard) {
                Items = null;
                Components = null;
            } else {
                Items?.Clear ();
                Components?.Clear ();
            }
            AverageItemSize = 0;
            FirstIndex = LastIndex = -1;
            content.sizeDelta = Vector2.zero;
        }

        /// <summary>アイテムの外部更新</summary>
        /// <param name="modifier">スクロールレクト、項目リスト、可視開始、終了のインデックスが渡われるメソッド</param>
        public virtual void Modify (Action<InfiniteScrollRect, List<IInfiniteScrollItem>, int, int> modifier) {
            var lockBackup = LockedUpdate;
            LockedUpdate = true;
            var first = 0;
            var locked = new List<(IInfiniteScrollItem item, float offset)> ();
            if (FirstIndex >= 0) {
                for (var i = FirstIndex; i <= LastIndex; i++) {
                    locked.Add (GetScroll (i));
                }
                first = Items.IndexOf (locked [0].item);
            }
            modifier (this, Items, FirstIndex, LastIndex);
            if (Items.Count > 0) {
                var lockedOne = locked.Find (l => l.item != null && Items.Contains (l.item));
                FirstIndex = lockedOne == default ? 0 : Items.IndexOf (lockedOne.item);
                LastIndex = -1;
                ApplyItems ();
                CalculatePositions ();
                SetScroll (lockedOne);
            } else {
                Clear ();
            }
            LockedUpdate = lockBackup;
        }

        /// <summary>初期化</summary>
        /// <param name="items">項目のリスト</param>
        /// <param name="index">最初に表示する項目</param>
        public virtual void Initialize (IEnumerable<IInfiniteScrollItem> items, int index = 0) {
            if (items == null) { throw new ArgumentNullException ("items"); }
            // 抹消
            Clear (true);
            // 配置
            if (vertical) {
                var y = reverseArrangement ? 0f : 1f;
                content.anchorMin = new Vector2 (0f, y);
                content.anchorMax = new Vector2 (1f, y);
                content.pivot = new Vector2 (0.5f, y);
                content.offsetMin = Vector2.zero;
                content.offsetMax = Vector2.zero;
            } else {
                var x = reverseArrangement ? 1f : 0f;
                content.anchorMin = new Vector2 (x, 0f);
                content.anchorMax = new Vector2 (x, 1f);
                content.pivot = new Vector2 (x, 0.5f);
                content.offsetMin = Vector2.zero;
                content.offsetMax = Vector2.zero;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate (transform as RectTransform);
            // 生成
            Items = new List<IInfiniteScrollItem> (items);
            if (index < 0 || index > Items.Count) { throw new ArgumentOutOfRangeException ("index"); }
            Components = new List<InfiniteScrollItemComponentBase> ();
            FirstIndex = index;
            LastIndex = -1;
            ApplyItems ();
            CalculatePositions ();
            SetScroll ((Items [FirstIndex], 0));
        }

        /// <summary>項目へスクロール</summary>
        /// <param name="point">基準項目とオフセットのタプル</param>
        public virtual void SetScroll ((IInfiniteScrollItem item, float offset) point) {
            if (point.item == null || !Items.Contains (point.item)) {
                Scroll = (vertical == reverseArrangement) ? 0f : 1f;
                return;
            }
            var scroll = (point.item.Position + point.offset) / (ContentSize - ViewportSize);
            Scroll = (vertical == reverseArrangement) ? scroll : 1f - scroll;
        }


        /// <summary>項目とそのスクロール変位を取得</summary>
        /// <param name="index">省略すると可視範囲の中央辺りの項目</param>
        /// <returns>項目と現在のスクロール位置に対する項目からのオフセットのタプル</returns>
        public virtual (IInfiniteScrollItem item, float offset) GetScroll (int index = -1) {
            if (Items.Count <= 0) { return (null, 0f); }
            if (index < 0 || index >= Items.Count) {
                index = Mathf.Clamp ((FirstIndex + LastIndex) / 2, 0, Items.Count - 1);
            }
            var offset = ContentSize < ViewportSize ? 0f : (vertical == reverseArrangement ? Scroll : 1f - Scroll) * (ContentSize - ViewportSize);
            return (Items [index], offset - Items [index].Position);
        }

        /// <summary>項目の位置とコンテントサイズを算出</summary>
        /// <param name="index">Items.Countを指定するとContentのサイズを返す</param>
        protected virtual void CalculatePositions () {
            // 物理項目をチェックしてサイズを校正
            foreach (var component in Components) {
                if (component.Index >= 0 && (component.Size != component.Item.Size)) {
                    component.SetSize (calibration: true);
                }
            }
            // 論理項目の位置を算出
            float pos = reverseArrangement ? (vertical ? padding.bottom : padding.right) : (vertical ? padding.top : padding.left);
            for (var i = 0; i < Items.Count; i++) {
                if (Items [i].Position != pos) {
                    Items [i].Position = pos;
                }
                if (Items [i].Size <= 0) {
                    Items [i].Size = AverageItemSize;
                }
                pos += Items [i].Size + spacing;
            }
            pos += reverseArrangement ? (vertical ? padding.top : padding.left) : (vertical ? padding.bottom : padding.right);
            ContentSize = pos;
            // 物理項目に位置を反映
            foreach (var component in Components) {
                if (component.Index >= 0) {
                    component.SetPosition (component.Item.Position);
                }
            }
        }

        /// <summary>物理項目の解放</summary>
        /// <param name="first">保全範囲開始</param>
        /// <param name="last">保全範囲終了</param>
        /// <param name="force">範囲を無視して全解放</param>
        protected virtual void ReleaseItems (int first, int last, bool force = false) {
            foreach (var component in Components) {
                if (force || component.Index < first || component.Index > last) {
                    component.Index = -1;
                }
            }
        }

        /// <summary>論理アイテムを反映する</summary>
        /// <param name="first">先頭のインデックス</param>
        /// <param name="last">与えられなければ既存の実態をクリアして再構築</param>
        protected virtual void ApplyItems () {
            var rebuild = LastIndex < 0;
            if (rebuild) {
                LastIndex = Items.Count - 1;
            }
            // 解放
            ReleaseItems (FirstIndex, LastIndex, rebuild);
            // 割当
            var pos = - GetScroll (FirstIndex).offset;
            for (var i = FirstIndex; i <= LastIndex; i++) {
                var component = Components.Find (c => c.Index == i);
                if (component == null) {
                    component = Components.Find (c => c.Index < 0);
                    if (component != null) {
                        // 再利用
                        component.Index = i;
                    } else {
                        // 新規
                        component = Items [i].Create (this, i);
                        Components.Add (component);
                    }
                    component.SetSize (calibration: true);
                    CalculatePositions ();
                }
                if (rebuild) {
                    if (Items [i].Size <= 0) {
                        Items [i].Size = AverageItemSize;
                    }
                    pos += Items [i].Size + spacing;
                    if (pos > ViewportSize) {
                        // 可視端への到達
                        LastIndex = i;
                    }
                }
            }
        }

        /// <summary>可視範囲の変化に追従</summary>
        protected virtual void FolowVisibleRange () {
            var top = (vertical == reverseArrangement ? Scroll : 1f - Scroll) * (ContentSize - ViewportSize);
            var bottom = top + ViewportSize;
            var first = -1;
            var last = -1;
            for (var i = 0; i < Items.Count; i++) {
                if ((Items [i].Position + Items [i].Size) > top && Items [i].Position < bottom) {
                    last = i;
                    if (first < 0) {
                        first = i;
                    }
                }
            }
            if (first >= 0 && (first != FirstIndex || last != LastIndex)) {
                var locked = GetScroll ();
                FirstIndex = first;
                LastIndex = last;
                ApplyItems ();
                SetScroll (locked);
            }
        }

        /// <summary>更新</summary>
        protected virtual void Update () {
            if (!LockedUpdate && Components?.Count > 0 && FirstIndex >= 0) {
                if (ResizeRequest) {
                    // 要求に応じてサイズ校正
                    var locked = GetScroll ();
                    CalculatePositions ();
                    SetScroll (locked);
                    ResizeRequest = false;
                } else if (lastViewportSize != viewport.rect.size) {
                    // ビューポートサイズの変化に追従してサイズ校正
                    foreach (var component in Components) {
                        if (component.Index >= 0) {
                            component.SetSize ();
                        }
                    }
                    lastViewportSize = viewport.rect.size;
                }
                FolowVisibleRange ();
            }
        }

        /// <summary>破棄</summary>
        protected override void OnDestroy () => Clear (true);

    }

    /// <summary>項目の寄せ方 (水平/垂直)</summary>
    public enum TextAnchor {
        /// <summary>下/左</summary>
        LowerLeft = 6,
        /// <summary>中央</summary>
        MiddleCenter = 4,
        /// <summary>上/右</summary>
        UpperRight = 2,
    }

}
