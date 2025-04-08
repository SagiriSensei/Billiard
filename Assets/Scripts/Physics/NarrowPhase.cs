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

    //重新计算重心
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
        //该函数认为该多面体为四面体
        //如果点在四面体内，返回true，否则返回false,同时计算出距离该点最近的平面
        float nearestDis = float.MaxValue;
        //计算原四面体面积
        float tetrahedronVolumn = MyMath.ComputePointToPlaneDis(triangles[0], vertices[3]) * MyMath.ComputeTriangleArea(triangles[0]) / 3;
        float tempVolumn = 0f;
        for (int i = 0; i < triangles.Count; i++)
        {
            float h = MyMath.ComputePointToPlaneDis(triangles[i], point);
            float s = MyMath.ComputeTriangleArea(triangles[i]);
            tempVolumn += h * s / 3;
            //忽略第一个三角形
            if (h <= nearestDis && i != 0)
            {
                nearestDis = h;
                closestTriangle = triangles[i];
            }
        }
        if (Mathf.Approximately(tetrahedronVolumn, tempVolumn)) return true;
        else return false;
    }

    //获得多面体中距离目标点最近的多边形
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
    //扩展多面体
    public void ExpandPolyhedron(Vector3 point)
    {
        AddVertice(point);

        int[] vertRefs = new int[vertices.Count];
        List<Triangle> triNeedRemove = new List<Triangle>();
        List<Tuple<int, int>> edges = new List<Tuple<int, int>>();
        //遍历所有三角形并移除所有从所给点能被看到的三角形
        for (int i = 0; i < triangles.Count; i++)
        {
            Vector3 obsDir = point - triangles[i].vertices[0];
            if (Vector3.Dot(obsDir, triangles[i].normal) > 0)
            {
                //该三角形能被看到
                triNeedRemove.Add(triangles[i]);
                //对三角形的三边进行操作
                //todo:精度问题可能导致的两点不为一点，也可能为一点
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
                        //如果边的反向在边表内
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
        //对边表内的边与新顶点形成新的三角形
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
        //多面体中不包含新的顶点
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
    //用于存储GJK算法完成后生成的四面体传入EPA算法
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
    //生成四面体
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
        //生成初始三角形
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
        //进行GJK算法
        Vector3 offset = new Vector3(0, 0, 0);
        while (true)
        {
            //todo:某种情况下会出现死循环，避免死循环
            if (count > 100)
            {
                break;
            }
            //获得新的支撑点
            d = t.normal;
            if (Vector3.Dot(Vector3.zero - t.vertices[0], t.normal) <= 0)
            {
                d = -d;
            } 
            Vector3 newPoint = Support(shapeA, shapeB, d);

            //将新支撑点和原单纯形内支撑点进行比较
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
                //如果新加入的点并没有更接近原点，则认为两个物体没有相撞
                //或者当新加入的点与原先单纯形内的点差别极小时，也认为两个物体没有相撞
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
            //避免死循环
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
                //找到最近平面
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
                //移除最近的平面，并用平面的外法向量获得新支撑点进行多边形扩展
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
                //两物体碰撞
                shapeA.SetCollideFlag(true);
                shapeB.SetCollideFlag(true);
                //进行EPA算法，生成碰撞信息
                Contact contact = EPA(shapeA, shapeB, m_tetrahedronInGJK);
                Collision collision = new Collision();
                //确保前一个碰撞体的id小于后一个碰撞体id
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
