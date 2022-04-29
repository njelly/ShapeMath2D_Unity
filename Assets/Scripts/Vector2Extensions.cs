using UnityEngine;

namespace Tofunaut.ShapeMath2D_Unity
{
    public static class Vector2Extensions
    {
        public static System.Numerics.Vector2 ToSystemVector2(this Vector2 v) => new System.Numerics.Vector2(v.x, v.y);
        public static Vector2 ToUnityVector2(this System.Numerics.Vector2 v) => new Vector2(v.X, v.Y);
    }
}