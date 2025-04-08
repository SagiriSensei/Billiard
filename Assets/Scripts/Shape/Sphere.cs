using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : Shape
{
    public float m_radius = 0.5f;

    public override void Awake()
    {
        base.Awake();
        //初始化惯性张量
        m_localInertiaTensor = Matrix4x4.identity;
        float I = 2.0f / 5 * m_mass * m_radius * m_radius;
        m_localInertiaTensor.SetRow(0, new Vector4(I, 0, 0, 0));
        m_localInertiaTensor.SetRow(1, new Vector4(0, I, 0, 0));
        m_localInertiaTensor.SetRow(2, new Vector4(0, 0, I, 0));
        m_attachedRigidBody.AddCollider(this);
    }

    public override void Start()
    {
        base.Start();
    }

    public override Vector3 GetFarthestPointInDirection(Vector3 direction)
    {
        direction = Vector3.Normalize(direction);
        float maxScale = Mathf.Max(transform.localScale.x, transform.localScale.y);
        maxScale = Mathf.Max(maxScale, transform.localScale.z);
        return GetCenterPositionInGlobal() + direction * m_radius * maxScale;
    }

    public void OnDrawGizmos()
    {
        if (m_isCollide)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        float maxScale = Mathf.Max(transform.localScale.x, transform.localScale.y);
        maxScale = Mathf.Max(maxScale, transform.localScale.z);
        Gizmos.DrawWireSphere(GetCenterPositionInGlobal(), m_radius * maxScale);
    }

    public override Vector3 GetContactPointInDirection(Vector3 direction)
    {
        return GetFarthestPointInDirection(direction);
    }
}
