using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace TriangleNet.IO;

public class TriangleWriter
{
	private static NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;

	public void Write(Mesh mesh, string filename)
	{
		WritePoly(mesh, Path.ChangeExtension(filename, ".poly"));
		WriteElements(mesh, Path.ChangeExtension(filename, ".ele"));
	}

	public void WriteNodes(Mesh mesh, string filename)
	{
		using StreamWriter writer = new StreamWriter(filename);
		WriteNodes(writer, mesh);
	}

	private void WriteNodes(StreamWriter writer, Mesh mesh)
	{
		int num = mesh.vertices.Count;
		int nextras = mesh.nextras;
		Behavior behavior = mesh.behavior;
		if (behavior.Jettison)
		{
			num = mesh.vertices.Count - mesh.undeads;
		}
		if (writer == null)
		{
			return;
		}
		writer.WriteLine("{0} {1} {2} {3}", num, mesh.mesh_dim, nextras, behavior.UseBoundaryMarkers ? "1" : "0");
		if (mesh.numbering == NodeNumbering.None)
		{
			mesh.Renumber();
		}
		if (mesh.numbering == NodeNumbering.Linear)
		{
			WriteNodes(writer, mesh.vertices.Values, behavior.UseBoundaryMarkers, nextras, behavior.Jettison);
			return;
		}
		Vertex[] array = new Vertex[mesh.vertices.Count];
		foreach (Vertex value in mesh.vertices.Values)
		{
			array[value.id] = value;
		}
		WriteNodes(writer, array, behavior.UseBoundaryMarkers, nextras, behavior.Jettison);
	}

	private void WriteNodes(StreamWriter writer, IEnumerable<Vertex> nodes, bool markers, int attribs, bool jettison)
	{
		int num = 0;
		foreach (Vertex node in nodes)
		{
			if (!jettison || node.type != VertexType.UndeadVertex)
			{
				writer.Write("{0} {1} {2}", num, node.x.ToString(nfi), node.y.ToString(nfi));
				if (markers)
				{
					writer.Write(" {0}", node.label);
				}
				writer.WriteLine();
				num++;
			}
		}
	}

	public void WriteElements(Mesh mesh, string filename)
	{
		Otri otri = default(Otri);
		bool useRegions = mesh.behavior.useRegions;
		int num = 0;
		otri.orient = 0;
		using StreamWriter streamWriter = new StreamWriter(filename);
		streamWriter.WriteLine("{0} 3 {1}", mesh.triangles.Count, useRegions ? 1 : 0);
		foreach (TriangleNet.Topology.Triangle triangle2 in mesh.triangles)
		{
			TriangleNet.Topology.Triangle triangle = (otri.tri = triangle2);
			Vertex vertex = otri.Org();
			Vertex vertex2 = otri.Dest();
			Vertex vertex3 = otri.Apex();
			streamWriter.Write("{0} {1} {2} {3}", num, vertex.id, vertex2.id, vertex3.id);
			if (useRegions)
			{
				streamWriter.Write(" {0}", otri.tri.label);
			}
			streamWriter.WriteLine();
			triangle.id = num++;
		}
	}

	public void WritePoly(IPolygon polygon, string filename)
	{
		bool hasSegmentMarkers = polygon.HasSegmentMarkers;
		using StreamWriter streamWriter = new StreamWriter(filename);
		streamWriter.WriteLine("{0} 2 0 {1}", polygon.Points.Count, polygon.HasPointMarkers ? "1" : "0");
		WriteNodes(streamWriter, polygon.Points, polygon.HasPointMarkers, 0, jettison: false);
		streamWriter.WriteLine("{0} {1}", polygon.Segments.Count, hasSegmentMarkers ? "1" : "0");
		int num = 0;
		foreach (ISegment segment in polygon.Segments)
		{
			Vertex vertex = segment.GetVertex(0);
			Vertex vertex2 = segment.GetVertex(1);
			if (hasSegmentMarkers)
			{
				streamWriter.WriteLine("{0} {1} {2} {3}", num, vertex.ID, vertex2.ID, segment.Label);
			}
			else
			{
				streamWriter.WriteLine("{0} {1} {2}", num, vertex.ID, vertex2.ID);
			}
			num++;
		}
		num = 0;
		streamWriter.WriteLine("{0}", polygon.Holes.Count);
		foreach (Point hole in polygon.Holes)
		{
			streamWriter.WriteLine("{0} {1} {2}", num++, hole.X.ToString(nfi), hole.Y.ToString(nfi));
		}
		if (polygon.Regions.Count <= 0)
		{
			return;
		}
		num = 0;
		streamWriter.WriteLine("{0}", polygon.Regions.Count);
		foreach (RegionPointer region in polygon.Regions)
		{
			streamWriter.WriteLine("{0} {1} {2} {3}", num, region.point.X.ToString(nfi), region.point.Y.ToString(nfi), region.id);
			num++;
		}
	}

	public void WritePoly(Mesh mesh, string filename)
	{
		WritePoly(mesh, filename, writeNodes: true);
	}

	public void WritePoly(Mesh mesh, string filename, bool writeNodes)
	{
		Osub osub = default(Osub);
		bool useBoundaryMarkers = mesh.behavior.UseBoundaryMarkers;
		using StreamWriter streamWriter = new StreamWriter(filename);
		if (writeNodes)
		{
			WriteNodes(streamWriter, mesh);
		}
		else
		{
			streamWriter.WriteLine("0 {0} {1} {2}", mesh.mesh_dim, mesh.nextras, useBoundaryMarkers ? "1" : "0");
		}
		streamWriter.WriteLine("{0} {1}", mesh.subsegs.Count, useBoundaryMarkers ? "1" : "0");
		osub.orient = 0;
		int num = 0;
		foreach (SubSegment value in mesh.subsegs.Values)
		{
			osub.seg = value;
			Vertex vertex = osub.Org();
			Vertex vertex2 = osub.Dest();
			if (useBoundaryMarkers)
			{
				streamWriter.WriteLine("{0} {1} {2} {3}", num, vertex.id, vertex2.id, osub.seg.boundary);
			}
			else
			{
				streamWriter.WriteLine("{0} {1} {2}", num, vertex.id, vertex2.id);
			}
			num++;
		}
		num = 0;
		streamWriter.WriteLine("{0}", mesh.holes.Count);
		foreach (Point hole in mesh.holes)
		{
			streamWriter.WriteLine("{0} {1} {2}", num++, hole.X.ToString(nfi), hole.Y.ToString(nfi));
		}
		if (mesh.regions.Count <= 0)
		{
			return;
		}
		num = 0;
		streamWriter.WriteLine("{0}", mesh.regions.Count);
		foreach (RegionPointer region in mesh.regions)
		{
			streamWriter.WriteLine("{0} {1} {2} {3}", num, region.point.X.ToString(nfi), region.point.Y.ToString(nfi), region.id);
			num++;
		}
	}

	public void WriteEdges(Mesh mesh, string filename)
	{
		Otri otri = default(Otri);
		Otri ot = default(Otri);
		Osub os = default(Osub);
		Behavior behavior = mesh.behavior;
		using StreamWriter streamWriter = new StreamWriter(filename);
		streamWriter.WriteLine("{0} {1}", mesh.NumberOfEdges, behavior.UseBoundaryMarkers ? "1" : "0");
		long num = 0L;
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			otri.tri = triangle;
			otri.orient = 0;
			while (otri.orient < 3)
			{
				otri.Sym(ref ot);
				if (otri.tri.id < ot.tri.id || ot.tri.id == -1)
				{
					Vertex vertex = otri.Org();
					Vertex vertex2 = otri.Dest();
					if (behavior.UseBoundaryMarkers)
					{
						if (behavior.useSegments)
						{
							otri.Pivot(ref os);
							if (os.seg.hash == -1)
							{
								streamWriter.WriteLine("{0} {1} {2} {3}", num, vertex.id, vertex2.id, 0);
							}
							else
							{
								streamWriter.WriteLine("{0} {1} {2} {3}", num, vertex.id, vertex2.id, os.seg.boundary);
							}
						}
						else
						{
							streamWriter.WriteLine("{0} {1} {2} {3}", num, vertex.id, vertex2.id, (ot.tri.id == -1) ? "1" : "0");
						}
					}
					else
					{
						streamWriter.WriteLine("{0} {1} {2}", num, vertex.id, vertex2.id);
					}
					num++;
				}
				otri.orient++;
			}
		}
	}

	public void WriteNeighbors(Mesh mesh, string filename)
	{
		Otri otri = default(Otri);
		Otri ot = default(Otri);
		int num = 0;
		using StreamWriter streamWriter = new StreamWriter(filename);
		streamWriter.WriteLine("{0} 3", mesh.triangles.Count);
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			otri.tri = triangle;
			otri.orient = 1;
			otri.Sym(ref ot);
			int id = ot.tri.id;
			otri.orient = 2;
			otri.Sym(ref ot);
			int id2 = ot.tri.id;
			otri.orient = 0;
			otri.Sym(ref ot);
			int id3 = ot.tri.id;
			streamWriter.WriteLine("{0} {1} {2} {3}", num++, id, id2, id3);
		}
	}
}
