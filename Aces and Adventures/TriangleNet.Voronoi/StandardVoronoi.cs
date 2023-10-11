using TriangleNet.Geometry;
using TriangleNet.Tools;
using TriangleNet.Topology.DCEL;

namespace TriangleNet.Voronoi;

public class StandardVoronoi : VoronoiBase
{
	public StandardVoronoi(Mesh mesh)
		: this(mesh, mesh.bounds, new DefaultVoronoiFactory(), RobustPredicates.Default)
	{
	}

	public StandardVoronoi(Mesh mesh, Rectangle box)
		: this(mesh, box, new DefaultVoronoiFactory(), RobustPredicates.Default)
	{
	}

	public StandardVoronoi(Mesh mesh, Rectangle box, IVoronoiFactory factory, IPredicates predicates)
		: base(mesh, factory, predicates, generate: true)
	{
		box.Expand(mesh.bounds);
		PostProcess(box);
	}

	private void PostProcess(Rectangle box)
	{
		foreach (HalfEdge ray in rays)
		{
			Point origin = ray.origin;
			Point c = ray.twin.origin;
			if (box.Contains(origin) || box.Contains(c))
			{
				IntersectionHelper.BoxRayIntersection(box, origin, c, ref c);
			}
		}
	}
}
