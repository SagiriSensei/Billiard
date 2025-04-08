using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Triangle
{
    public List<Vector3> vertices;
    public Vector3 centriod;
    public Vector3 normal;

    public Triangle(List<Vector3> vert, Vector3 norm)
    {
        vertices = vert;
        normal = norm;
        centriod = Vector3.zero;
        for (int i = 0; i < 3; i++)
        {
            centriod += vertices[i] / 3;
        }
    }

    public void ShowInGizmos(Vector3 offset)
    {
        Debug.DrawLine(vertices[0] + offset, vertices[1] + offset, Color.blue);
        Debug.DrawLine(vertices[1] + offset, vertices[2] + offset, Color.blue);
        Debug.DrawLine(vertices[2] + offset, vertices[0] + offset, Color.blue);
        Debug.DrawLine(centriod + offset, centriod + normal * 2 + offset);
    }
    public void DebugData()
    {
        Debug.Log(vertices.Count);
        for (int i = 0; i < vertices.Count; i++)
        {
            Debug.Log(vertices[i]);
        }
        Debug.Log("normal:" + normal);
    }
}
public struct Polyhedron
{
    public Vector3 centriod;
    public Triangle closestTriangle;
    public List<Vector3> vertices;
    public List<Triangle> triangles;

    //���¼�������
    private void ResetCentriod()
    {
        centriod = Vector3.zero;
        int n = vertices.Count;
        for (int i = 0; i < n; i++)
        {
            centriod += vertices[i] / n;
        }
    }

    public void Init()
    {
        vertices = new List<Vector3>();
        triangles = new List<Triangle>();
    }
    public bool ContainsPoint(Vector3 point)
    {
        //�ú�����Ϊ�ö�����Ϊ������
        //��������������ڣ�����true�����򷵻�false,ͬʱ���������õ������ƽ��
        float nearestDis = float.MaxValue;
        //����ԭ���������
        float tetrahedronVolumn = MyMath.ComputePointToPlaneDis(triangles[0], vertices[3]) * MyMath.ComputeTriangleArea(triangles[0]) / 3;
        float tempVolumn = 0f;
        for (int i = 0; i < triangles.Count; i++)
        {
            float h = MyMath.ComputePointToPlaneDis(triangles[i], point);
            float s = MyMath.ComputeTriangleArea(triangles[i]);
            tempVolumn += h * s / 3;
            //���Ե�һ��������
            if (h <= nearestDis && i != 0)
            {
                nearestDis = h;
                closestTriangle = triangles[i];
            }
        }
        if (Mathf.Approximately(tetrahedronVolumn, tempVolumn)) return true;
        else return false;
    }

    //��ö������о���Ŀ�������Ķ����
    public Triangle GetClosestTriangleToPoint(Vector3 point)
    {
        float nearestDis = float.MaxValue;
        for (int i = 0; i < triangles.Count; i++)
        {
            float h = MyMath.ComputePointToPlaneDis(triangles[i], point);
            if (h <= nearestDis)
            {
                nearestDis = h;
                closestTriangle = triangles[i];
            }
        }
        return closestTriangle;
    }
    //��չ������
    public void ExpandPolyhedron(Vector3 point)
    {
        AddVertice(point);

        int[] vertRefs = new int[vertices.Count];
        List<Triangle> triNeedRemove = new List<Triangle>();
        List<Tuple<int, int>> edges = new List<Tuple<int, int>>();
        //�������������β��Ƴ����д��������ܱ�������������
        for (int i = 0; i < triangles.Count; i++)
        {
            Vector3 obsDir = point - triangles[i].vertices[0];
            if (Vector3.Dot(obsDir, triangles[i].normal) > 0)
            {
                //���������ܱ�����
                triNeedRemove.Add(triangles[i]);
                //�������ε����߽��в���
                //todo:����������ܵ��µ����㲻Ϊһ�㣬Ҳ����Ϊһ��
                int[] vertIndex = new int[3];
                for (int j = 0; j < vertices.Count; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if (Vector3.Magnitude(vertices[j] - triangles[i].vertices[k]) <= 0.00001f)
                        {
                            vertIndex[k] = j;
                        }
                    }
                }

                int[] indicators = new int[] { 0, 1, 2, 0 };
                for (int j = 0; j < 3; j++)
                {
                    int index1 = vertIndex[indicators[j]];
                    int index2 = vertIndex[indicators[j + 1]];
                    if (edges.Contains(new Tuple<int, int>(index2, index1)))
                    {
                        //����ߵķ����ڱ߱���
                        edges.Remove(new Tuple<int, int>(index2, index1));
                    }
                    else
                    {
                        edges.Add(new Tuple<int, int>(index1, index2));
                    }
                }
            }
        }
        for (int i = 0; i < triNeedRemove.Count; i++)
        {
            RemoveTriangle(triNeedRemove[i]);
        }
        //�Ա߱��ڵı����¶����γ��µ�������
        for (int i = 0; i < edges.Count; i++)
        {
            Vector3 pointB = vertices[edges[i].Item1];
            Vector3 pointC = vertices[edges[i].Item2];
            Triangle t = MyMath.GenerateTriangleInCCWOrder(point, pointB, pointC, centriod);
            AddTriangle(t);
        }
    }

    public bool ContainsVertex(Vector3 point)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vector3.Magnitude(point - vertices[i]) <= 0.00001f) return true;
        }
        return false;
    }
    public void AddVertice(Vector3 point)
    {
        //�������в������µĶ���
        if (!vertices.Contains(point))
        {
            vertices.Add(point);
            ResetCentriod();
        }
    }

    public void AddTriangle(Triangle triangle)
    {
        triangles.Add(triangle);
        for (int i = 0; i < 3; i++)
        {
            AddVertice(triangle.vertices[i]);
        }
    }
    public bool RemoveTriangle(Triangle triangle)
    {
        return triangles.Remove(triangle);
    }

    public void ShowInGizmos(Vector3 offset)
    {
        for (int i = 0; i < triangles.Count; i++)
        {
            triangles[i].ShowInGizmos(offset);
        }
    }
}

public class NarrowPhase
{
    //���ڴ洢GJK�㷨��ɺ����ɵ������崫��EPA�㷨
    Polyhedron m_tetrahedronInGJK;

    public float m_epsilonInGJK = 0.00001f;
    public float m_epsilonInEPA = 0.00001f;

    public NarrowPhase()
    {

    }

    public Vector3 Support(Shape shapeA, Shape shapeB, Vector3 direction)
    {
        Vector3 pointA = shapeA.GetFarthestPointInDirection(direction);
        Vector3 pointB = shapeB.GetFarthestPointInDirection(-direction);
        return pointA - pointB;
    }
    //����������
    public Polyhedron GenerateTetrahedron(Triangle triangle, Vector3 pointD)
    {
        Polyhedron tetrahedron = new Polyhedron();
        tetrahedron.Init();
        Vector3 pointA = triangle.vertices[0];
        Vector3 pointB = triangle.vertices[1];
        Vector3 pointC = triangle.vertices[2];
        Vector3 innerPoint = (pointA + pointB + pointC + pointD) / 4;
        tetrahedron.AddTriangle(MyMath.GenerateTriangleInCCWOrder(pointA, pointB, pointC, innerPoint));
        tetrahedron.AddTriangle(MyMath.GenerateTriangleInCCWOrder(pointA, pointB, pointD, innerPoint));
        tetrahedron.AddTriangle(MyMath.GenerateTriangleInCCWOrder(pointA, pointC, pointD, innerPoint));
        tetrahedron.AddTriangle(MyMath.GenerateTriangleInCCWOrder(pointB, pointC, pointD, innerPoint));
        return tetrahedron;
    }

    public bool GJK(Shape shapeA, Shape shapeB)
    {
        m_tetrahedronInGJK = new Polyhedron();
        //���ɳ�ʼ������
        Vector3 d = new Vector3(1, 0, 0);
        Vector3 pointA = Support(shapeA, shapeB, d);
        Vector3 pointB = Support(shapeA, shapeB, -d);
        Vector3 AB = pointB - pointA;
        Vector3 AO = Vector3.zero - pointA;
        Vector3 perp = Vector3.Cross(Vector3.Cross(AB, AO), AB);
        Vector3 pointC;
        if (perp != Vector3.zero)
        {
            pointC = Support(shapeA, shapeB, perp);
        }
        else
        {
            d = new Vector3(0, 0, 1);
            pointC = Support(shapeA, shapeB, d);
            if (pointC == pointA || pointC == pointB)
            {
                pointC = Support(shapeA, shapeB, -d);
            }
        }
        Triangle t = MyMath.GenerateTriangleInCCWOrder(pointA, pointB, pointC, Vector3.zero);

        int count = 0;
        //����GJK�㷨
        Vector3 offset = new Vector3(0, 0, 0);
        while (true)
        {
            //todo:ĳ������»������ѭ����������ѭ��
            if (count > 100)
            {
                break;
            }
            //����µ�֧�ŵ�
            d = t.normal;
            if (Vector3.Dot(Vector3.zero - t.vertices[0], t.normal) <= 0)
            {
                d = -d;
            } 
            Vector3 newPoint = Support(shapeA, shapeB, d);

            //����֧�ŵ��ԭ��������֧�ŵ���бȽ�
            float dis = float.MaxValue;
            if (m_tetrahedronInGJK.vertices != null)
            {
                for (int i = 0; i < m_tetrahedronInGJK.vertices.Count; i++)
                {
                    if (Vector3.Magnitude(newPoint - m_tetrahedronInGJK.vertices[i]) <= dis)
                    {
                        dis = Vector3.Magnitude(newPoint - m_tetrahedronInGJK.vertices[i]);
                    }
                }
            }

            if (Vector3.Dot(d, newPoint) <= 0 || dis <= m_epsilonInGJK)
            {
                //����¼���ĵ㲢û�и��ӽ�ԭ�㣬����Ϊ��������û����ײ
                //���ߵ��¼���ĵ���ԭ�ȵ������ڵĵ���Сʱ��Ҳ��Ϊ��������û����ײ
                return false;
            }
            else
            {
                m_tetrahedronInGJK = GenerateTetrahedron(t, newPoint);
                if (m_tetrahedronInGJK.ContainsPoint(Vector3.zero))
                {
                    return true;
                }
                else
                {
                    t = m_tetrahedronInGJK.closestTriangle;
                }
            }
            count++;
        }
        return false;
    }

    public Contact EPA(Shape shapeA, Shape shapeB, Polyhedron polyhedron)
    {
        Contact contact = new Contact();
        float preClosestDis = float.MaxValue;
        int count = 0;
        Vector3 offset = new Vector3(0, 0, -1);
        while (true)
        {
            //polyhedron.ShowInGizmos(offset);
            offset += new Vector3(10, 0, 0);
            //������ѭ��
            if (count > 100)
            {
                break;
            }
            Triangle closestTriangle = polyhedron.GetClosestTriangleToPoint(Vector3.zero);
            float closestDis = MyMath.ComputePointToPlaneDis(closestTriangle, Vector3.zero);
            Vector3 newPoint = Support(shapeA, shapeB, closestTriangle.normal);
            bool exitLoop = Mathf.Abs(closestDis - preClosestDis) <= m_epsilonInEPA;
            exitLoop |= polyhedron.ContainsPoint(newPoint);
            if (exitLoop)
            {
                //�ҵ����ƽ��
                contact.normal = closestTriangle.normal;
                contact.globalPositionA = shapeA.GetFarthestPointInDirection(closestTriangle.normal);
                contact.globalPositionB = shapeB.GetFarthestPointInDirection(-closestTriangle.normal);
                contact.localPositionA = shapeA.GlobalToLocal(contact.globalPositionA);
                contact.localPositionB = shapeB.GlobalToLocal(contact.globalPositionB);
                contact.penetration = closestDis;
                break;
            }
            else
            {
                //�Ƴ������ƽ�棬����ƽ����ⷨ���������֧�ŵ���ж������չ
                polyhedron.ExpandPolyhedron(newPoint);
                preClosestDis = closestDis;
            }
            count++;
        }
        return contact;
    }

    public List<Collision> ComputeCollisionInformation(List<Tuple<int, int>> colliderPairs)
    {
        List<Collision> collisionInfos = new List<Collision>();
        for (int i = 0; i < colliderPairs.Count; i++)
        {
            Shape shapeA = PhysicsManager.instance.GetShapeById(colliderPairs[i].Item1);
            Shape shapeB = PhysicsManager.instance.GetShapeById(colliderPairs[i].Item2);
            if (GJK(shapeA, shapeB))
            {
                //��������ײ
                shapeA.SetCollideFlag(true);
                shapeB.SetCollideFlag(true);
                //����EPA�㷨��������ײ��Ϣ
                Contact contact = EPA(shapeA, shapeB, m_tetrahedronInGJK);
                Collision collision = new Collision();
                //ȷ��ǰһ����ײ���idС�ں�һ����ײ��id
                if (shapeA.GetId() > shapeB.GetId())
                {
                    Shape temp = shapeA;
                    shapeA = shapeB;
                    shapeB = temp;
                }
                collision.m_shapeA = shapeA;
                collision.m_shapeB = shapeB;
                collision.AddContact(contact);
                collisionInfos.Add(collision);
            }
        }
        return collisionInfos;
    }
}
