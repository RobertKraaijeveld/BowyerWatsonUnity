using System;
using System.Collections.Generic;
using Dataformatter.Datamodels;

using UnityEngine;

namespace MeshesGeneration.BowyerAlgorithm
{
    public class Triangle
    {
        public List<Edge> Edges = new List<Edge>();

        public bool IsWithinCircumCircle(XYPoint point)
        {
            var currentCircumCircle = this.GetCircumCircle();
            
            var circumcenterX = currentCircumCircle.Center.x;
            var circumcenterY = currentCircumCircle.Center.y; 
             

            return Math.Pow((point.X - circumcenterX), 2) + Math.Pow((point.Y - circumcenterY), 2) 
                   < Math.Pow(currentCircumCircle.Radius, 2);
        }

        public Circle GetCircumCircle()
        {
            // lines from a to b and a to c
            var A = new Vector3(this.Edges[0].StartPoint.X, this.Edges[0].StartPoint.Y); 
            var B = new Vector3(this.Edges[0].EndPoint.X, this.Edges[0].EndPoint.Y);
            var C = new Vector3(this.Edges[1].EndPoint.X, this.Edges[1].EndPoint.Y);

            var AB = B - A;
            var AC = C - A;

            // perpendicular vector on triangle
            var N = Vector3.Normalize(Vector3.Cross(AB, AC));

            // find the points halfway on AB and AC
            var halfAB = A + AB * 0.5f;
            var halfAC = A + AC * 0.5f;

            // build vectors perpendicular to ab and ac
            var perpAB = Vector3.Cross(AB, N);
            var perpAC = Vector3.Cross(AC, N);

            var center = LineLineIntersection(halfAB, perpAB, halfAC, perpAC);

            // the radius is the distance between center and any point
            // distance(A, B) = length(A-B)
            var radius = Vector3.Distance(center, A);

            return new Circle() { Center = center, Radius = radius };
        }

        public bool HasEdge(Edge anotherEdge)
        {
            for (var i = 0; i < Edges.Count; i++)
            {
                var currentEdge = Edges[i];

                if (currentEdge.Equals(anotherEdge))
                    return true;
            }
            return false;
        }

        public void ReplaceEdge(Edge newEdge, int insertionIndex)
        {
            this.Edges[insertionIndex] = newEdge;
        }

        public override string ToString()
        {
            var result = " | ";
            foreach (var edge in Edges)
                result += edge + " ";

            result += " | ";
            return result;
        }

        private Vector3 LineLineIntersection(Vector3 originD, Vector3 directionD, Vector3 originE, Vector3 directionE)
        {
            directionD.Normalize();
            directionE.Normalize();
            var N = Vector3.Cross(directionD, directionE);
            var SR = originD - originE;
            var absX = Math.Abs(N.x);
            var absY = Math.Abs(N.y);
            var absZ = Math.Abs(N.z);
            float t;
            if (absZ > absX && absZ > absY)
            {
                t = (SR.x * directionE.y - SR.y * directionE.x) / N.z;
            }
            else if (absX > absY)
            {
                t = (SR.y * directionE.z - SR.z * directionE.y) / N.x;
            }
            else
            {
                t = (SR.z * directionE.x - SR.x * directionE.z) / N.y;
            }
            return originD - t * directionD;
        }
    }
}