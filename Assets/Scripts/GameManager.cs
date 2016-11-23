using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

class GameManager : MonoBehaviour {

	public static GameManager instance;
	[HideInInspector]public PlayerController player;
    [HideInInspector]public GameObject board;
    //public List<Enemy> enemies;
    [HideInInspector]public TileMap tileMap;
    [HideInInspector]public bool playerTurn = true;

	void Awake() {
		//Ensure that there is a single GameManager.
		if(instance != null) {
			Object.Destroy(this.gameObject);
		} else {
			instance = this;
		}
		board = GameObject.Find("Board");
		player = GameObject.Find("Player").GetComponentInChildren<PlayerController>();
		tileMap = board.GetComponent<TileMap>();
        //Build the board.
        //Set player position to playerStart
        player.transform.root.position = tileMap.playerStart;
	}

	void Update() {
        //On playerTurn, the player controller controls the input.
        if (playerTurn)
        {
            return;
        }
        //Now its enemy's turn.
        
	}

	public TileMap getTileMap() {
		return tileMap;
	}

}