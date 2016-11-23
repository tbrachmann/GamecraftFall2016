using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Stance
{
    private float damageTakenModifier;
    private float damageGivenModifier;
    private string name;

    public Stance(float takenModifier, float givenModifier, string name)
    {
        this.damageTakenModifier = takenModifier;
        this.damageGivenModifier = givenModifier;
        this.name = name;
    }

    public float getDamageTakenModifier() {
        return damageTakenModifier;
    }

    public float getDamageGivenModifier() {
        return damageGivenModifier;
    }

    public string getName() {
        return name;
    }

}
