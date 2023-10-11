using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace TriangleNet.IO;

internal class DebugWriter
{
	private static NumberFormatInfo nfi;

	private int iteration;

	private string session;

	private StreamWriter stream;

	private string tmpFile;

	private int[] vertices;

	private int triangles;

	private static readonly DebugWriter instance;

	public static DebugWriter Session => instance;

	static DebugWriter()
	{
		nfi = CultureInfo.InvariantCulture.NumberFormat;
		instance = new DebugWriter();
	}

	private DebugWriter()
	{
	}

	public void Start(string session)
	{
		iteration = 0;
		this.session = session;
		if (stream != null)
		{
			throw new Exception("A session is active. Finish before starting a new.");
		}
		tmpFile = Path.GetTempFileName();
		stream = new StreamWriter(tmpFile);
	}

	public void Write(Mesh mesh, bool skip = false)
	{
		WriteMesh(mesh, skip);
		triangles = mesh.Triangles.Count;
	}

	public void Finish()
	{
		Finish(session + ".mshx");
	}

	private void Finish(string path)
	{
		if (stream == null)
		{
			return;
		}
		stream.Flush();
		stream.Dispose();
		stream = null;
		string s = "#!N" + iteration + Environment.NewLine;
		using (FileStream fileStream = new FileStream(path, FileMode.Create))
		{
			using GZipStream gZipStream = new GZipStream(fileStream, CompressionMode.Compress, leaveOpen: false);
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			gZipStream.Write(bytes, 0, bytes.Length);
			bytes = File.ReadAllBytes(tmpFile);
			gZipStream.Write(bytes, 0, bytes.Length);
		}
		File.Delete(tmpFile);
	}

	private void WriteGeometry(IPolygon geometry)
	{
		stream.WriteLine("#!G{0}", iteration++);
	}

	private void WriteMesh(Mesh mesh, bool skip)
	{
		if (triangles == mesh.triangles.Count && skip)
		{
			return;
		}
		stream.WriteLine("#!M{0}", iteration++);
		if (VerticesChanged(mesh))
		{
			HashVertices(mesh);
			stream.WriteLine("{0}", mesh.vertices.Count);
			foreach (Vertex value in mesh.vertices.Values)
			{
				stream.WriteLine("{0} {1} {2} {3}", value.id, value.x.ToString(nfi), value.y.ToString(nfi), value.label);
			}
		}
		else
		{
			stream.WriteLine("0");
		}
		stream.WriteLine("{0}", mesh.subsegs.Count);
		Osub osub = default(Osub);
		osub.orient = 0;
		foreach (SubSegment value2 in mesh.subsegs.Values)
		{
			if (value2.hash > 0)
			{
				osub.seg = value2;
				Vertex vertex = osub.Org();
				Vertex vertex2 = osub.Dest();
				stream.WriteLine("{0} {1} {2} {3}", osub.seg.hash, vertex.id, vertex2.id, osub.seg.boundary);
			}
		}
		Otri otri = default(Otri);
		Otri ot = default(Otri);
		otri.orient = 0;
		stream.WriteLine("{0}", mesh.triangles.Count);
		foreach (TriangleNet.Topology.Triangle triangle in mesh.triangles)
		{
			otri.tri = triangle;
			Vertex vertex = otri.Org();
			Vertex vertex2 = otri.Dest();
			Vertex vertex3 = otri.Apex();
			int num = ((vertex == null) ? (-1) : vertex.id);
			int num2 = ((vertex2 == null) ? (-1) : vertex2.id);
			int num3 = ((vertex3 == null) ? (-1) : vertex3.id);
			stream.Write("{0} {1} {2} {3}", otri.tri.hash, num, num2, num3);
			otri.orient = 1;
			otri.Sym(ref ot);
			int hash = ot.tri.hash;
			otri.orient = 2;
			otri.Sym(ref ot);
			int hash2 = ot.tri.hash;
			otri.orient = 0;
			otri.Sym(ref ot);
			int hash3 = ot.tri.hash;
			stream.WriteLine(" {0} {1} {2}", hash, hash2, hash3);
		}
	}

	private bool VerticesChanged(Mesh mesh)
	{
		if (vertices == null || mesh.Vertices.Count != vertices.Length)
		{
			return true;
		}
		int num = 0;
		foreach (Vertex vertex in mesh.Vertices)
		{
			if (vertex.id != vertices[num++])
			{
				return true;
			}
		}
		return false;
	}

	private void HashVertices(Mesh mesh)
	{
		if (vertices == null || mesh.Vertices.Count != vertices.Length)
		{
			vertices = new int[mesh.Vertices.Count];
		}
		int num = 0;
		foreach (Vertex vertex in mesh.Vertices)
		{
			vertices[num++] = vertex.id;
		}
	}
}
