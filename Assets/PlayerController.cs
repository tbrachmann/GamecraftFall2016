using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    Rigidbody player;
    RectTransform mouseHelp;
    Camera mainCam;
    float moveX;
    float moveY;
    public float speed = 2f;
    public LayerMask floorMask;
    RaycastHit floorPos;
    Ray camRay;
    bool playerMoving = false;
    Vector3 newPos;

    // Use this for initialization
    void Start () {
        player = this.GetComponent<Rigidbody>();
        mouseHelp = GameObject.FindGameObjectWithTag("WorldUI").GetComponent<RectTransform>();
        mainCam = FindObjectOfType<Camera>();
        //mouseHelp.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        camRay = mainCam.ScreenPointToRay(Input.mousePosition);
        //If it hits the floor
        if (Physics.Raycast(camRay, out floorPos, floorMask))
        {
            //mouseHelp.gameObject.SetActive(true);
            Vector3 mousePos1 = new Vector3(ConvertToFloorUnits(floorPos.point.x), 0.0001f, ConvertToFloorUnits(floorPos.point.z));
            //print("MousePos: " + floorPos.point.x + ", " + floorPos.point.z + "; MouseHelp: " + mousePos1.x + ", " + mousePos1.z);
            mouseHelp.transform.position = mousePos1;
            if (Input.GetMouseButtonDown(0) && !playerMoving)
            {
                newPos = mousePos1;
                newPos.y = 0.5f;
                playerMoving = true;
            }
            if(playerMoving)
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, newPos, Time.deltaTime * 3f);
                if (player.transform.position == newPos) playerMoving = false;
            }
        }
    }

   /* void Movement() {
        if (Physics.Raycast(camRay, out floorPos, floorMask))
        {
            newPos = new Vector3(ConvertToFloorUnits(floorPos.point.x), player.transform.position.y, ConvertToFloorUnits(floorPos.point.z));
            //player.transform.position = newPos;
        }
        if () {
            playerMoving = false;
        }
    }*/

    float ConvertToFloorUnits(float x) {
        if (x < 0) return (int)x - 0.5f;
        else return (int)x + 0.5f;
    }

}
