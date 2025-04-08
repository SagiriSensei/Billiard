using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBody : MonoBehaviour
{
    public bool m_useGravity = true;
    public bool m_isKenematic = false;
    public bool[] m_freezePosition = new bool[3];
    public bool[] m_freezeRotation = new bool[3];

    float m_mass;
    Matrix m_inverseMassMatrix;
    Matrix4x4 m_globalInverseInertiaTensor;
    Matrix4x4 m_localInverseInertiaTensor;
    Matrix4x4 m_localInertiaTensor;

    public Vector3 m_linearVelocity;
    public Vector3 m_angularVelocity;
    Vector3 m_forceAccumulator;
    Vector3 m_torqueAccumulator;
    Vector3 m_globalCentroid;
    Vector3 m_localCentriod;

    List<Shape> m_colliders;

    public void Awake()
    {
        m_globalCentroid = transform.position;
        m_localCentriod = Vector3.zero;
        m_linearVelocity = Vector3.zero;
        m_angularVelocity = Vector3.zero;
        m_forceAccumulator = Vector3.zero;
        m_torqueAccumulator = Vector3.zero;
        m_colliders = new List<Shape>();
    }

    public void Start()
    {
        m_inverseMassMatrix = new Matrix(3, 3);
        m_inverseMassMatrix[0, 0] = 1.0f / m_mass;
        m_inverseMassMatrix[1, 1] = 1.0f / m_mass;
        m_inverseMassMatrix[2, 2] = 1.0f / m_mass;
    }

    public bool UseGravity
    {
        get
        {
            return m_useGravity;
        }
    }

    public void IntegrateVelocity(float dt)
    {
        m_linearVelocity += m_forceAccumulator * dt / m_mass;
        m_angularVelocity += m_globalInverseInertiaTensor.MultiplyVector(m_torqueAccumulator) * dt;

        m_forceAccumulator = Vector3.zero;
        m_torqueAccumulator = Vector3.zero;
    }

    public void IntegratePosition(float dt)
    {
        m_globalCentroid += m_linearVelocity * dt;

        float angle = m_angularVelocity.magnitude * dt * Mathf.Rad2Deg;
        Vector3 axis = Vector3.Normalize(m_angularVelocity);
        transform.rotation = Quaternion.AngleAxis(angle, axis) * transform.rotation;

        transform.position = transform.rotation * (-m_localCentriod) + m_globalCentroid;
        UpdateGlobalInverseInertiaTensor();
    }

    public void UpdateGlobalInverseInertiaTensor()
    {
        Matrix4x4 orientation = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one);
        m_globalInverseInertiaTensor = orientation * m_localInverseInertiaTensor * orientation.inverse;
    }

    public void UpdateGlobalCentriod()
    {
        m_globalCentroid = LocalToGlobal(m_localCentriod);
    }

    public void AddCollider(Shape shape)
    {
        m_localCentriod = shape.m_localCentroid;
        m_mass = shape.m_mass;
        m_localInertiaTensor = shape.GetLocalInertiaTensor();
        m_localInverseInertiaTensor = shape.GetLocalInertiaTensor().inverse;
        UpdateGlobalInverseInertiaTensor();
    }
    
    public Vector3 LocalToGlobal(Vector3 position)
    {
        return transform.rotation * position +transform.position;
    }

    public Vector3 GlobalToLocal(Vector3 position)
    {
        return Quaternion.Inverse(transform.rotation) * (position - transform.position);
    }

    public void AddForce(Vector3 force, Vector3 pointAt)
    {
        m_forceAccumulator += force;
        m_torqueAccumulator += Vector3.Cross(pointAt - m_globalCentroid, force);
    }

    public float GetMass()
    {
        return m_mass;
    }

    public Matrix4x4 GetLocalInverseInertiaTensor()
    {
        return m_localInverseInertiaTensor;
    }

    public Matrix4x4 GetGlobalInverseInertiaTensor()
    {
        Matrix4x4 ret = m_globalInverseInertiaTensor;
        for (int i = 0; i < 3; i++)
        {
            if (m_freezeRotation[i] == true || m_isKenematic)
            {
                for (int j = 0; j < 3; j++)
                {
                    ret[i, j] = 0;
                }
            }
        }
        return ret;
    }

    public Matrix GetInverseMassMatrix()
    {
        Matrix ret = m_inverseMassMatrix;
        if (m_isKenematic) return ret.Zero();
        for (int i = 0; i < 3; i++)
        {
            if (m_freezePosition[i] == true)
            {
                ret[i, i] = 0;
            }
        }
        return m_inverseMassMatrix;
    }

    public Vector3 GetLinearVelocity()
    {
        return m_linearVelocity;
    }

    public Vector3 GetAngularVelocity()
    {
        return m_angularVelocity;
    }
    
    public void AddVelocity(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        m_linearVelocity += linearVelocity;
        m_angularVelocity += angularVelocity;
    }

    public void AddLinearVelocity(Vector3 linearVelocity)
    {
        m_linearVelocity += linearVelocity;
    }

    public void PrintVelocity()
    {
        Debug.Log(m_linearVelocity);
        Debug.Log(m_angularVelocity);
    }
}
