// MIT License
// 
// Copyright (c) 2022 Nathaniel Ellingson
// https://njellingson.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// This is a collection of static math functions for performing common geometry tests in 2D. Usually this sort of math 
// comes bundled with a bulky physics engine, but I've found that I frequently want to test whether or not, for example, a
// circle and a triangle intersect without having to instantiate a world and tick through a simulation. I'm tired of 
// reinventing the wheel each time I have what should be a simple geometry problem, so I wrote this. Maybe it'll be
// useful for you too?

// *** NOTE: POLYGONS MUST BE CONVEX AND VERTICES MUST GO CLOCKWISE AROUND THE SHAPE!!!! ***

// An example Unity project can be found here: https://github.com/njelly/ShapeMath2D_Unity

using System;
using Vector2 = System.Numerics.Vector2;

namespace Tofunaut
{
    public static class ShapeMath2D
    {
# region AABB
        
        public static bool AABBContainsPoint(Vector2 min, Vector2 max, Vector2 point) =>
            point.X >= min.X && point.X <= max.X && point.Y >= min.Y && point.Y <= max.Y;

        public static bool AABBIntersectsAABB(Vector2 minA, Vector2 maxA, Vector2 minB, Vector2 maxB) =>
            maxA.X - minA.X + maxB.X - minB.X > MathF.Max(maxA.X, maxB.X) - MathF.Min(minA.X, minB.X)
            && maxA.Y - minA.Y + maxB.Y - minB.Y > MathF.Max(maxA.Y, maxB.Y) - MathF.Min(minA.Y, minB.Y);

        public static unsafe void GetVerticesAABB(Vector2 min, Vector2 max, Vector2[] vertices)
        {
            fixed (Vector2* ptr = vertices)
                GetVerticesAABBUnsafe(min, max, ptr);
        }

        public static unsafe void GetVerticesAABBUnsafe(Vector2 min, Vector2 max, Vector2* vertices)
        {
            vertices[0] = max;
            vertices[1] = new Vector2(max.X, min.Y);
            vertices[2] = min;
            vertices[3] = new Vector2(min.X, max.Y);
        }

        public static unsafe void GetBoundingAABB(Vector2[] vertices, out Vector2 aabbMin, out Vector2 aabbMax)
        {
            fixed(Vector2* ptr = vertices)
                GetBoundingAABBUnsafe(ptr, vertices.Length, out aabbMin, out aabbMax);
        }
        
        public static unsafe void GetBoundingAABBUnsafe(Vector2* vertices, int length, out Vector2 aabbMin, out Vector2 aabbMax)
        {
            aabbMin = default;
            aabbMax = default;
            for (var i = 0; i < length; i++)
            {
                aabbMin = new Vector2(MathF.Min(aabbMin.X, vertices[i].X), MathF.Min(aabbMin.Y, vertices[i].Y));
                aabbMax = new Vector2(MathF.Max(aabbMax.X, vertices[i].X), MathF.Max(aabbMax.Y, vertices[i].Y));
            }
        }
        
#endregion AABB

# region Circle
        
        public static bool CircleContainsPoint(Vector2 center, float radius, Vector2 point) =>
            (point - center).LengthSquared() <= radius * radius;

        public static unsafe bool CircleIntersectsAABB(Vector2 center, float radius, Vector2 aabbMin, Vector2 aabbMax)
        {
            if (AABBContainsPoint(aabbMin, aabbMax, center))
                return true;

            var aabbVertices = stackalloc Vector2[4];
            GetVerticesAABBUnsafe(aabbMin, aabbMax, aabbVertices);
            for (var i = 0; i < 4; i++)
            {
                var nextIndex = (i + 1) % 4;
                var closestPointOnEdge = ClosestPointOnLineSegment(aabbVertices[i], aabbVertices[nextIndex], center);
                if (CircleContainsPoint(center, radius, closestPointOnEdge))
                    return true;
            }

            return false;
        }

        public static bool CircleIntersectsCircle(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB)
        {
            var radiusSum = radiusA + radiusB;
            return (centerB - centerA).LengthSquared() <= radiusSum * radiusSum;
        }

        public static unsafe void GetBoundingCircle(Vector2[] points, out Vector2 circleCenter, out float circleRadius)
        {
            fixed(Vector2* ptr = points)
                GetBoundingCircleUnsafe(ptr, points.Length, out circleCenter, out circleRadius);
        }

        /// <summary>
        /// Implements Welzl's algorithm for finding the smallest bounding circle containing a set of points in O(n) time.
        /// This website was very helpful: http://www.sunshine2k.de/coding/java/Welzl/Welzl.html
        /// </summary>
        public static unsafe void GetBoundingCircleUnsafe(Vector2* points, int length, out Vector2 circleCenter,
            out float circleRadius)
        {
            var pointsOnCircle = stackalloc Vector2[3];

            var numIterations = 0;
            
            welzl(length, 0, out circleCenter, out circleRadius);

            void welzl(int numUnchecked, int numPointsOnCircle, out Vector2 c, out float r)
            {
                if (numIterations++ > 999)
                {
                    c = default;
                    r = default;
                    return;
                }
                
                if (numUnchecked <= 0 || numPointsOnCircle == 3)
                {
                    calculateCircle(numPointsOnCircle, out c, out r);
                    return;
                }

                var p = points[numUnchecked - 1];
                welzl(numUnchecked - 1, numPointsOnCircle, out c, out r);
                if (!CircleContainsPoint(c, r, p))
                {
                    pointsOnCircle[numPointsOnCircle] = p;
                    welzl(numUnchecked - 1, numPointsOnCircle + 1, out c, out r);
                }
            }

            void calculateCircle(int numPointsOnCircle, out Vector2 c, out float r)
            {
                c = default;
                r = default;
                switch (numPointsOnCircle)
                {
                    case 1:
                        c = pointsOnCircle[0];
                        r = 0f;
                        break;
                    case 2:
                        c = (pointsOnCircle[1] + pointsOnCircle[0]) / 2f;
                        r = (pointsOnCircle[1] - pointsOnCircle[0]).Length() / 2f;
                        break;
                    case 3:
                        GetCircleFromTriangleUnsafe(pointsOnCircle, out c, out r);
                        break;
                }
            }
        }

        public static unsafe void GetCircleFromTriangle(Vector2[] points, out Vector2 circleCenter,
            out float circleRadius)
        {
            fixed(Vector2* ptr = points)
                GetCircleFromTriangleUnsafe(ptr, out circleCenter, out circleRadius);
        }

        public static unsafe void GetCircleFromTriangleUnsafe(Vector2* points, out Vector2 circleCenter,
            out float circleRadius)
        {
            GetLongestEdgeOfPolygonUnsafe(points, 3, out var longestEdge);

            var a = (longestEdge + 1) % 3;
            var b = (longestEdge + 2) % 3;
            var aToBMiddle = (points[a] + points[b]) / 2f;
            var bToCMiddle = (points[b] + points[longestEdge]) / 2f;
            var perpA = (aToBMiddle - points[a]).RotatedByRadians(MathF.PI / 2f) + aToBMiddle;
            var perpB = (bToCMiddle - points[b]).RotatedByRadians(MathF.PI / 2f) + bToCMiddle;

            LineIntersectsLine(aToBMiddle, perpA, bToCMiddle, perpB, out circleCenter);

            circleRadius = (points[0] - circleCenter).Length();
        }
        
#endregion Circle
        
#region Line

        public static Vector2 ClosestPointOnLine(Vector2 a, Vector2 b, Vector2 point)
        {
            var ap = point - a;
            var ab = b - a;
            var dist = Vector2.Dot(ap, ab) / ab.LengthSquared();
            return a + ab * dist;
        }

        public static Vector2 ClosestPointOnLineSegment(Vector2 a, Vector2 b, Vector2 point)
        {
            var ap = point - a;
            var ab = b - a;
            var dist = Vector2.Dot(ap, ab) / ab.LengthSquared();
            return dist switch
            {
                < 0 => a,
                > 1 => b,
                _ => a + ab * dist
            };
        }

        public static bool PointIsOnLeftSideOfLine(Vector2 a, Vector2 b, Vector2 point) =>
            (b.X - a.X) * (point.Y - a.Y) - (b.Y - a.Y) * (point.X - a.X) > 0;

        public static bool LineIntersectsLine(Vector2 a1, Vector2 b1, Vector2 a2, Vector2 b2,
            out Vector2 intersection, float tolerance = 0.0001f)
        {
            intersection = default;

            var x1 = a1.X;
            var x2 = b1.X;
            var x3 = a2.X;
            var x4 = b2.X;

            var y1 = a1.Y;
            var y2 = b1.Y;
            var y3 = a2.Y;
            var y4 = b2.Y;

            // equations of the form x = c (two vertical lines)
            if (MathF.Abs(x1 - x2) < tolerance && MathF.Abs(x3 - x4) < tolerance && MathF.Abs(x1 - x3) < tolerance)
                return false;

            //equations of the form y=c (two horizontal lines)
            if (MathF.Abs(y1 - y2) < tolerance && MathF.Abs(y3 - y4) < tolerance && MathF.Abs(y1 - y3) < tolerance)
                return false;

            //equations of the form x=c (two vertical parallel lines)
            if (MathF.Abs(x1 - x2) < tolerance && MathF.Abs(x3 - x4) < tolerance)
                return false;

            //equations of the form y=c (two horizontal parallel lines)
            if (MathF.Abs(y1 - y2) < tolerance && MathF.Abs(y3 - y4) < tolerance)
                return false;

            //general equation of line is y = mx + c where m is the slope
            //assume equation of line 1 as y1 = m1x1 + c1 
            //=> -m1x1 + y1 = c1 ----(1)
            //assume equation of line 2 as y2 = m2x2 + c2
            //=> -m2x2 + y2 = c2 -----(2)
            //if line 1 and 2 intersect then x1=x2=x & y1=y2=y where (x,y) is the intersection p
            //so we will get below two equations 
            //-m1x + y = c1 --------(3)
            //-m2x + y = c2 --------(4)

            float x, y;

            //lineA is vertical x1 = x2
            //slope will be infinity
            //so lets derive another solution
            if (MathF.Abs(x1 - x2) < tolerance)
            {
                //compute slope of line 2 (m2) and c2
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;

                //equation of vertical line is x = c
                //if line 1 and 2 intersect then x1=c1=x
                //subsitute x=x1 in (4) => -m2x1 + y = c2
                // => y = c2 + m2x1 
                x = x1;
                y = c2 + m2 * x1;
            }
            //other is vertical x3 = x4
            //slope will be infinity
            //so lets derive another solution
            else if (MathF.Abs(x3 - x4) < tolerance)
            {
                //compute slope of line 1 (m1) and c2
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;

                //equation of vertical line is x = c
                //if line 1 and 2 intersect then x3=c3=x
                //subsitute x=x3 in (3) => -m1x3 + y = c1
                // => y = c1 + m1x3 
                x = x3;
                y = c1 + m1 * x3;
            }
            //lineA & other are not vertical 
            //(could be horizontal we can handle it with slope = 0)
            else
            {
                //compute slope of line 1 (m1) and c2
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;

                //compute slope of line 2 (m2) and c2
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;

                //solving equations (3) & (4) => x = (c1-c2)/(m2-m1)
                //plugging x value in equation (4) => y = c2 + m2 * x
                x = (c1 - c2) / (m2 - m1);
                y = c2 + m2 * x;

                //verify by plugging intersection p (x, y)
                //in orginal equations (1) & (2) to see if they intersect
                //otherwise x,y values will not be finite and will fail this check
                if (!(MathF.Abs(-m1 * x + y - c1) < tolerance
                      && MathF.Abs(-m2 * x + y - c2) < tolerance))
                {
                    //return default (no intersection)
                    return false;
                }
            }

            //x,y can intersect outside the line segment since line is infinitely long
            //so finally check if x, y is within both the line segments
            intersection = new Vector2(x, y);
            return true;

            //if (IsInsideLine(lineA, x, y) &&
            //    IsInsideLine(other, x, y))
            //{
            //    return new Vector2(x, y);
            //}

            //return default (no intersection)
            // return default;
        }

        public static bool LineIntersectsLineSegment(Vector2 a1, Vector2 b1, Vector2 a2, Vector2 b2,
            out Vector2 intersection) => LineIntersectsLine(a1, b1, a2, b2, out intersection) &&
                                         IsInsideLineSegment(a2, b2, intersection);

        public static bool LineSegmentIntersectsLineSegment(Vector2 a1, Vector2 b1, Vector2 a2, Vector2 b2,
            out Vector2 intersection) => LineIntersectsLineSegment(a1, b1, a2, b2, out intersection) &&
                   IsInsideLineSegment(a1, b1, intersection);

        private static bool IsInsideLineSegment(Vector2 a, Vector2 b, Vector2 point) =>
            (point.X >= a.X && point.X <= b.X || point.X >= b.X && point.X <= a.X) &&
            (point.Y >= a.Y && point.Y <= b.Y || point.Y >= b.Y && point.Y <= a.Y);
        

#endregion Line

# region Polygon

        public static unsafe bool PolygonContainsPoint(Vector2[] vertices, Vector2 point)
        {
            fixed (Vector2* ptr = vertices)
                return PolygonContainsPointUnsafe(ptr, vertices.Length, point);
        }

        public static unsafe bool PolygonContainsPointUnsafe(Vector2* vertices, int length, Vector2 point)
        {
            var firstSide = PointIsOnLeftSideOfLine(vertices[0], vertices[1], point);
            for (var i = 1; i < length; i++)
            {
                var nextIndex = (i + 1) % length;
                if (firstSide != PointIsOnLeftSideOfLine(vertices[i], vertices[nextIndex], point))
                    return false;
            }

            return true;
        }

        public static unsafe bool PolygonIntersectsAABB(Vector2[] vertices, Vector2 aabbMin, Vector2 aabbMax)
        {
            fixed (Vector2* ptr = vertices)
                return PolygonIntersectsAABBUnsafe(ptr, vertices.Length, aabbMin, aabbMax);
        }

        public static unsafe bool PolygonIntersectsAABBUnsafe(Vector2* vertices, int length, Vector2 aabbMin, Vector2 aabbMax)
        {
            for (var i = 0; i < length; i++)
            {
                if (AABBContainsPoint(aabbMin, aabbMax, vertices[i]))
                    return true;
            }

            var aabbVertices = stackalloc Vector2[4];
            GetVerticesAABBUnsafe(aabbMin, aabbMax, aabbVertices);

            for (var i = 0; i < 4; i++)
            {
                if (PolygonContainsPointUnsafe(vertices, length, aabbVertices[i]))
                    return true;
            }

            for (var i = 0; i < length; i++)
            {
                var a = vertices[i];
                var b = vertices[(i + 1) % length];
                for (var j = 0; j < 4; j++)
                {
                    var otherA = aabbVertices[j];
                    var otherB = aabbVertices[(j + 1) % 4];

                    if (LineSegmentIntersectsLineSegment(a, b, otherA, otherB, out _))
                        return true;
                }
            }

            return false;
        }

        public static unsafe bool PolygonIntersectsCircle(Vector2[] vertices, Vector2 circleCenter, float circleRadius)
        {
            fixed (Vector2* ptr = vertices)
                return PolygonIntersectsCircleUnsafe(ptr, vertices.Length, circleCenter, circleRadius);
        }

        public static unsafe bool PolygonIntersectsCircleUnsafe(Vector2* vertices, int length, Vector2 circleCenter,
            float circleRadius)
        {
            if (PolygonContainsPointUnsafe(vertices, length, circleCenter))
                return true;

            for (var i = 0; i < length; i++)
            {
                var nextIndex = (i + 1) % length;
                var closestPoint = ClosestPointOnLineSegment(vertices[i], vertices[nextIndex], circleCenter);
                if (CircleContainsPoint(circleCenter, circleRadius, closestPoint))
                    return true;
            }

            return false;
        }

        public static unsafe bool PolygonIntersectsPolygon(Vector2[] verticesA, Vector2[] verticesB)
        {
            fixed (Vector2* ptrA = verticesA)
            {
                fixed (Vector2* ptrB = verticesB)
                    return PolygonIntersectsPolygonUnsafe(ptrA, verticesA.Length, ptrB, verticesB.Length);
            }
        }

        public static unsafe bool PolygonIntersectsPolygonUnsafe(Vector2* verticesA, int lengthA, Vector2* verticesB,
            int lengthB)
        {
            for (var i = 0; i < lengthA; i++)
            {
                if (PolygonContainsPointUnsafe(verticesB, lengthB, verticesA[i]))
                    return true;
            }
            
            for (var i = 0; i < lengthB; i++)
            {
                if (PolygonContainsPointUnsafe(verticesA, lengthA, verticesB[i]))
                    return true;
            }

            for (var i = 0; i < lengthA; i++)
            {
                var nextIndexA = (i + 1) % lengthA;
                for (var j = 0; j < lengthB; j++)
                {
                    var nextIndexB = (j + 1) % lengthB;
                    if (LineSegmentIntersectsLineSegment(verticesA[i], verticesA[nextIndexA], verticesB[j],
                            verticesB[nextIndexB], out _))
                        return true;
                }
            }

            return false;
        }

        public static unsafe void GetCenterOfPolygon(Vector2[] vertices, out Vector2 center)
        {
            fixed(Vector2* ptr = vertices)
                GetCenterOfPolygonUnsafe(ptr, vertices.Length, out center);
        }

        public static unsafe void GetCenterOfPolygonUnsafe(Vector2* vertices, int length, out Vector2 center)
        {
            center = Vector2.Zero;
            for (var i = 0; i < length; i++)
                center += vertices[i];

            center /= length;
        }

        public static unsafe void GetLongestEdgeOfPolygon(Vector2[] vertices, out int longestEdge)
        {
            fixed (Vector2* ptr = vertices)
                GetLongestEdgeOfPolygonUnsafe(ptr, vertices.Length, out longestEdge);
        }

        public static unsafe void GetLongestEdgeOfPolygonUnsafe(Vector2* vertices, int length, out int longestEdge)
        {
            var longestLengthSquared = (vertices[1] - vertices[0]).LengthSquared();
            longestEdge = 0;
            for (var i = 1; i < length; i++)
            {
                var nextVertex = (i + 1) / length;
                var lengthSquared = (vertices[nextVertex] - vertices[i]).LengthSquared();
                if (!(lengthSquared > longestLengthSquared)) 
                    continue;
                
                longestEdge = i;
                longestLengthSquared = lengthSquared;
            }
        }

# endregion
        
# region Vector2
        
        public static Vector2 RotatedByDegrees(this Vector2 v, float degree) => v.RotatedByDegrees(degree, Vector2.Zero);

        public static Vector2 RotatedByDegrees(this Vector2 v, float degrees, Vector2 center) =>
            v.RotatedByRadians(degrees * MathF.PI / 180, center);

        public static Vector2 RotatedByRadians(this Vector2 v, float radians) => v.RotatedByRadians(radians, Vector2.Zero);

        public static Vector2 RotatedByRadians(this Vector2 v, float radians, Vector2 center)
        {
            var cosTheta = MathF.Cos(radians);
            var sinTheta = MathF.Sin(radians);
            return new Vector2
            {
                X = cosTheta * (v.X - center.X) - sinTheta * (v.Y - center.Y) + center.X,
                Y = sinTheta * (v.X - center.X) + cosTheta * (v.Y - center.Y) + center.Y,
            };
        }
        
# endregion Vector2
    }
}