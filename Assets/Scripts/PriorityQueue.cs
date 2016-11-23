using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PriorityQueue : LinkedList<Tile>, ICollection<Tile>
{
    public PriorityQueue() : base() { }
    public PriorityQueue(IEnumerable<Tile> val) : base(val) { }

    public LinkedListNode<Tile> AddToQueue(Tile node, Dictionary<Tile, float> fScore){
        if (this.Contains(node)) {
            return null;
        }
        LinkedListNode<Tile> current = this.First;
        //if this queue has no values yet
        if (current == null) {
            return this.AddFirst(node);
        }
        //LinkedListNode<Vector3> next = current.Next;
        float newNodeFScore = fScore[node];
        while (current != null) {
            if (newNodeFScore < fScore[current.Value]) {
                return this.AddBefore(current, node);
            }
            current = current.Next;
        }
        return this.AddLast(node);
    }

}
