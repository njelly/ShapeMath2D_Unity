using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = System.Numerics.Vector2;

namespace Tofunaut.ShapeMath2D_Unity
{
    public class ShrinkWrapDemo : MonoBehaviour
    {
        [SerializeField] private float _randomRadius;
        [SerializeField] private int _numRandomPoints;
        [SerializeField] private int _maxHullVertices;

        private Vector2[] _randomPoints;
        private Vector2[] _cachedVectors;
        private Vector2 _boundingAABBMin;
        private Vector2 _boundingAABBMax;
        private Vector2 _boundingCircleCenter;
        private float _boundingCircleRadius;
        
        private void Start()
        {
            Regenerate();
        }

        public void Regenerate()
        {
            _randomPoints = new Vector2[_numRandomPoints];
            for (var i = 0; i < _randomPoints.Length; i++)
                _randomPoints[i] = Random.insideUnitCircle.ToSystemVector2() * _randomRadius;

            _cachedVectors = new Vector2[_maxHullVertices];
            
            ShapeMath2D.GetBoundingAABB(_randomPoints, out _boundingAABBMin, out _boundingAABBMax);
            ShapeMath2D.GetBoundingCircle(_randomPoints, out _boundingCircleCenter, out _boundingCircleRadius);
        }

        private void OnDrawGizmos()
        {
            if (_randomPoints == null)
                return;
            
            // render vertices
            Gizmos.color = Color.white;
            for (var i = 0; i < _randomPoints.Length; i++)
            {
                Shape.RenderShape(new Shape
                {
                    ShapeType = ShapeType.Circle,
                    Center = _randomPoints[i],
                    CircleRadius = 0.1f
                }, _cachedVectors);
            }
            
            // render random radius
            Gizmos.color = Color.yellow;
            Shape.RenderShape(new Shape
            {
                ShapeType = ShapeType.Circle,
                Center = Vector2.Zero,
                CircleRadius = _randomRadius,
            }, _cachedVectors);

            // render bounding circle
            Gizmos.color = Color.magenta;
            Shape.RenderShape(new Shape
            {
                ShapeType = ShapeType.Circle,
                Center = _boundingCircleCenter,
                CircleRadius = _boundingCircleRadius,
            }, _cachedVectors);
            Shape.RenderShape(new Shape
            {
                ShapeType = ShapeType.AABB,
                AABBMin = _boundingAABBMin,
                AABBMax = _boundingAABBMax,
            }, _cachedVectors);
            
            Gizmos.color = Color.red;
            ShapeMath2D.GetBoundingPolygon(_randomPoints, _cachedVectors, out var numBoundingPolygonVertices);
            var boundingPolygonArray = new Vector2[numBoundingPolygonVertices];
            Array.Copy(_cachedVectors, boundingPolygonArray, numBoundingPolygonVertices);
            Shape.RenderShape(new Shape
            {
                ShapeType = ShapeType.Polygon,
                PolygonVertices = boundingPolygonArray,
            }, _cachedVectors);
        }
    }
}