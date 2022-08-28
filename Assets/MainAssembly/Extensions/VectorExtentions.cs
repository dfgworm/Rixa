using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;

public static class VectorExtentions
{
    public static bool FuzzyEquals(this Vector2 v1, Vector2 v2)
    {
        return Mathf.Abs(v1.x - v2.x) < Vector2.kEpsilon && Mathf.Abs(v1.y - v2.y) < Vector2.kEpsilon;
    }
    public static bool FuzzyEquals(this Vector3 v1, Vector3 v2)
    {
        return Mathf.Abs(v1.x - v2.x) < Vector3.kEpsilon && Mathf.Abs(v1.y - v2.y) < Vector3.kEpsilon && Mathf.Abs(v1.z - v2.z) < Vector3.kEpsilon;
    }

}
