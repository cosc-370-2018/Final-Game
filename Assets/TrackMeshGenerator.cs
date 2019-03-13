using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TrackMeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public int xSize = 6;
    public int zSize = 10;

    public bool straight = true;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        if (straight)
        {
            GenerateStraightSection();
        } else
        {
            GenerateCurvedConnector();
        }

        //GenerateCurvedSection();
        
        CreateMesh();
    }

    void GenerateStraightSection()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                vertices[i] = new Vector3(x, 0, z);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tri = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tri] = vert;
                triangles[tri + 1] = vert + xSize + 1;
                triangles[tri + 2] = vert + 1;
                triangles[tri + 3] = vert + 1;
                triangles[tri + 4] = vert + xSize + 1;
                triangles[tri + 5] = vert + xSize + 2;

                vert++;
                tri += 6;
            }
            vert++;
        }
    }

    void GenerateCurvedSection()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        float xoffset = 0f;
        float zoffset = 0f;
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                vertices[i] = new Vector3(x + xoffset, 0, z + zoffset);
                i++;
                xoffset += 0.01f * z;
                if (z > 0)
                {
                    zoffset -= 0.08f * -x;
                }
                
            }
        }

        triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tri = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tri] = vert;
                triangles[tri + 1] = vert + xSize + 1;
                triangles[tri + 2] = vert + 1;
                triangles[tri + 3] = vert + 1;
                triangles[tri + 4] = vert + xSize + 1;
                triangles[tri + 5] = vert + xSize + 2;

                vert++;
                tri += 6;
            }
            vert++;
        }
    }

    void GenerateCurvedConnector() {
        vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0 + xSize, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0 + xSize, 0, 1),

            new Vector3(0 + xSize + 1, 0, 2),
            new Vector3(0 + xSize + 1, 0, 2 + xSize),
            new Vector3(0 + xSize + 2, 0, 2),
            new Vector3(0 + xSize + 2, 0, 2 + xSize),

            new Vector3(1.5f, 0, 4.5f),
            new Vector3(3.5f, 0, 6.5f)

        };

        triangles = new int[] {
            0, 2, 1,
            1, 2, 3,
            4, 5, 6,
            6, 5, 7,
            3, 2, 4,
            2, 8, 4,
            4, 8, 9,
            9, 5, 4
        };
    }

    void CreateMesh()
    {
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;

        for (int  i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
    }
}
