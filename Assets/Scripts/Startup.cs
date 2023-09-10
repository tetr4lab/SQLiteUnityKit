using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Startup : MonoBehaviour {
    /// <summary>ロードして自身に置き換える</summary>
    private void Start () {
        var prefab = Resources.Load<GameObject> ("Prefabs/Canvas2");
        var obj = Instantiate (prefab, transform.parent);
        obj.transform.SetSiblingIndex(transform.GetSiblingIndex ());
        Destroy (gameObject);
    }
}
