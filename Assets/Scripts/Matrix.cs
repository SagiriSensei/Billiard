using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matrix
{
    public int row;
    public int col;
    float[,] matrix;

    public Matrix(int row, int col)
    {
        this.row = row;
        this.col = col;
        matrix = new float[row, col];
    }

    public Matrix(List<Vector3> vector3s)
    {
        this.row = 1;
        this.col = 3 * vector3s.Count;
        matrix = new float[row, col];
        int count = 0;
        for (int i = 0; i < vector3s.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                matrix[0, count++] = vector3s[i][j];
            }
        }
    }

    public Matrix(Matrix4x4 m)
    {
        this.row = 3;
        this.col = 3;
        matrix = new float[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                matrix[i, j] = m[i, j];
            }
        }
    }

    public float this[int row, int col]
    { 
        get
        {
            return matrix[row, col];
        }
        set
        {
            matrix[row, col] = value;
        }
    }

    public Matrix Negative()
    {
        Matrix ret = new Matrix(row, col);
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                ret[i, j] = -matrix[i, j];
            }
        }
        return ret;
    }

    public Matrix Zero()
    {
        Matrix ret = new Matrix(row, col);
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                ret[i, j] = 0;
            }
        }
        return ret;
    }

    public Matrix Transpose()
    {
        Matrix ret = new Matrix(col, row);
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                ret[j, i] = matrix[i, j];
            }
        }
        return ret;
    }

    public void SetValue(int startRow, int startCol, Matrix m)
    {
        for (int i = startRow; i < startRow + m.row; i++)
        {
            for (int j = startCol; j < startCol + m.col; j++)
            {
                matrix[i, j] = m[i - startRow, j - startCol];
            }
        }
    }

    public void Print()
    {
        string s = "";
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                s += matrix[i, j] + "    ";
            }
            s += "\n";
        }
        Debug.Log(s);
    }

    public static Matrix operator *(Matrix m1, Matrix m2)
    {
        int row = m1.row;
        int col = m2.col;
        if (m1.col != m2.row) return null;
        int count = m1.col;
        Matrix ret = new Matrix(row, col);
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                for (int k = 0; k < count; k++)
                {
                    ret[i, j] += m1[i, k] * m2[k, j];
                }
            }
        }
        return ret;
    }
}
