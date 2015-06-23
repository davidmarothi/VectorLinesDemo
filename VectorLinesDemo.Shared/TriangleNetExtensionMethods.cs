using System.Collections.Generic;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNetMesh = TriangleNet.Mesh;

namespace VectorLinesDemo.Shared
{
    internal static class TriangleNetExtensionMethods
    {
        /// <summary>
        /// Add a polygon ring to the geometry and make it a hole.
        /// </summary>
        /// <remarks>
        /// WARNING: This works for convex polygons, but not for non-convex regions in general.
        /// </remarks>
        /// <param name="points">List of points which make up the hole.</param>
        /// <param name="mark">Common boundary mark for all segments of the hole.</param>
        public static void AddRingAsHole(this InputGeometry geometry, IEnumerable<TriangleNet.Geometry.Point> points, int mark = 0)
        {
            // Save the current number of points.
            int N = geometry.Count;
            int m = 0;

            foreach (var pt in points)
            {
                geometry.AddPoint(pt.X, pt.Y, pt.Boundary, pt.Attributes);
                m++;
            }

            for (int i = 0; i < m; i++)
            {
                geometry.AddSegment(N + i, N + ((i + 1) % m), mark);
            }

            //a lényeg az hogy kell egy a lyukon lévő pont,hogy jelezze hogy az egy lyuk poligon... a legegyszerübb trükk a következő, az egybefüggő (akár konkáv) poligont háromszögesítem, ezzel nincs gond, ezután kiválasztok egy testzőleges háromszöget, és veszem annak a középpontját..ez már tuti hogy a nagy poligon belső pontja
            TriangleNetMesh mesh = new TriangleNetMesh();
            var inputGeometry = new InputGeometry();
            inputGeometry.AddRing(points);
            mesh.Triangulate(inputGeometry);
            var firstTriangle = new List<Triangle>(mesh.Triangles)[0];

            double x = 0.0;
            double y = 0.0;
            for (int iii = 0; iii < 3; iii++)
            {
                var vertex = firstTriangle.GetVertex(iii);
                x += vertex.X;
                y += vertex.Y;
            }

            geometry.AddHole(x / 3, y / 3);
        }

        /// <summary>
        /// Add a polygon ring to the geometry.
        /// </summary>
        /// <param name="points">List of points which make up the polygon.</param>
        /// <param name="mark">Common boundary mark for all segments of the polygon.</param>
        public static void AddRing(this InputGeometry geometry,
                IEnumerable<Point> points, int mark = 0)
        {
            // Save the current number of points.
            int N = geometry.Count;

            int m = 0;
            foreach (var pt in points)
            {
                geometry.AddPoint(pt.X, pt.Y, pt.Boundary, pt.Attributes);
                m++;
            }

            for (int i = 0; i < m; i++)
            {
                geometry.AddSegment(N + i, N + ((i + 1) % m), mark);
            }
        }

        public static List<Vertex> GetTriangleList(this TriangleNetMesh mesh)
        {
            int meshTrianglesCount = mesh.Triangles.Count;
            List<Vertex> result = new List<Vertex>(meshTrianglesCount);

            foreach (Triangle triangle in mesh.Triangles)
            {
                result.Add(triangle.GetVertex(0));
                result.Add(triangle.GetVertex(1));
                result.Add(triangle.GetVertex(2));
            }

            return result;
        }
    }
}
