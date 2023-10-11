using TriangleNet.Geometry;
using TriangleNet.Tools;
using TriangleNet.Topology.DCEL;

namespace TriangleNet.Voronoi;

public class BoundedVoronoi : VoronoiBase
{
	private int offset;

	public BoundedVoronoi(Mesh mesh)
		: this(mesh, new DefaultVoronoiFactory(), RobustPredicates.Default)
	{
	}

	public BoundedVoronoi(Mesh mesh, IVoronoiFactory factory, IPredicates predicates)
		: base(mesh, factory, predicates, generate: true)
	{
		offset = vertices.Count;
		vertices.Capacity = offset + mesh.hullsize;
		PostProcess();
		ResolveBoundaryEdges();
	}

	private void PostProcess()
	{
		foreach (HalfEdge ray in rays)
		{
			HalfEdge twin = ray.twin;
			TriangleNet.Geometry.Vertex vertex = (TriangleNet.Geometry.Vertex)ray.face.generator;
			TriangleNet.Geometry.Vertex vertex2 = (TriangleNet.Geometry.Vertex)twin.face.generator;
			if (predicates.CounterClockwise(vertex, vertex2, ray.origin) <= 0.0)
			{
				HandleCase1(ray, vertex, vertex2);
			}
			else
			{
				HandleCase2(ray, vertex, vertex2);
			}
		}
	}

	private void HandleCase1(HalfEdge edge, TriangleNet.Geometry.Vertex v1, TriangleNet.Geometry.Vertex v2)
	{
		TriangleNet.Topology.DCEL.Vertex origin = edge.twin.origin;
		origin.x = (v1.x + v2.x) / 2.0;
		origin.y = (v1.y + v2.y) / 2.0;
		TriangleNet.Topology.DCEL.Vertex vertex = factory.CreateVertex(v1.x, v1.y);
		HalfEdge halfEdge = factory.CreateHalfEdge(edge.twin.origin, edge.face);
		HalfEdge halfEdge2 = factory.CreateHalfEdge(vertex, edge.face);
		edge.next = halfEdge;
		halfEdge.next = halfEdge2;
		halfEdge2.next = edge.face.edge;
		vertex.leaving = halfEdge2;
		edge.face.edge = halfEdge2;
		edges.Add(halfEdge);
		edges.Add(halfEdge2);
		halfEdge2.id = (halfEdge.id = edges.Count) + 1;
		vertex.id = offset++;
		vertices.Add(vertex);
	}

	private void HandleCase2(HalfEdge edge, TriangleNet.Geometry.Vertex v1, TriangleNet.Geometry.Vertex v2)
	{
		Point c = edge.origin;
		Point c2 = edge.twin.origin;
		HalfEdge next = edge.twin.next;
		HalfEdge next2 = next.twin.next;
		IntersectionHelper.IntersectSegments(v1, v2, next.origin, next.twin.origin, ref c2);
		IntersectionHelper.IntersectSegments(v1, v2, next2.origin, next2.twin.origin, ref c);
		next.twin.next = edge.twin;
		edge.twin.next = next2;
		edge.twin.face = next2.face;
		next.origin = edge.twin.origin;
		edge.twin.twin = null;
		edge.twin = null;
		TriangleNet.Topology.DCEL.Vertex vertex = factory.CreateVertex(v1.x, v1.y);
		HalfEdge halfEdge = (edge.next = factory.CreateHalfEdge(vertex, edge.face));
		halfEdge.next = edge.face.edge;
		edge.face.edge = halfEdge;
		edges.Add(halfEdge);
		halfEdge.id = edges.Count;
		vertex.id = offset++;
		vertices.Add(vertex);
	}
}
