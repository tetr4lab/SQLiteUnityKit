using System;
using System.Collections;
using System.Collections.Generic;

namespace InfiniteScroll {

    /// <summary>
    /// 論理項目(インターフェイス)
    ///   継承したクラスを用意して、スクロールレクトの初期化に使用する
    ///   論理項目のリストの一部が物理項目に反映される
    /// </summary>
    public interface IInfiniteScrollItem {

        /// <summary>
        /// 物理項目を生成
        ///   GameObjectを生成して、InfiniteScrollItemComponentを継承したコンポーネントをアタッチする
        /// </summary>
        /// <returns>生成したGameObjectにアタッチされているコンポーネントを返す</returns>
        public abstract InfiniteScrollItemComponentBase Create (InfiniteScrollRect scrollRect, int index);

        /// <summary>スクロール方向の位置 (実態へ反映)</summary>
        public abstract float Position { get; set; }

        /// <summary>スクロール方向のサイズ (物理項目から反映)</summary>
        public abstract float Size { get; set; }

        /// <summary>内容に変更があった</summary>
        public abstract bool Dirty { get; set; }

    }

}
