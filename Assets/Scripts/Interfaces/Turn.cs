using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Any object that takes a turn must implement this interface.
interface Turn
{
        void StartTurn();
        void EndTurn();
}
