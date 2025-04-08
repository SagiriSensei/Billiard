using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NSquardBroadPhase : BroadPhase
{
    List<Shape> m_shapeList;
    List<Tuple<int, int>> m_shapePairList;

    public NSquardBroadPhase()
    {
        m_shapeList = new List<Shape>();
        m_shapePairList = new List<Tuple<int, int>>();
    }
        
    public override void Add(Shape shape)
    {
        if (!m_shapeList.Contains(shape))
        {
            m_shapeList.Add(shape);
        }
    }

    public override void Remove(Shape shape)
    {
        if (m_shapeList.Contains(shape))
        {
            m_shapeList.Remove(shape);
        }
    }

    public override List<Tuple<int, int>> ComputeColliderPairs()
    {
        m_shapePairList.Clear();
        for (int i = 0; i < m_shapeList.Count; i++)
        {
            for (int j = i + 1; j < m_shapeList.Count; j++)
            {
                int id1 = PhysicsManager.instance.GetShapeId(m_shapeList[i]);
                int id2 = PhysicsManager.instance.GetShapeId(m_shapeList[j]);
                Tuple<int, int> pair = new Tuple<int, int>(id1, id2);
                m_shapePairList.Add(pair);
            }
        }
        return m_shapePairList;
    }
}
