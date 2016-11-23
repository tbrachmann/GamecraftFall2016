using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* If attacks get more complicated than this then they might need to
 * become a class, so we can deal with knockback and such. Then the 
 * player or enemy would have to delegate to this class.*/
public struct Attack {

    private int damage;
    private int range;
    private string name;

    public Attack(int damage, int range, string name) {
        this.damage = damage;
        this.range = range;
        this.name = name;
    }

    public int getDamage()
    {
        return damage;
    }

    public int getRange()
    {
        return range;
    }

    public string getName()
    {
        return name;
    }

}
