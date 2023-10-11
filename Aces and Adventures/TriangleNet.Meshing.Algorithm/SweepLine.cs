using System;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Tools;
using TriangleNet.Topology;

namespace TriangleNet.Meshing.Algorithm;

public class SweepLine : ITriangulator
{
	private class SweepEvent
	{
		public double xkey;

		public double ykey;

		public Vertex vertexEvent;

		public Otri otriEvent;

		public int heapposition;
	}

	private class SweepEventVertex : Vertex
	{
		public SweepEvent evt;

		public SweepEventVertex(SweepEvent e)
		{
			evt = e;
		}
	}

	private class SplayNode
	{
		public Otri keyedge;

		public Vertex keydest;

		public SplayNode lchild;

		public SplayNode rchild;
	}

	private static int randomseed = 1;

	private static int SAMPLERATE = 10;

	private IPredicates predicates;

	private Mesh mesh;

	private double xminextreme;

	private List<SplayNode> splaynodes;

	private static int randomnation(int choices)
	{
		randomseed = (randomseed * 1366 + 150889) % 714025;
		return randomseed / (714025 / choices + 1);
	}

	public IMesh Triangulate(IList<Vertex> points, Configuration config)
	{
		predicates = config.Predicates();
		mesh = new Mesh(config);
		mesh.TransferNodes(points);
		xminextreme = 10.0 * mesh.bounds.Left - 9.0 * mesh.bounds.Right;
		Otri ot = default(Otri);
		Otri searchtri = default(Otri);
		Otri newotri = default(Otri);
		Otri newotri2 = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		Otri ot4 = default(Otri);
		bool farright = false;
		splaynodes = new List<SplayNode>();
		SplayNode splayroot = null;
		int count = points.Count;
		CreateHeap(out var eventheap, count);
		mesh.MakeTriangle(ref newotri);
		mesh.MakeTriangle(ref newotri2);
		newotri.Bond(ref newotri2);
		newotri.Lnext();
		newotri2.Lprev();
		newotri.Bond(ref newotri2);
		newotri.Lnext();
		newotri2.Lprev();
		newotri.Bond(ref newotri2);
		Vertex vertexEvent = eventheap[0].vertexEvent;
		HeapDelete(eventheap, count, 0);
		count--;
		Vertex vertexEvent2;
		do
		{
			if (count == 0)
			{
				Log.Instance.Error("Input vertices are all identical.", "SweepLine.Triangulate()");
				throw new Exception("Input vertices are all identical.");
			}
			vertexEvent2 = eventheap[0].vertexEvent;
			HeapDelete(eventheap, count, 0);
			count--;
			if (vertexEvent.x == vertexEvent2.x && vertexEvent.y == vertexEvent2.y)
			{
				if (Log.Verbose)
				{
					Log.Instance.Warning("A duplicate vertex appeared and was ignored (ID " + vertexEvent2.id + ").", "SweepLine.Triangulate().1");
				}
				vertexEvent2.type = VertexType.UndeadVertex;
				mesh.undeads++;
			}
		}
		while (vertexEvent.x == vertexEvent2.x && vertexEvent.y == vertexEvent2.y);
		newotri.SetOrg(vertexEvent);
		newotri.SetDest(vertexEvent2);
		newotri2.SetOrg(vertexEvent2);
		newotri2.SetDest(vertexEvent);
		newotri.Lprev(ref ot);
		Vertex vertex = vertexEvent2;
		while (count > 0)
		{
			SweepEvent sweepEvent = eventheap[0];
			HeapDelete(eventheap, count, 0);
			count--;
			bool flag = true;
			if (sweepEvent.xkey < mesh.bounds.Left)
			{
				Otri flipedge = sweepEvent.otriEvent;
				flipedge.Oprev(ref ot2);
				Check4DeadEvent(ref ot2, eventheap, ref count);
				flipedge.Onext(ref ot3);
				Check4DeadEvent(ref ot3, eventheap, ref count);
				if (ot2.Equals(ot))
				{
					flipedge.Lprev(ref ot);
				}
				mesh.Flip(ref flipedge);
				flipedge.SetApex(null);
				flipedge.Lprev(ref newotri);
				flipedge.Lnext(ref newotri2);
				newotri.Sym(ref ot2);
				if (randomnation(SAMPLERATE) == 0)
				{
					flipedge.Sym();
					Vertex pa = flipedge.Dest();
					Vertex pb = flipedge.Apex();
					Vertex pc = flipedge.Org();
					splayroot = CircleTopInsert(splayroot, newotri, pa, pb, pc, sweepEvent.ykey);
				}
			}
			else
			{
				Vertex vertexEvent3 = sweepEvent.vertexEvent;
				if (vertexEvent3.x == vertex.x && vertexEvent3.y == vertex.y)
				{
					if (Log.Verbose)
					{
						Log.Instance.Warning("A duplicate vertex appeared and was ignored (ID " + vertexEvent3.id + ").", "SweepLine.Triangulate().2");
					}
					vertexEvent3.type = VertexType.UndeadVertex;
					mesh.undeads++;
					flag = false;
				}
				else
				{
					vertex = vertexEvent3;
					splayroot = FrontLocate(splayroot, ot, vertexEvent3, ref searchtri, ref farright);
					Check4DeadEvent(ref searchtri, eventheap, ref count);
					searchtri.Copy(ref ot3);
					searchtri.Sym(ref ot2);
					mesh.MakeTriangle(ref newotri);
					mesh.MakeTriangle(ref newotri2);
					Vertex vertex2 = ot3.Dest();
					newotri.SetOrg(vertex2);
					newotri.SetDest(vertexEvent3);
					newotri2.SetOrg(vertexEvent3);
					newotri2.SetDest(vertex2);
					newotri.Bond(ref newotri2);
					newotri.Lnext();
					newotri2.Lprev();
					newotri.Bond(ref newotri2);
					newotri.Lnext();
					newotri2.Lprev();
					newotri.Bond(ref ot2);
					newotri2.Bond(ref ot3);
					if (!farright && ot3.Equals(ot))
					{
						newotri.Copy(ref ot);
					}
					if (randomnation(SAMPLERATE) == 0)
					{
						splayroot = SplayInsert(splayroot, newotri, vertexEvent3);
					}
					else if (randomnation(SAMPLERATE) == 0)
					{
						newotri2.Lnext(ref ot4);
						splayroot = SplayInsert(splayroot, ot4, vertexEvent3);
					}
				}
			}
			if (flag)
			{
				Vertex pa = ot2.Apex();
				Vertex pb = newotri.Dest();
				Vertex pc = newotri.Apex();
				double num = predicates.CounterClockwise(pa, pb, pc);
				if (num > 0.0)
				{
					SweepEvent sweepEvent2 = new SweepEvent();
					sweepEvent2.xkey = xminextreme;
					sweepEvent2.ykey = CircleTop(pa, pb, pc, num);
					sweepEvent2.otriEvent = newotri;
					HeapInsert(eventheap, count, sweepEvent2);
					count++;
					newotri.SetOrg(new SweepEventVertex(sweepEvent2));
				}
				pa = newotri2.Apex();
				pb = newotri2.Org();
				pc = ot3.Apex();
				double num2 = predicates.CounterClockwise(pa, pb, pc);
				if (num2 > 0.0)
				{
					SweepEvent sweepEvent2 = new SweepEvent();
					sweepEvent2.xkey = xminextreme;
					sweepEvent2.ykey = CircleTop(pa, pb, pc, num2);
					sweepEvent2.otriEvent = ot3;
					HeapInsert(eventheap, count, sweepEvent2);
					count++;
					ot3.SetOrg(new SweepEventVertex(sweepEvent2));
				}
			}
		}
		splaynodes.Clear();
		ot.Lprev();
		mesh.hullsize = RemoveGhosts(ref ot);
		return mesh;
	}

	private void HeapInsert(SweepEvent[] heap, int heapsize, SweepEvent newevent)
	{
		double xkey = newevent.xkey;
		double ykey = newevent.ykey;
		int num = heapsize;
		bool flag = num > 0;
		while (flag)
		{
			int num2 = num - 1 >> 1;
			if (heap[num2].ykey < ykey || (heap[num2].ykey == ykey && heap[num2].xkey <= xkey))
			{
				flag = false;
				continue;
			}
			heap[num] = heap[num2];
			heap[num].heapposition = num;
			num = num2;
			flag = num > 0;
		}
		heap[num] = newevent;
		newevent.heapposition = num;
	}

	private void Heapify(SweepEvent[] heap, int heapsize, int eventnum)
	{
		SweepEvent sweepEvent = heap[eventnum];
		double xkey = sweepEvent.xkey;
		double ykey = sweepEvent.ykey;
		int num = 2 * eventnum + 1;
		bool flag = num < heapsize;
		while (flag)
		{
			int num2 = ((!(heap[num].ykey < ykey) && (heap[num].ykey != ykey || !(heap[num].xkey < xkey))) ? eventnum : num);
			int num3 = num + 1;
			if (num3 < heapsize && (heap[num3].ykey < heap[num2].ykey || (heap[num3].ykey == heap[num2].ykey && heap[num3].xkey < heap[num2].xkey)))
			{
				num2 = num3;
			}
			if (num2 == eventnum)
			{
				flag = false;
				continue;
			}
			heap[eventnum] = heap[num2];
			heap[eventnum].heapposition = eventnum;
			heap[num2] = sweepEvent;
			sweepEvent.heapposition = num2;
			eventnum = num2;
			num = 2 * eventnum + 1;
			flag = num < heapsize;
		}
	}

	private void HeapDelete(SweepEvent[] heap, int heapsize, int eventnum)
	{
		SweepEvent sweepEvent = heap[heapsize - 1];
		if (eventnum > 0)
		{
			double xkey = sweepEvent.xkey;
			double ykey = sweepEvent.ykey;
			bool flag;
			do
			{
				int num = eventnum - 1 >> 1;
				if (heap[num].ykey < ykey || (heap[num].ykey == ykey && heap[num].xkey <= xkey))
				{
					flag = false;
					continue;
				}
				heap[eventnum] = heap[num];
				heap[eventnum].heapposition = eventnum;
				eventnum = num;
				flag = eventnum > 0;
			}
			while (flag);
		}
		heap[eventnum] = sweepEvent;
		sweepEvent.heapposition = eventnum;
		Heapify(heap, heapsize - 1, eventnum);
	}

	private void CreateHeap(out SweepEvent[] eventheap, int size)
	{
		int num = 3 * size / 2;
		eventheap = new SweepEvent[num];
		int num2 = 0;
		foreach (Vertex value in mesh.vertices.Values)
		{
			SweepEvent sweepEvent = new SweepEvent();
			sweepEvent.vertexEvent = value;
			sweepEvent.xkey = value.x;
			sweepEvent.ykey = value.y;
			HeapInsert(eventheap, num2++, sweepEvent);
		}
	}

	private SplayNode Splay(SplayNode splaytree, Point searchpoint, ref Otri searchtri)
	{
		if (splaytree == null)
		{
			return null;
		}
		if (splaytree.keyedge.Dest() == splaytree.keydest)
		{
			bool flag = RightOfHyperbola(ref splaytree.keyedge, searchpoint);
			SplayNode splayNode;
			if (flag)
			{
				splaytree.keyedge.Copy(ref searchtri);
				splayNode = splaytree.rchild;
			}
			else
			{
				splayNode = splaytree.lchild;
			}
			if (splayNode == null)
			{
				return splaytree;
			}
			if (splayNode.keyedge.Dest() != splayNode.keydest)
			{
				splayNode = Splay(splayNode, searchpoint, ref searchtri);
				if (splayNode == null)
				{
					if (flag)
					{
						splaytree.rchild = null;
					}
					else
					{
						splaytree.lchild = null;
					}
					return splaytree;
				}
			}
			bool flag2 = RightOfHyperbola(ref splayNode.keyedge, searchpoint);
			SplayNode splayNode2;
			if (!flag2)
			{
				splayNode2 = (splayNode.lchild = Splay(splayNode.lchild, searchpoint, ref searchtri));
			}
			else
			{
				splayNode.keyedge.Copy(ref searchtri);
				splayNode2 = (splayNode.rchild = Splay(splayNode.rchild, searchpoint, ref searchtri));
			}
			if (splayNode2 == null)
			{
				if (flag)
				{
					splaytree.rchild = splayNode.lchild;
					splayNode.lchild = splaytree;
				}
				else
				{
					splaytree.lchild = splayNode.rchild;
					splayNode.rchild = splaytree;
				}
				return splayNode;
			}
			if (flag2)
			{
				if (flag)
				{
					splaytree.rchild = splayNode.lchild;
					splayNode.lchild = splaytree;
				}
				else
				{
					splaytree.lchild = splayNode2.rchild;
					splayNode2.rchild = splaytree;
				}
				splayNode.rchild = splayNode2.lchild;
				splayNode2.lchild = splayNode;
			}
			else
			{
				if (flag)
				{
					splaytree.rchild = splayNode2.lchild;
					splayNode2.lchild = splaytree;
				}
				else
				{
					splaytree.lchild = splayNode.rchild;
					splayNode.rchild = splaytree;
				}
				splayNode.lchild = splayNode2.rchild;
				splayNode2.rchild = splayNode;
			}
			return splayNode2;
		}
		SplayNode splayNode3 = Splay(splaytree.lchild, searchpoint, ref searchtri);
		SplayNode splayNode4 = Splay(splaytree.rchild, searchpoint, ref searchtri);
		splaynodes.Remove(splaytree);
		if (splayNode3 == null)
		{
			return splayNode4;
		}
		if (splayNode4 == null)
		{
			return splayNode3;
		}
		if (splayNode3.rchild == null)
		{
			splayNode3.rchild = splayNode4.lchild;
			splayNode4.lchild = splayNode3;
			return splayNode4;
		}
		if (splayNode4.lchild == null)
		{
			splayNode4.lchild = splayNode3.rchild;
			splayNode3.rchild = splayNode4;
			return splayNode3;
		}
		SplayNode rchild = splayNode3.rchild;
		while (rchild.rchild != null)
		{
			rchild = rchild.rchild;
		}
		rchild.rchild = splayNode4;
		return splayNode3;
	}

	private SplayNode SplayInsert(SplayNode splayroot, Otri newkey, Point searchpoint)
	{
		SplayNode splayNode = new SplayNode();
		splaynodes.Add(splayNode);
		newkey.Copy(ref splayNode.keyedge);
		splayNode.keydest = newkey.Dest();
		if (splayroot == null)
		{
			splayNode.lchild = null;
			splayNode.rchild = null;
		}
		else if (RightOfHyperbola(ref splayroot.keyedge, searchpoint))
		{
			splayNode.lchild = splayroot;
			splayNode.rchild = splayroot.rchild;
			splayroot.rchild = null;
		}
		else
		{
			splayNode.lchild = splayroot.lchild;
			splayNode.rchild = splayroot;
			splayroot.lchild = null;
		}
		return splayNode;
	}

	private SplayNode FrontLocate(SplayNode splayroot, Otri bottommost, Vertex searchvertex, ref Otri searchtri, ref bool farright)
	{
		bottommost.Copy(ref searchtri);
		splayroot = Splay(splayroot, searchvertex, ref searchtri);
		bool flag = false;
		while (!flag && RightOfHyperbola(ref searchtri, searchvertex))
		{
			searchtri.Onext();
			flag = searchtri.Equals(bottommost);
		}
		farright = flag;
		return splayroot;
	}

	private SplayNode CircleTopInsert(SplayNode splayroot, Otri newkey, Vertex pa, Vertex pb, Vertex pc, double topy)
	{
		Point point = new Point();
		Otri searchtri = default(Otri);
		double num = predicates.CounterClockwise(pa, pb, pc);
		double num2 = pa.x - pc.x;
		double num3 = pa.y - pc.y;
		double num4 = pb.x - pc.x;
		double num5 = pb.y - pc.y;
		double num6 = num2 * num2 + num3 * num3;
		double num7 = num4 * num4 + num5 * num5;
		point.x = pc.x - (num3 * num7 - num5 * num6) / (2.0 * num);
		point.y = topy;
		return SplayInsert(Splay(splayroot, point, ref searchtri), newkey, point);
	}

	private bool RightOfHyperbola(ref Otri fronttri, Point newsite)
	{
		Statistic.HyperbolaCount++;
		Vertex vertex = fronttri.Dest();
		Vertex vertex2 = fronttri.Apex();
		if (vertex.y < vertex2.y || (vertex.y == vertex2.y && vertex.x < vertex2.x))
		{
			if (newsite.x >= vertex2.x)
			{
				return true;
			}
		}
		else if (newsite.x <= vertex.x)
		{
			return false;
		}
		double num = vertex.x - newsite.x;
		double num2 = vertex.y - newsite.y;
		double num3 = vertex2.x - newsite.x;
		double num4 = vertex2.y - newsite.y;
		return num2 * (num3 * num3 + num4 * num4) > num4 * (num * num + num2 * num2);
	}

	private double CircleTop(Vertex pa, Vertex pb, Vertex pc, double ccwabc)
	{
		Statistic.CircleTopCount++;
		double num = pa.x - pc.x;
		double num2 = pa.y - pc.y;
		double num3 = pb.x - pc.x;
		double num4 = pb.y - pc.y;
		double num5 = pa.x - pb.x;
		double num6 = pa.y - pb.y;
		double num7 = num * num + num2 * num2;
		double num8 = num3 * num3 + num4 * num4;
		double num9 = num5 * num5 + num6 * num6;
		return pc.y + (num * num8 - num3 * num7 + Math.Sqrt(num7 * num8 * num9)) / (2.0 * ccwabc);
	}

	private void Check4DeadEvent(ref Otri checktri, SweepEvent[] eventheap, ref int heapsize)
	{
		int num = -1;
		SweepEventVertex sweepEventVertex = checktri.Org() as SweepEventVertex;
		if (sweepEventVertex != null)
		{
			num = sweepEventVertex.evt.heapposition;
			HeapDelete(eventheap, heapsize, num);
			heapsize--;
			checktri.SetOrg(null);
		}
	}

	private int RemoveGhosts(ref Otri startghost)
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		bool flag = !mesh.behavior.Poly;
		TriangleNet.Topology.Triangle dummytri = mesh.dummytri;
		startghost.Lprev(ref ot);
		ot.Sym();
		dummytri.neighbors[0] = ot;
		startghost.Copy(ref ot2);
		int num = 0;
		do
		{
			num++;
			ot2.Lnext(ref ot3);
			ot2.Lprev();
			ot2.Sym();
			if (flag && ot2.tri.id != -1)
			{
				Vertex vertex = ot2.Org();
				if (vertex.label == 0)
				{
					vertex.label = 1;
				}
			}
			ot2.Dissolve(dummytri);
			ot3.Sym(ref ot2);
			mesh.TriangleDealloc(ot3.tri);
		}
		while (!ot2.Equals(startghost));
		return num;
	}
}
