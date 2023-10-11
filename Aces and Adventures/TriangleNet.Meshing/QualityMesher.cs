using System;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Logging;
using TriangleNet.Meshing.Data;
using TriangleNet.Tools;
using TriangleNet.Topology;

namespace TriangleNet.Meshing;

internal class QualityMesher
{
	private IPredicates predicates;

	private Queue<BadSubseg> badsubsegs;

	private BadTriQueue queue;

	private Mesh mesh;

	private Behavior behavior;

	private NewLocation newLocation;

	private ILog<LogItem> logger;

	private TriangleNet.Topology.Triangle newvertex_tri;

	public QualityMesher(Mesh mesh, Configuration config)
	{
		logger = Log.Instance;
		badsubsegs = new Queue<BadSubseg>();
		queue = new BadTriQueue();
		this.mesh = mesh;
		predicates = config.Predicates();
		behavior = mesh.behavior;
		newLocation = new NewLocation(mesh, predicates);
		newvertex_tri = new TriangleNet.Topology.Triangle();
	}

	public void Apply(QualityOptions quality, bool delaunay = false)
	{
		if (quality != null)
		{
			behavior.Quality = true;
			behavior.MinAngle = quality.MinimumAngle;
			behavior.MaxAngle = quality.MaximumAngle;
			behavior.MaxArea = quality.MaximumArea;
			behavior.UserTest = quality.UserTest;
			behavior.VarArea = quality.VariableArea;
			behavior.ConformingDelaunay = behavior.ConformingDelaunay || delaunay;
			mesh.steinerleft = ((quality.SteinerPoints == 0) ? (-1) : quality.SteinerPoints);
		}
		if (!behavior.Poly)
		{
			behavior.VarArea = false;
		}
		mesh.infvertex1 = null;
		mesh.infvertex2 = null;
		mesh.infvertex3 = null;
		if (behavior.useSegments)
		{
			mesh.checksegments = true;
		}
		if (behavior.Quality && mesh.triangles.Count > 0)
		{
			EnforceQuality();
		}
	}

	public void AddBadSubseg(BadSubseg badseg)
	{
		badsubsegs.Enqueue(badseg);
	}

	public int CheckSeg4Encroach(ref Osub testsubseg)
	{
		Otri ot = default(Otri);
		Osub os = default(Osub);
		int num = 0;
		int num2 = 0;
		Vertex vertex = testsubseg.Org();
		Vertex vertex2 = testsubseg.Dest();
		testsubseg.Pivot(ref ot);
		if (ot.tri.id != -1)
		{
			num2++;
			Vertex vertex3 = ot.Apex();
			double num3 = (vertex.x - vertex3.x) * (vertex2.x - vertex3.x) + (vertex.y - vertex3.y) * (vertex2.y - vertex3.y);
			if (num3 < 0.0 && (behavior.ConformingDelaunay || num3 * num3 >= (2.0 * behavior.goodAngle - 1.0) * (2.0 * behavior.goodAngle - 1.0) * ((vertex.x - vertex3.x) * (vertex.x - vertex3.x) + (vertex.y - vertex3.y) * (vertex.y - vertex3.y)) * ((vertex2.x - vertex3.x) * (vertex2.x - vertex3.x) + (vertex2.y - vertex3.y) * (vertex2.y - vertex3.y))))
			{
				num = 1;
			}
		}
		testsubseg.Sym(ref os);
		os.Pivot(ref ot);
		if (ot.tri.id != -1)
		{
			num2++;
			Vertex vertex3 = ot.Apex();
			double num3 = (vertex.x - vertex3.x) * (vertex2.x - vertex3.x) + (vertex.y - vertex3.y) * (vertex2.y - vertex3.y);
			if (num3 < 0.0 && (behavior.ConformingDelaunay || num3 * num3 >= (2.0 * behavior.goodAngle - 1.0) * (2.0 * behavior.goodAngle - 1.0) * ((vertex.x - vertex3.x) * (vertex.x - vertex3.x) + (vertex.y - vertex3.y) * (vertex.y - vertex3.y)) * ((vertex2.x - vertex3.x) * (vertex2.x - vertex3.x) + (vertex2.y - vertex3.y) * (vertex2.y - vertex3.y))))
			{
				num += 2;
			}
		}
		if (num > 0 && (behavior.NoBisect == 0 || (behavior.NoBisect == 1 && num2 == 2)))
		{
			BadSubseg badSubseg = new BadSubseg();
			if (num == 1)
			{
				badSubseg.subseg = testsubseg;
				badSubseg.org = vertex;
				badSubseg.dest = vertex2;
			}
			else
			{
				badSubseg.subseg = os;
				badSubseg.org = vertex2;
				badSubseg.dest = vertex;
			}
			badsubsegs.Enqueue(badSubseg);
		}
		return num;
	}

	public void TestTriangle(ref Otri testtri)
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Osub os = default(Osub);
		Vertex vertex = testtri.Org();
		Vertex vertex2 = testtri.Dest();
		Vertex vertex3 = testtri.Apex();
		double num = vertex.x - vertex2.x;
		double num2 = vertex.y - vertex2.y;
		double num3 = vertex2.x - vertex3.x;
		double num4 = vertex2.y - vertex3.y;
		double num5 = vertex3.x - vertex.x;
		double num6 = vertex3.y - vertex.y;
		double num7 = num * num;
		double num8 = num2 * num2;
		double num9 = num3 * num3;
		double num10 = num4 * num4;
		double num11 = num5 * num5;
		double num12 = num6 * num6;
		double num13 = num7 + num8;
		double num14 = num9 + num10;
		double num15 = num11 + num12;
		double minedge;
		Vertex vertex4;
		Vertex vertex5;
		double num16;
		if (num13 < num14 && num13 < num15)
		{
			minedge = num13;
			num16 = num3 * num5 + num4 * num6;
			num16 = num16 * num16 / (num14 * num15);
			vertex4 = vertex;
			vertex5 = vertex2;
			testtri.Copy(ref ot);
		}
		else if (num14 < num15)
		{
			minedge = num14;
			num16 = num * num5 + num2 * num6;
			num16 = num16 * num16 / (num13 * num15);
			vertex4 = vertex2;
			vertex5 = vertex3;
			testtri.Lnext(ref ot);
		}
		else
		{
			minedge = num15;
			num16 = num * num3 + num2 * num4;
			num16 = num16 * num16 / (num13 * num14);
			vertex4 = vertex3;
			vertex5 = vertex;
			testtri.Lprev(ref ot);
		}
		if (behavior.VarArea || behavior.fixedArea || behavior.UserTest != null)
		{
			double num17 = 0.5 * (num * num4 - num2 * num3);
			if (behavior.fixedArea && num17 > behavior.MaxArea)
			{
				queue.Enqueue(ref testtri, minedge, vertex3, vertex, vertex2);
				return;
			}
			if (behavior.VarArea && num17 > testtri.tri.area && testtri.tri.area > 0.0)
			{
				queue.Enqueue(ref testtri, minedge, vertex3, vertex, vertex2);
				return;
			}
			if (behavior.UserTest != null && behavior.UserTest(testtri.tri, num17))
			{
				queue.Enqueue(ref testtri, minedge, vertex3, vertex, vertex2);
				return;
			}
		}
		double num18 = ((num13 > num14 && num13 > num15) ? ((num14 + num15 - num13) / (2.0 * Math.Sqrt(num14 * num15))) : ((!(num14 > num15)) ? ((num13 + num14 - num15) / (2.0 * Math.Sqrt(num13 * num14))) : ((num13 + num15 - num14) / (2.0 * Math.Sqrt(num13 * num15)))));
		if (!(num16 > behavior.goodAngle) && (!(num18 < behavior.maxGoodAngle) || behavior.MaxAngle == 0.0))
		{
			return;
		}
		if (vertex4.type == VertexType.SegmentVertex && vertex5.type == VertexType.SegmentVertex)
		{
			ot.Pivot(ref os);
			if (os.seg.hash == -1)
			{
				ot.Copy(ref ot2);
				do
				{
					ot.Oprev();
					ot.Pivot(ref os);
				}
				while (os.seg.hash == -1);
				Vertex vertex6 = os.SegOrg();
				Vertex vertex7 = os.SegDest();
				do
				{
					ot2.Dnext();
					ot2.Pivot(ref os);
				}
				while (os.seg.hash == -1);
				Vertex vertex8 = os.SegOrg();
				Vertex vertex9 = os.SegDest();
				Vertex vertex10 = null;
				if (vertex7.x == vertex8.x && vertex7.y == vertex8.y)
				{
					vertex10 = vertex7;
				}
				else if (vertex6.x == vertex9.x && vertex6.y == vertex9.y)
				{
					vertex10 = vertex6;
				}
				if (vertex10 != null)
				{
					double num19 = (vertex4.x - vertex10.x) * (vertex4.x - vertex10.x) + (vertex4.y - vertex10.y) * (vertex4.y - vertex10.y);
					double num20 = (vertex5.x - vertex10.x) * (vertex5.x - vertex10.x) + (vertex5.y - vertex10.y) * (vertex5.y - vertex10.y);
					if (num19 < 1.001 * num20 && num19 > 0.999 * num20)
					{
						return;
					}
				}
			}
		}
		queue.Enqueue(ref testtri, minedge, vertex3, vertex, vertex2);
	}

	private void TallyEncs()
	{
		Osub testsubseg = default(Osub);
		testsubseg.orient = 0;
		foreach (SubSegment value in mesh.subsegs.Values)
		{
			testsubseg.seg = value;
			CheckSeg4Encroach(ref testsubseg);
		}
	}

	private void SplitEncSegs(bool triflaws)
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Osub os = default(Osub);
		Osub osub = default(Osub);
		while (badsubsegs.Count > 0 && mesh.steinerleft != 0)
		{
			BadSubseg badSubseg = badsubsegs.Dequeue();
			osub = badSubseg.subseg;
			Vertex vertex = osub.Org();
			Vertex vertex2 = osub.Dest();
			if (!Osub.IsDead(osub.seg) && vertex == badSubseg.org && vertex2 == badSubseg.dest)
			{
				osub.Pivot(ref ot);
				ot.Lnext(ref ot2);
				ot2.Pivot(ref os);
				bool flag = os.seg.hash != -1;
				ot2.Lnext();
				ot2.Pivot(ref os);
				bool flag2 = os.seg.hash != -1;
				if (!behavior.ConformingDelaunay && !flag && !flag2)
				{
					Vertex vertex3 = ot.Apex();
					while (vertex3.type == VertexType.FreeVertex && (vertex.x - vertex3.x) * (vertex2.x - vertex3.x) + (vertex.y - vertex3.y) * (vertex2.y - vertex3.y) < 0.0)
					{
						mesh.DeleteVertex(ref ot2);
						osub.Pivot(ref ot);
						vertex3 = ot.Apex();
						ot.Lprev(ref ot2);
					}
				}
				ot.Sym(ref ot2);
				if (ot2.tri.id != -1)
				{
					ot2.Lnext();
					ot2.Pivot(ref os);
					bool flag3 = os.seg.hash != -1;
					flag2 = flag2 || flag3;
					ot2.Lnext();
					ot2.Pivot(ref os);
					bool flag4 = os.seg.hash != -1;
					flag = flag || flag4;
					if (!behavior.ConformingDelaunay && !flag4 && !flag3)
					{
						Vertex vertex3 = ot2.Org();
						while (vertex3.type == VertexType.FreeVertex && (vertex.x - vertex3.x) * (vertex2.x - vertex3.x) + (vertex.y - vertex3.y) * (vertex2.y - vertex3.y) < 0.0)
						{
							mesh.DeleteVertex(ref ot2);
							ot.Sym(ref ot2);
							vertex3 = ot2.Apex();
							ot2.Lprev();
						}
					}
				}
				double num3;
				if (flag || flag2)
				{
					double num = Math.Sqrt((vertex2.x - vertex.x) * (vertex2.x - vertex.x) + (vertex2.y - vertex.y) * (vertex2.y - vertex.y));
					double num2 = 1.0;
					while (num > 3.0 * num2)
					{
						num2 *= 2.0;
					}
					while (num < 1.5 * num2)
					{
						num2 *= 0.5;
					}
					num3 = num2 / num;
					if (flag2)
					{
						num3 = 1.0 - num3;
					}
				}
				else
				{
					num3 = 0.5;
				}
				Vertex vertex4 = new Vertex(vertex.x + num3 * (vertex2.x - vertex.x), vertex.y + num3 * (vertex2.y - vertex.y), osub.seg.boundary);
				vertex4.type = VertexType.SegmentVertex;
				vertex4.hash = mesh.hash_vtx++;
				vertex4.id = vertex4.hash;
				mesh.vertices.Add(vertex4.hash, vertex4);
				vertex4.z = vertex.z + num3 * (vertex2.z - vertex.z);
				if (!Behavior.NoExact)
				{
					double num4 = predicates.CounterClockwise(vertex, vertex2, vertex4);
					double num5 = (vertex.x - vertex2.x) * (vertex.x - vertex2.x) + (vertex.y - vertex2.y) * (vertex.y - vertex2.y);
					if (num4 != 0.0 && num5 != 0.0)
					{
						num4 /= num5;
						if (!double.IsNaN(num4))
						{
							vertex4.x += num4 * (vertex2.y - vertex.y);
							vertex4.y += num4 * (vertex.x - vertex2.x);
						}
					}
				}
				if ((vertex4.x == vertex.x && vertex4.y == vertex.y) || (vertex4.x == vertex2.x && vertex4.y == vertex2.y))
				{
					logger.Error("Ran out of precision: I attempted to split a segment to a smaller size than can be accommodated by the finite precision of floating point arithmetic.", "Quality.SplitEncSegs()");
					throw new Exception("Ran out of precision");
				}
				InsertVertexResult insertVertexResult = mesh.InsertVertex(vertex4, ref ot, ref osub, segmentflaws: true, triflaws);
				if (insertVertexResult != 0 && insertVertexResult != InsertVertexResult.Encroaching)
				{
					logger.Error("Failure to split a segment.", "Quality.SplitEncSegs()");
					throw new Exception("Failure to split a segment.");
				}
				if (mesh.steinerleft > 0)
				{
					mesh.steinerleft--;
				}
				CheckSeg4Encroach(ref osub);
				osub.Next();
				CheckSeg4Encroach(ref osub);
			}
			badSubseg.org = null;
		}
	}

	private void TallyFaces()
	{
		Otri testtri = default(Otri);
		testtri.orient = 0;
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			testtri.tri = triangle;
			TestTriangle(ref testtri);
		}
	}

	private void SplitTriangle(BadTriangle badtri)
	{
		Otri otri = default(Otri);
		double xi = 0.0;
		double eta = 0.0;
		otri = badtri.poortri;
		Vertex vertex = otri.Org();
		Vertex vertex2 = otri.Dest();
		Vertex vertex3 = otri.Apex();
		if (Otri.IsDead(otri.tri) || !(vertex == badtri.org) || !(vertex2 == badtri.dest) || !(vertex3 == badtri.apex))
		{
			return;
		}
		bool flag = false;
		Point point = ((!behavior.fixedArea && !behavior.VarArea) ? newLocation.FindLocation(vertex, vertex2, vertex3, ref xi, ref eta, offcenter: true, otri) : predicates.FindCircumcenter(vertex, vertex2, vertex3, ref xi, ref eta, behavior.offconstant));
		if ((point.x == vertex.x && point.y == vertex.y) || (point.x == vertex2.x && point.y == vertex2.y) || (point.x == vertex3.x && point.y == vertex3.y))
		{
			if (Log.Verbose)
			{
				logger.Warning("New vertex falls on existing vertex.", "Quality.SplitTriangle()");
				flag = true;
			}
		}
		else
		{
			Vertex vertex4 = new Vertex(point.x, point.y, 0);
			vertex4.type = VertexType.FreeVertex;
			if (eta < xi)
			{
				otri.Lprev();
			}
			vertex4.tri.tri = newvertex_tri;
			Osub splitseg = default(Osub);
			switch (mesh.InsertVertex(vertex4, ref otri, ref splitseg, segmentflaws: true, triflaws: true))
			{
			case InsertVertexResult.Successful:
				vertex4.hash = mesh.hash_vtx++;
				vertex4.id = vertex4.hash;
				Interpolation.InterpolateZ(vertex4, vertex4.tri.tri);
				mesh.vertices.Add(vertex4.hash, vertex4);
				if (mesh.steinerleft > 0)
				{
					mesh.steinerleft--;
				}
				break;
			case InsertVertexResult.Encroaching:
				mesh.UndoVertex();
				break;
			default:
				if (Log.Verbose)
				{
					logger.Warning("New vertex falls on existing vertex.", "Quality.SplitTriangle()");
					flag = true;
				}
				break;
			case InsertVertexResult.Violating:
				break;
			}
		}
		if (flag)
		{
			logger.Error("The new vertex is at the circumcenter of triangle: This probably means that I am trying to refine triangles to a smaller size than can be accommodated by the finite precision of floating point arithmetic.", "Quality.SplitTriangle()");
			throw new Exception("The new vertex is at the circumcenter of triangle.");
		}
	}

	private void EnforceQuality()
	{
		TallyEncs();
		SplitEncSegs(triflaws: false);
		if (behavior.MinAngle > 0.0 || behavior.VarArea || behavior.fixedArea || behavior.UserTest != null)
		{
			TallyFaces();
			mesh.checkquality = true;
			while (queue.Count > 0 && mesh.steinerleft != 0)
			{
				BadTriangle badtri = queue.Dequeue();
				SplitTriangle(badtri);
				if (badsubsegs.Count > 0)
				{
					queue.Enqueue(badtri);
					SplitEncSegs(triflaws: true);
				}
			}
		}
		if (Log.Verbose && behavior.ConformingDelaunay && badsubsegs.Count > 0 && mesh.steinerleft == 0)
		{
			logger.Warning("I ran out of Steiner points, but the mesh has encroached subsegments, and therefore might not be truly Delaunay. If the Delaunay property is important to you, try increasing the number of Steiner points.", "Quality.EnforceQuality()");
		}
	}
}
