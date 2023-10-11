using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Tools;
using TriangleNet.Topology;

namespace TriangleNet.Meshing.Algorithm;

public class Dwyer : ITriangulator
{
	private IPredicates predicates;

	public bool UseDwyer = true;

	private Vertex[] sortarray;

	private Mesh mesh;

	public IMesh Triangulate(IList<Vertex> points, Configuration config)
	{
		predicates = config.Predicates();
		mesh = new Mesh(config);
		mesh.TransferNodes(points);
		Otri farleft = default(Otri);
		Otri farright = default(Otri);
		int count = points.Count;
		sortarray = new Vertex[count];
		int num = 0;
		foreach (Vertex point in points)
		{
			sortarray[num++] = point;
		}
		VertexSorter.Sort(sortarray);
		num = 0;
		for (int i = 1; i < count; i++)
		{
			if (sortarray[num].x == sortarray[i].x && sortarray[num].y == sortarray[i].y)
			{
				if (Log.Verbose)
				{
					Log.Instance.Warning($"A duplicate vertex appeared and was ignored (ID {sortarray[i].id}).", "Dwyer.Triangulate()");
				}
				sortarray[i].type = VertexType.UndeadVertex;
				mesh.undeads++;
			}
			else
			{
				num++;
				sortarray[num] = sortarray[i];
			}
		}
		num++;
		if (UseDwyer)
		{
			VertexSorter.Alternate(sortarray, num);
		}
		DivconqRecurse(0, num - 1, 0, ref farleft, ref farright);
		mesh.hullsize = RemoveGhosts(ref farleft);
		return mesh;
	}

	private void MergeHulls(ref Otri farleft, ref Otri innerleft, ref Otri innerright, ref Otri farright, int axis)
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri newotri = default(Otri);
		Otri ot3 = default(Otri);
		Otri ot4 = default(Otri);
		Otri ot5 = default(Otri);
		Otri ot6 = default(Otri);
		Otri newotri2 = default(Otri);
		Vertex vertex = innerleft.Dest();
		Vertex vertex2 = innerleft.Apex();
		Vertex vertex3 = innerright.Org();
		Vertex vertex4 = innerright.Apex();
		Vertex vertex5;
		Vertex vertex7;
		if (UseDwyer && axis == 1)
		{
			vertex5 = farleft.Org();
			Vertex vertex6 = farleft.Apex();
			vertex7 = farright.Dest();
			Vertex vertex8 = farright.Apex();
			while (vertex6.y < vertex5.y)
			{
				farleft.Lnext();
				farleft.Sym();
				vertex5 = vertex6;
				vertex6 = farleft.Apex();
			}
			innerleft.Sym(ref ot6);
			Vertex vertex9 = ot6.Apex();
			while (vertex9.y > vertex.y)
			{
				ot6.Lnext(ref innerleft);
				vertex2 = vertex;
				vertex = vertex9;
				innerleft.Sym(ref ot6);
				vertex9 = ot6.Apex();
			}
			while (vertex4.y < vertex3.y)
			{
				innerright.Lnext();
				innerright.Sym();
				vertex3 = vertex4;
				vertex4 = innerright.Apex();
			}
			farright.Sym(ref ot6);
			vertex9 = ot6.Apex();
			while (vertex9.y > vertex7.y)
			{
				ot6.Lnext(ref farright);
				vertex8 = vertex7;
				vertex7 = vertex9;
				farright.Sym(ref ot6);
				vertex9 = ot6.Apex();
			}
		}
		bool flag;
		do
		{
			flag = false;
			if (predicates.CounterClockwise(vertex, vertex2, vertex3) > 0.0)
			{
				innerleft.Lprev();
				innerleft.Sym();
				vertex = vertex2;
				vertex2 = innerleft.Apex();
				flag = true;
			}
			if (predicates.CounterClockwise(vertex4, vertex3, vertex) > 0.0)
			{
				innerright.Lnext();
				innerright.Sym();
				vertex3 = vertex4;
				vertex4 = innerright.Apex();
				flag = true;
			}
		}
		while (flag);
		innerleft.Sym(ref ot);
		innerright.Sym(ref ot2);
		mesh.MakeTriangle(ref newotri2);
		newotri2.Bond(ref innerleft);
		newotri2.Lnext();
		newotri2.Bond(ref innerright);
		newotri2.Lnext();
		newotri2.SetOrg(vertex3);
		newotri2.SetDest(vertex);
		vertex5 = farleft.Org();
		if (vertex == vertex5)
		{
			newotri2.Lnext(ref farleft);
		}
		vertex7 = farright.Dest();
		if (vertex3 == vertex7)
		{
			newotri2.Lprev(ref farright);
		}
		Vertex vertex10 = vertex;
		Vertex vertex11 = vertex3;
		Vertex vertex12 = ot.Apex();
		Vertex vertex13 = ot2.Apex();
		while (true)
		{
			bool flag2 = predicates.CounterClockwise(vertex12, vertex10, vertex11) <= 0.0;
			bool flag3 = predicates.CounterClockwise(vertex13, vertex10, vertex11) <= 0.0;
			if (flag2 && flag3)
			{
				break;
			}
			if (!flag2)
			{
				ot.Lprev(ref newotri);
				newotri.Sym();
				Vertex vertex14 = newotri.Apex();
				if (vertex14 != null)
				{
					bool flag4 = predicates.InCircle(vertex10, vertex11, vertex12, vertex14) > 0.0;
					while (flag4)
					{
						newotri.Lnext();
						newotri.Sym(ref ot4);
						newotri.Lnext();
						newotri.Sym(ref ot3);
						newotri.Bond(ref ot4);
						ot.Bond(ref ot3);
						ot.Lnext();
						ot.Sym(ref ot5);
						newotri.Lprev();
						newotri.Bond(ref ot5);
						ot.SetOrg(vertex10);
						ot.SetDest(null);
						ot.SetApex(vertex14);
						newotri.SetOrg(null);
						newotri.SetDest(vertex12);
						newotri.SetApex(vertex14);
						vertex12 = vertex14;
						ot3.Copy(ref newotri);
						vertex14 = newotri.Apex();
						flag4 = vertex14 != null && predicates.InCircle(vertex10, vertex11, vertex12, vertex14) > 0.0;
					}
				}
			}
			if (!flag3)
			{
				ot2.Lnext(ref newotri);
				newotri.Sym();
				Vertex vertex14 = newotri.Apex();
				if (vertex14 != null)
				{
					bool flag4 = predicates.InCircle(vertex10, vertex11, vertex13, vertex14) > 0.0;
					while (flag4)
					{
						newotri.Lprev();
						newotri.Sym(ref ot4);
						newotri.Lprev();
						newotri.Sym(ref ot3);
						newotri.Bond(ref ot4);
						ot2.Bond(ref ot3);
						ot2.Lprev();
						ot2.Sym(ref ot5);
						newotri.Lnext();
						newotri.Bond(ref ot5);
						ot2.SetOrg(null);
						ot2.SetDest(vertex11);
						ot2.SetApex(vertex14);
						newotri.SetOrg(vertex13);
						newotri.SetDest(null);
						newotri.SetApex(vertex14);
						vertex13 = vertex14;
						ot3.Copy(ref newotri);
						vertex14 = newotri.Apex();
						flag4 = vertex14 != null && predicates.InCircle(vertex10, vertex11, vertex13, vertex14) > 0.0;
					}
				}
			}
			if (flag2 || (!flag3 && predicates.InCircle(vertex12, vertex10, vertex11, vertex13) > 0.0))
			{
				newotri2.Bond(ref ot2);
				ot2.Lprev(ref newotri2);
				newotri2.SetDest(vertex10);
				vertex11 = vertex13;
				newotri2.Sym(ref ot2);
				vertex13 = ot2.Apex();
			}
			else
			{
				newotri2.Bond(ref ot);
				ot.Lnext(ref newotri2);
				newotri2.SetOrg(vertex11);
				vertex10 = vertex12;
				newotri2.Sym(ref ot);
				vertex12 = ot.Apex();
			}
		}
		mesh.MakeTriangle(ref newotri);
		newotri.SetOrg(vertex10);
		newotri.SetDest(vertex11);
		newotri.Bond(ref newotri2);
		newotri.Lnext();
		newotri.Bond(ref ot2);
		newotri.Lnext();
		newotri.Bond(ref ot);
		if (UseDwyer && axis == 1)
		{
			vertex5 = farleft.Org();
			Vertex vertex6 = farleft.Apex();
			vertex7 = farright.Dest();
			Vertex vertex8 = farright.Apex();
			farleft.Sym(ref ot6);
			Vertex vertex9 = ot6.Apex();
			while (vertex9.x < vertex5.x)
			{
				ot6.Lprev(ref farleft);
				vertex6 = vertex5;
				vertex5 = vertex9;
				farleft.Sym(ref ot6);
				vertex9 = ot6.Apex();
			}
			while (vertex8.x > vertex7.x)
			{
				farright.Lprev();
				farright.Sym();
				vertex7 = vertex8;
				vertex8 = farright.Apex();
			}
		}
	}

	private void DivconqRecurse(int left, int right, int axis, ref Otri farleft, ref Otri farright)
	{
		Otri newotri = default(Otri);
		Otri newotri2 = default(Otri);
		Otri newotri3 = default(Otri);
		Otri newotri4 = default(Otri);
		Otri farright2 = default(Otri);
		Otri farleft2 = default(Otri);
		int num = right - left + 1;
		switch (num)
		{
		case 2:
			mesh.MakeTriangle(ref farleft);
			farleft.SetOrg(sortarray[left]);
			farleft.SetDest(sortarray[left + 1]);
			mesh.MakeTriangle(ref farright);
			farright.SetOrg(sortarray[left + 1]);
			farright.SetDest(sortarray[left]);
			farleft.Bond(ref farright);
			farleft.Lprev();
			farright.Lnext();
			farleft.Bond(ref farright);
			farleft.Lprev();
			farright.Lnext();
			farleft.Bond(ref farright);
			farright.Lprev(ref farleft);
			break;
		case 3:
		{
			mesh.MakeTriangle(ref newotri);
			mesh.MakeTriangle(ref newotri2);
			mesh.MakeTriangle(ref newotri3);
			mesh.MakeTriangle(ref newotri4);
			double num3 = predicates.CounterClockwise(sortarray[left], sortarray[left + 1], sortarray[left + 2]);
			if (num3 == 0.0)
			{
				newotri.SetOrg(sortarray[left]);
				newotri.SetDest(sortarray[left + 1]);
				newotri2.SetOrg(sortarray[left + 1]);
				newotri2.SetDest(sortarray[left]);
				newotri3.SetOrg(sortarray[left + 2]);
				newotri3.SetDest(sortarray[left + 1]);
				newotri4.SetOrg(sortarray[left + 1]);
				newotri4.SetDest(sortarray[left + 2]);
				newotri.Bond(ref newotri2);
				newotri3.Bond(ref newotri4);
				newotri.Lnext();
				newotri2.Lprev();
				newotri3.Lnext();
				newotri4.Lprev();
				newotri.Bond(ref newotri4);
				newotri2.Bond(ref newotri3);
				newotri.Lnext();
				newotri2.Lprev();
				newotri3.Lnext();
				newotri4.Lprev();
				newotri.Bond(ref newotri2);
				newotri3.Bond(ref newotri4);
				newotri2.Copy(ref farleft);
				newotri3.Copy(ref farright);
				break;
			}
			newotri.SetOrg(sortarray[left]);
			newotri2.SetDest(sortarray[left]);
			newotri4.SetOrg(sortarray[left]);
			if (num3 > 0.0)
			{
				newotri.SetDest(sortarray[left + 1]);
				newotri2.SetOrg(sortarray[left + 1]);
				newotri3.SetDest(sortarray[left + 1]);
				newotri.SetApex(sortarray[left + 2]);
				newotri3.SetOrg(sortarray[left + 2]);
				newotri4.SetDest(sortarray[left + 2]);
			}
			else
			{
				newotri.SetDest(sortarray[left + 2]);
				newotri2.SetOrg(sortarray[left + 2]);
				newotri3.SetDest(sortarray[left + 2]);
				newotri.SetApex(sortarray[left + 1]);
				newotri3.SetOrg(sortarray[left + 1]);
				newotri4.SetDest(sortarray[left + 1]);
			}
			newotri.Bond(ref newotri2);
			newotri.Lnext();
			newotri.Bond(ref newotri3);
			newotri.Lnext();
			newotri.Bond(ref newotri4);
			newotri2.Lprev();
			newotri3.Lnext();
			newotri2.Bond(ref newotri3);
			newotri2.Lprev();
			newotri4.Lprev();
			newotri2.Bond(ref newotri4);
			newotri3.Lnext();
			newotri4.Lprev();
			newotri3.Bond(ref newotri4);
			newotri2.Copy(ref farleft);
			if (num3 > 0.0)
			{
				newotri3.Copy(ref farright);
			}
			else
			{
				farleft.Lnext(ref farright);
			}
			break;
		}
		default:
		{
			int num2 = num >> 1;
			DivconqRecurse(left, left + num2 - 1, 1 - axis, ref farleft, ref farright2);
			DivconqRecurse(left + num2, right, 1 - axis, ref farleft2, ref farright);
			MergeHulls(ref farleft, ref farright2, ref farleft2, ref farright, axis);
			break;
		}
		}
	}

	private int RemoveGhosts(ref Otri startghost)
	{
		Otri ot = default(Otri);
		Otri ot2 = default(Otri);
		Otri ot3 = default(Otri);
		bool flag = !mesh.behavior.Poly;
		startghost.Lprev(ref ot);
		ot.Sym();
		mesh.dummytri.neighbors[0] = ot;
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
			ot2.Dissolve(mesh.dummytri);
			ot3.Sym(ref ot2);
			mesh.TriangleDealloc(ot3.tri);
		}
		while (!ot2.Equals(startghost));
		return num;
	}
}
