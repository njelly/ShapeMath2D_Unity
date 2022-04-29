using System;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Tofunaut.ShapeMath2D_Unity
{
    public class GameManager : MonoBehaviour
    {
        private Shape[] _shapes;
        private int _currentShapeIndex;
        private Vector2[] _cachedVectors;

        private void Awake()
        {
            _cachedVectors = new Vector2[32];
            _shapes = new[]
            {
                new Shape
                {
                    ShapeType = ShapeType.Circle,
                    Center = Vector2.Zero,
                    CircleRadius = 2,
                },
                new Shape
                {
                    ShapeType = ShapeType.Circle,
                    Center = new Vector2(3f, -2.45f),
                    CircleRadius = 1.2f,
                },
                new Shape
                {
                    ShapeType = ShapeType.AABB,
                    AABBMin = new Vector2(1, 3.2f),
                    AABBMax = new Vector2(2.1f, 4f),
                },
                new Shape
                {
                    ShapeType = ShapeType.AABB,
                    AABBMin = new Vector2(-1, -2f),
                    AABBMax = new Vector2(4f, -1f),
                },
                new Shape
                {
                    ShapeType = ShapeType.Polygon,
                    PolygonVertices = new []
                    {
                        new Vector2(0, 0),
                        new Vector2(0, 4),
                        new Vector2(5, 0),
                        new Vector2(2.5f, -1f),
                    }
                },
                new Shape
                {
                    ShapeType = ShapeType.Polygon,
                    PolygonVertices = new []
                    {
                        new Vector2(-4, -4),
                        new Vector2(-3, -4),
                        new Vector2(-3.5f, -3f),
                    }
                },
            };
        }

        private void Update()
        {
            var moveDelta = Vector2.Zero;
            if (Input.GetKey(KeyCode.W))
                moveDelta += new Vector2(0f, 1f);
            if (Input.GetKey(KeyCode.S))
                moveDelta += new Vector2(0f, -1f);
            if (Input.GetKey(KeyCode.D))
                moveDelta += new Vector2(1f, 0f);
            if (Input.GetKey(KeyCode.A))
                moveDelta += new Vector2(-1f, 0f);

            moveDelta *= Time.deltaTime * 3f;
            _shapes[_currentShapeIndex].Translate(moveDelta);

            var rotateDelta = 0f;
            if (Input.GetKey(KeyCode.E))
                rotateDelta += Mathf.PI * 2f;
            if (Input.GetKey(KeyCode.Q))
                rotateDelta += Mathf.PI * -2f;

            rotateDelta *= Time.deltaTime;
            _shapes[_currentShapeIndex].Rotate(rotateDelta);

            if (Input.GetKeyDown(KeyCode.RightArrow))
                _currentShapeIndex++;
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                _currentShapeIndex--;

            if (_currentShapeIndex < 0)
                _currentShapeIndex = _shapes.Length - 1;

            if (_currentShapeIndex >= _shapes.Length)
                _currentShapeIndex = 0;
        }

        private void OnDrawGizmos()
        {
            if (_shapes == null)
                return;
            
            for (var i = 0; i < _shapes.Length; i++)
            {
                var startColor = Gizmos.color;

                if (_currentShapeIndex == i)
                {
                    var doesIntersect = false;
                    for (var j = 0; j < _shapes.Length; j++)
                    {
                        if (j == i)
                            continue;

                        doesIntersect |= _shapes[j].Intersects(_shapes[i]);
                    }

                    Gizmos.color = doesIntersect ? Color.green : Color.white;
                }
                else
                {
                    Gizmos.color = Color.gray;
                }
                
                RenderShape(_shapes[i]);
                
                Gizmos.color = startColor;
            }
        }

        private void RenderShape(Shape shape)
        {
            var numVertices = 0;
            switch (shape.ShapeType)
            {
                case ShapeType.AABB:
                    numVertices = 4;
                    ShapeMath2D.GetVerticesAABB(shape.AABBMin, shape.AABBMax, _cachedVectors);
                    break;
                case ShapeType.Circle:
                    numVertices = _cachedVectors.Length;
                    for (var i = 0; i < _cachedVectors.Length; i++)
                    {
                        var angle = i * Mathf.PI * 2f / numVertices;
                        _cachedVectors[i] = shape.Center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * shape.CircleRadius;;
                    }
                    break;
                case ShapeType.Polygon:
                    numVertices = shape.PolygonVertices.Length;
                    Array.Copy(shape.PolygonVertices, _cachedVectors, shape.PolygonVertices.Length);
                    break;
            }

            for (var i = 0; i < numVertices; i++)
            {
                var nextIndex = (i + 1) % numVertices;
                Gizmos.DrawLine(_cachedVectors[i].ToUnityVector2(), _cachedVectors[nextIndex].ToUnityVector2());
            }
        }
    }
}