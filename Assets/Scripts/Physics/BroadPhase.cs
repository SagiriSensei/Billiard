using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class BroadPhase
{
    abstract public void Add(Shape shape);
    abstract public void Remove(Shape shape);
    abstract public List<Tuple<int, int>> ComputeColliderPairs();
}
