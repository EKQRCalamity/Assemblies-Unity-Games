using System;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

namespace TriangleNet.IO;

public static class FileProcessor
{
	private static List<IFileFormat> formats;

	static FileProcessor()
	{
		formats = new List<IFileFormat>();
		formats.Add(new TriangleFormat());
	}

	public static void Add(IFileFormat format)
	{
		formats.Add(format);
	}

	public static bool IsSupported(string file)
	{
		foreach (IFileFormat format in formats)
		{
			if (format.IsSupported(file))
			{
				return true;
			}
		}
		return false;
	}

	public static IPolygon Read(string filename)
	{
		foreach (IPolygonFormat format in formats)
		{
			if (format != null && format.IsSupported(filename))
			{
				return format.Read(filename);
			}
		}
		throw new Exception("File format not supported.");
	}

	public static void Write(IPolygon polygon, string filename)
	{
		foreach (IPolygonFormat format in formats)
		{
			if (format != null && format.IsSupported(filename))
			{
				format.Write(polygon, filename);
				return;
			}
		}
		throw new Exception("File format not supported.");
	}

	public static IMesh Import(string filename)
	{
		foreach (IMeshFormat format in formats)
		{
			if (format != null && format.IsSupported(filename))
			{
				return format.Import(filename);
			}
		}
		throw new Exception("File format not supported.");
	}

	public static void Write(IMesh mesh, string filename)
	{
		foreach (IMeshFormat format in formats)
		{
			if (format != null && format.IsSupported(filename))
			{
				format.Write(mesh, filename);
				return;
			}
		}
		throw new Exception("File format not supported.");
	}
}
