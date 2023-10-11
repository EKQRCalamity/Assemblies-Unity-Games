using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TriangleNet.Geometry;

namespace TriangleNet.IO;

public class TriangleReader
{
	private static NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;

	private int startIndex;

	public static bool IsNullOrWhiteSpace(string value)
	{
		if (value == null)
		{
			return true;
		}
		return string.IsNullOrEmpty(value.Trim());
	}

	private bool TryReadLine(StreamReader reader, out string[] token)
	{
		token = null;
		if (reader.EndOfStream)
		{
			return false;
		}
		string text = reader.ReadLine().Trim();
		while (IsNullOrWhiteSpace(text) || text.StartsWith("#"))
		{
			if (reader.EndOfStream)
			{
				return false;
			}
			text = reader.ReadLine().Trim();
		}
		token = text.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		return true;
	}

	private void ReadVertex(List<Vertex> data, int index, string[] line, int attributes, int marks)
	{
		double x = double.Parse(line[1], nfi);
		double y = double.Parse(line[2], nfi);
		Vertex vertex = new Vertex(x, y);
		if (marks > 0 && line.Length > 3 + attributes)
		{
			vertex.Label = int.Parse(line[3 + attributes]);
		}
		_ = 0;
		data.Add(vertex);
	}

	public void Read(string filename, out Polygon polygon)
	{
		polygon = null;
		string text = Path.ChangeExtension(filename, ".poly");
		if (File.Exists(text))
		{
			polygon = ReadPolyFile(text);
			return;
		}
		text = Path.ChangeExtension(filename, ".node");
		polygon = ReadNodeFile(text);
	}

	public void Read(string filename, out Polygon geometry, out List<ITriangle> triangles)
	{
		triangles = null;
		Read(filename, out geometry);
		string text = Path.ChangeExtension(filename, ".ele");
		if (File.Exists(text) && geometry != null)
		{
			triangles = ReadEleFile(text);
		}
	}

	public IPolygon Read(string filename)
	{
		Polygon polygon = null;
		Read(filename, out polygon);
		return polygon;
	}

	public Polygon ReadNodeFile(string nodefilename)
	{
		return ReadNodeFile(nodefilename, readElements: false);
	}

	public Polygon ReadNodeFile(string nodefilename, bool readElements)
	{
		startIndex = 0;
		int num = 0;
		int attributes = 0;
		int marks = 0;
		Polygon polygon;
		using (StreamReader reader = new StreamReader(nodefilename))
		{
			if (!TryReadLine(reader, out var token))
			{
				throw new Exception("Can't read input file.");
			}
			num = int.Parse(token[0]);
			if (num < 3)
			{
				throw new Exception("Input must have at least three input vertices.");
			}
			if (token.Length > 1 && int.Parse(token[1]) != 2)
			{
				throw new Exception("Triangle only works with two-dimensional meshes.");
			}
			if (token.Length > 2)
			{
				attributes = int.Parse(token[2]);
			}
			if (token.Length > 3)
			{
				marks = int.Parse(token[3]);
			}
			polygon = new Polygon(num);
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					if (!TryReadLine(reader, out token))
					{
						throw new Exception("Can't read input file (vertices).");
					}
					if (token.Length < 3)
					{
						throw new Exception("Invalid vertex.");
					}
					if (i == 0)
					{
						startIndex = int.Parse(token[0], nfi);
					}
					ReadVertex(polygon.Points, i, token, attributes, marks);
				}
			}
		}
		if (readElements)
		{
			string text = Path.ChangeExtension(nodefilename, ".ele");
			if (File.Exists(text))
			{
				ReadEleFile(text, readArea: true);
			}
		}
		return polygon;
	}

	public Polygon ReadPolyFile(string polyfilename)
	{
		return ReadPolyFile(polyfilename, readElements: false, readArea: false);
	}

	public Polygon ReadPolyFile(string polyfilename, bool readElements)
	{
		return ReadPolyFile(polyfilename, readElements, readArea: false);
	}

	public Polygon ReadPolyFile(string polyfilename, bool readElements, bool readArea)
	{
		startIndex = 0;
		int num = 0;
		int attributes = 0;
		int marks = 0;
		Polygon polygon;
		using (StreamReader reader = new StreamReader(polyfilename))
		{
			if (!TryReadLine(reader, out var token))
			{
				throw new Exception("Can't read input file.");
			}
			num = int.Parse(token[0]);
			if (token.Length > 1 && int.Parse(token[1]) != 2)
			{
				throw new Exception("Triangle only works with two-dimensional meshes.");
			}
			if (token.Length > 2)
			{
				attributes = int.Parse(token[2]);
			}
			if (token.Length > 3)
			{
				marks = int.Parse(token[3]);
			}
			if (num > 0)
			{
				polygon = new Polygon(num);
				for (int i = 0; i < num; i++)
				{
					if (!TryReadLine(reader, out token))
					{
						throw new Exception("Can't read input file (vertices).");
					}
					if (token.Length < 3)
					{
						throw new Exception("Invalid vertex.");
					}
					if (i == 0)
					{
						startIndex = int.Parse(token[0], nfi);
					}
					ReadVertex(polygon.Points, i, token, attributes, marks);
				}
			}
			else
			{
				polygon = ReadNodeFile(Path.ChangeExtension(polyfilename, ".node"));
				num = polygon.Points.Count;
			}
			List<Vertex> points = polygon.Points;
			if (points.Count == 0)
			{
				throw new Exception("No nodes available.");
			}
			if (!TryReadLine(reader, out token))
			{
				throw new Exception("Can't read input file (segments).");
			}
			int num2 = int.Parse(token[0]);
			int num3 = 0;
			if (token.Length > 1)
			{
				num3 = int.Parse(token[1]);
			}
			for (int j = 0; j < num2; j++)
			{
				if (!TryReadLine(reader, out token))
				{
					throw new Exception("Can't read input file (segments).");
				}
				if (token.Length < 3)
				{
					throw new Exception("Segment has no endpoints.");
				}
				int num4 = int.Parse(token[1]) - startIndex;
				int num5 = int.Parse(token[2]) - startIndex;
				int label = 0;
				if (num3 > 0 && token.Length > 3)
				{
					label = int.Parse(token[3]);
				}
				if (num4 < 0 || num4 >= num)
				{
					if (Log.Verbose)
					{
						Log.Instance.Warning("Invalid first endpoint of segment.", "MeshReader.ReadPolyfile()");
					}
				}
				else if (num5 < 0 || num5 >= num)
				{
					if (Log.Verbose)
					{
						Log.Instance.Warning("Invalid second endpoint of segment.", "MeshReader.ReadPolyfile()");
					}
				}
				else
				{
					polygon.Add(new Segment(points[num4], points[num5], label));
				}
			}
			if (!TryReadLine(reader, out token))
			{
				throw new Exception("Can't read input file (holes).");
			}
			int num6 = int.Parse(token[0]);
			if (num6 > 0)
			{
				for (int k = 0; k < num6; k++)
				{
					if (!TryReadLine(reader, out token))
					{
						throw new Exception("Can't read input file (holes).");
					}
					if (token.Length < 3)
					{
						throw new Exception("Invalid hole.");
					}
					polygon.Holes.Add(new Point(double.Parse(token[1], nfi), double.Parse(token[2], nfi)));
				}
			}
			if (TryReadLine(reader, out token))
			{
				int num7 = int.Parse(token[0]);
				if (num7 > 0)
				{
					for (int l = 0; l < num7; l++)
					{
						if (!TryReadLine(reader, out token))
						{
							throw new Exception("Can't read input file (region).");
						}
						if (token.Length < 4)
						{
							throw new Exception("Invalid region attributes.");
						}
						if (!int.TryParse(token[3], out var result))
						{
							result = l;
						}
						double result2 = 0.0;
						if (token.Length > 4)
						{
							double.TryParse(token[4], NumberStyles.Number, nfi, out result2);
						}
						polygon.Regions.Add(new RegionPointer(double.Parse(token[1], nfi), double.Parse(token[2], nfi), result, result2));
					}
				}
			}
		}
		if (readElements)
		{
			string text = Path.ChangeExtension(polyfilename, ".ele");
			if (File.Exists(text))
			{
				ReadEleFile(text, readArea);
			}
		}
		return polygon;
	}

	public List<ITriangle> ReadEleFile(string elefilename)
	{
		return ReadEleFile(elefilename, readArea: false);
	}

	private List<ITriangle> ReadEleFile(string elefilename, bool readArea)
	{
		int num = 0;
		int num2 = 0;
		List<ITriangle> list;
		using (StreamReader reader = new StreamReader(elefilename))
		{
			bool flag = false;
			if (!TryReadLine(reader, out var token))
			{
				throw new Exception("Can't read input file (elements).");
			}
			num = int.Parse(token[0]);
			num2 = 0;
			if (token.Length > 2)
			{
				num2 = int.Parse(token[2]);
				flag = true;
			}
			if (num2 > 1)
			{
				Log.Instance.Warning("Triangle attributes not supported.", "FileReader.Read");
			}
			list = new List<ITriangle>(num);
			for (int i = 0; i < num; i++)
			{
				if (!TryReadLine(reader, out token))
				{
					throw new Exception("Can't read input file (elements).");
				}
				if (token.Length < 4)
				{
					throw new Exception("Triangle has no nodes.");
				}
				InputTriangle inputTriangle = new InputTriangle(int.Parse(token[1]) - startIndex, int.Parse(token[2]) - startIndex, int.Parse(token[3]) - startIndex);
				if (num2 > 0 && flag)
				{
					int result = 0;
					flag = int.TryParse(token[4], out result);
					inputTriangle.label = result;
				}
				list.Add(inputTriangle);
			}
		}
		if (readArea)
		{
			string text = Path.ChangeExtension(elefilename, ".area");
			if (File.Exists(text))
			{
				ReadAreaFile(text, num);
			}
		}
		return list;
	}

	private double[] ReadAreaFile(string areafilename, int intriangles)
	{
		double[] array = null;
		using StreamReader reader = new StreamReader(areafilename);
		if (!TryReadLine(reader, out var token))
		{
			throw new Exception("Can't read input file (area).");
		}
		if (int.Parse(token[0]) != intriangles)
		{
			Log.Instance.Warning("Number of area constraints doesn't match number of triangles.", "ReadAreaFile()");
			return null;
		}
		array = new double[intriangles];
		for (int i = 0; i < intriangles; i++)
		{
			if (!TryReadLine(reader, out token))
			{
				throw new Exception("Can't read input file (area).");
			}
			if (token.Length != 2)
			{
				throw new Exception("Triangle has no nodes.");
			}
			array[i] = double.Parse(token[1], nfi);
		}
		return array;
	}

	public List<Edge> ReadEdgeFile(string edgeFile, int invertices)
	{
		List<Edge> list = null;
		startIndex = 0;
		using StreamReader reader = new StreamReader(edgeFile);
		if (!TryReadLine(reader, out var token))
		{
			throw new Exception("Can't read input file (segments).");
		}
		int num = int.Parse(token[0]);
		int num2 = 0;
		if (token.Length > 1)
		{
			num2 = int.Parse(token[1]);
		}
		if (num > 0)
		{
			list = new List<Edge>(num);
		}
		for (int i = 0; i < num; i++)
		{
			if (!TryReadLine(reader, out token))
			{
				throw new Exception("Can't read input file (segments).");
			}
			if (token.Length < 3)
			{
				throw new Exception("Segment has no endpoints.");
			}
			int num3 = int.Parse(token[1]) - startIndex;
			int num4 = int.Parse(token[2]) - startIndex;
			int label = 0;
			if (num2 > 0 && token.Length > 3)
			{
				label = int.Parse(token[3]);
			}
			if (num3 < 0 || num3 >= invertices)
			{
				if (Log.Verbose)
				{
					Log.Instance.Warning("Invalid first endpoint of segment.", "MeshReader.ReadPolyfile()");
				}
			}
			else if (num4 < 0 || num4 >= invertices)
			{
				if (Log.Verbose)
				{
					Log.Instance.Warning("Invalid second endpoint of segment.", "MeshReader.ReadPolyfile()");
				}
			}
			else
			{
				list.Add(new Edge(num3, num4, label));
			}
		}
		return list;
	}
}
