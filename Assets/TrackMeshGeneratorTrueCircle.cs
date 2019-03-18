using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TrackMeshGeneratorTrueCircle : MonoBehaviour
{
    Mesh mesh;

    int NumOfStraights = 0;
    int NumOfCurves = 1;
    int NumOfCurveSegments = 20;

    Vector3[] vertices;
    int[] triangles;

    OffsetData offset_data;

    public class OffsetData
    {
        public Vector3 position_offset;
        public float rotation_offset;
        public int vertices_index;
        public int triangles_index;

        public OffsetData()
        {
            position_offset = Vector3.zero;
            rotation_offset = 0f;
        }

        public OffsetData(Vector3 position, float rotation, int vertex, int triangle)
        {
            position_offset = position;
            rotation_offset = rotation;
            vertices_index = vertex;
            triangles_index = triangle;
        }

    }

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        int VerticesInCurves = NumOfCurves * (NumOfCurveSegments * 2);
        //vertices = new Vector3[2 + (2*NumOfStraights) + VerticesInCurves];
        //triangles = new int[3*((NumOfStraights*2) + (NumOfCurveSegments*2))];
        float track_width = 1f;
        vertices = new Vector3[44];
        triangles = new int[3*2*20];

        // create first 2 starting points
        vertices[0] = new Vector3(10, 0, 0);
        vertices[1] = new Vector3(10, 0, 0);
        Debug.Log(vertices[0] + " " + vertices[1]);

        offset_data = new OffsetData(Vector3.zero, 0f, 2, 0);
        //offset_data = GenerateStraightSection(10f, track_width, offset_data);
        offset_data = GenerateCurvedSection(120f, 2f, track_width, NumOfCurveSegments, true, offset_data);

        Debug.Log(vertices[0] + " " + vertices[1]);
        for (int i = 0; i < vertices.Length; i++)
        {
            Debug.Log(vertices[i] + " " + i);
        }
        Debug.Log(vertices.Length);

        CreateMesh();
    }

    OffsetData GenerateStraightSection(float track_length, float track_width, OffsetData offset)
    {
        vertices[offset.vertices_index] = new Vector3(0.5f*track_width, 0, track_length) + offset.position_offset;
        vertices[offset.vertices_index+1] = new Vector3(-0.5f*track_width, 0, track_length) + offset.position_offset;

        triangles[offset.triangles_index] = offset.vertices_index - 2;
        triangles[offset.triangles_index+1] = offset.vertices_index - 1;
        triangles[offset.triangles_index+2] = offset.vertices_index + 0;

        triangles[offset.triangles_index+3] = offset.vertices_index + 0;
        triangles[offset.triangles_index+4] = offset.vertices_index - 1;
        triangles[offset.triangles_index+5] = offset.vertices_index + 1;

        OffsetData new_offset = new OffsetData(new Vector3(0, 0, track_length) + offset.position_offset, offset.rotation_offset, offset.vertices_index + 2, offset.triangles_index + 2);
        return new_offset;
    }

    OffsetData GenerateCurvedSection(float curve_angle, float radius, float track_width, int segments, bool right_turn, OffsetData offset)
    {
        Vector3[] vertices_int;
        int[] triangles_int;
        vertices_int = new Vector3[2+(segments*2)];  // 2 triangles per (rectangle) segment
        triangles_int = new int[segments*6];  // 2 triangles per segment = 6 vertices per segment

        float angle;
        float segment_angle = (curve_angle*Mathf.Deg2Rad) / segments;
        float inner_x;
        float inner_z;
        float outer_x;
        float outer_z;
        OffsetData new_offset;

        float end_direction = 0f;
        Vector3 end_position = Vector3.zero;

        Vector3 inner_point;
        Vector3 outer_point;
        Vector3 internal_offset;

        // set rotational offset
        if (right_turn == true) {
            angle = Mathf.PI + (offset.rotation_offset*Mathf.Deg2Rad);
            internal_offset = new Vector3(radius, 0, 0);
        } else {
            angle = 0f + (offset.rotation_offset*Mathf.Deg2Rad);
            internal_offset = new Vector3(-radius, 0, 0);
        }


        for (int i = 0; i < segments; i++)
        {
            if (right_turn == true) {
                angle -= segment_angle;
            } else {
                angle += segment_angle;
            }

            // generate inner and outer point coordinates
            inner_x = Mathf.Cos(angle) * (radius-(track_width/2));
            outer_x = Mathf.Cos(angle) * (radius+(track_width/2));
            inner_z = Mathf.Sin(angle) * (radius-(track_width/2));
            outer_z = Mathf.Sin(angle) * (radius+(track_width/2));

            // offset = positional offset of the entire curve
            inner_point = new Vector3(inner_x, 0, inner_z) + internal_offset + offset.position_offset;
            outer_point = new Vector3(outer_x, 0, outer_z) + internal_offset + offset.position_offset;

            vertices_int[(i*2)+2] = inner_point;
            vertices_int[(i*2)+3] = outer_point;

            // we set these every loop but don't do anything with them until after the loop is done, because all we're trying to get is points from the last set of calculations.
            end_position = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        }

        if (right_turn == true)
        {
            // walk through each line and create the next rectangle segment (made of 2 triangles)
            for (int i = 0; i < segments; i++)
            {
                // step through triangles array in multiples of 6, but the vertices array in multiples of 2
                // aka, on each increment, we discard 2 points (which form a radial line) and get 2 new points (a new radial line)
                triangles_int[(i*6)+0] = (i*2)+0;
                triangles_int[(i*6)+1] = (i*2)+1;
                triangles_int[(i*6)+2] = (i*2)+2;

                triangles_int[(i*6)+3] = (i*2)+2;
                triangles_int[(i*6)+4] = (i*2)+1;
                triangles_int[(i*6)+5] = (i*2)+3;
            }
        } else {
            // walk through each line and create the next rectangle segment (made of 2 triangles)
            for (int i = 0; i < segments; i++)
            {
                // step through triangles array in multiples of 6, but the vertices array in multiples of 2
                // aka, on each increment, we discard 2 points (which form a radial line) and get 2 new points (a new radial line)
                triangles_int[(i*6)+0] = ((i+offset.vertices_index)*2)+0;
                triangles_int[(i*6)+1] = ((i+offset.vertices_index)*2)+2;
                triangles_int[(i*6)+2] = ((i+offset.vertices_index)*2)+1;

                triangles_int[(i*6)+3] = ((i+offset.vertices_index)*2)+2;
                triangles_int[(i*6)+4] = ((i+offset.vertices_index)*2)+3;
                triangles_int[(i*6)+5] = ((i+offset.vertices_index)*2)+1;
            }
        }

        for (int i = 0; i < vertices_int.Length; i++)
        {
            vertices[i+offset.vertices_index] = vertices_int[i];
            Debug.Log((i+offset.vertices_index) + " : " + vertices_int[i] + " : " + vertices[i+offset.vertices_index]);
            //Debug.Log("length = " + vertices.Length + " " + vertices[i+offset.vertices_index]);
        }
        for (int i = 0; i < triangles.Length; i++)
        {
            //Debug.Log("ib = " + i);
            triangles[i+offset.triangles_index] = triangles_int[i];
            //Debug.Log("length = " + triangles.Length + " " + triangles[i+offset.triangles_index]);
        }

        end_direction = angle*Mathf.Rad2Deg;
        new_offset = new OffsetData(end_position, end_direction, offset.vertices_index+vertices_int.Length, offset.triangles_index+triangles_int.Length);

        return new_offset;
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
