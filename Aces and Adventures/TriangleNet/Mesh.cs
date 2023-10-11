using System;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Logging;
using TriangleNet.Meshing;
using TriangleNet.Meshing.Data;
using TriangleNet.Meshing.Iterators;
using TriangleNet.Tools;
using TriangleNet.Topology;

namespace TriangleNet;

public class Mesh : IMesh
{
	private IPredicates predicates;

	private ILog<LogItem> logger;

	private QualityMesher qualityMesher;

	private Stack<Otri> flipstack;

	internal TrianglePool triangles;

	internal Dictionary<int, SubSegment> subsegs;

	internal Dictionary<int, Vertex> vertices;

	internal int hash_vtx;

	internal int hash_seg;

	internal int hash_tri;

	internal List<Point> holes;

	internal List<RegionPointer> regions;

	internal Rectangle bounds;

	internal int invertices;

	internal int insegments;

	internal int undeads;

	internal int mesh_dim;

	internal int nextras;

	internal int hullsize;

	internal int steinerleft;

	internal bool checksegments;

	internal bool checkquality;

	internal Vertex infvertex1;

	internal Vertex infvertex2;

	internal Vertex infvertex3;

	internal TriangleLocator locator;

	internal Behavior behavior;

	internal NodeNumbering numbering;

	internal const int DUMMY = -1;

	internal TriangleNet.Topology.Triangle dummytri;

	internal SubSegment dummysub;

	public Rectangle Bounds => bounds;

	public ICollection<Vertex> Vertices => vertices.Values;

	public IList<Point> Holes => holes;

	public ICollection<TriangleNet.Topology.Triangle> Triangles => triangles;

	public ICollection<SubSegment> Segments => subsegs.Values;

	public IEnumerable<Edge> Edges
	{
		get
		{
			EdgeIterator e = new EdgeIterator(this);
			while (e.MoveNext())
			{
				yield return e.Current;
			}
		}
	}

	public int NumberOfInputPoints => invertices;

	public int NumberOfEdges => (3 * triangles.Count + hullsize) / 2;

	public bool IsPolygon => insegments > 0;

	public NodeNumbering CurrentNumbering => numbering;

	private void Initialize()
	{
		dummysub = new SubSegment();
		dummysub.hash = -1;
		dummysub.subsegs[0].seg = dummysub;
		dummysub.subsegs[1].seg = dummysub;
		dummytri = new TriangleNet.Topology.Triangle();
		dummytri.hash = (dummytri.id = -1);
		dummytri.neighbors[0].tri = dummytri;
		dummytri.neighbors[1].tri = dummytri;
		dummytri.neighbors[2].tri = dummytri;
		dummytri.subsegs[0].seg = dummysub;
		dummytri.subsegs[1].seg = dummysub;
		dummytri.subsegs[2].seg = dummysub;
	}

	public Mesh(Configuration config)
	{
		Initialize();
		logger = Log.Instance;
		behavior = new Behavior();
		vertices = new Dictionary<int, Vertex>();
		subsegs = new Dictionary<int, SubSegment>();
		triangles = config.TrianglePool();
		flipstack = new Stack<Otri>();
		holes = new List<Point>();
		regions = new List<RegionPointer>();
		steinerleft = -1;
		predicates = config.Predicates();
		locator = new TriangleLocator(this, predicates);
	}

	public void Refine(QualityOptions quality, bool delaunay = false)
	{
		invertices = vertices.Count;
		if (behavior.Poly)
		{
			insegments = (behavior.useSegments ? subsegs.Count : hullsize);
		}
		Reset();
		if (qualityMesher == null)
		{
			qualityMesher = new QualityMesher(this, new Configuration());
		}
		qualityMesher.Apply(quality, delaunay);
	}

	public void Renumber()
	{
		Renumber(NodeNumbering.Linear);
	}

	public void Renumber(NodeNumbering num)
	{
		if (num == numbering)
		{
			return;
		}
		int num2;
		switch (num)
		{
		case NodeNumbering.Linear:
			num2 = 0;
			foreach (Vertex value in vertices.Values)
			{
				value.id = num2++;
			}
			break;
		case NodeNumbering.CuthillMcKee:
		{
			int[] array = new CuthillMcKee().Renumber(this);
			foreach (Vertex value2 in vertices.Values)
			{
				value2.id = array[value2.id];
			}
			break;
		}
		}
		numbering = num;
		num2 = 0;
		foreach (TriangleNet.Topology.Triangle triangle in triangles)
		{
			triangle.id = num2++;
		}
	}

	internal void SetQualityMesher(QualityMesher qmesher)
	{
		qualityMesher = qmesher;
	}

	internal void CopyTo(Mesh target)
	{
		target.vertices = vertices;
		target.triangles = triangles;
		target.subsegs = subsegs;
		target.holes = holes;
		target.regions = regions;
		target.hash_vtx = hash_vtx;
		target.hash_seg = hash_seg;
		target.hash_tri = hash_tri;
		target.numbering = numbering;
		target.hullsize = hullsize;
	}

	private void ResetData()
	{
		vertices.Clear();
		triangles.Restart();
		subsegs.Clear();
		holes.Clear();
		regions.Clear();
		hash_vtx = 0;
		hash_seg = 0;
		hash_tri = 0;
		flipstack.Clear();
		hullsize = 0;
		Reset();
		locator.Reset();
	}

	private void Reset()
	{
		numbering = NodeNumbering.None;
		undeads = 0;
		checksegments = false;
		checkquality = false;
		Statistic.InCircleCount = 0L;
		Statistic.CounterClockwiseCount = 0L;
		Statistic.InCircleAdaptCount = 0L;
		Statistic.CounterClockwiseAdaptCount = 0L;
		Statistic.Orient3dCount = 0L;
		Statistic.HyperbolaCount = 0L;
		Statistic.CircleTopCount = 0L;
		Statistic.CircumcenterCount = 0L;
	}

	internal void TransferNodes(IList<Vertex> points)
	{
		invertices = points.Count;
		mesh_dim = 2;
		bounds = new Rectangle();
		if (invertices < 3)
		{
			logger.Error("Input must have at least three input vertices.", "Mesh.TransferNodes()");
			throw new Exception("Input must have at least three input vertices.");
		}
		Vertex vertex = points[0];
		int num = nextras;
		nextras = num;
		bool flag = vertex.id != points[1].id;
		foreach (Vertex point in points)
		{
			if (flag)
			{
				point.hash = point.id;
				hash_vtx = Math.Max(point.hash + 1, hash_vtx);
			}
			else
			{
				point.hash = (point.id = hash_vtx++);
			}
			vertices.Add(point.hash, point);
			bounds.Expand(point);
		}
	}

	internal void MakeVertexMap()
	{
		Otri tri = default(Otri);
		foreach (TriangleNet.Topology.Triangle triangle in triangles)
		{
			tri.tri = triangle;
			tri.orient = 0;
			while (tri.orient < 3)
			{
				tri.Org().tri = tri;
				tri.orient++;
			}
		}
	}

	internal void MakeTriangle(ref Otri newotri)
	{
		TriangleNet.Topology.Triangle triangle = triangles.Get();
		triangle.subsegs[0].seg = dummysub;
		triangle.subsegs[1].seg = dummysub;
		triangle.subsegs[2].seg = dummysub;
		triangle.neighbors[0].tri = dummytri;
		triangle.neighbors[1].tri = dummytri;
		triangle.neighbors[2].tri = dummytri;
		newotri.tri = triangle;
		newotri.orient = 0;
	}

	internal void MakeSegment(ref Osub newsubseg)
	{
		SubSegment subSegment = new SubSegment();
		subSegment.hash = hash_seg++;
		subSegment.subsegs[0].seg = dummysub;
		subSegment.subsegs[1].seg = dummysub;
		subSegment.triangles[0].tri = dummytri;
		subSegment.triangles[1].tri = dummytri;
		newsubseg.seg = subSegment;
		newsubseg.orient = 0;
		subsegs.Add(subSegment.hash, subSegment);
	}

	internal InsertVertexResult InsertVertex(Vertex newvertex, ref Otri searchtri, ref Osub splitseg, bool segmentflaws, bool triflaws)
	{
		Otri searchtri2 = default(Otri);
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		Otri ot4 = default(Otri);
		Otri ot5 = default(Otri);
		Otri newotri = default(Otri);
		Otri newotri2 = default(Otri);
		Otri newotri3 = default(Otri);
		Otri ot6 = default(Otri);
		Otri ot7 = default(Otri);
		Otri ot8 = default(Otri);
		Otri ot9 = default(Otri);
		Otri ot10 = default(Otri);
		Osub os = default(Osub);
		Osub os2 = default(Osub);
		Osub os3 = default(Osub);
		Osub os4 = default(Osub);
		Osub os5 = default(Osub);
		Osub os6 = default(Osub);
		Osub os7 = default(Osub);
		Osub os8 = default(Osub);
		LocateResult locateResult;
		if (splitseg.seg == null)
		{
			if (searchtri.tri.id == -1)
			{
				searchtri2.tri = dummytri;
				searchtri2.orient = 0;
				searchtri2.Sym();
				locateResult = locator.Locate(newvertex, ref searchtri2);
			}
			else
			{
				searchtri.Copy(ref searchtri2);
				locateResult = locator.PreciseLocate(newvertex, ref searchtri2, stopatsubsegment: true);
			}
		}
		else
		{
			searchtri.Copy(ref searchtri2);
			locateResult = LocateResult.OnEdge;
		}
		Vertex vertex3;
		Vertex vertex;
		Vertex vertex2;
		switch (locateResult)
		{
		case LocateResult.OnVertex:
			searchtri2.Copy(ref searchtri);
			locator.Update(ref searchtri2);
			return InsertVertexResult.Duplicate;
		case LocateResult.OnEdge:
		case LocateResult.Outside:
		{
			if (checksegments && splitseg.seg == null)
			{
				searchtri2.Pivot(ref os5);
				if (os5.seg.hash != -1)
				{
					if (segmentflaws)
					{
						bool flag = behavior.NoBisect != 2;
						if (flag && behavior.NoBisect == 1)
						{
							searchtri2.Sym(ref ot10);
							flag = ot10.tri.id != -1;
						}
						if (flag)
						{
							BadSubseg badSubseg = new BadSubseg();
							badSubseg.subseg = os5;
							badSubseg.org = os5.Org();
							badSubseg.dest = os5.Dest();
							qualityMesher.AddBadSubseg(badSubseg);
						}
					}
					searchtri2.Copy(ref searchtri);
					locator.Update(ref searchtri2);
					return InsertVertexResult.Violating;
				}
			}
			searchtri2.Lprev(ref ot3);
			ot3.Sym(ref ot7);
			searchtri2.Sym(ref ot5);
			bool flag2 = ot5.tri.id != -1;
			if (flag2)
			{
				ot5.Lnext();
				ot5.Sym(ref ot9);
				MakeTriangle(ref newotri3);
			}
			else
			{
				hullsize++;
			}
			MakeTriangle(ref newotri2);
			vertex = searchtri2.Org();
			vertex2 = searchtri2.Dest();
			vertex3 = searchtri2.Apex();
			newotri2.SetOrg(vertex3);
			newotri2.SetDest(vertex);
			newotri2.SetApex(newvertex);
			searchtri2.SetOrg(newvertex);
			newotri2.tri.label = ot3.tri.label;
			if (behavior.VarArea)
			{
				newotri2.tri.area = ot3.tri.area;
			}
			if (flag2)
			{
				Vertex dest = ot5.Dest();
				newotri3.SetOrg(vertex);
				newotri3.SetDest(dest);
				newotri3.SetApex(newvertex);
				ot5.SetOrg(newvertex);
				newotri3.tri.label = ot5.tri.label;
				if (behavior.VarArea)
				{
					newotri3.tri.area = ot5.tri.area;
				}
			}
			if (checksegments)
			{
				ot3.Pivot(ref os2);
				if (os2.seg.hash != -1)
				{
					ot3.SegDissolve(dummysub);
					newotri2.SegBond(ref os2);
				}
				if (flag2)
				{
					ot5.Pivot(ref os4);
					if (os4.seg.hash != -1)
					{
						ot5.SegDissolve(dummysub);
						newotri3.SegBond(ref os4);
					}
				}
			}
			newotri2.Bond(ref ot7);
			newotri2.Lprev();
			newotri2.Bond(ref ot3);
			newotri2.Lprev();
			if (flag2)
			{
				newotri3.Bond(ref ot9);
				newotri3.Lnext();
				newotri3.Bond(ref ot5);
				newotri3.Lnext();
				newotri3.Bond(ref newotri2);
			}
			if (splitseg.seg != null)
			{
				splitseg.SetDest(newvertex);
				Vertex segOrg = splitseg.SegOrg();
				Vertex segDest = splitseg.SegDest();
				splitseg.Sym();
				splitseg.Pivot(ref os7);
				InsertSubseg(ref newotri2, splitseg.seg.boundary);
				newotri2.Pivot(ref os8);
				os8.SetSegOrg(segOrg);
				os8.SetSegDest(segDest);
				splitseg.Bond(ref os8);
				os8.Sym();
				os8.Bond(ref os7);
				splitseg.Sym();
				if (newvertex.label == 0)
				{
					newvertex.label = splitseg.seg.boundary;
				}
			}
			if (checkquality)
			{
				flipstack.Clear();
				flipstack.Push(default(Otri));
				flipstack.Push(searchtri2);
			}
			searchtri2.Lnext();
			break;
		}
		default:
			searchtri2.Lnext(ref ot2);
			searchtri2.Lprev(ref ot3);
			ot2.Sym(ref ot6);
			ot3.Sym(ref ot7);
			MakeTriangle(ref newotri);
			MakeTriangle(ref newotri2);
			vertex = searchtri2.Org();
			vertex2 = searchtri2.Dest();
			vertex3 = searchtri2.Apex();
			newotri.SetOrg(vertex2);
			newotri.SetDest(vertex3);
			newotri.SetApex(newvertex);
			newotri2.SetOrg(vertex3);
			newotri2.SetDest(vertex);
			newotri2.SetApex(newvertex);
			searchtri2.SetApex(newvertex);
			newotri.tri.label = searchtri2.tri.label;
			newotri2.tri.label = searchtri2.tri.label;
			if (behavior.VarArea)
			{
				double area = searchtri2.tri.area;
				newotri.tri.area = area;
				newotri2.tri.area = area;
			}
			if (checksegments)
			{
				ot2.Pivot(ref os);
				if (os.seg.hash != -1)
				{
					ot2.SegDissolve(dummysub);
					newotri.SegBond(ref os);
				}
				ot3.Pivot(ref os2);
				if (os2.seg.hash != -1)
				{
					ot3.SegDissolve(dummysub);
					newotri2.SegBond(ref os2);
				}
			}
			newotri.Bond(ref ot6);
			newotri2.Bond(ref ot7);
			newotri.Lnext();
			newotri2.Lprev();
			newotri.Bond(ref newotri2);
			newotri.Lnext();
			ot2.Bond(ref newotri);
			newotri2.Lprev();
			ot3.Bond(ref newotri2);
			if (checkquality)
			{
				flipstack.Clear();
				flipstack.Push(searchtri2);
			}
			break;
		}
		InsertVertexResult result = InsertVertexResult.Successful;
		if (newvertex.tri.tri != null)
		{
			newvertex.tri.SetOrg(vertex);
			newvertex.tri.SetDest(vertex2);
			newvertex.tri.SetApex(vertex3);
		}
		Vertex vertex4 = searchtri2.Org();
		vertex = vertex4;
		vertex2 = searchtri2.Dest();
		while (true)
		{
			bool flag3 = true;
			if (checksegments)
			{
				searchtri2.Pivot(ref os6);
				if (os6.seg.hash != -1)
				{
					flag3 = false;
					if (segmentflaws && qualityMesher.CheckSeg4Encroach(ref os6) > 0)
					{
						result = InsertVertexResult.Encroaching;
					}
				}
			}
			if (flag3)
			{
				searchtri2.Sym(ref ot);
				if (ot.tri.id == -1)
				{
					flag3 = false;
				}
				else
				{
					Vertex vertex5 = ot.Apex();
					flag3 = ((!(vertex2 == infvertex1) && !(vertex2 == infvertex2) && !(vertex2 == infvertex3)) ? ((!(vertex == infvertex1) && !(vertex == infvertex2) && !(vertex == infvertex3)) ? (!(vertex5 == infvertex1) && !(vertex5 == infvertex2) && !(vertex5 == infvertex3) && predicates.InCircle(vertex2, newvertex, vertex, vertex5) > 0.0) : (predicates.CounterClockwise(vertex5, vertex2, newvertex) > 0.0)) : (predicates.CounterClockwise(newvertex, vertex, vertex5) > 0.0));
					if (flag3)
					{
						ot.Lprev(ref ot4);
						ot4.Sym(ref ot8);
						ot.Lnext(ref ot5);
						ot5.Sym(ref ot9);
						searchtri2.Lnext(ref ot2);
						ot2.Sym(ref ot6);
						searchtri2.Lprev(ref ot3);
						ot3.Sym(ref ot7);
						ot4.Bond(ref ot6);
						ot2.Bond(ref ot7);
						ot3.Bond(ref ot9);
						ot5.Bond(ref ot8);
						if (checksegments)
						{
							ot4.Pivot(ref os3);
							ot2.Pivot(ref os);
							ot3.Pivot(ref os2);
							ot5.Pivot(ref os4);
							if (os3.seg.hash == -1)
							{
								ot5.SegDissolve(dummysub);
							}
							else
							{
								ot5.SegBond(ref os3);
							}
							if (os.seg.hash == -1)
							{
								ot4.SegDissolve(dummysub);
							}
							else
							{
								ot4.SegBond(ref os);
							}
							if (os2.seg.hash == -1)
							{
								ot2.SegDissolve(dummysub);
							}
							else
							{
								ot2.SegBond(ref os2);
							}
							if (os4.seg.hash == -1)
							{
								ot3.SegDissolve(dummysub);
							}
							else
							{
								ot3.SegBond(ref os4);
							}
						}
						searchtri2.SetOrg(vertex5);
						searchtri2.SetDest(newvertex);
						searchtri2.SetApex(vertex);
						ot.SetOrg(newvertex);
						ot.SetDest(vertex5);
						ot.SetApex(vertex2);
						int label = Math.Min(ot.tri.label, searchtri2.tri.label);
						ot.tri.label = label;
						searchtri2.tri.label = label;
						if (behavior.VarArea)
						{
							double area = ((!(ot.tri.area <= 0.0) && !(searchtri2.tri.area <= 0.0)) ? (0.5 * (ot.tri.area + searchtri2.tri.area)) : (-1.0));
							ot.tri.area = area;
							searchtri2.tri.area = area;
						}
						if (checkquality)
						{
							flipstack.Push(searchtri2);
						}
						searchtri2.Lprev();
						vertex2 = vertex5;
					}
				}
			}
			if (!flag3)
			{
				if (triflaws)
				{
					qualityMesher.TestTriangle(ref searchtri2);
				}
				searchtri2.Lnext();
				searchtri2.Sym(ref ot10);
				if (vertex2 == vertex4 || ot10.tri.id == -1)
				{
					break;
				}
				ot10.Lnext(ref searchtri2);
				vertex = vertex2;
				vertex2 = searchtri2.Dest();
			}
		}
		searchtri2.Lnext(ref searchtri);
		Otri ot11 = default(Otri);
		searchtri2.Lnext(ref ot11);
		locator.Update(ref ot11);
		return result;
	}

	internal void InsertSubseg(ref Otri tri, int subsegmark)
	{
		Otri ot = default(Otri);
		Osub os = default(Osub);
		Vertex vertex = tri.Org();
		Vertex vertex2 = tri.Dest();
		if (vertex.label == 0)
		{
			vertex.label = subsegmark;
		}
		if (vertex2.label == 0)
		{
			vertex2.label = subsegmark;
		}
		tri.Pivot(ref os);
		if (os.seg.hash == -1)
		{
			MakeSegment(ref os);
			os.SetOrg(vertex2);
			os.SetDest(vertex);
			os.SetSegOrg(vertex2);
			os.SetSegDest(vertex);
			tri.SegBond(ref os);
			tri.Sym(ref ot);
			os.Sym();
			ot.SegBond(ref os);
			os.seg.boundary = subsegmark;
		}
		else if (os.seg.boundary == 0)
		{
			os.seg.boundary = subsegmark;
		}
	}

	internal void Flip(ref Otri flipedge)
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		Otri ot4 = default(Otri);
		Otri ot5 = default(Otri);
		Otri ot6 = default(Otri);
		Otri ot7 = default(Otri);
		Otri ot8 = default(Otri);
		Otri ot9 = default(Otri);
		Osub os = default(Osub);
		Osub os2 = default(Osub);
		Osub os3 = default(Osub);
		Osub os4 = default(Osub);
		Vertex apex = flipedge.Org();
		Vertex apex2 = flipedge.Dest();
		Vertex vertex = flipedge.Apex();
		flipedge.Sym(ref ot5);
		Vertex vertex2 = ot5.Apex();
		ot5.Lprev(ref ot3);
		ot3.Sym(ref ot8);
		ot5.Lnext(ref ot4);
		ot4.Sym(ref ot9);
		flipedge.Lnext(ref ot);
		ot.Sym(ref ot6);
		flipedge.Lprev(ref ot2);
		ot2.Sym(ref ot7);
		ot3.Bond(ref ot6);
		ot.Bond(ref ot7);
		ot2.Bond(ref ot9);
		ot4.Bond(ref ot8);
		if (checksegments)
		{
			ot3.Pivot(ref os3);
			ot.Pivot(ref os);
			ot2.Pivot(ref os2);
			ot4.Pivot(ref os4);
			if (os3.seg.hash == -1)
			{
				ot4.SegDissolve(dummysub);
			}
			else
			{
				ot4.SegBond(ref os3);
			}
			if (os.seg.hash == -1)
			{
				ot3.SegDissolve(dummysub);
			}
			else
			{
				ot3.SegBond(ref os);
			}
			if (os2.seg.hash == -1)
			{
				ot.SegDissolve(dummysub);
			}
			else
			{
				ot.SegBond(ref os2);
			}
			if (os4.seg.hash == -1)
			{
				ot2.SegDissolve(dummysub);
			}
			else
			{
				ot2.SegBond(ref os4);
			}
		}
		flipedge.SetOrg(vertex2);
		flipedge.SetDest(vertex);
		flipedge.SetApex(apex);
		ot5.SetOrg(vertex);
		ot5.SetDest(vertex2);
		ot5.SetApex(apex2);
	}

	internal void Unflip(ref Otri flipedge)
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		Otri ot4 = default(Otri);
		Otri ot5 = default(Otri);
		Otri ot6 = default(Otri);
		Otri ot7 = default(Otri);
		Otri ot8 = default(Otri);
		Otri ot9 = default(Otri);
		Osub os = default(Osub);
		Osub os2 = default(Osub);
		Osub os3 = default(Osub);
		Osub os4 = default(Osub);
		Vertex apex = flipedge.Org();
		Vertex apex2 = flipedge.Dest();
		Vertex vertex = flipedge.Apex();
		flipedge.Sym(ref ot5);
		Vertex vertex2 = ot5.Apex();
		ot5.Lprev(ref ot3);
		ot3.Sym(ref ot8);
		ot5.Lnext(ref ot4);
		ot4.Sym(ref ot9);
		flipedge.Lnext(ref ot);
		ot.Sym(ref ot6);
		flipedge.Lprev(ref ot2);
		ot2.Sym(ref ot7);
		ot3.Bond(ref ot9);
		ot.Bond(ref ot8);
		ot2.Bond(ref ot6);
		ot4.Bond(ref ot7);
		if (checksegments)
		{
			ot3.Pivot(ref os3);
			ot.Pivot(ref os);
			ot2.Pivot(ref os2);
			ot4.Pivot(ref os4);
			if (os3.seg.hash == -1)
			{
				ot.SegDissolve(dummysub);
			}
			else
			{
				ot.SegBond(ref os3);
			}
			if (os.seg.hash == -1)
			{
				ot2.SegDissolve(dummysub);
			}
			else
			{
				ot2.SegBond(ref os);
			}
			if (os2.seg.hash == -1)
			{
				ot4.SegDissolve(dummysub);
			}
			else
			{
				ot4.SegBond(ref os2);
			}
			if (os4.seg.hash == -1)
			{
				ot3.SegDissolve(dummysub);
			}
			else
			{
				ot3.SegBond(ref os4);
			}
		}
		flipedge.SetOrg(vertex);
		flipedge.SetDest(vertex2);
		flipedge.SetApex(apex2);
		ot5.SetOrg(vertex2);
		ot5.SetDest(vertex);
		ot5.SetApex(apex);
	}

	private void TriangulatePolygon(Otri firstedge, Otri lastedge, int edgecount, bool doflip, bool triflaws)
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		int num = 1;
		Vertex a = lastedge.Apex();
		Vertex b = firstedge.Dest();
		firstedge.Onext(ref ot2);
		Vertex c = ot2.Dest();
		ot2.Copy(ref ot);
		for (int i = 2; i <= edgecount - 2; i++)
		{
			ot.Onext();
			Vertex vertex = ot.Dest();
			if (predicates.InCircle(a, b, c, vertex) > 0.0)
			{
				ot.Copy(ref ot2);
				c = vertex;
				num = i;
			}
		}
		if (num > 1)
		{
			ot2.Oprev(ref ot3);
			TriangulatePolygon(firstedge, ot3, num + 1, doflip: true, triflaws);
		}
		if (num < edgecount - 2)
		{
			ot2.Sym(ref ot3);
			TriangulatePolygon(ot2, lastedge, edgecount - num, doflip: true, triflaws);
			ot3.Sym(ref ot2);
		}
		if (doflip)
		{
			Flip(ref ot2);
			if (triflaws)
			{
				ot2.Sym(ref ot);
				qualityMesher.TestTriangle(ref ot);
			}
		}
		ot2.Copy(ref lastedge);
	}

	internal void DeleteVertex(ref Otri deltri)
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		Otri ot4 = default(Otri);
		Otri ot5 = default(Otri);
		Otri ot6 = default(Otri);
		Otri ot7 = default(Otri);
		Otri ot8 = default(Otri);
		Osub os = default(Osub);
		Osub os2 = default(Osub);
		Vertex dyingvertex = deltri.Org();
		VertexDealloc(dyingvertex);
		deltri.Onext(ref ot);
		int num = 1;
		while (!deltri.Equals(ot))
		{
			num++;
			ot.Onext();
		}
		if (num > 3)
		{
			deltri.Onext(ref ot2);
			deltri.Oprev(ref ot3);
			TriangulatePolygon(ot2, ot3, num, doflip: false, behavior.NoBisect == 0);
		}
		deltri.Lprev(ref ot4);
		deltri.Dnext(ref ot5);
		ot5.Sym(ref ot7);
		ot4.Oprev(ref ot6);
		ot6.Sym(ref ot8);
		deltri.Bond(ref ot7);
		ot4.Bond(ref ot8);
		ot5.Pivot(ref os);
		if (os.seg.hash != -1)
		{
			deltri.SegBond(ref os);
		}
		ot6.Pivot(ref os2);
		if (os2.seg.hash != -1)
		{
			ot4.SegBond(ref os2);
		}
		Vertex org = ot5.Org();
		deltri.SetOrg(org);
		if (behavior.NoBisect == 0)
		{
			qualityMesher.TestTriangle(ref deltri);
		}
		TriangleDealloc(ot5.tri);
		TriangleDealloc(ot6.tri);
	}

	internal void UndoVertex()
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		Otri ot4 = default(Otri);
		Otri ot5 = default(Otri);
		Otri ot6 = default(Otri);
		Otri ot7 = default(Otri);
		Osub os = default(Osub);
		Osub os2 = default(Osub);
		Osub os3 = default(Osub);
		while (flipstack.Count > 0)
		{
			Otri flipedge = flipstack.Pop();
			if (flipstack.Count == 0)
			{
				flipedge.Dprev(ref ot);
				ot.Lnext();
				flipedge.Onext(ref ot2);
				ot2.Lprev();
				ot.Sym(ref ot4);
				ot2.Sym(ref ot5);
				Vertex apex = ot.Dest();
				flipedge.SetApex(apex);
				flipedge.Lnext();
				flipedge.Bond(ref ot4);
				ot.Pivot(ref os);
				flipedge.SegBond(ref os);
				flipedge.Lnext();
				flipedge.Bond(ref ot5);
				ot2.Pivot(ref os2);
				flipedge.SegBond(ref os2);
				TriangleDealloc(ot.tri);
				TriangleDealloc(ot2.tri);
			}
			else if (flipstack.Peek().tri == null)
			{
				flipedge.Lprev(ref ot7);
				ot7.Sym(ref ot2);
				ot2.Lnext();
				ot2.Sym(ref ot5);
				Vertex org = ot2.Dest();
				flipedge.SetOrg(org);
				ot7.Bond(ref ot5);
				ot2.Pivot(ref os2);
				ot7.SegBond(ref os2);
				TriangleDealloc(ot2.tri);
				flipedge.Sym(ref ot7);
				if (ot7.tri.id != -1)
				{
					ot7.Lnext();
					ot7.Dnext(ref ot3);
					ot3.Sym(ref ot6);
					ot7.SetOrg(org);
					ot7.Bond(ref ot6);
					ot3.Pivot(ref os3);
					ot7.SegBond(ref os3);
					TriangleDealloc(ot3.tri);
				}
				flipstack.Clear();
			}
			else
			{
				Unflip(ref flipedge);
			}
		}
	}

	internal void TriangleDealloc(TriangleNet.Topology.Triangle dyingtriangle)
	{
		Otri.Kill(dyingtriangle);
		triangles.Release(dyingtriangle);
	}

	internal void VertexDealloc(Vertex dyingvertex)
	{
		dyingvertex.type = VertexType.DeadVertex;
		vertices.Remove(dyingvertex.hash);
	}

	internal void SubsegDealloc(SubSegment dyingsubseg)
	{
		Osub.Kill(dyingsubseg);
		subsegs.Remove(dyingsubseg.hash);
	}
}
