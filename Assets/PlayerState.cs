using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public abstract class PlayerState 
{
    protected PlayerController controller;

	public PlayerState(PlayerController controller)
	{
        this.controller = controller;
	}

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update();
    public abstract PlayerState HandleInput();

}
