using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Startup : MonoBehaviour {

	public GameObject ConsolePrefab;

	void Start () {
		Instantiate (ConsolePrefab, this.transform);
	}

}
