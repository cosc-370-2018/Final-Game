using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TrackMeshGeneratorTrueCircle : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

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

        // define track attributes here
        float track_width = 1f;
        int NumOfCurveSegments = 20;
        OffsetData offset_data = new OffsetData(Vector3.zero, 0f, 0, 0);

        // dynamic track attributes
        int NumOfStraights = 1;
        int NumOfCurves = 2;

        // calculate variables and create arrays of appropriate lengths
        // do not change these
        int totalVertices = (NumOfStraights*4)+(NumOfCurves*(2+(2*NumOfCurveSegments)));
        int totalTriangles = 3*((NumOfStraights*2)+(NumOfCurves*(2*NumOfCurveSegments)));
        vertices = new Vector3[totalVertices];
        triangles = new int[totalTriangles];


        Debug.Log("TriLen:" + triangles.Length);
        Debug.Log("VrtLen:" + vertices.Length);

        offset_data = GenerateStraightSection(10f, track_width, offset_data);
        offset_data = GenerateCurvedSection(120f, 2f, track_width, NumOfCurveSegments, false, offset_data);
        offset_data = GenerateCurvedSection(120f, 2f, track_width, NumOfCurveSegments, true, offset_data);
        //(2, 0, 10)
        //offset_data = GenerateCurvedSection(120f, 2f, track_width, NumOfCurveSegments, false, offset_data);


        for (int i = 0; i < vertices.Length; i++)
        {
            Debug.Log("vrts @ " + i + ":" + vertices[i]);
        }
        for (int i = 0; i < triangles.Length; i++)
        {
            Debug.Log("tris @ " + i + ":" + triangles[i]);
        }

        CreateMesh();
    }

    OffsetData GenerateStraightSection(float track_length, float track_width, OffsetData offset)
    {
        vertices[offset.vertices_index+0] = new Vector3(0.5f*track_width, 0, 0) + offset.position_offset;
        vertices[offset.vertices_index+1] = new Vector3(-0.5f*track_width, 0, 0) + offset.position_offset;
        vertices[offset.vertices_index+2] = new Vector3(0.5f*track_width, 0, track_length) + offset.position_offset;
        vertices[offset.vertices_index+3] = new Vector3(-0.5f*track_width, 0, track_length) + offset.position_offset;

        triangles[offset.triangles_index+0] = offset.vertices_index + 0;
        triangles[offset.triangles_index+1] = offset.vertices_index + 1;
        triangles[offset.triangles_index+2] = offset.vertices_index + 2;

        triangles[offset.triangles_index+3] = offset.vertices_index + 2;
        triangles[offset.triangles_index+4] = offset.vertices_index + 1;
        triangles[offset.triangles_index+5] = offset.vertices_index + 3;

        // Vector3(0, track_length)
        // x = 0
        // y = track_length

        // https://matthew-brett.github.io/teaching/rotation_2d.html
        // x,y = original x and y coordinates, pre-rotation (y is actually z in this case)
        // Vector3((Mathf.Cos(offset.rotation_offset)*x)-(Mathf.Sin(offset.rotation_offset)*y), 0, (Mathf.Sin(offset.rotation_offset)*x)+(Mathf.Cos(offset.rotation_offset)*y))
        // should use this formula to make a "Vector3Rotate(vector, angle)" function
        // Vector3((Mathf.Cos(offset.rotation_offset)*0)-(Mathf.Sin(offset.rotation_offset)*track_length), 0, (Mathf.Sin(offset.rotation_offset)*0)+(Mathf.Cos(offset.rotation_offset)*track_length))
        
        OffsetData new_offset = new OffsetData(new Vector3(0, 0, track_length) + offset.position_offset, offset.rotation_offset, offset.vertices_index + 4, offset.triangles_index + 6);
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

        // offset starting angle so the initial time through the loop will be working on the initial points (at 0)
        if (right_turn == true) {
            angle += segment_angle;
        } else {
            angle -= segment_angle;
        }

        // create all the points
        for (int i = 0; i <= segments; i++)
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

            // offset.positional_offset = positional offset of the entire curve
            inner_point = new Vector3(inner_x, 0, inner_z) + internal_offset + offset.position_offset;
            outer_point = new Vector3(outer_x, 0, outer_z) + internal_offset + offset.position_offset;

            vertices_int[(i*2)] = inner_point;  // +2
            vertices_int[(i*2)+1] = outer_point;  // +3

            //Debug.Log("i:" + i);
            //Debug.Log("angle:" + angle);
            //Debug.Log("inner_p:" + inner_point);
            //Debug.Log("outer_p:" + outer_point);

            // we set these every loop but don't do anything with them until after the loop is done, because all we're trying to get is points from the last set of calculations.
            end_position = new Vector3((Mathf.Cos(angle)*radius), 0, (Mathf.Sin(angle)*radius)) + offset.position_offset;
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
                triangles_int[(i*6)+0] = (i*2)+0;
                triangles_int[(i*6)+1] = (i*2)+2;
                triangles_int[(i*6)+2] = (i*2)+1;

                triangles_int[(i*6)+3] = (i*2)+2;
                triangles_int[(i*6)+4] = (i*2)+3;
                triangles_int[(i*6)+5] = (i*2)+1;
            }
        }

        for (int i = 0; i < vertices_int.Length; i++)
        {
            vertices[i+offset.vertices_index] = vertices_int[i];
        }
        for (int i = 0; i < triangles_int.Length; i++)
        {
            triangles[i+offset.triangles_index] = triangles_int[i]+offset.vertices_index;
        }

        end_direction = (angle*Mathf.Rad2Deg)-offset.rotation_offset;
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
