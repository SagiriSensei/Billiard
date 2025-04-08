using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    public static PhysicsManager instance;

    //��������ײ����б��
    int m_id = 0;
    float m_totalTime  = 0;
    float m_deltaTime = 0;
    //ʣ��ʱ��ȥ������һ��ģ��
    float m_leftTime = 0;
    float m_gravity = 0;
    BroadPhase m_broadPhase;
    NarrowPhase m_narrowPhase;
    Coroutine m_loopCoroutine;
    //��Ϸ�����е�������ײ��
    List<Shape> m_shapeList;
    List<Collision> m_curFrameCollisions;
    List<Tuple<int, int>> m_lastFrameColliderPairs;
    List<Tuple<int, int>> m_curFrameColliderPairs;

    Dictionary<int, Shape> m_idToShape;
    Dictionary<Shape, int> m_shapeToId;

    public ResolvePhase m_resolvePhase;

    private void Awake()
    {
        instance = this;

        m_broadPhase = new NSquardBroadPhase();
        m_narrowPhase = new NarrowPhase();
        m_resolvePhase = new ResolvePhase();
        m_shapeList = new List<Shape>();
        m_idToShape = new Dictionary<int, Shape>();
        m_shapeToId = new Dictionary<Shape, int>();
    }

    private void Start()
    {
        m_deltaTime = PhysicsSettings.DeltaTime;
        m_gravity = PhysicsSettings.Gravity;
        m_leftTime = m_deltaTime;

        m_lastFrameColliderPairs = null;
        m_loopCoroutine = StartCoroutine("Loop");
    }

    public int GetShapeId(Shape shape)
    {
        if (m_shapeToId.ContainsKey(shape))
        {
            return m_shapeToId[shape];
        }
        return -1;
    }

    public Shape GetShapeById(int id)
    {
        if (m_idToShape.ContainsKey(id))
        {
            return m_idToShape[id];
        }
        return null;
    }

    public void AddShape(Shape shape)
    {
        if (!m_shapeList.Contains(shape))
        {
            m_idToShape[m_id] = shape;
            m_shapeToId[shape] = m_id;
            shape.SetId(m_id++);
            m_shapeList.Add(shape);
            m_broadPhase.Add(shape);
        }
    }

    public void RemoveShape(Shape shape)
    {
        if (m_shapeList.Contains(shape))
        {
            int id = m_shapeToId[shape];
            m_idToShape.Remove(id);
            m_shapeList.Remove(shape);
            m_broadPhase.Remove(shape);
        }
    }

    //����ģ��������
    private void Simulate()
    {
        //��ʼģ��ǰ�����һ֡��ײ��־
        for (int i = 0; i < m_shapeList.Count; i++)
        {
            m_shapeList[i].ClearFlags();
        }
        //���������ٶ�
        for (int i = 0; i < m_shapeList.Count; i++)
        {
            RigidBody rb = m_shapeList[i].GetRigidBody();
            if (rb.UseGravity) rb.AddForce(new Vector3(0, -m_gravity * rb.GetMass(), 0), m_shapeList[i].GetCenterPositionInGlobal());
            m_shapeList[i].IntegrateVelocity(m_deltaTime);
        }
        //�����ײ�����Ϣ
        m_curFrameColliderPairs = m_broadPhase.ComputeColliderPairs();
        m_curFrameCollisions = m_narrowPhase.ComputeCollisionInformation(m_curFrameColliderPairs);
        HandleCollisionInfos();
        //������ײԼ��
        m_resolvePhase.ResoloveCollision(m_curFrameCollisions);
        //��������λ��
        for (int i = 0; i < m_shapeList.Count; i++)
        {
            m_shapeList[i].IntegratePosition(m_deltaTime);
        }
        m_lastFrameColliderPairs = m_curFrameColliderPairs;
    }

    private void HandleCollisionInfos()
    {
        m_curFrameColliderPairs.Clear();
        for (int i = 0; i < m_curFrameCollisions.Count; i++)
        {
            Collision collision = m_curFrameCollisions[i];
            Tuple<int, int> pair = new Tuple<int, int>(collision.m_shapeA.GetId(), collision.m_shapeB.GetId());
            m_curFrameColliderPairs.Add(pair);
        }
    }

    //����ģ��ѭ��Э��
    private IEnumerator Loop()
    {
        while (true)
        {
            float fixedDeltaTime = Time.fixedDeltaTime;
            while (fixedDeltaTime > 0)
            {
                if (fixedDeltaTime >= m_leftTime)
                {
                    fixedDeltaTime -= m_leftTime;
                    m_totalTime += m_leftTime;
                    Simulate();//����һ��ģ��
                    m_leftTime = m_deltaTime;
                }
                else
                {
                    m_leftTime -= fixedDeltaTime;
                    m_totalTime += fixedDeltaTime;
                    fixedDeltaTime = 0;
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
