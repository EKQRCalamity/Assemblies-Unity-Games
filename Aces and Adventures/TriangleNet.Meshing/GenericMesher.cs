using System;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.IO;
using TriangleNet.Meshing.Algorithm;

namespace TriangleNet.Meshing;

public class GenericMesher
{
	private Configuration config;

	private ITriangulator triangulator;

	public GenericMesher()
		: this(new Dwyer(), new Configuration())
	{
	}

	public GenericMesher(ITriangulator triangulator)
		: this(triangulator, new Configuration())
	{
	}

	public GenericMesher(Configuration config)
		: this(new Dwyer(), config)
	{
	}

	public GenericMesher(ITriangulator triangulator, Configuration config)
	{
		this.config = config;
		this.triangulator = triangulator;
	}

	public IMesh Triangulate(IList<Vertex> points)
	{
		return triangulator.Triangulate(points, config);
	}

	public IMesh Triangulate(IPolygon polygon)
	{
		return Triangulate(polygon, null, null);
	}

	public IMesh Triangulate(IPolygon polygon, ConstraintOptions options)
	{
		return Triangulate(polygon, options, null);
	}

	public IMesh Triangulate(IPolygon polygon, QualityOptions quality)
	{
		return Triangulate(polygon, null, quality);
	}

	public IMesh Triangulate(IPolygon polygon, ConstraintOptions options, QualityOptions quality)
	{
		Mesh obj = (Mesh)triangulator.Triangulate(polygon.Points, config);
		ConstraintMesher constraintMesher = new ConstraintMesher(obj, config);
		QualityMesher qualityMesher = new QualityMesher(obj, config);
		obj.SetQualityMesher(qualityMesher);
		constraintMesher.Apply(polygon, options);
		qualityMesher.Apply(quality);
		return obj;
	}

	public static IMesh StructuredMesh(double width, double height, int nx, int ny)
	{
		if (width <= 0.0)
		{
			throw new ArgumentException("width");
		}
		if (height <= 0.0)
		{
			throw new ArgumentException("height");
		}
		return StructuredMesh(new Rectangle(0.0, 0.0, width, height), nx, ny);
	}

	public static IMesh StructuredMesh(Rectangle bounds, int nx, int ny)
	{
		Polygon polygon = new Polygon((nx + 1) * (ny + 1));
		double num = bounds.Width / (double)nx;
		double num2 = bounds.Height / (double)ny;
		double left = bounds.Left;
		double bottom = bounds.Bottom;
		int num3 = 0;
		Vertex[] array = new Vertex[(nx + 1) * (ny + 1)];
		for (int i = 0; i <= nx; i++)
		{
			double x = left + (double)i * num;
			for (int j = 0; j <= ny; j++)
			{
				double y = bottom + (double)j * num2;
				array[num3++] = new Vertex(x, y);
			}
		}
		polygon.Points.AddRange(array);
		num3 = 0;
		Vertex[] array2 = array;
		foreach (Vertex obj in array2)
		{
			obj.hash = (obj.id = num3++);
		}
		List<ISegment> segments = polygon.Segments;
		segments.Capacity = 2 * (nx + ny);
		for (int j = 0; j < ny; j++)
		{
			Vertex vertex = array[j];
			Vertex vertex2 = array[j + 1];
			segments.Add(new Segment(vertex, vertex2, 1));
			Vertex vertex3 = vertex;
			int k = (vertex2.Label = 1);
			vertex3.Label = k;
			vertex = array[nx * (ny + 1) + j];
			vertex2 = array[nx * (ny + 1) + (j + 1)];
			segments.Add(new Segment(vertex, vertex2, 1));
			Vertex vertex4 = vertex;
			k = (vertex2.Label = 1);
			vertex4.Label = k;
		}
		for (int i = 0; i < nx; i++)
		{
			Vertex vertex = array[(ny + 1) * i];
			Vertex vertex2 = array[(ny + 1) * (i + 1)];
			segments.Add(new Segment(vertex, vertex2, 1));
			Vertex vertex5 = vertex;
			int k = (vertex2.Label = 1);
			vertex5.Label = k;
			vertex = array[ny + (ny + 1) * i];
			vertex2 = array[ny + (ny + 1) * (i + 1)];
			segments.Add(new Segment(vertex, vertex2, 1));
			Vertex vertex6 = vertex;
			k = (vertex2.Label = 1);
			vertex6.Label = k;
		}
		InputTriangle[] array3 = new InputTriangle[2 * nx * ny];
		num3 = 0;
		for (int i = 0; i < nx; i++)
		{
			for (int j = 0; j < ny; j++)
			{
				int num8 = j + (ny + 1) * i;
				int num9 = j + (ny + 1) * (i + 1);
				if ((i + j) % 2 == 0)
				{
					array3[num3++] = new InputTriangle(num8, num9, num9 + 1);
					array3[num3++] = new InputTriangle(num8, num9 + 1, num8 + 1);
				}
				else
				{
					array3[num3++] = new InputTriangle(num8, num9, num8 + 1);
					array3[num3++] = new InputTriangle(num9, num9 + 1, num8 + 1);
				}
			}
		}
		ITriangle[] triangles = array3;
		return Converter.ToMesh(polygon, triangles);
	}
}
