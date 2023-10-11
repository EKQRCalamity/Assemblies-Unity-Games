using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using TriangleNet.Topology.DCEL;

namespace TriangleNet.Meshing;

public static class Converter
{
	public static Mesh ToMesh(Polygon polygon, IList<ITriangle> triangles)
	{
		return ToMesh(polygon, triangles.ToArray());
	}

	public static Mesh ToMesh(Polygon polygon, ITriangle[] triangles)
	{
		Otri newotri = default(Otri);
		Osub newsubseg = default(Osub);
		int num = 0;
		int num2 = ((triangles != null) ? triangles.Length : 0);
		int count = polygon.Segments.Count;
		Mesh mesh = new Mesh(new Configuration());
		mesh.TransferNodes(polygon.Points);
		mesh.regions.AddRange(polygon.Regions);
		mesh.behavior.useRegions = polygon.Regions.Count > 0;
		if (polygon.Segments.Count > 0)
		{
			mesh.behavior.Poly = true;
			mesh.holes.AddRange(polygon.Holes);
		}
		for (num = 0; num < num2; num++)
		{
			mesh.MakeTriangle(ref newotri);
		}
		if (mesh.behavior.Poly)
		{
			mesh.insegments = count;
			for (num = 0; num < count; num++)
			{
				mesh.MakeSegment(ref newsubseg);
			}
		}
		List<Otri>[] vertexarray = SetNeighbors(mesh, triangles);
		SetSegments(mesh, polygon, vertexarray);
		return mesh;
	}

	private static List<Otri>[] SetNeighbors(Mesh mesh, ITriangle[] triangles)
	{
		Otri item = default(Otri);
		Otri ot = default(Otri);
		Otri otri = default(Otri);
		Otri ot2 = default(Otri);
		int[] array = new int[3];
		List<Otri>[] array2 = new List<Otri>[mesh.vertices.Count];
		int i;
		for (i = 0; i < mesh.vertices.Count; i++)
		{
			Otri item2 = default(Otri);
			item2.tri = mesh.dummytri;
			array2[i] = new List<Otri>(3);
			array2[i].Add(item2);
		}
		i = 0;
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			item.tri = triangle;
			for (int j = 0; j < 3; j++)
			{
				array[j] = triangles[i].GetVertexID(j);
				if (array[j] < 0 || array[j] >= mesh.invertices)
				{
					Log.Instance.Error("Triangle has an invalid vertex index.", "MeshReader.Reconstruct()");
					throw new Exception("Triangle has an invalid vertex index.");
				}
			}
			item.tri.label = triangles[i].Label;
			if (mesh.behavior.VarArea)
			{
				item.tri.area = triangles[i].Area;
			}
			item.orient = 0;
			item.SetOrg(mesh.vertices[array[0]]);
			item.SetDest(mesh.vertices[array[1]]);
			item.SetApex(mesh.vertices[array[2]]);
			item.orient = 0;
			while (item.orient < 3)
			{
				int num = array[item.orient];
				int num2 = array2[num].Count - 1;
				Otri otri2 = array2[num][num2];
				array2[num].Add(item);
				otri = otri2;
				if (otri.tri.id != -1)
				{
					TriangleNet.Geometry.Vertex vertex = item.Dest();
					TriangleNet.Geometry.Vertex vertex2 = item.Apex();
					do
					{
						TriangleNet.Geometry.Vertex vertex3 = otri.Dest();
						TriangleNet.Geometry.Vertex vertex4 = otri.Apex();
						if (vertex2 == vertex3)
						{
							item.Lprev(ref ot);
							ot.Bond(ref otri);
						}
						if (vertex == vertex4)
						{
							otri.Lprev(ref ot2);
							item.Bond(ref ot2);
						}
						num2--;
						otri2 = array2[num][num2];
						otri = otri2;
					}
					while (otri.tri.id != -1);
				}
				item.orient++;
			}
			i++;
		}
		return array2;
	}

	private static void SetSegments(Mesh mesh, Polygon polygon, List<Otri>[] vertexarray)
	{
		Otri otri = default(Otri);
		Otri ot = default(Otri);
		Osub os = default(Osub);
		int num = 0;
		if (mesh.behavior.Poly)
		{
			int num2 = 0;
			int num3 = 0;
			foreach (SubSegment value in mesh.subsegs.Values)
			{
				os.seg = value;
				TriangleNet.Geometry.Vertex vertex = polygon.Segments[num3].GetVertex(0);
				TriangleNet.Geometry.Vertex vertex2 = polygon.Segments[num3].GetVertex(1);
				num2 = polygon.Segments[num3].Label;
				if (vertex.id < 0 || vertex.id >= mesh.invertices || vertex2.id < 0 || vertex2.id >= mesh.invertices)
				{
					Log.Instance.Error("Segment has an invalid vertex index.", "MeshReader.Reconstruct()");
					throw new Exception("Segment has an invalid vertex index.");
				}
				os.orient = 0;
				os.SetOrg(vertex);
				os.SetDest(vertex2);
				os.SetSegOrg(vertex);
				os.SetSegDest(vertex2);
				os.seg.boundary = num2;
				os.orient = 0;
				while (os.orient < 2)
				{
					int num4 = ((os.orient == 1) ? vertex.id : vertex2.id);
					int num5 = vertexarray[num4].Count - 1;
					Otri item = vertexarray[num4][num5];
					otri = vertexarray[num4][num5];
					TriangleNet.Geometry.Vertex vertex3 = os.Org();
					bool flag = true;
					while (flag && otri.tri.id != -1)
					{
						TriangleNet.Geometry.Vertex vertex4 = otri.Dest();
						if (vertex3 == vertex4)
						{
							vertexarray[num4].Remove(item);
							otri.SegBond(ref os);
							otri.Sym(ref ot);
							if (ot.tri.id == -1)
							{
								mesh.InsertSubseg(ref otri, 1);
								num++;
							}
							flag = false;
						}
						num5--;
						item = vertexarray[num4][num5];
						otri = vertexarray[num4][num5];
					}
					os.orient++;
				}
				num3++;
			}
		}
		for (int num3 = 0; num3 < mesh.vertices.Count; num3++)
		{
			int num6 = vertexarray[num3].Count - 1;
			otri = vertexarray[num3][num6];
			while (otri.tri.id != -1)
			{
				num6--;
				Otri otri2 = vertexarray[num3][num6];
				otri.SegDissolve(mesh.dummysub);
				otri.Sym(ref ot);
				if (ot.tri.id == -1)
				{
					mesh.InsertSubseg(ref otri, 1);
					num++;
				}
				otri = otri2;
			}
		}
		mesh.hullsize = num;
	}

	public static DcelMesh ToDCEL(Mesh mesh)
	{
		DcelMesh dcelMesh = new DcelMesh();
		TriangleNet.Topology.DCEL.Vertex[] array = new TriangleNet.Topology.DCEL.Vertex[mesh.vertices.Count];
		Face[] array2 = new Face[mesh.triangles.Count];
		dcelMesh.HalfEdges.Capacity = 2 * mesh.NumberOfEdges;
		mesh.Renumber();
		foreach (TriangleNet.Geometry.Vertex value in mesh.vertices.Values)
		{
			TriangleNet.Topology.DCEL.Vertex vertex = new TriangleNet.Topology.DCEL.Vertex(value.x, value.y);
			vertex.id = value.id;
			vertex.label = value.label;
			array[value.id] = vertex;
		}
		List<HalfEdge>[] array3 = new List<HalfEdge>[mesh.triangles.Count];
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			Face face = new Face(null);
			face.id = triangle.id;
			array2[triangle.id] = face;
			array3[triangle.id] = new List<HalfEdge>(3);
		}
		Otri otri = default(Otri);
		Otri ot = default(Otri);
		_ = mesh.triangles.Count;
		List<HalfEdge> halfEdges = dcelMesh.HalfEdges;
		int num = 0;
		Dictionary<int, HalfEdge> dictionary = new Dictionary<int, HalfEdge>();
		foreach (TriangleNet.Topology.Triangle triangle2 in mesh.triangles)
		{
			int id = triangle2.id;
			otri.tri = triangle2;
			for (int i = 0; i < 3; i++)
			{
				otri.orient = i;
				otri.Sym(ref ot);
				int id2 = ot.tri.id;
				if (id < id2 || id2 < 0)
				{
					Face face = array2[id];
					TriangleNet.Geometry.Vertex vertex2 = otri.Org();
					TriangleNet.Geometry.Vertex vertex3 = otri.Dest();
					HalfEdge halfEdge = new HalfEdge(array[vertex2.id], face);
					HalfEdge halfEdge2 = new HalfEdge(array[vertex3.id], (id2 < 0) ? Face.Empty : array2[id2]);
					array3[id].Add(halfEdge);
					if (id2 >= 0)
					{
						array3[id2].Add(halfEdge2);
					}
					else
					{
						dictionary.Add(vertex3.id, halfEdge2);
					}
					halfEdge.origin.leaving = halfEdge;
					halfEdge2.origin.leaving = halfEdge2;
					halfEdge.twin = halfEdge2;
					halfEdge2.twin = halfEdge;
					halfEdge.id = num++;
					halfEdge2.id = num++;
					halfEdges.Add(halfEdge);
					halfEdges.Add(halfEdge2);
				}
			}
		}
		List<HalfEdge>[] array4 = array3;
		foreach (List<HalfEdge> list in array4)
		{
			HalfEdge halfEdge = list[0];
			HalfEdge halfEdge3 = list[1];
			if (halfEdge.twin.origin.id == halfEdge3.origin.id)
			{
				halfEdge.next = halfEdge3;
				halfEdge3.next = list[2];
				list[2].next = halfEdge;
			}
			else
			{
				halfEdge.next = list[2];
				halfEdge3.next = halfEdge;
				list[2].next = halfEdge3;
			}
		}
		foreach (HalfEdge value2 in dictionary.Values)
		{
			value2.next = dictionary[value2.twin.origin.id];
		}
		dcelMesh.Vertices.AddRange(array);
		dcelMesh.Faces.AddRange(array2);
		return dcelMesh;
	}
}
