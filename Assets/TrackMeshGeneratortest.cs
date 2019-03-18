using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TrackMeshGeneratortest : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    Line baseline;
    int[] triangles;

    public int xSize = 6;
    public int zSize = 10;
    private Vector3 forward;

    public class Line
    {
        Vector3 point_a;
        Vector3 point_b;

        public Line()
        {
            point_a = Vector3.zero;
            point_b = Vector3.zero;
        }

        public Line(Vector3 a, Vector3 b)
        {
            point_a = a;
            point_b = b;
        }

        public Vector3 A()
        {
            return point_a;
        }

        public Vector3 B()
        {
            return point_b;
        }

        public void SetA(Vector3 newA)
        {
            point_a = newA;
        }

        public void SetB(Vector3 newB)
        {
            point_b = newB;
        }

    }

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        //GenerateStraightSection();
        GenerateCurvedSection(2, 2, 8);
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

    void GenerateCurvedSection(float track_width, float segment_length, int segments)
    {
        vertices = new Vector3[2+((segments)*3)];
        triangles = new int[9*segments];

        // starting reference point for new mesh
        baseline = new Line(new Vector3(-track_width/2, 0, 0), new Vector3(track_width/2, 0, 0));

        vertices[0] = baseline.A();
        vertices[1] = baseline.B();

        for (int i = 2; i < (segments*3); i += 3)
        {
            Line midline;
            Vector3 outer_point;

            forward = Vector3.Cross(baseline.B()-baseline.A(), Vector3.up).normalized;

            midline = new Line(baseline.A() + (forward * segment_length/2), baseline.B() + (forward * segment_length/2));

            outer_point = baseline.A() + (forward * segment_length);

            vertices[i] = midline.A();  // offset by 2 because baseline takes the first 2 spots
            vertices[i+1] = midline.B();
            vertices[i+2] = outer_point;


            // set hypotenus as the new baseline
            baseline.SetA(outer_point);
            baseline.SetB(midline.B());
        }

        for (int i = 0; i < (segments*9); i += 9)
        {
            // form first triangle with baseline points
            triangles[(i*9)] = i*9;  // baseline[0]
            triangles[(i*9)+1] = (i*9)+2;  // midline[0]
            triangles[(i*9)+2] = (i*9)+1;  // baseline[1]

            // second triangle
            triangles[(i*9)+3] = (i*9)+1;  // baseline[1]
            triangles[(i*9)+4] = (i*9)+2;  // midline[0]
            triangles[(i*9)+5] = (i*9)+3;  // midline[1]

            // third triangle
            triangles[(i*9)+6] = (i*9)+2;  // midline[0]
            triangles[(i*9)+7] = (i*9)+4;  // outer_point
            triangles[(i*9)+8] = (i*9)+3;  // midline[1]
        }
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
