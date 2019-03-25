using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrackUtils {

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

    public class TrackPart
    {
        public string type;
        public float distance;
        public float angle;
        public float radius;
        public bool rightturn;

        public TrackPart()
        {
            type = "straight";
            distance = 1f;
        }

        public TrackPart(string inType, float inDist)
        {
            type = "straight";
            distance = inDist;
        }

        public TrackPart(string inType, float inAngle, float inRadius, bool inRightTurn)
        {
            type = "curve";
            angle = inAngle;
            radius = inRadius;
            rightturn = inRightTurn;
        }
    }

}
