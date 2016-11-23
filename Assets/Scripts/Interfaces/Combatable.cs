using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface Combatable
{
    void dealDamage(Combatable target, float damage);
    void takeDamage(float damage);
}
