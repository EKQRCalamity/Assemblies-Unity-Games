using System;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Logging;
using TriangleNet.Meshing.Iterators;
using TriangleNet.Topology;

namespace TriangleNet.Meshing;

internal class ConstraintMesher
{
	private IPredicates predicates;

	private Mesh mesh;

	private Behavior behavior;

	private TriangleLocator locator;

	private List<TriangleNet.Topology.Triangle> viri;

	private ILog<LogItem> logger;

	public ConstraintMesher(Mesh mesh, Configuration config)
	{
		this.mesh = mesh;
		predicates = config.Predicates();
		behavior = mesh.behavior;
		locator = mesh.locator;
		viri = new List<TriangleNet.Topology.Triangle>();
		logger = Log.Instance;
	}

	public void Apply(IPolygon input, ConstraintOptions options)
	{
		behavior.Poly = input.Segments.Count > 0;
		if (options != null)
		{
			behavior.ConformingDelaunay = options.ConformingDelaunay;
			behavior.Convex = options.Convex;
			behavior.NoBisect = options.SegmentSplitting;
			if (behavior.ConformingDelaunay)
			{
				behavior.Quality = true;
			}
		}
		behavior.useRegions = input.Regions.Count > 0;
		mesh.infvertex1 = null;
		mesh.infvertex2 = null;
		mesh.infvertex3 = null;
		if (behavior.useSegments)
		{
			mesh.checksegments = true;
			FormSkeleton(input);
		}
		if (behavior.Poly && mesh.triangles.Count > 0)
		{
			mesh.holes.AddRange(input.Holes);
			mesh.regions.AddRange(input.Regions);
			CarveHoles();
		}
	}

	private void CarveHoles()
	{
		Otri searchtri = default(Otri);
		TriangleNet.Topology.Triangle[] array = null;
		TriangleNet.Topology.Triangle dummytri = mesh.dummytri;
		if (!mesh.behavior.Convex)
		{
			InfectHull();
		}
		if (!mesh.behavior.NoHoles)
		{
			foreach (Point hole in mesh.holes)
			{
				if (mesh.bounds.Contains(hole))
				{
					searchtri.tri = dummytri;
					searchtri.orient = 0;
					searchtri.Sym();
					Vertex a = searchtri.Org();
					Vertex b = searchtri.Dest();
					if (predicates.CounterClockwise(a, b, hole) > 0.0 && mesh.locator.Locate(hole, ref searchtri) != LocateResult.Outside && !searchtri.IsInfected())
					{
						searchtri.Infect();
						viri.Add(searchtri.tri);
					}
				}
			}
		}
		if (mesh.regions.Count > 0)
		{
			int num = 0;
			array = new TriangleNet.Topology.Triangle[mesh.regions.Count];
			foreach (RegionPointer region in mesh.regions)
			{
				array[num] = dummytri;
				if (mesh.bounds.Contains(region.point))
				{
					searchtri.tri = dummytri;
					searchtri.orient = 0;
					searchtri.Sym();
					Vertex a = searchtri.Org();
					Vertex b = searchtri.Dest();
					if (predicates.CounterClockwise(a, b, region.point) > 0.0 && mesh.locator.Locate(region.point, ref searchtri) != LocateResult.Outside && !searchtri.IsInfected())
					{
						array[num] = searchtri.tri;
						array[num].label = region.id;
						array[num].area = region.area;
					}
				}
				num++;
			}
		}
		if (viri.Count > 0)
		{
			Plague();
		}
		if (array != null)
		{
			RegionIterator regionIterator = new RegionIterator(mesh);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].id != -1 && !Otri.IsDead(array[i]))
				{
					regionIterator.Process(array[i]);
				}
			}
		}
		viri.Clear();
	}

	private void FormSkeleton(IPolygon input)
	{
		mesh.insegments = 0;
		if (behavior.Poly)
		{
			if (mesh.triangles.Count == 0)
			{
				return;
			}
			if (input.Segments.Count > 0)
			{
				mesh.MakeVertexMap();
			}
			foreach (ISegment segment in input.Segments)
			{
				mesh.insegments++;
				Vertex vertex = segment.GetVertex(0);
				Vertex vertex2 = segment.GetVertex(1);
				if (vertex.x == vertex2.x && vertex.y == vertex2.y)
				{
					if (Log.Verbose)
					{
						logger.Warning("Endpoints of segment (IDs " + vertex.id + "/" + vertex2.id + ") are coincident.", "Mesh.FormSkeleton()");
					}
				}
				else
				{
					InsertSegment(vertex, vertex2, segment.Label);
				}
			}
		}
		if (behavior.Convex || !behavior.Poly)
		{
			MarkHull();
		}
	}

	private void InfectHull()
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		Osub os = default(Osub);
		TriangleNet.Topology.Triangle dummytri = mesh.dummytri;
		ot.tri = dummytri;
		ot.orient = 0;
		ot.Sym();
		ot.Copy(ref ot3);
		do
		{
			if (!ot.IsInfected())
			{
				ot.Pivot(ref os);
				if (os.seg.hash == -1)
				{
					if (!ot.IsInfected())
					{
						ot.Infect();
						viri.Add(ot.tri);
					}
				}
				else if (os.seg.boundary == 0)
				{
					os.seg.boundary = 1;
					Vertex vertex = ot.Org();
					Vertex vertex2 = ot.Dest();
					if (vertex.label == 0)
					{
						vertex.label = 1;
					}
					if (vertex2.label == 0)
					{
						vertex2.label = 1;
					}
				}
			}
			ot.Lnext();
			ot.Oprev(ref ot2);
			while (ot2.tri.id != -1)
			{
				ot2.Copy(ref ot);
				ot.Oprev(ref ot2);
			}
		}
		while (!ot.Equals(ot3));
	}

	private void Plague()
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Osub os = default(Osub);
		SubSegment dummysub = mesh.dummysub;
		TriangleNet.Topology.Triangle dummytri = mesh.dummytri;
		for (int i = 0; i < viri.Count; i++)
		{
			ot.tri = viri[i];
			ot.Uninfect();
			ot.orient = 0;
			while (ot.orient < 3)
			{
				ot.Sym(ref ot2);
				ot.Pivot(ref os);
				if (ot2.tri.id == -1 || ot2.IsInfected())
				{
					if (os.seg.hash != -1)
					{
						mesh.SubsegDealloc(os.seg);
						if (ot2.tri.id != -1)
						{
							ot2.Uninfect();
							ot2.SegDissolve(dummysub);
							ot2.Infect();
						}
					}
				}
				else if (os.seg.hash == -1)
				{
					ot2.Infect();
					viri.Add(ot2.tri);
				}
				else
				{
					os.TriDissolve(dummytri);
					if (os.seg.boundary == 0)
					{
						os.seg.boundary = 1;
					}
					Vertex vertex = ot2.Org();
					Vertex vertex2 = ot2.Dest();
					if (vertex.label == 0)
					{
						vertex.label = 1;
					}
					if (vertex2.label == 0)
					{
						vertex2.label = 1;
					}
				}
				ot.orient++;
			}
			ot.Infect();
		}
		foreach (TriangleNet.Topology.Triangle virus in viri)
		{
			ot.tri = virus;
			ot.orient = 0;
			while (ot.orient < 3)
			{
				Vertex vertex3 = ot.Org();
				if (vertex3 != null)
				{
					bool flag = true;
					ot.SetOrg(null);
					ot.Onext(ref ot2);
					while (ot2.tri.id != -1 && !ot2.Equals(ot))
					{
						if (ot2.IsInfected())
						{
							ot2.SetOrg(null);
						}
						else
						{
							flag = false;
						}
						ot2.Onext();
					}
					if (ot2.tri.id == -1)
					{
						ot.Oprev(ref ot2);
						while (ot2.tri.id != -1)
						{
							if (ot2.IsInfected())
							{
								ot2.SetOrg(null);
							}
							else
							{
								flag = false;
							}
							ot2.Oprev();
						}
					}
					if (flag)
					{
						vertex3.type = VertexType.UndeadVertex;
						mesh.undeads++;
					}
				}
				ot.orient++;
			}
			ot.orient = 0;
			while (ot.orient < 3)
			{
				ot.Sym(ref ot2);
				if (ot2.tri.id == -1)
				{
					mesh.hullsize--;
				}
				else
				{
					ot2.Dissolve(dummytri);
					mesh.hullsize++;
				}
				ot.orient++;
			}
			mesh.TriangleDealloc(ot.tri);
		}
		viri.Clear();
	}

	private FindDirectionResult FindDirection(ref Otri searchtri, Vertex searchpoint)
	{
		Otri ot = default(Otri);
		Vertex vertex = searchtri.Org();
		Vertex c = searchtri.Dest();
		Vertex c2 = searchtri.Apex();
		double num = predicates.CounterClockwise(searchpoint, vertex, c2);
		bool flag = num > 0.0;
		double num2 = predicates.CounterClockwise(vertex, searchpoint, c);
		bool flag2 = num2 > 0.0;
		if (flag && flag2)
		{
			searchtri.Onext(ref ot);
			if (ot.tri.id == -1)
			{
				flag = false;
			}
			else
			{
				flag2 = false;
			}
		}
		while (flag)
		{
			searchtri.Onext();
			if (searchtri.tri.id == -1)
			{
				logger.Error("Unable to find a triangle on path.", "Mesh.FindDirection().1");
				throw new Exception("Unable to find a triangle on path.");
			}
			c2 = searchtri.Apex();
			num2 = num;
			num = predicates.CounterClockwise(searchpoint, vertex, c2);
			flag = num > 0.0;
		}
		while (flag2)
		{
			searchtri.Oprev();
			if (searchtri.tri.id == -1)
			{
				logger.Error("Unable to find a triangle on path.", "Mesh.FindDirection().2");
				throw new Exception("Unable to find a triangle on path.");
			}
			c = searchtri.Dest();
			num = num2;
			num2 = predicates.CounterClockwise(vertex, searchpoint, c);
			flag2 = num2 > 0.0;
		}
		if (num == 0.0)
		{
			return FindDirectionResult.Leftcollinear;
		}
		if (num2 == 0.0)
		{
			return FindDirectionResult.Rightcollinear;
		}
		return FindDirectionResult.Within;
	}

	private void SegmentIntersection(ref Otri splittri, ref Osub splitsubseg, Vertex endpoint2)
	{
		Osub os = default(Osub);
		SubSegment dummysub = mesh.dummysub;
		Vertex vertex = splittri.Apex();
		Vertex vertex2 = splittri.Org();
		Vertex vertex3 = splittri.Dest();
		double num = vertex3.x - vertex2.x;
		double num2 = vertex3.y - vertex2.y;
		double num3 = endpoint2.x - vertex.x;
		double num4 = endpoint2.y - vertex.y;
		double num5 = vertex2.x - endpoint2.x;
		double num6 = vertex2.y - endpoint2.y;
		double num7 = num2 * num3 - num * num4;
		if (num7 == 0.0)
		{
			logger.Error("Attempt to find intersection of parallel segments.", "Mesh.SegmentIntersection()");
			throw new Exception("Attempt to find intersection of parallel segments.");
		}
		double num8 = (num4 * num5 - num3 * num6) / num7;
		Vertex vertex4 = new Vertex(vertex2.x + num8 * (vertex3.x - vertex2.x), vertex2.y + num8 * (vertex3.y - vertex2.y), splitsubseg.seg.boundary);
		vertex4.hash = mesh.hash_vtx++;
		vertex4.id = vertex4.hash;
		vertex4.z = vertex2.z + num8 * (vertex3.z - vertex2.z);
		mesh.vertices.Add(vertex4.hash, vertex4);
		if (mesh.InsertVertex(vertex4, ref splittri, ref splitsubseg, segmentflaws: false, triflaws: false) != 0)
		{
			logger.Error("Failure to split a segment.", "Mesh.SegmentIntersection()");
			throw new Exception("Failure to split a segment.");
		}
		vertex4.tri = splittri;
		if (mesh.steinerleft > 0)
		{
			mesh.steinerleft--;
		}
		splitsubseg.Sym();
		splitsubseg.Pivot(ref os);
		splitsubseg.Dissolve(dummysub);
		os.Dissolve(dummysub);
		do
		{
			splitsubseg.SetSegOrg(vertex4);
			splitsubseg.Next();
		}
		while (splitsubseg.seg.hash != -1);
		do
		{
			os.SetSegOrg(vertex4);
			os.Next();
		}
		while (os.seg.hash != -1);
		FindDirection(ref splittri, vertex);
		Vertex vertex5 = splittri.Dest();
		Vertex vertex6 = splittri.Apex();
		if (vertex6.x == vertex.x && vertex6.y == vertex.y)
		{
			splittri.Onext();
		}
		else if (vertex5.x != vertex.x || vertex5.y != vertex.y)
		{
			logger.Error("Topological inconsistency after splitting a segment.", "Mesh.SegmentIntersection()");
			throw new Exception("Topological inconsistency after splitting a segment.");
		}
	}

	private bool ScoutSegment(ref Otri searchtri, Vertex endpoint2, int newmark)
	{
		Otri ot = default(Otri);
		Osub os = default(Osub);
		FindDirectionResult findDirectionResult = FindDirection(ref searchtri, endpoint2);
		Vertex vertex = searchtri.Dest();
		Vertex vertex2 = searchtri.Apex();
		if ((vertex2.x == endpoint2.x && vertex2.y == endpoint2.y) || (vertex.x == endpoint2.x && vertex.y == endpoint2.y))
		{
			if (vertex2.x == endpoint2.x && vertex2.y == endpoint2.y)
			{
				searchtri.Lprev();
			}
			mesh.InsertSubseg(ref searchtri, newmark);
			return true;
		}
		switch (findDirectionResult)
		{
		case FindDirectionResult.Leftcollinear:
			searchtri.Lprev();
			mesh.InsertSubseg(ref searchtri, newmark);
			return ScoutSegment(ref searchtri, endpoint2, newmark);
		case FindDirectionResult.Rightcollinear:
			mesh.InsertSubseg(ref searchtri, newmark);
			searchtri.Lnext();
			return ScoutSegment(ref searchtri, endpoint2, newmark);
		default:
			searchtri.Lnext(ref ot);
			ot.Pivot(ref os);
			if (os.seg.hash == -1)
			{
				return false;
			}
			SegmentIntersection(ref ot, ref os, endpoint2);
			ot.Copy(ref searchtri);
			mesh.InsertSubseg(ref searchtri, newmark);
			return ScoutSegment(ref searchtri, endpoint2, newmark);
		}
	}

	private void DelaunayFixup(ref Otri fixuptri, bool leftside)
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Osub os = default(Osub);
		fixuptri.Lnext(ref ot);
		ot.Sym(ref ot2);
		if (ot2.tri.id == -1)
		{
			return;
		}
		ot.Pivot(ref os);
		if (os.seg.hash != -1)
		{
			return;
		}
		Vertex vertex = ot.Apex();
		Vertex vertex2 = ot.Org();
		Vertex vertex3 = ot.Dest();
		Vertex vertex4 = ot2.Apex();
		if (leftside)
		{
			if (predicates.CounterClockwise(vertex, vertex2, vertex4) <= 0.0)
			{
				return;
			}
		}
		else if (predicates.CounterClockwise(vertex4, vertex3, vertex) <= 0.0)
		{
			return;
		}
		if (!(predicates.CounterClockwise(vertex3, vertex2, vertex4) > 0.0) || !(predicates.InCircle(vertex2, vertex4, vertex3, vertex) <= 0.0))
		{
			mesh.Flip(ref ot);
			fixuptri.Lprev();
			DelaunayFixup(ref fixuptri, leftside);
			DelaunayFixup(ref ot2, leftside);
		}
	}

	private void ConstrainedEdge(ref Otri starttri, Vertex endpoint2, int newmark)
	{
		Otri ot = default(Otri);
		Otri fixuptri = default(Otri);
		Osub os = default(Osub);
		Vertex a = starttri.Org();
		starttri.Lnext(ref ot);
		mesh.Flip(ref ot);
		bool flag = false;
		bool flag2 = false;
		do
		{
			Vertex vertex = ot.Org();
			if (vertex.x == endpoint2.x && vertex.y == endpoint2.y)
			{
				ot.Oprev(ref fixuptri);
				DelaunayFixup(ref ot, leftside: false);
				DelaunayFixup(ref fixuptri, leftside: true);
				flag2 = true;
				continue;
			}
			double num = predicates.CounterClockwise(a, endpoint2, vertex);
			if (num == 0.0)
			{
				flag = true;
				ot.Oprev(ref fixuptri);
				DelaunayFixup(ref ot, leftside: false);
				DelaunayFixup(ref fixuptri, leftside: true);
				flag2 = true;
				continue;
			}
			if (num > 0.0)
			{
				ot.Oprev(ref fixuptri);
				DelaunayFixup(ref fixuptri, leftside: true);
				ot.Lprev();
			}
			else
			{
				DelaunayFixup(ref ot, leftside: false);
				ot.Oprev();
			}
			ot.Pivot(ref os);
			if (os.seg.hash == -1)
			{
				mesh.Flip(ref ot);
				continue;
			}
			flag = true;
			SegmentIntersection(ref ot, ref os, endpoint2);
			flag2 = true;
		}
		while (!flag2);
		mesh.InsertSubseg(ref ot, newmark);
		if (flag && !ScoutSegment(ref ot, endpoint2, newmark))
		{
			ConstrainedEdge(ref ot, endpoint2, newmark);
		}
	}

	private void InsertSegment(Vertex endpoint1, Vertex endpoint2, int newmark)
	{
		Otri otri = default(Otri);
		Otri otri2 = default(Otri);
		Vertex vertex = null;
		TriangleNet.Topology.Triangle dummytri = mesh.dummytri;
		otri = endpoint1.tri;
		if (otri.tri != null)
		{
			vertex = otri.Org();
		}
		if (vertex != endpoint1)
		{
			otri.tri = dummytri;
			otri.orient = 0;
			otri.Sym();
			if (locator.Locate(endpoint1, ref otri) != LocateResult.OnVertex)
			{
				logger.Error("Unable to locate PSLG vertex in triangulation.", "Mesh.InsertSegment().1");
				throw new Exception("Unable to locate PSLG vertex in triangulation.");
			}
		}
		locator.Update(ref otri);
		if (ScoutSegment(ref otri, endpoint2, newmark))
		{
			return;
		}
		endpoint1 = otri.Org();
		vertex = null;
		otri2 = endpoint2.tri;
		if (otri2.tri != null)
		{
			vertex = otri2.Org();
		}
		if (vertex != endpoint2)
		{
			otri2.tri = dummytri;
			otri2.orient = 0;
			otri2.Sym();
			if (locator.Locate(endpoint2, ref otri2) != LocateResult.OnVertex)
			{
				logger.Error("Unable to locate PSLG vertex in triangulation.", "Mesh.InsertSegment().2");
				throw new Exception("Unable to locate PSLG vertex in triangulation.");
			}
		}
		locator.Update(ref otri2);
		if (!ScoutSegment(ref otri2, endpoint1, newmark))
		{
			endpoint2 = otri2.Org();
			ConstrainedEdge(ref otri, endpoint2, newmark);
		}
	}

	private void MarkHull()
	{
		Otri tri = default(Otri);
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		tri.tri = mesh.dummytri;
		tri.orient = 0;
		tri.Sym();
		tri.Copy(ref ot2);
		do
		{
			mesh.InsertSubseg(ref tri, 1);
			tri.Lnext();
			tri.Oprev(ref ot);
			while (ot.tri.id != -1)
			{
				ot.Copy(ref tri);
				tri.Oprev(ref ot);
			}
		}
		while (!tri.Equals(ot2));
	}
}
