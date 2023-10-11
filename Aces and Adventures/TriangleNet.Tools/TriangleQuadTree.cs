using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;

namespace TriangleNet.Tools;

public class TriangleQuadTree
{
	private class QuadNode
	{
		private const int SW = 0;

		private const int SE = 1;

		private const int NW = 2;

		private const int NE = 3;

		private const double EPS = 1E-06;

		private static readonly byte[] BITVECTOR = new byte[4] { 1, 2, 4, 8 };

		private Rectangle bounds;

		private Point pivot;

		private TriangleQuadTree tree;

		private QuadNode[] regions;

		private List<int> triangles;

		private byte bitRegions;

		public QuadNode(Rectangle box, TriangleQuadTree tree)
			: this(box, tree, init: false)
		{
		}

		public QuadNode(Rectangle box, TriangleQuadTree tree, bool init)
		{
			this.tree = tree;
			bounds = new Rectangle(box.Left, box.Bottom, box.Width, box.Height);
			pivot = new Point((box.Left + box.Right) / 2.0, (box.Bottom + box.Top) / 2.0);
			bitRegions = 0;
			regions = new QuadNode[4];
			triangles = new List<int>();
			if (init)
			{
				int num = tree.triangles.Length;
				triangles.Capacity = num;
				for (int i = 0; i < num; i++)
				{
					triangles.Add(i);
				}
			}
		}

		public List<int> FindTriangles(Point searchPoint)
		{
			int num = FindRegion(searchPoint);
			if (regions[num] == null)
			{
				return triangles;
			}
			return regions[num].FindTriangles(searchPoint);
		}

		public void CreateSubRegion(int currentDepth)
		{
			double width = bounds.Right - pivot.x;
			double height = bounds.Top - pivot.y;
			Rectangle box = new Rectangle(bounds.Left, bounds.Bottom, width, height);
			regions[0] = new QuadNode(box, tree);
			box = new Rectangle(pivot.x, bounds.Bottom, width, height);
			regions[1] = new QuadNode(box, tree);
			box = new Rectangle(bounds.Left, pivot.y, width, height);
			regions[2] = new QuadNode(box, tree);
			box = new Rectangle(pivot.x, pivot.y, width, height);
			regions[3] = new QuadNode(box, tree);
			Point[] array = new Point[3];
			foreach (int triangle2 in triangles)
			{
				ITriangle triangle = tree.triangles[triangle2];
				array[0] = triangle.GetVertex(0);
				array[1] = triangle.GetVertex(1);
				array[2] = triangle.GetVertex(2);
				AddTriangleToRegion(array, triangle2);
			}
			for (int i = 0; i < 4; i++)
			{
				if (regions[i].triangles.Count > tree.sizeBound && currentDepth < tree.maxDepth)
				{
					regions[i].CreateSubRegion(currentDepth + 1);
				}
			}
		}

		private void AddTriangleToRegion(Point[] triangle, int index)
		{
			bitRegions = 0;
			if (IsPointInTriangle(pivot, triangle[0], triangle[1], triangle[2]))
			{
				AddToRegion(index, 0);
				AddToRegion(index, 1);
				AddToRegion(index, 2);
				AddToRegion(index, 3);
				return;
			}
			FindTriangleIntersections(triangle, index);
			if (bitRegions == 0)
			{
				int num = FindRegion(triangle[0]);
				regions[num].triangles.Add(index);
			}
		}

		private void FindTriangleIntersections(Point[] triangle, int index)
		{
			int num = 2;
			int num2 = 0;
			while (num2 < 3)
			{
				double num3 = triangle[num2].x - triangle[num].x;
				double num4 = triangle[num2].y - triangle[num].y;
				if (num3 != 0.0)
				{
					FindIntersectionsWithX(num3, num4, triangle, index, num);
				}
				if (num4 != 0.0)
				{
					FindIntersectionsWithY(num3, num4, triangle, index, num);
				}
				num = num2++;
			}
		}

		private void FindIntersectionsWithX(double dx, double dy, Point[] triangle, int index, int k)
		{
			double num = (pivot.x - triangle[k].x) / dx;
			if (num < 1.000001 && num > -1E-06)
			{
				double num2 = triangle[k].y + num * dy;
				if (num2 < pivot.y && num2 >= bounds.Bottom)
				{
					AddToRegion(index, 0);
					AddToRegion(index, 1);
				}
				else if (num2 <= bounds.Top)
				{
					AddToRegion(index, 2);
					AddToRegion(index, 3);
				}
			}
			num = (bounds.Left - triangle[k].x) / dx;
			if (num < 1.000001 && num > -1E-06)
			{
				double num3 = triangle[k].y + num * dy;
				if (num3 < pivot.y && num3 >= bounds.Bottom)
				{
					AddToRegion(index, 0);
				}
				else if (num3 <= bounds.Top)
				{
					AddToRegion(index, 2);
				}
			}
			num = (bounds.Right - triangle[k].x) / dx;
			if (num < 1.000001 && num > -1E-06)
			{
				double num4 = triangle[k].y + num * dy;
				if (num4 < pivot.y && num4 >= bounds.Bottom)
				{
					AddToRegion(index, 1);
				}
				else if (num4 <= bounds.Top)
				{
					AddToRegion(index, 3);
				}
			}
		}

		private void FindIntersectionsWithY(double dx, double dy, Point[] triangle, int index, int k)
		{
			double num = (pivot.y - triangle[k].y) / dy;
			if (num < 1.000001 && num > -1E-06)
			{
				double num2 = triangle[k].x + num * dx;
				if (num2 > pivot.x && num2 <= bounds.Right)
				{
					AddToRegion(index, 1);
					AddToRegion(index, 3);
				}
				else if (num2 >= bounds.Left)
				{
					AddToRegion(index, 0);
					AddToRegion(index, 2);
				}
			}
			num = (bounds.Bottom - triangle[k].y) / dy;
			if (num < 1.000001 && num > -1E-06)
			{
				double num2 = triangle[k].x + num * dx;
				if (num2 > pivot.x && num2 <= bounds.Right)
				{
					AddToRegion(index, 1);
				}
				else if (num2 >= bounds.Left)
				{
					AddToRegion(index, 0);
				}
			}
			num = (bounds.Top - triangle[k].y) / dy;
			if (num < 1.000001 && num > -1E-06)
			{
				double num2 = triangle[k].x + num * dx;
				if (num2 > pivot.x && num2 <= bounds.Right)
				{
					AddToRegion(index, 3);
				}
				else if (num2 >= bounds.Left)
				{
					AddToRegion(index, 2);
				}
			}
		}

		private int FindRegion(Point point)
		{
			int num = 2;
			if (point.y < pivot.y)
			{
				num = 0;
			}
			if (point.x > pivot.x)
			{
				num++;
			}
			return num;
		}

		private void AddToRegion(int index, int region)
		{
			if ((bitRegions & BITVECTOR[region]) == 0)
			{
				regions[region].triangles.Add(index);
				bitRegions |= BITVECTOR[region];
			}
		}
	}

	private QuadNode root;

	internal ITriangle[] triangles;

	internal int sizeBound;

	internal int maxDepth;

	public TriangleQuadTree(Mesh mesh, int maxDepth = 10, int sizeBound = 10)
	{
		this.maxDepth = maxDepth;
		this.sizeBound = sizeBound;
		ITriangle[] array = mesh.Triangles.ToArray();
		triangles = array;
		int num = 0;
		root = new QuadNode(mesh.Bounds, this, init: true);
		root.CreateSubRegion(++num);
	}

	public ITriangle Query(double x, double y)
	{
		Point point = new Point(x, y);
		foreach (int item in root.FindTriangles(point))
		{
			ITriangle triangle = triangles[item];
			if (IsPointInTriangle(point, triangle.GetVertex(0), triangle.GetVertex(1), triangle.GetVertex(2)))
			{
				return triangle;
			}
		}
		return null;
	}

	internal static bool IsPointInTriangle(Point p, Point t0, Point t1, Point t2)
	{
		Point point = new Point(t1.x - t0.x, t1.y - t0.y);
		Point point2 = new Point(t2.x - t0.x, t2.y - t0.y);
		Point p2 = new Point(p.x - t0.x, p.y - t0.y);
		Point q = new Point(0.0 - point.y, point.x);
		Point q2 = new Point(0.0 - point2.y, point2.x);
		double num = DotProduct(p2, q2) / DotProduct(point, q2);
		double num2 = DotProduct(p2, q) / DotProduct(point2, q);
		if (num >= 0.0 && num2 >= 0.0 && num + num2 <= 1.0)
		{
			return true;
		}
		return false;
	}

	internal static double DotProduct(Point p, Point q)
	{
		return p.x * q.x + p.y * q.y;
	}
}
