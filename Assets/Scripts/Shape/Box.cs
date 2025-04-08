using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Shape
{
    List<Vector3> m_vertices;
    float[ , ] m_data = new float[8, 3] {{ -0.5f, 0.5f, -0.5f }, { 0.5f, 0.5f, -0.5f }, { 0.5f, -0.5f, -0.5f }, { -0.5f, -0.5f, -0.5f }, 
        { -0.5f, 0.5f, 0.5f }, { 0.5f, 0.5f, 0.5f }, { 0.5f, -0.5f, 0.5f }, { -0.5f, -0.5f, 0.5f }};
    int[ , ] m_indexes = new int[6, 4] { { 1, 2, 3, 4 }, { 5, 8, 7, 6 }, { 2, 6, 7, 3 }, { 1, 5, 8, 4 }, { 1, 2, 6, 5 }, { 3, 4, 8, 7 }};
    public Vector3 m_size = Vector3.one;

    public Box()
    {
        m_vertices = new List<Vector3>();
        for (int i = 0; i < 8; i++)
        {
            Vector3 vert = new Vector3(m_data[i, 0], m_data[i, 1], m_data[i, 2]);
            m_vertices.Add(vert);
        }
    }

    public override void Awake()
    {
        base.Awake();
        m_localInertiaTensor = new Matrix4x4();
        m_localInertiaTensor = Matrix4x4.identity;
        float boxW = m_size.x * transform.localScale.x;
        float boxH = m_size.y * transform.localScale.y;
        float boxL = m_size.z * transform.localScale.z;
        float Ix = 1.0f / 12 * m_mass * (boxH * boxH + boxL * boxL);
        float Iy = 1.0f / 12 * m_mass * (boxW * boxW + boxL * boxL);
        float Iz = 1.0f / 12 * m_mass * (boxW * boxW + boxH * boxH);
        m_localInertiaTensor[0, 0] = Ix;
        m_localInertiaTensor[1, 1] = Iy;
        m_localInertiaTensor[2, 2] = Iz;
        m_attachedRigidBody.AddCollider(this);
    }

    public override Vector3 GetFarthestPointInDirection(Vector3 direction)
    {
        float farthestDis = float.MinValue;
        Vector3 farthestPoint = Vector3.zero;
        for (int i = 0; i < 8; i++)
        {
            Vector3 vertex = GetVertexPositionInGlobal(m_vertices[i]);
            if (Vector3.Dot(direction, vertex) >= farthestDis)
            {
                farthestDis = Vector3.Dot(direction, vertex);
                farthestPoint = vertex;
            }
        }
        return farthestPoint;
    }

    public override Vector3 GetContactPointInDirection(Vector3 direction)
    {
        float farthestDis = float.MinValue;
        Vector3 farthestPoint = Vector3.zero;
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            Vector3 vertex = GetVertexPositionInGlobal(m_vertices[i]);
            if (Mathf.Abs(Vector3.Dot(direction, vertex) - farthestDis) <= 0.01f)
            {
                count++;
                farthestPoint += vertex;
            }
            else if (Vector3.Dot(direction, vertex) > farthestDis)
            {
                count = 1;
                farthestDis = Vector3.Dot(direction, vertex);
                farthestPoint = vertex;
            }
        }
        return farthestPoint / count;
    }

    public Vector3 GetVertexPositionInGlobal(Vector3 vertexModelPosition)
    {
        vertexModelPosition = Vector3.Scale(vertexModelPosition, transform.localScale);
        vertexModelPosition = Vector3.Scale(vertexModelPosition, m_size);
        return LocalToGlobal(vertexModelPosition + m_localCentroid);
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
        int[] indicators = new int[] { 0, 1, 2, 3, 0 };
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Vector3 pointA = GetVertexPositionInGlobal(m_vertices[m_indexes[i, indicators[j]] - 1]);
                Vector3 pointB = GetVertexPositionInGlobal(m_vertices[m_indexes[i, indicators[j + 1]] - 1]);
                Gizmos.DrawLine(pointA, pointB);
            }
        }
    }
}
