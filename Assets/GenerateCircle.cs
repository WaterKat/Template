using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCircle : MonoBehaviour
{
    void Start()
    {
        GetComponent<MeshFilter>().mesh = GenerateMesh();
    }

    public int Resolution = 4;
    public int Radius = 1;
    Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[Resolution+1];
        vertices[0] = Vector3.zero;

        for (int i = 1; i <= Resolution; i++)
        {
            float angle = -(i-1) * Mathf.PI * 2 / Resolution;
            vertices[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle),0);
        }

        mesh.vertices = vertices;

        foreach (var item in mesh.vertices)
        {
            Debug.Log(item);
        }

        int[] triangles = new int[Resolution * 3];

        triangles[0] = 0;
        triangles[1] = Resolution + 1;
        triangles[2] = 1;

        for (int i = 1; i < Resolution; i++)
        {
            triangles[i * 3] = 0;
            triangles[(i * 3) + 1] = i;
            triangles[(i * 3) + 2] = i+1;
        }

        for (int i = 0; i < Resolution; i++)
        {
            Debug.Log(triangles[i*3] + "/" + triangles[(i*3) + 1] + "/" + triangles[(i*3) + 2]);
        }

        mesh.triangles = triangles;

        return mesh;
    }
}
