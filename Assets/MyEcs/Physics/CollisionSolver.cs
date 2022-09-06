using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MyEcs.Physics
{
    public enum ColliderType : byte
    {
        none,
        rectangle,
        circle,
        point,
    }
    public static class CollisionSolver
    {
        public static bool Solve(ref ECPosition pos1, ref ECCollider col1, ref ECPosition pos2, ref ECCollider col2)
            => Solve(col1.type, pos1.position, col1.size, col2.type, pos2.position, col2.size);
        public static bool Solve(ColliderType col1, Vector2 pos1, Vector2 size1, ColliderType col2, Vector2 pos2, Vector2 size2)
        {
            if (col1 == ColliderType.rectangle && col2 == ColliderType.rectangle)
                return SolveRectRect(pos1, size1, pos2, size2);

            else if (col1 == ColliderType.rectangle && col2 == ColliderType.circle)
                return SolveRectCircle(pos1, size1, pos2, size2.x);
            else if (col1 == ColliderType.circle && col2 == ColliderType.rectangle)
                return SolveRectCircle(pos2, size2, pos1, size1.x);

            else if (col1 == ColliderType.rectangle && col2 == ColliderType.point)
                return SolveRectPoint(pos1, size1, pos2);
            else if (col1 == ColliderType.point && col2 == ColliderType.rectangle)
                return SolveRectPoint(pos2, size2, pos1);

            else if (col1 == ColliderType.circle && col2 == ColliderType.circle)
                return SolveCircleCircle(pos1, size1.x, pos2, size2.x);

            else if (col1 == ColliderType.circle && col2 == ColliderType.point)
                return SolveCirclePoint(pos1, size1.x, pos2);
            else if (col1 == ColliderType.point && col2 == ColliderType.circle)
                return SolveCirclePoint(pos2, size2.x, pos1);

            else
                return false;
        }

        public static bool SolveRectRect(Vector2 pos1, Vector2 size1, Vector2 pos2, Vector2 size2)
        {
            Vector2 diff = pos1 - pos2;
            if (Mathf.Abs(diff.x) >= size1.x / 2 + size2.x / 2 || Math.Abs(diff.y) >= size1.y / 2 + size2.y / 2)
                return false;
            return true;
        }
        public static bool SolveRectCircle(Vector2 pos1, Vector2 size1, Vector2 pos2, float rad2)
        {
            Vector2 halfSize = size1 / 2;
            Vector2 diff = pos2 - pos1;
            if (Mathf.Abs(diff.x) >= halfSize.x + rad2 || Math.Abs(diff.y) >= halfSize.y + rad2)
                return false;
            if (Mathf.Abs(diff.x) < halfSize.x || Math.Abs(diff.y) < halfSize.y)
                return true;
            Vector2 corner = pos1 + new Vector2(Mathf.Sign(diff.x), Mathf.Sign(diff.y)) * halfSize;
            return SolveCirclePoint(pos2, rad2, corner);
        }
        public static bool SolveRectPoint(Vector2 pos1, Vector2 size1, Vector2 pos2)
        {
            Vector2 diff = pos1 - pos2;
            if (Mathf.Abs(diff.x) >= size1.x / 2 || Math.Abs(diff.y) >= size1.y / 2)
                return false;
            return true;
        }
        public static bool SolveCircleCircle(Vector2 pos1, float rad1, Vector2 pos2, float rad2)
        {
            float radSum = rad1 + rad2;
            return (pos1 - pos2).sqrMagnitude < radSum * radSum;
        }
        public static bool SolveCirclePoint(Vector2 pos1, float rad1, Vector2 pos2)
        {
            return (pos1 - pos2).sqrMagnitude < rad1 * rad1;
        }

    }
}