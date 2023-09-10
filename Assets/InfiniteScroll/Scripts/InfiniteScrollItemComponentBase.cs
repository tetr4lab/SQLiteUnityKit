using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InfiniteScroll {

    /// <summary>
    /// 物理項目(抽象クラス)
    ///   このクラスを継承したクラスを、Contentに配置するGameObjectにアタッチする
    ///   論理項目と物理項目(これ)を同期する役割を担う
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent (typeof (RectTransform))]
    public abstract class InfiniteScrollItemComponentBase : MonoBehaviour {

        #region static

        /// <summary>レイアウトを確定するまでの最大ビルド回数</summary>
        protected const int MaxNumberOfLayoutRebuilds = 10;

        /// <summary>コンポーネント付きオブジェクトの生成</summary>
        public static InfiniteScrollItemComponentBase Create (InfiniteScrollRect scrollRect, int index) {
            throw new NotImplementedException ("Must be implemented in a derived class");
#pragma warning disable CS0162 // 到達できないコードが検出されました
            // 以下参考
            var obj = new GameObject ("InfiniteScrollItem");
            var rect = obj.AddComponent<RectTransform> ();
            obj.transform.SetParent (scrollRect.content);
            var component = (obj.GetComponent<InfiniteScrollItemComponentBase> () ?? obj.AddComponent<InfiniteScrollItemComponentBase> ());
            component.ScrollRect = scrollRect;
            component.Index = index;
            component.Initialize ();
            return component;
#pragma warning restore CS0162 // 到達できないコードが検出されました
        }

        #endregion

        /// <summary>初期化</summary>
        protected virtual void Initialize () { }

        /// <summary>親のスクロールレクト</summary>
        public virtual InfiniteScrollRect ScrollRect { get; protected set; }

        /// <summary>項目のパディング</summary>
        protected virtual RectOffset padding => ScrollRect.padding;

        /// <summary>項目のサイズを制御</summary>
        protected virtual bool controlChildSize => ScrollRect.controlChildSize;

        /// <summary>項目の配向</summary>
        protected virtual TextAnchor childAlignment => ScrollRect.childAlignment;

        /// <summary>逆並び</summary>
        protected virtual bool reverseArrangement => ScrollRect.reverseArrangement;

        /// <summary>スクロールの向き 垂直/!水平</summary>
        protected virtual bool vertical => ScrollRect.vertical;

        /// <summary>ビューポート矩形</summary>
        protected virtual Rect viewportRect => ScrollRect.viewport.rect;

        /// <summary>レクトトランスフォーム</summary>
        protected virtual RectTransform RectTransform => _rectTransform ?? transform as RectTransform ?? gameObject.AddComponent<RectTransform> ();
        protected RectTransform _rectTransform;

        /// <summary>サイズ</summary>
        public virtual float Size => ScrollRect.vertical ? RectTransform.rect.height : RectTransform.rect.width;

        /// <summary>リンク中の論理項目インデックス</summary>
        public virtual int Index {
            get => _index;
            set {
                _index = value;
                if (value >= 0) {
                    Apply ();
                    SetPosition (Item.Position);
                    gameObject.SetActive (true);
                } else {
                    gameObject.SetActive (false);
                }
            }
        }
        protected int _index;

        /// <summary>リンク中の論理項目</summary>
        public virtual IInfiniteScrollItem Item => (_index < 0 || _index >= ScrollRect.Count) ? null : ScrollRect [_index];

        /// <summary>論理項目のコンテンツを反映</summary>
        protected virtual void Apply () => Item.Dirty = false;

        /// <summary>更新</summary>
        protected virtual void Update () {
            if (ScrollRect && !ScrollRect.LockedUpdate && Index >= 0) {
                if (Item.Dirty) {
                    Apply ();
                }
                if (Item.Size != Size) {
                    // サイズが変化した
                    ScrollRect.ResizeRequest = true;
                }
            }
        }

        /// <summary>物理項目のサイズを確定し論理項目に反映する</summary>
        protected internal virtual void SetSize (bool calibration = false) {
            if (controlChildSize) {
                RectTransform.sizeDelta = vertical
                    ? new Vector2 (viewportRect.size.x - padding.left - padding.right, RectTransform.sizeDelta.y)
                    : new Vector2 (RectTransform.sizeDelta.x, viewportRect.size.y - padding.top - padding.bottom);
            }
            if (calibration) {
                for (var i = 0; i < MaxNumberOfLayoutRebuilds; i++) {
                    var lastSize = Size;
                    LayoutRebuilder.ForceRebuildLayoutImmediate (RectTransform);
                    if (lastSize == Size) {
                        break;
                    }
                }
                ScrollRect.AverageItemSize = Size;
            }
            Item.Size = Size;
        }

        /// <summary>項目の位置決め</summary>
        protected internal virtual void SetPosition (float pos) {
            pos *= (vertical == reverseArrangement) ? 1f : -1f;
            Vector2 anchor;
            Vector2 anchoredPosition;
            if (reverseArrangement) {
                switch (childAlignment) {
                    case TextAnchor.UpperRight:
                        anchor.x = vertical ? 1f : 1f;
                        anchor.y = vertical ? 0f : 1f;
                        anchoredPosition.x = vertical ? -padding.right : pos;
                        anchoredPosition.y = vertical ? pos : -padding.top;
                        break;
                    case TextAnchor.MiddleCenter:
                        anchor.x = vertical ? 0.5f : 1f;
                        anchor.y = vertical ? 0f : 0.5f;
                        anchoredPosition.x = vertical ? 0f : pos;
                        anchoredPosition.y = vertical ? pos : 0f;
                        break;
                    case TextAnchor.LowerLeft:
                        anchor.x = vertical ? 0f : 1f;
                        anchor.y = vertical ? 0f : 0f;
                        anchoredPosition.x = vertical ? padding.left : pos;
                        anchoredPosition.y = vertical ? pos : padding.bottom;
                        break;
                    default:
                        anchor.x = 1f;
                        anchor.y = 0f;
                        anchoredPosition.x = 0f;
                        anchoredPosition.y = 0f;
                        break;
                }
            } else {
                switch (childAlignment) {
                    case TextAnchor.UpperRight:
                        anchor.x = vertical ? 1f : 0f;
                        anchor.y = vertical ? 1f : 1f;
                        anchoredPosition.x = vertical ? -padding.right : pos;
                        anchoredPosition.y = vertical ? pos : -padding.top;
                        break;
                    case TextAnchor.MiddleCenter:
                        anchor.x = vertical ? 0.5f : 0f;
                        anchor.y = vertical ? 1f : 0.5f;
                        anchoredPosition.x = vertical ? 0f : pos;
                        anchoredPosition.y = vertical ? pos : 0f;
                        break;
                    case TextAnchor.LowerLeft:
                        anchor.x = vertical ? 0f : 0f;
                        anchor.y = vertical ? 1f : 0f;
                        anchoredPosition.x = vertical ? padding.left : pos;
                        anchoredPosition.y = vertical ? pos : padding.bottom;
                        break;
                    default:
                        anchor.x = 0f;
                        anchor.y = 1f;
                        anchoredPosition.x = 0f;
                        anchoredPosition.y = 0f;
                        break;
                }
            }
            RectTransform.anchorMin = anchor;
            RectTransform.anchorMax = anchor;
            RectTransform.pivot = anchor;
            RectTransform.anchoredPosition = anchoredPosition;
        }

        /// <summary>文字列化</summary>
        public override string ToString () => $"{base.ToString ()}({Index}, {Size}, {Item})";

    }

}
