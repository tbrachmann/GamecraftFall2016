using UnityEngine;
using System.Collections;

public class SceneChanger : MonoBehaviour {

	public string levelToLoad;

	public void StartButtonPress() {
		Debug.Log ("pressed start button");
		Application.LoadLevel (levelToLoad);
	}

	public void SettingsButtonPress() {
		Debug.Log ("pressed settings button");
		//Application.LoadLevel (levelToLoad);
	}
}
