using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMath
{
    //计算点到平面的距离
    public static float ComputePointToPlaneDis(Triangle triangle, Vector3 point)
    {
        Vector3 pa = triangle.vertices[0] - point;
        float h = Math.Abs(Vector3.Dot(pa, triangle.normal)) / Vector3.Magnitude(triangle.normal);
        return h;
    }
    //计算三角形面积
    public static float ComputeTriangleArea(Triangle triangle)
    {
        //计算底面积
        Vector3 ab = triangle.vertices[1] - triangle.vertices[0];
        Vector3 bc = triangle.vertices[2] - triangle.vertices[1];
        float s = Vector3.Magnitude(Vector3.Cross(ab, bc)) / 2;
        return s;
    }

    public static List<Vector3> GeneratePointList(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        List<Vector3> points = new List<Vector3>();
        points.Add(pointA);
        points.Add(pointB);
        points.Add(pointC);
        return points;
    }

    //生成一个逆时针存储的三角形
    public static Triangle GenerateTriangleInCCWOrder(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 innerPoint)
    {
        List<Vector3> points = GeneratePointList(pointA, pointB, pointC);
        Vector3 edgeAB = points[1] - points[0];
        Vector3 edgeAC = points[2] - points[0];
        Vector3 perp = Vector3.Cross(edgeAB, edgeAC);
        Vector3 obsDir = points[0] - innerPoint;
        if (Vector3.Dot(obsDir, perp) <= 0)
        {
            Vector3 temp = points[1];
            points[1] = points[2];
            points[2] = temp;
            perp = -perp;
        }
        Triangle t = new Triangle(points, Vector3.Normalize(perp));
        return t;
    }

    public static Vector2 SquareToCircle(Vector2 square)
    {
        float x = square.x;
        float y = square.y;
        Vector2 circle = new Vector2();
        circle.x = x * Mathf.Sqrt(1 - y * y / 2);
        circle.y = y * Mathf.Sqrt(1 - x * x / 2);
        return circle;
    }
}
