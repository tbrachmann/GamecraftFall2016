using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PriorityQueue : LinkedList<Vector3>, ICollection<Vector3>
{
    public PriorityQueue() : base() { }
    public PriorityQueue(IEnumerable<Vector3> val) : base(val) { }

    public LinkedListNode<Vector3> AddToQueue(Vector3 node, Dictionary<Vector3, float> fScore){
        if (this.Contains(node)) {
            return null;
        }
        LinkedListNode<Vector3> current = this.First;
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
