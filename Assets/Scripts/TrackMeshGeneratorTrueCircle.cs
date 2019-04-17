using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrackUtils;

public class TrackMeshGeneratorTrueCircle : MonoBehaviour
{
    Transform track_object;
    Transform left_walls_object;
    Transform right_walls_object;
    Mesh track_mesh;
    Mesh left_walls_mesh;
    Mesh right_walls_mesh;

    Vector3[] track_vertices;
    int[] track_triangles;
    Vector3[] left_walls_vertices;
    int[] left_walls_triangles;
    Vector3[] right_walls_vertices;
    int[] right_walls_triangles;

	public int track_number = 0;

    void Start()
    {
        track_object = this.transform.GetChild(0);
        left_walls_object = this.transform.GetChild(1);
        right_walls_object = this.transform.GetChild(2);

        track_mesh = new Mesh();
        left_walls_mesh = new Mesh();
        right_walls_mesh = new Mesh();

        track_object.GetComponent<MeshFilter>().mesh = track_mesh;
        left_walls_object.GetComponent<MeshFilter>().mesh = left_walls_mesh;
        right_walls_object.GetComponent<MeshFilter>().mesh = right_walls_mesh;

        // define track attributes here
        float track_width = 20f;
        int NumOfCurveSegments = 20;
        float building_height = 8f;
        TrackUtils.OffsetData offset_data = new TrackUtils.OffsetData(Vector3.zero, 0f, 0, 0);

        // dynamic track attributes
        int NumOfStraights = 0;
        int NumOfCurves = 0;

        List<TrackUtils.TrackPart> track = new List<TrackUtils.TrackPart>();

        ////////////////////////////////////////////////////////////////////
        // construct track here
        // straight section --> TrackUtils.TrackPart("straight", length (float));
        // curve section --> TrackUtils.TrackPart("curve", degree of turn (in degrees, as a float), turn radius (float), right turn? (true or false));

		switch (track_number) {
			case 1:
				TrackOne(track, track_width);
				break;
			case 2:
				TrackTwo(track, track_width);
				break;
			default:
				GenericTrack(track, track_width);
				break;
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
        track_vertices = new Vector3[totalVertices];
        track_triangles = new int[totalTriangles];
        left_walls_vertices = new Vector3[totalVertices];
        left_walls_triangles = new int[totalTriangles];
        right_walls_vertices = new Vector3[totalVertices];
        right_walls_triangles = new int[totalTriangles];

        // iterate through track[] and create all the segments
        for (int i = 0; i < finalTrack.Length; i++)
        {
            if (finalTrack[i].type == "curve")
            {
                offset_data = GenerateCurvedSection(finalTrack[i].angle, finalTrack[i].radius, track_width, NumOfCurveSegments, finalTrack[i].rightturn, building_height, offset_data);
            } else {
                offset_data = GenerateStraightSection(finalTrack[i].distance, track_width, building_height, offset_data);
            }
        }
        for (int i = 0; i < left_walls_vertices.Length; i++) {
            //Debug.Log(left_walls_vertices[i]);
        }

        CreateMesh(track_mesh, track_vertices, track_triangles);
        CreateMesh(left_walls_mesh, left_walls_vertices, left_walls_triangles);
        CreateMesh(right_walls_mesh, right_walls_vertices, right_walls_triangles);
    }

    void CreateMesh(Mesh input_mesh, Vector3[] input_vertices, int[] input_triangles)
    {
        input_mesh.vertices = input_vertices;
        input_mesh.triangles = input_triangles;
        input_mesh.uv = GenerateUVs(input_mesh, input_vertices);
        input_mesh.RecalculateNormals();
    }

    Vector2[] GenerateUVs(Mesh mesh, Vector3[] vertices) {
        Bounds bounds = mesh.bounds;
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < vertices.Length; i++) 
        {
            uvs[i] = new Vector2(vertices[i].x / bounds.size.x, vertices[i].z / bounds.size.z);
        }

        return uvs;
    }

    TrackUtils.OffsetData GenerateStraightSection(float track_length, float track_width, float building_height, TrackUtils.OffsetData offset)
    {
        track_vertices[offset.vertices_index+0] = Rotate_Vector3(new Vector3(0.5f*track_width, 0, 0), offset.rotation_offset) + offset.position_offset;
        track_vertices[offset.vertices_index+1] = Rotate_Vector3(new Vector3(-0.5f*track_width, 0, 0), offset.rotation_offset) + offset.position_offset;
        track_vertices[offset.vertices_index+2] = Rotate_Vector3(new Vector3(0.5f*track_width, 0, track_length), offset.rotation_offset) + offset.position_offset;
        track_vertices[offset.vertices_index+3] = Rotate_Vector3(new Vector3(-0.5f*track_width, 0, track_length), offset.rotation_offset) + offset.position_offset;

        track_triangles[offset.triangles_index+0] = offset.vertices_index + 0;
        track_triangles[offset.triangles_index+1] = offset.vertices_index + 1;
        track_triangles[offset.triangles_index+2] = offset.vertices_index + 2;
        track_triangles[offset.triangles_index+3] = offset.vertices_index + 2;
        track_triangles[offset.triangles_index+4] = offset.vertices_index + 1;
        track_triangles[offset.triangles_index+5] = offset.vertices_index + 3;

        left_walls_vertices[offset.vertices_index+0] = Rotate_Vector3(new Vector3(-0.5f*track_width, 0, 0), offset.rotation_offset) + offset.position_offset;
        left_walls_vertices[offset.vertices_index+1] = Rotate_Vector3(new Vector3(-0.5f*track_width, building_height, 0), offset.rotation_offset) + offset.position_offset;
        left_walls_vertices[offset.vertices_index+2] = Rotate_Vector3(new Vector3(-0.5f*track_width, 0, track_length), offset.rotation_offset) + offset.position_offset;
        left_walls_vertices[offset.vertices_index+3] = Rotate_Vector3(new Vector3(-0.5f*track_width, building_height, track_length), offset.rotation_offset) + offset.position_offset;

        left_walls_triangles[offset.triangles_index+0] = offset.vertices_index + 0;
        left_walls_triangles[offset.triangles_index+1] = offset.vertices_index + 1;
        left_walls_triangles[offset.triangles_index+2] = offset.vertices_index + 2;
        left_walls_triangles[offset.triangles_index+3] = offset.vertices_index + 2;
        left_walls_triangles[offset.triangles_index+4] = offset.vertices_index + 1;
        left_walls_triangles[offset.triangles_index+5] = offset.vertices_index + 3;

        right_walls_vertices[offset.vertices_index+0] = Rotate_Vector3(new Vector3(0.5f*track_width, 0, 0), offset.rotation_offset) + offset.position_offset;
        right_walls_vertices[offset.vertices_index+1] = Rotate_Vector3(new Vector3(0.5f*track_width, building_height, 0), offset.rotation_offset) + offset.position_offset;
        right_walls_vertices[offset.vertices_index+2] = Rotate_Vector3(new Vector3(0.5f*track_width, 0, track_length), offset.rotation_offset) + offset.position_offset;
        right_walls_vertices[offset.vertices_index+3] = Rotate_Vector3(new Vector3(0.5f*track_width, building_height, track_length), offset.rotation_offset) + offset.position_offset;

        right_walls_triangles[offset.triangles_index+0] = offset.vertices_index + 0;
        right_walls_triangles[offset.triangles_index+1] = offset.vertices_index + 2;
        right_walls_triangles[offset.triangles_index+2] = offset.vertices_index + 1;
        right_walls_triangles[offset.triangles_index+3] = offset.vertices_index + 2;
        right_walls_triangles[offset.triangles_index+4] = offset.vertices_index + 3;
        right_walls_triangles[offset.triangles_index+5] = offset.vertices_index + 1;


        TrackUtils.OffsetData new_offset = new TrackUtils.OffsetData(Rotate_Vector3(new Vector3(0, 0, track_length), offset.rotation_offset) + offset.position_offset, offset.rotation_offset, offset.vertices_index + 4, offset.triangles_index + 6);
        return new_offset;
    }

    TrackUtils.OffsetData GenerateCurvedSection(float curve_angle, float radius, float track_width, int segments, bool right_turn, float building_height, TrackUtils.OffsetData offset)
    {
        Vector3[] vertices_int;
        int[] triangles_int;
        Vector3[] left_building_vertices_int;
        int[] left_building_triangles_int;
        Vector3[] right_building_vertices_int;
        int[] right_building_triangles_int;
        vertices_int = new Vector3[2+(segments*2)];  // 2 triangles per (rectangle) segment
        triangles_int = new int[segments*6];  // 2 triangles per segment = 6 vertices per segment
        left_building_vertices_int = new Vector3[2+(segments*2)];  // 2 triangles per (rectangle) segment
        left_building_triangles_int = new int[segments*6];  // 2 triangles per segment = 6 vertices per segment
        right_building_vertices_int = new Vector3[2+(segments*2)];  // 2 triangles per (rectangle) segment
        right_building_triangles_int = new int[segments*6];  // 2 triangles per segment = 6 vertices per segment

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
        Vector3 building_inner_point;
        Vector3 building_outer_point;
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
            building_outer_point = new Vector3(outer_x, building_height, outer_z) + internal_offset + offset.position_offset;
            building_inner_point = new Vector3(inner_x, building_height, inner_z) + internal_offset + offset.position_offset;

            // if it's a right turn, the right side is the inside of the curve and left on the outside
            if (right_turn == true) {
                left_building_vertices_int[(i*2)] = outer_point;
                left_building_vertices_int[(i*2)+1] = building_outer_point;
                right_building_vertices_int[(i*2)] = inner_point;
                right_building_vertices_int[(i*2)+1] = building_inner_point;
            } else {
                left_building_vertices_int[(i*2)] = inner_point;
                left_building_vertices_int[(i*2)+1] = building_inner_point;
                right_building_vertices_int[(i*2)] = outer_point;
                right_building_vertices_int[(i*2)+1] = building_outer_point;
            }

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

                left_building_triangles_int[(i*6)+0] = (i*2)+0;
                left_building_triangles_int[(i*6)+1] = (i*2)+1;
                left_building_triangles_int[(i*6)+2] = (i*2)+2;

                left_building_triangles_int[(i*6)+3] = (i*2)+2;
                left_building_triangles_int[(i*6)+4] = (i*2)+1;
                left_building_triangles_int[(i*6)+5] = (i*2)+3;

                right_building_triangles_int[(i*6)+0] = (i*2)+0;
                right_building_triangles_int[(i*6)+1] = (i*2)+2;
                right_building_triangles_int[(i*6)+2] = (i*2)+1;

                right_building_triangles_int[(i*6)+3] = (i*2)+2;
                right_building_triangles_int[(i*6)+4] = (i*2)+3;
                right_building_triangles_int[(i*6)+5] = (i*2)+1;
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

                left_building_triangles_int[(i*6)+0] = (i*2)+0;
                left_building_triangles_int[(i*6)+1] = (i*2)+1;
                left_building_triangles_int[(i*6)+2] = (i*2)+2;

                left_building_triangles_int[(i*6)+3] = (i*2)+2;
                left_building_triangles_int[(i*6)+4] = (i*2)+1;
                left_building_triangles_int[(i*6)+5] = (i*2)+3;

                right_building_triangles_int[(i*6)+0] = (i*2)+0;
                right_building_triangles_int[(i*6)+1] = (i*2)+2;
                right_building_triangles_int[(i*6)+2] = (i*2)+1;

                right_building_triangles_int[(i*6)+3] = (i*2)+2;
                right_building_triangles_int[(i*6)+4] = (i*2)+3;
                right_building_triangles_int[(i*6)+5] = (i*2)+1;
            }
        }

        for (int i = 0; i < vertices_int.Length; i++)
        {
            track_vertices[i+offset.vertices_index] = vertices_int[i];
            left_walls_vertices[i+offset.vertices_index] = left_building_vertices_int[i];
            right_walls_vertices[i+offset.vertices_index] = right_building_vertices_int[i];
        }
        for (int i = 0; i < triangles_int.Length; i++)
        {
            track_triangles[i+offset.triangles_index] = triangles_int[i]+offset.vertices_index;
            left_walls_triangles[i+offset.triangles_index] = left_building_triangles_int[i]+offset.vertices_index;
            right_walls_triangles[i+offset.triangles_index] = right_building_triangles_int[i]+offset.vertices_index;
            //Debug.Log(left_walls_triangles[i+offset.triangles_index] + " " + (left_walls_triangles_int[i]+offset.vertices_index));
        }

        if (right_turn == true) {
            end_direction = offset.rotation_offset-curve_angle;
        } else {
            end_direction = offset.rotation_offset+curve_angle;
        }
        new_offset = new TrackUtils.OffsetData(end_position, end_direction, offset.vertices_index+vertices_int.Length, offset.triangles_index+triangles_int.Length);

        return new_offset;
    }

    Vector3 Rotate_Vector3(Vector3 vector, float deg_angle)
    {
        float angle = Mathf.Deg2Rad * deg_angle;
        return new Vector3((Mathf.Cos(angle)*vector.x)-(Mathf.Sin(angle)*vector.z), vector.y, (Mathf.Sin(angle)*vector.x)+(Mathf.Cos(angle)*vector.z));
    }

	void GenericTrack(List<TrackUtils.TrackPart> track, float track_width) {
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
	}

	void TrackOne(List<TrackUtils.TrackPart> track, float track_width) {
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 20f, track_width, false));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 90f, track_width, true));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 40f, track_width, true));
		track.Add(new TrackUtils.TrackPart("curve", 40f, track_width, true));
		track.Add(new TrackUtils.TrackPart("curve", 40f, track_width, true));
		track.Add(new TrackUtils.TrackPart("curve", 40f, track_width, true));
		track.Add(new TrackUtils.TrackPart("curve", 95f, track_width, false));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 90f, track_width, true));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 90f, track_width, true));
		track.Add(new TrackUtils.TrackPart("curve", 40f, track_width, true));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 6.15f, track_width, true));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
	}

	void TrackTwo(List<TrackUtils.TrackPart> track, float track_width) {
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 20f, track_width, true));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 90f, track_width, true));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 40f, track_width, true));
		track.Add(new TrackUtils.TrackPart("curve", 40f, track_width, true));
		track.Add(new TrackUtils.TrackPart("curve", 95f, track_width, false));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 90f, track_width, true));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 90f, track_width, true));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 6.15f, track_width, true));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 90f, track_width, false));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 90f, track_width, true));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
		track.Add(new TrackUtils.TrackPart("curve", 61f, track_width, true));
		track.Add(new TrackUtils.TrackPart("straight", 50f));
	}
}
