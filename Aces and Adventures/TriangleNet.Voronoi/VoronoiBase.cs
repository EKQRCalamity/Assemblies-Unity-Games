using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using TriangleNet.Topology.DCEL;

namespace TriangleNet.Voronoi;

public abstract class VoronoiBase : DcelMesh
{
	protected IPredicates predicates;

	protected IVoronoiFactory factory;

	protected List<HalfEdge> rays;

	protected VoronoiBase(Mesh mesh, IVoronoiFactory factory, IPredicates predicates, bool generate)
		: base(initialize: false)
	{
		this.factory = factory;
		this.predicates = predicates;
		if (generate)
		{
			Generate(mesh);
		}
	}

	protected void Generate(Mesh mesh)
	{
		mesh.Renumber();
		edges = new List<HalfEdge>();
		rays = new List<HalfEdge>();
		TriangleNet.Topology.DCEL.Vertex[] array = new TriangleNet.Topology.DCEL.Vertex[mesh.triangles.Count + mesh.hullsize];
		Face[] array2 = new Face[mesh.vertices.Count];
		if (factory == null)
		{
			factory = new DefaultVoronoiFactory();
		}
		factory.Initialize(array.Length, 2 * mesh.NumberOfEdges, array2.Length);
		List<HalfEdge>[] map = ComputeVertices(mesh, array);
		foreach (TriangleNet.Geometry.Vertex value in mesh.vertices.Values)
		{
			array2[value.id] = factory.CreateFace(value);
		}
		ComputeEdges(mesh, array, array2, map);
		ConnectEdges(map);
		vertices = new List<TriangleNet.Topology.DCEL.Vertex>(array);
		faces = new List<Face>(array2);
	}

	protected List<HalfEdge>[] ComputeVertices(Mesh mesh, TriangleNet.Topology.DCEL.Vertex[] vertices)
	{
		Otri otri = default(Otri);
		double xi = 0.0;
		double eta = 0.0;
		List<HalfEdge>[] array = new List<HalfEdge>[mesh.triangles.Count];
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			int id = triangle.id;
			otri.tri = triangle;
			Point point = predicates.FindCircumcenter(otri.Org(), otri.Dest(), otri.Apex(), ref xi, ref eta);
			TriangleNet.Topology.DCEL.Vertex vertex = factory.CreateVertex(point.x, point.y);
			vertex.id = id;
			vertices[id] = vertex;
			array[id] = new List<HalfEdge>();
		}
		return array;
	}

	protected void ComputeEdges(Mesh mesh, TriangleNet.Topology.DCEL.Vertex[] vertices, Face[] faces, List<HalfEdge>[] map)
	{
		Otri ot = default(Otri);
		int count = mesh.triangles.Count;
		int num = 0;
		int num2 = 0;
		Otri otri = default(Otri);
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			int id = triangle.id;
			otri.tri = triangle;
			for (int i = 0; i < 3; i++)
			{
				otri.orient = i;
				otri.Sym(ref ot);
				int id2 = ot.tri.id;
				if (id < id2 || id2 < 0)
				{
					TriangleNet.Geometry.Vertex vertex = otri.Org();
					TriangleNet.Geometry.Vertex vertex2 = otri.Dest();
					Face face = faces[vertex.id];
					Face face2 = faces[vertex2.id];
					TriangleNet.Topology.DCEL.Vertex vertex3 = vertices[id];
					TriangleNet.Topology.DCEL.Vertex vertex4;
					HalfEdge halfEdge;
					HalfEdge halfEdge2;
					if (id2 < 0)
					{
						double num3 = vertex2.y - vertex.y;
						double num4 = vertex.x - vertex2.x;
						vertex4 = factory.CreateVertex(vertex3.x + num3, vertex3.y + num4);
						vertex4.id = count + num++;
						vertices[vertex4.id] = vertex4;
						halfEdge = factory.CreateHalfEdge(vertex4, face);
						halfEdge2 = factory.CreateHalfEdge(vertex3, face2);
						face.edge = halfEdge;
						face.bounded = false;
						map[id].Add(halfEdge2);
						rays.Add(halfEdge2);
					}
					else
					{
						vertex4 = vertices[id2];
						halfEdge = factory.CreateHalfEdge(vertex4, face);
						halfEdge2 = factory.CreateHalfEdge(vertex3, face2);
						map[id2].Add(halfEdge);
						map[id].Add(halfEdge2);
					}
					vertex3.leaving = halfEdge2;
					vertex4.leaving = halfEdge;
					halfEdge.twin = halfEdge2;
					halfEdge2.twin = halfEdge;
					halfEdge.id = num2++;
					halfEdge2.id = num2++;
					edges.Add(halfEdge);
					edges.Add(halfEdge2);
				}
			}
		}
	}

	protected virtual void ConnectEdges(List<HalfEdge>[] map)
	{
		int num = map.Length;
		foreach (HalfEdge edge in edges)
		{
			int id = edge.face.generator.id;
			int id2 = edge.twin.origin.id;
			if (id2 >= num)
			{
				continue;
			}
			foreach (HalfEdge item in map[id2])
			{
				if (item.face.generator.id == id)
				{
					edge.next = item;
					break;
				}
			}
		}
	}

	protected override IEnumerable<IEdge> EnumerateEdges()
	{
		List<IEdge> list = new List<IEdge>(edges.Count / 2);
		foreach (HalfEdge edge in edges)
		{
			HalfEdge twin = edge.twin;
			if (twin == null)
			{
				list.Add(new Edge(edge.origin.id, edge.next.origin.id));
			}
			else if (edge.id < twin.id)
			{
				list.Add(new Edge(edge.origin.id, twin.origin.id));
			}
		}
		return list;
	}
}
