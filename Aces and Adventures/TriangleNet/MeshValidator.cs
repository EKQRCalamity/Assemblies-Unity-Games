using TriangleNet.Geometry;
using TriangleNet.Logging;
using TriangleNet.Topology;

namespace TriangleNet;

public static class MeshValidator
{
	private static RobustPredicates predicates = RobustPredicates.Default;

	public static bool IsConsistent(Mesh mesh)
	{
		Otri otri = default(Otri);
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		ILog<LogItem> instance = Log.Instance;
		bool noExact = Behavior.NoExact;
		Behavior.NoExact = false;
		int num = 0;
		foreach (TriangleNet.Topology.Triangle triangle2 in mesh.triangles)
		{
			TriangleNet.Topology.Triangle triangle = (otri.tri = triangle2);
			otri.orient = 0;
			while (otri.orient < 3)
			{
				Vertex vertex = otri.Org();
				Vertex vertex2 = otri.Dest();
				if (otri.orient == 0)
				{
					Vertex pc = otri.Apex();
					if (predicates.CounterClockwise(vertex, vertex2, pc) <= 0.0)
					{
						if (Log.Verbose)
						{
							instance.Warning($"Triangle is flat or inverted (ID {triangle.id}).", "MeshValidator.IsConsistent()");
						}
						num++;
					}
				}
				otri.Sym(ref ot);
				if (ot.tri.id != -1)
				{
					ot.Sym(ref ot2);
					if (otri.tri != ot2.tri || otri.orient != ot2.orient)
					{
						if (otri.tri == ot2.tri && Log.Verbose)
						{
							instance.Warning("Asymmetric triangle-triangle bond: (Right triangle, wrong orientation)", "MeshValidator.IsConsistent()");
						}
						num++;
					}
					Vertex vertex3 = ot.Org();
					Vertex vertex4 = ot.Dest();
					if (vertex != vertex4 || vertex2 != vertex3)
					{
						if (Log.Verbose)
						{
							instance.Warning("Mismatched edge coordinates between two triangles.", "MeshValidator.IsConsistent()");
						}
						num++;
					}
				}
				otri.orient++;
			}
		}
		mesh.MakeVertexMap();
		foreach (Vertex value in mesh.vertices.Values)
		{
			if (value.tri.tri == null && Log.Verbose)
			{
				instance.Warning("Vertex (ID " + value.id + ") not connected to mesh (duplicate input vertex?)", "MeshValidator.IsConsistent()");
			}
		}
		Behavior.NoExact = noExact;
		return num == 0;
	}

	public static bool IsDelaunay(Mesh mesh)
	{
		return IsDelaunay(mesh, constrained: false);
	}

	public static bool IsConstrainedDelaunay(Mesh mesh)
	{
		return IsDelaunay(mesh, constrained: true);
	}

	private static bool IsDelaunay(Mesh mesh, bool constrained)
	{
		Otri otri = default(Otri);
		Otri ot = default(Otri);
		Osub os = default(Osub);
		ILog<LogItem> instance = Log.Instance;
		bool noExact = Behavior.NoExact;
		Behavior.NoExact = false;
		int num = 0;
		Vertex infvertex = mesh.infvertex1;
		Vertex infvertex2 = mesh.infvertex2;
		Vertex infvertex3 = mesh.infvertex3;
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			otri.tri = triangle;
			otri.orient = 0;
			while (otri.orient < 3)
			{
				Vertex vertex = otri.Org();
				Vertex vertex2 = otri.Dest();
				Vertex vertex3 = otri.Apex();
				otri.Sym(ref ot);
				Vertex vertex4 = ot.Apex();
				bool flag = otri.tri.id < ot.tri.id && !Otri.IsDead(ot.tri) && ot.tri.id != -1 && vertex != infvertex && vertex != infvertex2 && vertex != infvertex3 && vertex2 != infvertex && vertex2 != infvertex2 && vertex2 != infvertex3 && vertex3 != infvertex && vertex3 != infvertex2 && vertex3 != infvertex3 && vertex4 != infvertex && vertex4 != infvertex2 && vertex4 != infvertex3;
				if (constrained && mesh.checksegments && flag)
				{
					otri.Pivot(ref os);
					if (os.seg.hash != -1)
					{
						flag = false;
					}
				}
				if (flag && predicates.NonRegular(vertex, vertex2, vertex3, vertex4) > 0.0)
				{
					if (Log.Verbose)
					{
						instance.Warning($"Non-regular pair of triangles found (IDs {otri.tri.id}/{ot.tri.id}).", "MeshValidator.IsDelaunay()");
					}
					num++;
				}
				otri.orient++;
			}
		}
		Behavior.NoExact = noExact;
		return num == 0;
	}
}
