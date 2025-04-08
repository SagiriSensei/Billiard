using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RigidBody))]
abstract public class Shape : MonoBehaviour
{
    [SerializeField] protected int m_id;
    protected bool m_isCollide = false;
    protected Matrix4x4 m_localInertiaTensor;
    protected RigidBody m_attachedRigidBody;
    
    public float m_mass;
    public float m_friction;
    public Vector3 m_localCentroid = Vector3.zero;

    virtual public void Awake()
    {
        m_attachedRigidBody = GetComponent<RigidBody>();
    }

    virtual public void Start()
    {
        PhysicsManager.instance.AddShape(this);
    }
    //设置碰撞标志，表示该帧是否碰撞
    public void SetCollideFlag(bool collideFlag)
    {
        m_isCollide = collideFlag;
    }
    public void SetId(int id)
    {
        m_id = id;
    }
    public int GetId()
    {
        return m_id;
    }
    public void ClearFlags()
    {
        m_isCollide = false;
    }

    public void IntegrateVelocity(float dt)
    {
        if (m_attachedRigidBody != null)
        {
            m_attachedRigidBody.IntegrateVelocity(dt);
        }
    }

    public void IntegratePosition(float dt)
    {
        if (m_attachedRigidBody != null)
        {
            m_attachedRigidBody.IntegratePosition(dt);
        }
    }

    public Vector3 GlobalToLocal(Vector3 globalPosition)
    {
        return Quaternion.Inverse(transform.rotation) * (globalPosition - transform.position);
    }
    public Vector3 LocalToGlobal(Vector3 modelPosition)
    {
        return transform.rotation * modelPosition + transform.position;
    }
    public Vector3 GetCenterPositionInGlobal()
    {
        return LocalToGlobal(m_localCentroid);
    }

    public float GetFriction()
    {
        return m_friction;
    }

    public Matrix4x4 GetLocalInertiaTensor()
    {
        return m_localInertiaTensor;
    }

    public RigidBody GetRigidBody()
    {
        return m_attachedRigidBody;
    }
    abstract public Vector3 GetFarthestPointInDirection(Vector3 direction);
    abstract public Vector3 GetContactPointInDirection(Vector3 direction);
}
