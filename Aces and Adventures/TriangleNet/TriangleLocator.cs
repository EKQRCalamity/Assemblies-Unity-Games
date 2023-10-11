using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace TriangleNet;

public class TriangleLocator
{
	private TriangleSampler sampler;

	private Mesh mesh;

	private IPredicates predicates;

	internal Otri recenttri;

	public TriangleLocator(Mesh mesh)
		: this(mesh, RobustPredicates.Default)
	{
	}

	public TriangleLocator(Mesh mesh, IPredicates predicates)
	{
		this.mesh = mesh;
		this.predicates = predicates;
		sampler = new TriangleSampler(mesh);
	}

	public void Update(ref Otri otri)
	{
		otri.Copy(ref recenttri);
	}

	public void Reset()
	{
		sampler.Reset();
		recenttri.tri = null;
	}

	public LocateResult PreciseLocate(Point searchpoint, ref Otri searchtri, bool stopatsubsegment)
	{
		Otri ot = default(Otri);
		Osub os = default(Osub);
		Vertex vertex = searchtri.Org();
		Vertex vertex2 = searchtri.Dest();
		Vertex vertex3 = searchtri.Apex();
		while (true)
		{
			if (vertex3.x == searchpoint.x && vertex3.y == searchpoint.y)
			{
				searchtri.Lprev();
				return LocateResult.OnVertex;
			}
			double num = predicates.CounterClockwise(vertex, vertex3, searchpoint);
			double num2 = predicates.CounterClockwise(vertex3, vertex2, searchpoint);
			bool flag;
			if (num > 0.0)
			{
				flag = !(num2 > 0.0) || (vertex3.x - searchpoint.x) * (vertex2.x - vertex.x) + (vertex3.y - searchpoint.y) * (vertex2.y - vertex.y) > 0.0;
			}
			else
			{
				if (!(num2 > 0.0))
				{
					if (num == 0.0)
					{
						searchtri.Lprev();
						return LocateResult.OnEdge;
					}
					if (num2 == 0.0)
					{
						searchtri.Lnext();
						return LocateResult.OnEdge;
					}
					return LocateResult.InTriangle;
				}
				flag = false;
			}
			if (flag)
			{
				searchtri.Lprev(ref ot);
				vertex2 = vertex3;
			}
			else
			{
				searchtri.Lnext(ref ot);
				vertex = vertex3;
			}
			ot.Sym(ref searchtri);
			if (mesh.checksegments && stopatsubsegment)
			{
				ot.Pivot(ref os);
				if (os.seg.hash != -1)
				{
					ot.Copy(ref searchtri);
					return LocateResult.Outside;
				}
			}
			if (searchtri.tri.id == -1)
			{
				break;
			}
			vertex3 = searchtri.Apex();
		}
		ot.Copy(ref searchtri);
		return LocateResult.Outside;
	}

	public LocateResult Locate(Point searchpoint, ref Otri searchtri)
	{
		Otri otri = default(Otri);
		Vertex vertex = searchtri.Org();
		double num = (searchpoint.x - vertex.x) * (searchpoint.x - vertex.x) + (searchpoint.y - vertex.y) * (searchpoint.y - vertex.y);
		if (recenttri.tri != null && !Otri.IsDead(recenttri.tri))
		{
			vertex = recenttri.Org();
			if (vertex.x == searchpoint.x && vertex.y == searchpoint.y)
			{
				recenttri.Copy(ref searchtri);
				return LocateResult.OnVertex;
			}
			double num2 = (searchpoint.x - vertex.x) * (searchpoint.x - vertex.x) + (searchpoint.y - vertex.y) * (searchpoint.y - vertex.y);
			if (num2 < num)
			{
				recenttri.Copy(ref searchtri);
				num = num2;
			}
		}
		sampler.Update();
		foreach (TriangleNet.Topology.Triangle item in sampler)
		{
			otri.tri = item;
			if (!Otri.IsDead(otri.tri))
			{
				vertex = otri.Org();
				double num2 = (searchpoint.x - vertex.x) * (searchpoint.x - vertex.x) + (searchpoint.y - vertex.y) * (searchpoint.y - vertex.y);
				if (num2 < num)
				{
					otri.Copy(ref searchtri);
					num = num2;
				}
			}
		}
		vertex = searchtri.Org();
		Vertex vertex2 = searchtri.Dest();
		if (vertex.x == searchpoint.x && vertex.y == searchpoint.y)
		{
			return LocateResult.OnVertex;
		}
		if (vertex2.x == searchpoint.x && vertex2.y == searchpoint.y)
		{
			searchtri.Lnext();
			return LocateResult.OnVertex;
		}
		double num3 = predicates.CounterClockwise(vertex, vertex2, searchpoint);
		if (num3 < 0.0)
		{
			searchtri.Sym();
		}
		else if (num3 == 0.0 && vertex.x < searchpoint.x == searchpoint.x < vertex2.x && vertex.y < searchpoint.y == searchpoint.y < vertex2.y)
		{
			return LocateResult.OnEdge;
		}
		return PreciseLocate(searchpoint, ref searchtri, stopatsubsegment: false);
	}
}
