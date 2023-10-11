using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace TriangleNet.Meshing.Algorithm;

public class Incremental : ITriangulator
{
	private Mesh mesh;

	public IMesh Triangulate(IList<Vertex> points, Configuration config)
	{
		mesh = new Mesh(config);
		mesh.TransferNodes(points);
		Otri searchtri = default(Otri);
		GetBoundingBox();
		foreach (Vertex value in mesh.vertices.Values)
		{
			searchtri.tri = mesh.dummytri;
			Osub splitseg = default(Osub);
			if (mesh.InsertVertex(value, ref searchtri, ref splitseg, segmentflaws: false, triflaws: false) == InsertVertexResult.Duplicate)
			{
				if (Log.Verbose)
				{
					Log.Instance.Warning("A duplicate vertex appeared and was ignored.", "Incremental.Triangulate()");
				}
				value.type = VertexType.UndeadVertex;
				mesh.undeads++;
			}
		}
		mesh.hullsize = RemoveBox();
		return mesh;
	}

	private void GetBoundingBox()
	{
		Otri newotri = default(Otri);
		Rectangle bounds = mesh.bounds;
		double num = bounds.Width;
		if (bounds.Height > num)
		{
			num = bounds.Height;
		}
		if (num == 0.0)
		{
			num = 1.0;
		}
		mesh.infvertex1 = new Vertex(bounds.Left - 50.0 * num, bounds.Bottom - 40.0 * num);
		mesh.infvertex2 = new Vertex(bounds.Right + 50.0 * num, bounds.Bottom - 40.0 * num);
		mesh.infvertex3 = new Vertex(0.5 * (bounds.Left + bounds.Right), bounds.Top + 60.0 * num);
		mesh.MakeTriangle(ref newotri);
		newotri.SetOrg(mesh.infvertex1);
		newotri.SetDest(mesh.infvertex2);
		newotri.SetApex(mesh.infvertex3);
		mesh.dummytri.neighbors[0] = newotri;
	}

	private int RemoveBox()
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		Otri ot4 = default(Otri);
		Otri ot5 = default(Otri);
		Otri ot6 = default(Otri);
		bool flag = !mesh.behavior.Poly;
		ot4.tri = mesh.dummytri;
		ot4.orient = 0;
		ot4.Sym();
		ot4.Lprev(ref ot5);
		ot4.Lnext();
		ot4.Sym();
		ot4.Lprev(ref ot2);
		ot2.Sym();
		ot4.Lnext(ref ot3);
		ot3.Sym();
		if (ot3.tri.id == -1)
		{
			ot2.Lprev();
			ot2.Sym();
		}
		mesh.dummytri.neighbors[0] = ot2;
		int num = -2;
		while (!ot4.Equals(ot5))
		{
			num++;
			ot4.Lprev(ref ot6);
			ot6.Sym();
			if (flag && ot6.tri.id != -1)
			{
				Vertex vertex = ot6.Org();
				if (vertex.label == 0)
				{
					vertex.label = 1;
				}
			}
			ot6.Dissolve(mesh.dummytri);
			ot4.Lnext(ref ot);
			ot.Sym(ref ot4);
			mesh.TriangleDealloc(ot.tri);
			if (ot4.tri.id == -1)
			{
				ot6.Copy(ref ot4);
			}
		}
		mesh.TriangleDealloc(ot5.tri);
		return num;
	}
}
