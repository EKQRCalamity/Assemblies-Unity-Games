using System;
using TriangleNet.Geometry;
using TriangleNet.Topology.DCEL;
using TriangleNet.Voronoi;

namespace TriangleNet.Smoothing;

internal class VoronoiFactory : IVoronoiFactory
{
	private class ObjectPool<T> where T : class
	{
		private int index;

		private int count;

		private T[] pool;

		public int Count => count;

		public int Capacity
		{
			get
			{
				return pool.Length;
			}
			set
			{
				Resize(value);
			}
		}

		public ObjectPool(int capacity = 3)
		{
			index = 0;
			count = 0;
			pool = new T[capacity];
		}

		public ObjectPool(T[] pool)
		{
			index = 0;
			count = 0;
			this.pool = pool;
		}

		public bool TryGet(out T obj)
		{
			if (index < count)
			{
				obj = pool[index++];
				return true;
			}
			obj = null;
			return false;
		}

		public void Put(T obj)
		{
			int num = pool.Length;
			if (num <= count)
			{
				Resize(2 * num);
			}
			pool[count++] = obj;
			index++;
		}

		public void Release()
		{
			index = 0;
		}

		private void Resize(int size)
		{
			if (size > count)
			{
				Array.Resize(ref pool, size);
			}
		}
	}

	private ObjectPool<TriangleNet.Topology.DCEL.Vertex> vertices;

	private ObjectPool<HalfEdge> edges;

	private ObjectPool<Face> faces;

	public VoronoiFactory()
	{
		vertices = new ObjectPool<TriangleNet.Topology.DCEL.Vertex>();
		edges = new ObjectPool<HalfEdge>();
		faces = new ObjectPool<Face>();
	}

	public void Initialize(int vertexCount, int edgeCount, int faceCount)
	{
		vertices.Capacity = vertexCount;
		edges.Capacity = edgeCount;
		faces.Capacity = faceCount;
		for (int i = vertices.Count; i < vertexCount; i++)
		{
			vertices.Put(new TriangleNet.Topology.DCEL.Vertex(0.0, 0.0));
		}
		for (int j = edges.Count; j < edgeCount; j++)
		{
			edges.Put(new HalfEdge(null));
		}
		for (int k = faces.Count; k < faceCount; k++)
		{
			faces.Put(new Face(null));
		}
		Reset();
	}

	public void Reset()
	{
		vertices.Release();
		edges.Release();
		faces.Release();
	}

	public TriangleNet.Topology.DCEL.Vertex CreateVertex(double x, double y)
	{
		if (vertices.TryGet(out var obj))
		{
			obj.x = x;
			obj.y = y;
			obj.leaving = null;
			return obj;
		}
		obj = new TriangleNet.Topology.DCEL.Vertex(x, y);
		vertices.Put(obj);
		return obj;
	}

	public HalfEdge CreateHalfEdge(TriangleNet.Topology.DCEL.Vertex origin, Face face)
	{
		if (edges.TryGet(out var obj))
		{
			obj.origin = origin;
			obj.face = face;
			obj.next = null;
			obj.twin = null;
			if (face != null && face.edge == null)
			{
				face.edge = obj;
			}
			return obj;
		}
		obj = new HalfEdge(origin, face);
		edges.Put(obj);
		return obj;
	}

	public Face CreateFace(TriangleNet.Geometry.Vertex vertex)
	{
		if (faces.TryGet(out var obj))
		{
			obj.id = vertex.id;
			obj.generator = vertex;
			obj.edge = null;
			return obj;
		}
		obj = new Face(vertex);
		faces.Put(obj);
		return obj;
	}
}
