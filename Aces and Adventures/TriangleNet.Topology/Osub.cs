using TriangleNet.Geometry;

namespace TriangleNet.Topology;

public struct Osub
{
	internal SubSegment seg;

	internal int orient;

	public SubSegment Segment => seg;

	public override string ToString()
	{
		if (seg == null)
		{
			return "O-TID [null]";
		}
		return $"O-SID {seg.hash}";
	}

	public void Sym(ref Osub os)
	{
		os.seg = seg;
		os.orient = 1 - orient;
	}

	public void Sym()
	{
		orient = 1 - orient;
	}

	public void Pivot(ref Osub os)
	{
		os = seg.subsegs[orient];
	}

	internal void Pivot(ref Otri ot)
	{
		ot = seg.triangles[orient];
	}

	public void Next(ref Osub ot)
	{
		ot = seg.subsegs[1 - orient];
	}

	public void Next()
	{
		this = seg.subsegs[1 - orient];
	}

	public Vertex Org()
	{
		return seg.vertices[orient];
	}

	public Vertex Dest()
	{
		return seg.vertices[1 - orient];
	}

	internal void SetOrg(Vertex vertex)
	{
		seg.vertices[orient] = vertex;
	}

	internal void SetDest(Vertex vertex)
	{
		seg.vertices[1 - orient] = vertex;
	}

	internal Vertex SegOrg()
	{
		return seg.vertices[2 + orient];
	}

	internal Vertex SegDest()
	{
		return seg.vertices[3 - orient];
	}

	internal void SetSegOrg(Vertex vertex)
	{
		seg.vertices[2 + orient] = vertex;
	}

	internal void SetSegDest(Vertex vertex)
	{
		seg.vertices[3 - orient] = vertex;
	}

	internal void Bond(ref Osub os)
	{
		seg.subsegs[orient] = os;
		os.seg.subsegs[os.orient] = this;
	}

	internal void Dissolve(SubSegment dummy)
	{
		seg.subsegs[orient].seg = dummy;
	}

	internal bool Equal(Osub os)
	{
		if (seg == os.seg)
		{
			return orient == os.orient;
		}
		return false;
	}

	internal void TriDissolve(Triangle dummy)
	{
		seg.triangles[orient].tri = dummy;
	}

	internal static bool IsDead(SubSegment sub)
	{
		return sub.subsegs[0].seg == null;
	}

	internal static void Kill(SubSegment sub)
	{
		sub.subsegs[0].seg = null;
		sub.subsegs[1].seg = null;
	}
}
