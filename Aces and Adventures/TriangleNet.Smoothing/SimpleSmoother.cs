using System;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using TriangleNet.Topology.DCEL;
using TriangleNet.Voronoi;

namespace TriangleNet.Smoothing;

public class SimpleSmoother : ISmoother
{
	private TrianglePool pool;

	private Configuration config;

	private IVoronoiFactory factory;

	private ConstraintOptions options;

	public SimpleSmoother()
		: this(new VoronoiFactory())
	{
	}

	public SimpleSmoother(IVoronoiFactory factory)
	{
		this.factory = factory;
		pool = new TrianglePool();
		config = new Configuration(() => RobustPredicates.Default, () => pool.Restart());
		options = new ConstraintOptions
		{
			ConformingDelaunay = true
		};
	}

	public SimpleSmoother(IVoronoiFactory factory, Configuration config)
	{
		this.factory = factory;
		this.config = config;
		options = new ConstraintOptions
		{
			ConformingDelaunay = true
		};
	}

	public void Smooth(IMesh mesh)
	{
		Smooth(mesh, 10);
	}

	public void Smooth(IMesh mesh, int limit)
	{
		Mesh mesh2 = (Mesh)mesh;
		GenericMesher genericMesher = new GenericMesher(config);
		IPredicates predicates = config.Predicates();
		options.SegmentSplitting = mesh2.behavior.NoBisect;
		for (int i = 0; i < limit; i++)
		{
			Step(mesh2, factory, predicates);
			mesh2 = (Mesh)genericMesher.Triangulate(Rebuild(mesh2), options);
			factory.Reset();
		}
		mesh2.CopyTo((Mesh)mesh);
	}

	private void Step(Mesh mesh, IVoronoiFactory factory, IPredicates predicates)
	{
		foreach (Face face in new BoundedVoronoi(mesh, factory, predicates).Faces)
		{
			if (face.generator.label == 0)
			{
				Centroid(face, out var x, out var y);
				face.generator.x = x;
				face.generator.y = y;
			}
		}
	}

	private void Centroid(Face face, out double x, out double y)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		HalfEdge halfEdge = face.Edge;
		int iD = halfEdge.Next.ID;
		do
		{
			Point origin = halfEdge.Origin;
			Point origin2 = halfEdge.Twin.Origin;
			double num4 = origin.x * origin2.y - origin2.x * origin.y;
			num += num4;
			num2 += (origin2.x + origin.x) * num4;
			num3 += (origin2.y + origin.y) * num4;
			halfEdge = halfEdge.Next;
		}
		while (halfEdge.Next.ID != iD);
		x = num2 / (3.0 * num);
		y = num3 / (3.0 * num);
	}

	private Polygon Rebuild(Mesh mesh)
	{
		Polygon polygon = new Polygon(mesh.vertices.Count);
		foreach (TriangleNet.Geometry.Vertex value in mesh.vertices.Values)
		{
			value.type = VertexType.InputVertex;
			polygon.Points.Add(value);
		}
		List<ISegment> collection = new List<SubSegment>(mesh.subsegs.Values).ConvertAll((Converter<SubSegment, ISegment>)((SubSegment x) => x));
		polygon.Segments.AddRange(collection);
		polygon.Holes.AddRange(mesh.holes);
		polygon.Regions.AddRange(mesh.regions);
		return polygon;
	}
}
