using TriangleNet.Geometry;

namespace TriangleNet.Topology;

public struct Otri
{
	internal Triangle tri;

	internal int orient;

	private static readonly int[] plus1Mod3 = new int[3] { 1, 2, 0 };

	private static readonly int[] minus1Mod3 = new int[3] { 2, 0, 1 };

	public Triangle Triangle
	{
		get
		{
			return tri;
		}
		set
		{
			tri = value;
		}
	}

	public override string ToString()
	{
		if (tri == null)
		{
			return "O-TID [null]";
		}
		return $"O-TID {tri.hash}";
	}

	public void Sym(ref Otri ot)
	{
		ot.tri = tri.neighbors[orient].tri;
		ot.orient = tri.neighbors[orient].orient;
	}

	public void Sym()
	{
		int num = orient;
		orient = tri.neighbors[num].orient;
		tri = tri.neighbors[num].tri;
	}

	public void Lnext(ref Otri ot)
	{
		ot.tri = tri;
		ot.orient = plus1Mod3[orient];
	}

	public void Lnext()
	{
		orient = plus1Mod3[orient];
	}

	public void Lprev(ref Otri ot)
	{
		ot.tri = tri;
		ot.orient = minus1Mod3[orient];
	}

	public void Lprev()
	{
		orient = minus1Mod3[orient];
	}

	public void Onext(ref Otri ot)
	{
		ot.tri = tri;
		ot.orient = minus1Mod3[orient];
		int num = ot.orient;
		ot.orient = ot.tri.neighbors[num].orient;
		ot.tri = ot.tri.neighbors[num].tri;
	}

	public void Onext()
	{
		orient = minus1Mod3[orient];
		int num = orient;
		orient = tri.neighbors[num].orient;
		tri = tri.neighbors[num].tri;
	}

	public void Oprev(ref Otri ot)
	{
		ot.tri = tri.neighbors[orient].tri;
		ot.orient = tri.neighbors[orient].orient;
		ot.orient = plus1Mod3[ot.orient];
	}

	public void Oprev()
	{
		int num = orient;
		orient = tri.neighbors[num].orient;
		tri = tri.neighbors[num].tri;
		orient = plus1Mod3[orient];
	}

	public void Dnext(ref Otri ot)
	{
		ot.tri = tri.neighbors[orient].tri;
		ot.orient = tri.neighbors[orient].orient;
		ot.orient = minus1Mod3[ot.orient];
	}

	public void Dnext()
	{
		int num = orient;
		orient = tri.neighbors[num].orient;
		tri = tri.neighbors[num].tri;
		orient = minus1Mod3[orient];
	}

	public void Dprev(ref Otri ot)
	{
		ot.tri = tri;
		ot.orient = plus1Mod3[orient];
		int num = ot.orient;
		ot.orient = ot.tri.neighbors[num].orient;
		ot.tri = ot.tri.neighbors[num].tri;
	}

	public void Dprev()
	{
		orient = plus1Mod3[orient];
		int num = orient;
		orient = tri.neighbors[num].orient;
		tri = tri.neighbors[num].tri;
	}

	public void Rnext(ref Otri ot)
	{
		ot.tri = tri.neighbors[orient].tri;
		ot.orient = tri.neighbors[orient].orient;
		ot.orient = plus1Mod3[ot.orient];
		int num = ot.orient;
		ot.orient = ot.tri.neighbors[num].orient;
		ot.tri = ot.tri.neighbors[num].tri;
	}

	public void Rnext()
	{
		int num = orient;
		orient = tri.neighbors[num].orient;
		tri = tri.neighbors[num].tri;
		orient = plus1Mod3[orient];
		num = orient;
		orient = tri.neighbors[num].orient;
		tri = tri.neighbors[num].tri;
	}

	public void Rprev(ref Otri ot)
	{
		ot.tri = tri.neighbors[orient].tri;
		ot.orient = tri.neighbors[orient].orient;
		ot.orient = minus1Mod3[ot.orient];
		int num = ot.orient;
		ot.orient = ot.tri.neighbors[num].orient;
		ot.tri = ot.tri.neighbors[num].tri;
	}

	public void Rprev()
	{
		int num = orient;
		orient = tri.neighbors[num].orient;
		tri = tri.neighbors[num].tri;
		orient = minus1Mod3[orient];
		num = orient;
		orient = tri.neighbors[num].orient;
		tri = tri.neighbors[num].tri;
	}

	public Vertex Org()
	{
		return tri.vertices[plus1Mod3[orient]];
	}

	public Vertex Dest()
	{
		return tri.vertices[minus1Mod3[orient]];
	}

	public Vertex Apex()
	{
		return tri.vertices[orient];
	}

	public void Copy(ref Otri ot)
	{
		ot.tri = tri;
		ot.orient = orient;
	}

	public bool Equals(Otri ot)
	{
		if (tri == ot.tri)
		{
			return orient == ot.orient;
		}
		return false;
	}

	internal void SetOrg(Vertex v)
	{
		tri.vertices[plus1Mod3[orient]] = v;
	}

	internal void SetDest(Vertex v)
	{
		tri.vertices[minus1Mod3[orient]] = v;
	}

	internal void SetApex(Vertex v)
	{
		tri.vertices[orient] = v;
	}

	internal void Bond(ref Otri ot)
	{
		tri.neighbors[orient].tri = ot.tri;
		tri.neighbors[orient].orient = ot.orient;
		ot.tri.neighbors[ot.orient].tri = tri;
		ot.tri.neighbors[ot.orient].orient = orient;
	}

	internal void Dissolve(Triangle dummy)
	{
		tri.neighbors[orient].tri = dummy;
		tri.neighbors[orient].orient = 0;
	}

	internal void Infect()
	{
		tri.infected = true;
	}

	internal void Uninfect()
	{
		tri.infected = false;
	}

	internal bool IsInfected()
	{
		return tri.infected;
	}

	internal void Pivot(ref Osub os)
	{
		os = tri.subsegs[orient];
	}

	internal void SegBond(ref Osub os)
	{
		tri.subsegs[orient] = os;
		os.seg.triangles[os.orient] = this;
	}

	internal void SegDissolve(SubSegment dummy)
	{
		tri.subsegs[orient].seg = dummy;
	}

	internal static bool IsDead(Triangle tria)
	{
		return tria.neighbors[0].tri == null;
	}

	internal static void Kill(Triangle tri)
	{
		tri.neighbors[0].tri = null;
		tri.neighbors[2].tri = null;
	}
}
