using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public interface PlayerState 
{

    void Enter();
    void Exit();
    void Update();
    PlayerState HandleInput();

}
