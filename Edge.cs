using Dataformatter.Datamodels;

namespace MeshesGeneration.BowyerAlgorithm
{
    public class Edge
    {
        private double Length { get; set; }
        public XYPoint StartPoint { get; set; }
        public XYPoint EndPoint { get; set; }

        public Edge(XYPoint startPoint, XYPoint endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Length = startPoint.EuclideanDistance(endPoint);
        }

        public bool CrossesThrough(Edge anotherEdge)
        {
            return Intersects(anotherEdge) && LineGoesThroughEdge(anotherEdge);
        }

        public XYPoint GetMidPoint()
        {
            var firstPointDivision = (StartPoint.X + StartPoint.Y) / 2;
            var secondPointDivision = (EndPoint.X + EndPoint.Y) / 2; 
         
            return new XYPoint() { X = firstPointDivision, Y = secondPointDivision };
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            var edge = obj as Edge;
            if (edge == null)
            {
                return false;
            }

            return (edge.StartPoint.Equals(StartPoint) && edge.EndPoint.Equals(EndPoint))
                    ||
                   (edge.StartPoint.Equals(EndPoint) && edge.EndPoint.Equals(StartPoint));
        }

        public override string ToString()
        {
            return StartPoint + " to " + EndPoint;
        }

        private bool Intersects(Edge anotherEdge)
        {
            var a = StartPoint;
            var b = EndPoint;
            var c = anotherEdge.StartPoint;
            var d = anotherEdge.EndPoint;

            var aSide = (d.X - c.X) * (a.Y - c.Y) - (d.Y - c.Y) * (a.X - c.X) > 0;
            var bSide = (d.X - c.X) * (b.Y - c.Y) - (d.Y - c.Y) * (b.X - c.X) > 0;
            var cSide = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X) > 0;
            var dSide = (b.X - a.X) * (d.Y - a.Y) - (b.Y - a.Y) * (d.X - a.X) > 0;

            return aSide != bSide && cSide != dSide;
        }

        private bool LineGoesThroughEdge(Edge anotherEdge)
        {
            var interSectionPoint = GetIntersectionPoint(anotherEdge);
            var distanceToIntersectionPoint = StartPoint.EuclideanDistance(interSectionPoint);

            return distanceToIntersectionPoint > 0
                   && distanceToIntersectionPoint < Length;
        }

        public XYPoint GetIntersectionPoint(Edge anotherEdge)
        {
            var xOne = StartPoint.X;
            var yOne = StartPoint.Y;
            var xTwo = EndPoint.X;
            var yTwo = EndPoint.Y;
        
            var xThree = anotherEdge.StartPoint.X;
            var yThree = anotherEdge.StartPoint.Y;
            var xFour = anotherEdge.EndPoint.X;
            var yFour = anotherEdge.EndPoint.Y;

            var xDividend = (((xTwo*yOne) - (xOne*yTwo)) * (xFour - xThree)) - 
                            (((xFour*yThree) - (xThree*yFour)) * (xTwo - xOne));

            var xDivisor = ((xTwo - xOne) * (yFour - yThree)) - 
                           ((xFour - xThree) * (yTwo - yOne));

            var yDividend = (((xTwo*yOne) - (xOne*yTwo)) * (yFour - yThree)) - 
                            (((xFour*yThree) - (xThree*yFour)) * (yTwo - yOne));

            var yDivisor = ((xTwo - xOne) * (yFour - yThree)) - 
                           ((xFour - xThree) * (yTwo - yOne));
        
            return new XYPoint { X = xDividend/xDivisor, Y = yDividend/yDivisor};
        }
    }
}
