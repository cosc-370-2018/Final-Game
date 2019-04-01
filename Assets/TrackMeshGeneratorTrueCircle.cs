using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrackUtils;

[RequireComponent(typeof(MeshFilter))]
public class TrackMeshGeneratorTrueCircle : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    Vector3 Rotate_Vector3(Vector3 vector, float deg_angle)
    {
        float angle = Mathf.Deg2Rad * deg_angle;
        return new Vector3((Mathf.Cos(angle)*vector.x)-(Mathf.Sin(angle)*vector.z), 0, (Mathf.Sin(angle)*vector.x)+(Mathf.Cos(angle)*vector.z));
    }

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // define track attributes here
        float track_width = 20f;
        int NumOfCurveSegments = 20;
        TrackUtils.OffsetData offset_data = new TrackUtils.OffsetData(Vector3.zero, 0f, 0, 0);

        // dynamic track attributes
        int NumOfStraights = 0;
        int NumOfCurves = 0;

        List<TrackUtils.TrackPart> track = new List<TrackUtils.TrackPart>();

        ////////////////////////////////////////////////////////////////////
        // construct track here
        // straight section --> TrackUtils.TrackPart("straight", length (float));
        // curve section --> TrackUtils.TrackPart("curve", degree of turn (in degrees, as a float), turn radius (float), right turn? (true or false));

		for (int i = 0; i < 8; i++) {
			if (i % 2 == 0) {
				if (i == 2 || i == 6) {
					track.Add(new TrackUtils.TrackPart("straight", 50f));
				} else {
					track.Add(new TrackUtils.TrackPart("straight", 100f));
				}
			} else {
				track.Add(new TrackUtils.TrackPart("curve", 90f, track_width, true));
			}
		}

        ////////////////////////////////////////////////////////////////////

        // count # of each type of track part
        TrackUtils.TrackPart[] finalTrack = track.ToArray();
        for (int i = 0; i < finalTrack.Length; i++)
        {
            if (finalTrack[i].type == "curve")
            {
                NumOfCurves++;
            } else {
                NumOfStraights++;
            }
        }

        // calculate variables and create arrays of appropriate lengths
        // do not mess with these
        int totalVertices = (NumOfStraights*4)+(NumOfCurves*(2+(2*NumOfCurveSegments)));
        int totalTriangles = 3*((NumOfStraights*2)+(NumOfCurves*(2*NumOfCurveSegments)));
        vertices = new Vector3[totalVertices];
        triangles = new int[totalTriangles];

        // iterate through track[] and create all the segments
        for (int i = 0; i < finalTrack.Length; i++)
        {
            if (finalTrack[i].type == "curve")
            {
                offset_data = GenerateCurvedSection(finalTrack[i].angle, finalTrack[i].radius, track_width, NumOfCurveSegments, finalTrack[i].rightturn, offset_data);
            } else {
                offset_data = GenerateStraightSection(finalTrack[i].distance, track_width, offset_data);
            }
        }

        CreateMesh();
    }

    TrackUtils.OffsetData GenerateStraightSection(float track_length, float track_width, TrackUtils.OffsetData offset)
    {
        vertices[offset.vertices_index+0] = Rotate_Vector3(new Vector3(0.5f*track_width, 0, 0), offset.rotation_offset) + offset.position_offset;
        vertices[offset.vertices_index+1] = Rotate_Vector3(new Vector3(-0.5f*track_width, 0, 0), offset.rotation_offset) + offset.position_offset;
        vertices[offset.vertices_index+2] = Rotate_Vector3(new Vector3(0.5f*track_width, 0, track_length), offset.rotation_offset) + offset.position_offset;
        vertices[offset.vertices_index+3] = Rotate_Vector3(new Vector3(-0.5f*track_width, 0, track_length), offset.rotation_offset) + offset.position_offset;

        triangles[offset.triangles_index+0] = offset.vertices_index + 0;
        triangles[offset.triangles_index+1] = offset.vertices_index + 1;
        triangles[offset.triangles_index+2] = offset.vertices_index + 2;

        triangles[offset.triangles_index+3] = offset.vertices_index + 2;
        triangles[offset.triangles_index+4] = offset.vertices_index + 1;
        triangles[offset.triangles_index+5] = offset.vertices_index + 3;


        TrackUtils.OffsetData new_offset = new TrackUtils.OffsetData(Rotate_Vector3(new Vector3(0, 0, track_length), offset.rotation_offset) + offset.position_offset, offset.rotation_offset, offset.vertices_index + 4, offset.triangles_index + 6);
        return new_offset;
    }

    TrackUtils.OffsetData GenerateCurvedSection(float curve_angle, float radius, float track_width, int segments, bool right_turn, TrackUtils.OffsetData offset)
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
        TrackUtils.OffsetData new_offset;

        float end_direction = 0f;
        Vector3 end_position = Vector3.zero;

        Vector3 inner_point;
        Vector3 outer_point;
        Vector3 internal_offset;

        // set rotational offset
        if (right_turn == true) {
            angle = Mathf.PI + (offset.rotation_offset*Mathf.Deg2Rad);
            internal_offset = Rotate_Vector3(new Vector3(radius, 0, 0), offset.rotation_offset);
        } else {
            angle = 0f + (offset.rotation_offset*Mathf.Deg2Rad);
            internal_offset = Rotate_Vector3(new Vector3(-radius, 0, 0), offset.rotation_offset);
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

            vertices_int[(i*2)] = inner_point;
            vertices_int[(i*2)+1] = outer_point;


            // we set these every loop but don't do anything with them until after the loop is done, because all we're trying to get is points from the last set of calculations.
            end_position = new Vector3((Mathf.Cos(angle)*radius), 0, (Mathf.Sin(angle)*radius)) + internal_offset + offset.position_offset;
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

        if (right_turn == true) {
            end_direction = offset.rotation_offset-curve_angle;
        } else {
            end_direction = offset.rotation_offset+curve_angle;
        }
        new_offset = new TrackUtils.OffsetData(end_position, end_direction, offset.vertices_index+vertices_int.Length, offset.triangles_index+triangles_int.Length);

        return new_offset;
    }

    void CreateMesh()
    {
        mesh.vertices = vertices;
        mesh.triangles = triangles;
		mesh.uv = GenerateUVs(mesh);
        mesh.RecalculateNormals();
    }

	Vector2[] GenerateUVs(Mesh mesh) {
		Bounds bounds = mesh.bounds;
		Vector2[] uvs = new Vector2[vertices.Length];
		
		for (int i = 0; i < vertices.Length; i++) 
		{
			uvs[i] = new Vector2(vertices[i].x / bounds.size.x, vertices[i].z / bounds.size.z);
		}

		return uvs;
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
