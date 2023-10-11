using System.Collections.Generic;
using TriangleNet.Geometry;

namespace TriangleNet.Topology.DCEL;

public class DcelMesh
{
	protected List<Vertex> vertices;

	protected List<HalfEdge> edges;

	protected List<Face> faces;

	public List<Vertex> Vertices => vertices;

	public List<HalfEdge> HalfEdges => edges;

	public List<Face> Faces => faces;

	public IEnumerable<IEdge> Edges => EnumerateEdges();

	public DcelMesh()
		: this(initialize: true)
	{
	}

	protected DcelMesh(bool initialize)
	{
		if (initialize)
		{
			vertices = new List<Vertex>();
			edges = new List<HalfEdge>();
			faces = new List<Face>();
		}
	}

	public virtual bool IsConsistent(bool closed = true, int depth = 0)
	{
		foreach (Vertex vertex in vertices)
		{
			if (vertex.id >= 0)
			{
				if (vertex.leaving == null)
				{
					return false;
				}
				if (vertex.Leaving.Origin.id != vertex.id)
				{
					return false;
				}
			}
		}
		foreach (Face face in faces)
		{
			if (face.ID >= 0)
			{
				if (face.edge == null)
				{
					return false;
				}
				if (face.id != face.edge.face.id)
				{
					return false;
				}
			}
		}
		foreach (HalfEdge edge2 in edges)
		{
			if (edge2.id >= 0)
			{
				if (edge2.twin == null)
				{
					return false;
				}
				if (edge2.origin == null)
				{
					return false;
				}
				if (edge2.face == null)
				{
					return false;
				}
				if (closed && edge2.next == null)
				{
					return false;
				}
			}
		}
		foreach (HalfEdge edge3 in edges)
		{
			if (edge3.id < 0)
			{
				continue;
			}
			HalfEdge twin = edge3.twin;
			HalfEdge next = edge3.next;
			if (edge3.id != twin.twin.id)
			{
				return false;
			}
			if (closed)
			{
				if (next.origin.id != twin.origin.id)
				{
					return false;
				}
				if (next.twin.next.origin.id != edge3.twin.origin.id)
				{
					return false;
				}
			}
		}
		if (closed && depth > 0)
		{
			foreach (Face face2 in faces)
			{
				if (face2.id >= 0)
				{
					HalfEdge edge = face2.edge;
					HalfEdge next2 = edge.next;
					int id = edge.id;
					int num = 0;
					while (next2.id != id && num < depth)
					{
						next2 = next2.next;
						num++;
					}
					if (next2.id != id)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public void ResolveBoundaryEdges()
	{
		Dictionary<int, HalfEdge> dictionary = new Dictionary<int, HalfEdge>();
		foreach (HalfEdge edge in edges)
		{
			if (edge.twin == null)
			{
				HalfEdge halfEdge = (edge.twin = new HalfEdge(edge.next.origin, Face.Empty));
				halfEdge.twin = edge;
				dictionary.Add(halfEdge.origin.id, halfEdge);
			}
		}
		int count = edges.Count;
		foreach (HalfEdge value in dictionary.Values)
		{
			value.id = count++;
			value.next = dictionary[value.twin.origin.id];
			edges.Add(value);
		}
	}

	protected virtual IEnumerable<IEdge> EnumerateEdges()
	{
		List<IEdge> list = new List<IEdge>(edges.Count / 2);
		foreach (HalfEdge edge in edges)
		{
			HalfEdge twin = edge.twin;
			if (edge.id < twin.id)
			{
				list.Add(new Edge(edge.origin.id, twin.origin.id));
			}
		}
		return list;
	}
}
