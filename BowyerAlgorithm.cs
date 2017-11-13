using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Dataformatter;
using Dataformatter.Misc;
using Dataformatter.Datamodels;
using UnityEngine;

namespace MeshesGeneration.BowyerAlgorithm
{
    public class BowyerAlgorithm
    {
        private readonly int SUPERTRIANGLE_ENLARGEMENT_FACTOR;
        private Triangle _superTriangle;
        
        private List<XYPoint> _inputPoints = new List<XYPoint>();
        
        private readonly List<Triangle> _newTrianglesForCurrentIteration = new List<Triangle>();
        private readonly List<Triangle> _triangulation = new List<Triangle>();
        private readonly List<Triangle> _currentTriangles = new List<Triangle>();

        
        public BowyerAlgorithm(List<Vector3> vectors, int supertriangleEnlargementFactor)
        {
            SUPERTRIANGLE_ENLARGEMENT_FACTOR = supertriangleEnlargementFactor;
            
            foreach (var v in vectors)
                _inputPoints.Add(new XYPoint() {X = v.x, Y = v.y});
        }


        public List<Triangle> ComputeFinalTriangulation()
        {
            CreateSuperTriangle();

            _inputPoints = _inputPoints.OrderBy(p => p.X).ToList();

            foreach (var currentPoint in _inputPoints)
            {
                var badTriangles = DetermineBadTriangles(currentPoint);
                var polygonsWithOriginalTriangles = DeterminePolygons(badTriangles);

                RemoveBadTrianglesFromTriangulation(badTriangles);
                CreateNewTriangles(currentPoint, polygonsWithOriginalTriangles);


                for (int i = 0; i < _newTrianglesForCurrentIteration.Count(); i++)
                {
                    for (int j = 0; j < _newTrianglesForCurrentIteration[i].Edges.Count(); j++)
                        RemoveIntersectingEdges(_currentTriangles, _newTrianglesForCurrentIteration[i].Edges[j]);
                }

                _newTrianglesForCurrentIteration.Clear();
            }
            //RemoveSuperTriangleVertices();

            return _triangulation;
        }

        private void CreateSuperTriangle()
        {
            var lowerLeftX = _inputPoints.Min(p => p.X) - SUPERTRIANGLE_ENLARGEMENT_FACTOR - 1500; 
            var lowerLeftY = _inputPoints.Min(p => p.Y) - SUPERTRIANGLE_ENLARGEMENT_FACTOR; 

            var lowerRightX = _inputPoints.Max(p => p.X) + SUPERTRIANGLE_ENLARGEMENT_FACTOR + 1500; 
            var lowerRightY = _inputPoints.Min(p => p.Y) - SUPERTRIANGLE_ENLARGEMENT_FACTOR; 

            var topX = _inputPoints.Max(p => p.X) - _inputPoints.Min(p => p.X); 
            var topY = _inputPoints.Max(p => p.Y) + SUPERTRIANGLE_ENLARGEMENT_FACTOR;
            
            
            var lowerLeftCorner = new XYPoint {X = lowerLeftX, Y = lowerLeftY};
            var lowerRightCorner = new XYPoint {X = lowerRightX, Y = lowerRightY};
            var pyramidTop = new XYPoint
            {
                X = topX,
                Y = topY
            };

            var a = new Edge(lowerLeftCorner, pyramidTop);
            var b = new Edge(pyramidTop, lowerRightCorner);
            var c = new Edge(lowerRightCorner, lowerLeftCorner);
            var superTriangle = new Triangle {Edges = new List<Edge> {a, b, c}};

            _superTriangle = superTriangle;
            _currentTriangles.Add(superTriangle);
            _triangulation.Add(superTriangle);
        }

        private List<Triangle> DetermineBadTriangles(XYPoint currentPoint)
        {
            var badTriangles = new List<Triangle>();

            foreach (var triangle in _currentTriangles)
            {
                if (triangle.IsWithinCircumCircle(currentPoint))
                    badTriangles.Add(triangle);
            }
            return badTriangles;
        }

        private Dictionary<Triangle, List<Edge>> DeterminePolygons(List<Triangle> badTriangles)
        {
            var polygonEdgesWithOriginalTriangles = new Dictionary<Triangle, List<Edge>>();

            foreach (var badTriangle in badTriangles)
            {
                polygonEdgesWithOriginalTriangles.Add(badTriangle, new List<Edge>());

                foreach (var edge in badTriangle.Edges)
                {
                    var edgeIsSharedWithOtherBadTriangles = badTriangles.Count(bt => bt.HasEdge(edge)) > 1;

                    if (edgeIsSharedWithOtherBadTriangles == false)
                        polygonEdgesWithOriginalTriangles[badTriangle].Add(edge);
                }
            }
            return polygonEdgesWithOriginalTriangles;
        }

        private void RemoveBadTrianglesFromTriangulation(List<Triangle> badTriangles)
        {
            for (var i = 0; i < badTriangles.Count; i++)
            {
                var currentBadTriangle = badTriangles[i];

                if (_currentTriangles.Contains(currentBadTriangle))
                {
                    _triangulation.Remove(currentBadTriangle);
                    _currentTriangles.Remove(currentBadTriangle);
                }
            }
        }

        private void CreateNewTriangles(XYPoint currentPoint,
            Dictionary<Triangle, List<Edge>> allTrianglesWithPolygonEdges)
        {
            foreach (var triangleWithPolygonEdges in allTrianglesWithPolygonEdges)
            {
                foreach (var edge in triangleWithPolygonEdges.Value)
                {
                    var a = new Edge(edge.StartPoint, currentPoint);
                    var b = new Edge(currentPoint, edge.EndPoint);
                    var c = new Edge(edge.EndPoint, edge.StartPoint);

                    var newTriangle = new Triangle {Edges = new List<Edge> {a, b, c}};

                    _newTrianglesForCurrentIteration.Add(newTriangle);
                    _currentTriangles.Add(newTriangle);
                    _triangulation.Add(newTriangle);
                }
            }
        }

        private void RemoveSuperTriangleVertices()
        {
            var uniqueSuperTriangleVertices = new HashSet<XYPoint>
            {
                _superTriangle.Edges[0].StartPoint,
                _superTriangle.Edges[1].StartPoint,
                _superTriangle.Edges[2].StartPoint,
            };

            var trianglesToBeRemoved = new List<Triangle>();
            foreach (var triangle in _triangulation)
            {
                foreach (var edge in triangle.Edges)
                {
                    var anyEdgesEqualToSuperTriangleVertices = uniqueSuperTriangleVertices.Contains(edge.StartPoint)
                                                               || uniqueSuperTriangleVertices.Contains(edge.EndPoint);

                    if (anyEdgesEqualToSuperTriangleVertices)
                        trianglesToBeRemoved.Add(triangle);
                }
            }

            trianglesToBeRemoved.ForEach(t => _triangulation.Remove(t));
        }


        private void RemoveIntersectingEdges(List<Triangle> trianglesToCheck, Edge subjectEdge)
        {
            var guiltyTrianglesAndEdgesWithTheirReplacements = new Dictionary<Triangle, List<Tuple<int, Edge>>>();

            //determining which edges are to be removed
            foreach (var triangle in trianglesToCheck)
            {
                guiltyTrianglesAndEdgesWithTheirReplacements.Add(triangle, new List<Tuple<int, Edge>>());

                for (var i = 0; i < triangle.Edges.Count; i++)
                {
                    var currEdge = triangle.Edges[i];

                    if (currEdge.CrossesThrough(subjectEdge))
                    {
                        guiltyTrianglesAndEdgesWithTheirReplacements[triangle].Add(new Tuple<int, Edge>(i, subjectEdge));
                    }
                }
            }

            //Replacing the edges
            foreach (var triangle in _triangulation)
            {
                if (guiltyTrianglesAndEdgesWithTheirReplacements.ContainsKey(triangle))
                {
                    for (int i = 0; i < guiltyTrianglesAndEdgesWithTheirReplacements[triangle].Count; i++)
                    {
                        var guiltyEdgeForThisTriangle = guiltyTrianglesAndEdgesWithTheirReplacements[triangle][i].Item1;
                        var replacingEdge = guiltyTrianglesAndEdgesWithTheirReplacements[triangle][i].Item2;

                        triangle.Edges.RemoveAt(guiltyEdgeForThisTriangle);
                        triangle.Edges.Add(replacingEdge);
                    }
                }
            }
        }
    }
}