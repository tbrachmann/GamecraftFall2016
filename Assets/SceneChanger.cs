using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChanger : MonoBehaviour {

	public string levelToLoad;

	public void buttonPress() {
		string button = gameObject.name;
		if (button == "playButton") {
			Debug.Log ("pressed start button");
		}
		else if (button == "settingsButton") {
			Debug.Log ("pressed settings");
		}

		SceneManager.LoadScene(levelToLoad);
	}
}
