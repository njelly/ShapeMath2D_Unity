using System;

namespace Tofunaut.ShapeMath2D_Unity
{
    public enum ShapeType
    {
        AABB,
        Circle,
        Polygon,
    }
    
    public class Shape
    {
        public ShapeType ShapeType;
        public float CircleRadius;
        public System.Numerics.Vector2 Center;
        public System.Numerics.Vector2[] PolygonVertices;
        public System.Numerics.Vector2 AABBMin;
        public System.Numerics.Vector2 AABBMax;

        public void Translate(System.Numerics.Vector2 delta)
        {
            switch (ShapeType)
            {
                case ShapeType.AABB:
                    AABBMin += delta;
                    AABBMax += delta;
                    break;
                case ShapeType.Circle:
                    Center += delta;
                    break;
                case ShapeType.Polygon:
                    Center += delta;
                    for (var i = 0; i < PolygonVertices.Length; i++)
                        PolygonVertices[i] += delta;
                    break;
            }
        }

        public void Rotate(float radians)
        {
            if (ShapeType != ShapeType.Polygon)
                return;

            var vertexSum = System.Numerics.Vector2.Zero;
            for (var i = 0; i < PolygonVertices.Length; i++)
            {
                PolygonVertices[i] = PolygonVertices[i].RotatedByRadians(radians, Center);
                vertexSum += PolygonVertices[i];
            }

            Center = vertexSum / PolygonVertices.Length;
        }

        public bool Intersects(Shape otherShape)
        {
            switch (ShapeType)
            {
                case ShapeType.AABB:
                    switch (otherShape.ShapeType)
                    {
                        case ShapeType.AABB:
                            return ShapeMath2D.AABBIntersectsAABB(AABBMin, AABBMax, otherShape.AABBMin, otherShape.AABBMax);
                        case ShapeType.Circle:
                            return ShapeMath2D.CircleIntersectsAABB(otherShape.Center, otherShape.CircleRadius,
                                AABBMin, AABBMax);
                        case ShapeType.Polygon:
                            return ShapeMath2D.PolygonIntersectsAABB(otherShape.PolygonVertices, AABBMin, AABBMax);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case ShapeType.Circle:
                    switch (otherShape.ShapeType)
                    {
                        case ShapeType.AABB:
                            return ShapeMath2D.CircleIntersectsAABB(Center, CircleRadius, otherShape.AABBMin, otherShape.AABBMax);
                        case ShapeType.Circle:
                            return ShapeMath2D.CircleIntersectsCircle(Center, CircleRadius, otherShape.Center, otherShape.CircleRadius);
                        case ShapeType.Polygon:
                            return ShapeMath2D.PolygonIntersectsCircle(otherShape.PolygonVertices, Center, CircleRadius);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case ShapeType.Polygon:
                    switch (otherShape.ShapeType)
                    {
                        case ShapeType.AABB:
                            return ShapeMath2D.PolygonIntersectsAABB(PolygonVertices, otherShape.AABBMin, otherShape.AABBMax);
                        case ShapeType.Circle:
                            return ShapeMath2D.PolygonIntersectsCircle(PolygonVertices, otherShape.Center, otherShape.CircleRadius);
                        case ShapeType.Polygon:
                            return ShapeMath2D.PolygonIntersectsPolygon(PolygonVertices, otherShape.PolygonVertices);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}