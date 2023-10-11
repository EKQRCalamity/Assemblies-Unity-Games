using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeV2
{
	public List<Vector2>[,] Partitions;

	public List<Vector2> FilteredResult;

	public float[] PartitionDepthLengths;

	public float[] PartitionDepthWidths;

	public int MaxPartitionDepthLevel;

	public int NumDivisions;

	public int PIndexMax;

	public float MainPartitionLengthX;

	public float PartitionGrainSizeX;

	public float HalfPartitionGrainSizeX;

	public float PartitionGrainSizeReciprocalX;

	public float HalfMainPartitionLengthX;

	public float MainPartitionLengthY;

	public float PartitionGrainSizeY;

	public float HalfPartitionGrainSizeY;

	public float PartitionGrainSizeReciprocalY;

	public float HalfMainPartitionLengthY;

	private float CenterY;

	public QuadTreeV2(Rect bounds, float desiredGrainSize)
	{
		float num = (float)Math.Sqrt(bounds.width * bounds.height);
		int num2 = 0;
		if (desiredGrainSize != 0f)
		{
			num2 = (int)(Math.Log10(num / desiredGrainSize) / Math.Log10(2.0) + 1.0);
		}
		MaxPartitionDepthLevel = num2;
		NumDivisions = Math.Min((int)Math.Pow(2.0, num2), 65534);
		PIndexMax = NumDivisions - 1;
		MainPartitionLengthX = bounds.width;
		HalfMainPartitionLengthX = MainPartitionLengthX * 0.5f;
		PartitionGrainSizeX = MainPartitionLengthX / (float)NumDivisions;
		PartitionGrainSizeReciprocalX = 1f / PartitionGrainSizeX;
		HalfPartitionGrainSizeX = PartitionGrainSizeX * 0.5f;
		MainPartitionLengthY = bounds.height;
		HalfMainPartitionLengthY = MainPartitionLengthY * 0.5f;
		PartitionGrainSizeY = MainPartitionLengthY / (float)NumDivisions;
		PartitionGrainSizeReciprocalY = 1f / PartitionGrainSizeY;
		HalfPartitionGrainSizeY = PartitionGrainSizeY * 0.5f;
		Partitions = new List<Vector2>[NumDivisions, NumDivisions];
		for (int i = 0; i < NumDivisions; i++)
		{
			for (int j = 0; j < NumDivisions; j++)
			{
				Partitions[i, j] = new List<Vector2>();
			}
		}
		PartitionDepthLengths = new float[MaxPartitionDepthLevel + 1];
		for (int k = 0; k < PartitionDepthLengths.Length; k++)
		{
			PartitionDepthLengths[k] = MainPartitionLengthX / (float)Math.Pow(2.0, k);
		}
		PartitionDepthWidths = new float[MaxPartitionDepthLevel + 1];
		for (int l = 1; l <= PartitionDepthWidths.Length; l++)
		{
			PartitionDepthWidths[l - 1] = MainPartitionLengthY / (float)Math.Pow(2.0, l);
		}
		FilteredResult = new List<Vector2>();
		Vector2 center = bounds.center;
		HalfMainPartitionLengthX -= center.x;
		HalfMainPartitionLengthY -= center.y;
		CenterY = center.y;
	}

	public QuadTreeV2(Rect bounds, float desiredGrainSize, List<Vector2> points)
		: this(bounds, desiredGrainSize)
	{
		for (int i = 0; i < points.Count; i++)
		{
			Add(points[i]);
		}
	}

	public void Filter(Vector2 position, float radius)
	{
		FilteredResult.Clear();
		int val = (int)((position.x - radius + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int val2 = (int)((position.x + radius + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int val3 = (int)((position.y - radius + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		int val4 = (int)((position.y + radius + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		val = Math.Max(0, val);
		val2 = Math.Min(PIndexMax, val2);
		val3 = Math.Max(0, val3);
		val4 = Math.Min(PIndexMax, val4);
		for (int i = val; i <= val2; i++)
		{
			for (int j = val3; j <= val4; j++)
			{
				List<Vector2> list = Partitions[i, j];
				for (int k = 0; k < list.Count; k++)
				{
					FilteredResult.Add(list[k]);
				}
			}
		}
	}

	public void FilterCircle(Vector2 position, float radius)
	{
		FilteredResult.Clear();
		float num = radius * radius;
		int val = (int)((position.x - radius + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int val2 = (int)((position.x + radius + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int val3 = (int)((position.y - radius + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		int val4 = (int)((position.y + radius + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		val = Math.Max(0, val);
		val2 = Math.Min(PIndexMax, val2);
		val3 = Math.Max(0, val3);
		val4 = Math.Min(PIndexMax, val4);
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		for (int i = val; i <= val2; i++)
		{
			for (int j = val3; j <= val4; j++)
			{
				List<Vector2> list = Partitions[i, j];
				for (int k = 0; k < list.Count; k++)
				{
					vector2 = list[k];
					vector.x = vector2.x - position.x;
					vector.y = vector2.y - position.y;
					if (vector.x * vector.x + vector.y * vector.y <= num)
					{
						FilteredResult.Add(vector2);
					}
				}
			}
		}
	}

	public void FilterHalfCircle(Vector2 position, Vector2 direction, float radius)
	{
		FilteredResult.Clear();
		float num = radius * radius;
		int val = (int)((position.x - radius + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int val2 = (int)((position.x + radius + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int val3 = (int)((position.y - radius + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		int val4 = (int)((position.y + radius + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		val = Math.Max(0, val);
		val2 = Math.Min(PIndexMax, val2);
		val3 = Math.Max(0, val3);
		val4 = Math.Min(PIndexMax, val4);
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		for (int i = val; i <= val2; i++)
		{
			for (int j = val3; j <= val4; j++)
			{
				List<Vector2> list = Partitions[i, j];
				for (int k = 0; k < list.Count; k++)
				{
					vector2 = list[k];
					vector.x = vector2.x - position.x;
					vector.y = vector2.y - position.y;
					if (vector.x * vector.x + vector.y * vector.y <= num && vector.x * direction.x + vector.y * direction.y >= 0f)
					{
						FilteredResult.Add(vector2);
					}
				}
			}
		}
	}

	public Vector2? ClosestInHalfCircle(Vector2 position, Vector2 direction, float radius)
	{
		Vector2? result = null;
		float num = radius * radius;
		int val = (int)((position.x - radius + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int val2 = (int)((position.x + radius + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int val3 = (int)((position.y - radius + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		int val4 = (int)((position.y + radius + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		val = Math.Max(0, val);
		val2 = Math.Min(PIndexMax, val2);
		val3 = Math.Max(0, val3);
		val4 = Math.Min(PIndexMax, val4);
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		float num2 = num;
		for (int i = val; i <= val2; i++)
		{
			for (int j = val3; j <= val4; j++)
			{
				List<Vector2> list = Partitions[i, j];
				for (int k = 0; k < list.Count; k++)
				{
					vector2 = list[k];
					vector.x = vector2.x - position.x;
					vector.y = vector2.y - position.y;
					float num3 = vector.x * vector.x + vector.y * vector.y;
					if (num3 <= num2 && vector.x * direction.x + vector.y * direction.y >= 0f)
					{
						num2 = num3;
						result = vector2;
					}
				}
			}
		}
		return result;
	}

	public void Filter(Vector2 position, Vector2 direction, float length, float width)
	{
		FilteredResult.Clear();
		Vector2 partitionPos = new Vector2(0f - HalfMainPartitionLengthX, CenterY);
		FilterRect(ref position, ref direction, length, width, partitionPos, 0);
	}

	public void FilterRectRay(Vector2 position, Vector2 direction, float length, float width)
	{
		FilteredResult.Clear();
		Vector2 partitionPos = new Vector2(0f - HalfMainPartitionLengthX, CenterY);
		FilterRect(ref position, ref direction, length, width, partitionPos, 0);
		for (int num = FilteredResult.Count - 1; num >= 0; num--)
		{
			if (!MathUtil.PointInRectRay2D(position, direction, length, width, FilteredResult[num]))
			{
				FilteredResult.RemoveAt(num);
			}
		}
	}

	protected void FilterRect(ref Vector2 pos, ref Vector2 dir, float length, float width, Vector2 partitionPos, int partitionDepth)
	{
		float num = PartitionDepthLengths[partitionDepth];
		float num2 = PartitionDepthWidths[partitionDepth];
		float num3 = num * 0.5f;
		float num4 = num2 * 0.5f;
		if (!MathUtil.RectRectSAT(pos, dir, length, width, partitionPos, Vector2.right, num, num2))
		{
			return;
		}
		if (partitionDepth < MaxPartitionDepthLevel)
		{
			partitionDepth++;
			FilterRect(ref pos, ref dir, length, width, new Vector2(partitionPos.x, partitionPos.y - num4), partitionDepth);
			FilterRect(ref pos, ref dir, length, width, new Vector2(partitionPos.x + num3, partitionPos.y - num4), partitionDepth);
			FilterRect(ref pos, ref dir, length, width, new Vector2(partitionPos.x, partitionPos.y + num4), partitionDepth);
			FilterRect(ref pos, ref dir, length, width, new Vector2(partitionPos.x + num3, partitionPos.y + num4), partitionDepth);
			return;
		}
		partitionPos.x += HalfPartitionGrainSizeX;
		int value = (int)((partitionPos.x + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int value2 = (int)((partitionPos.y + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		value = Mathf.Clamp(value, 0, PIndexMax);
		value2 = Mathf.Clamp(value2, 0, PIndexMax);
		List<Vector2> list = Partitions[value, value2];
		for (int i = 0; i < list.Count; i++)
		{
			FilteredResult.Add(list[i]);
		}
	}

	public void Add(Vector2 p)
	{
		int num = (int)((p.x + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int num2 = (int)((p.y + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		num = ((num >= 0) ? ((num > PIndexMax) ? PIndexMax : num) : 0);
		num2 = ((num2 >= 0) ? ((num2 > PIndexMax) ? PIndexMax : num2) : 0);
		Partitions[num, num2].Add(p);
	}

	public void Remove(Vector2 p)
	{
		int num = (int)((p.x + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int num2 = (int)((p.y + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		num = ((num >= 0) ? ((num > PIndexMax) ? PIndexMax : num) : 0);
		num2 = ((num2 >= 0) ? ((num2 > PIndexMax) ? PIndexMax : num2) : 0);
		Partitions[num, num2].Remove(p);
	}

	public void Clear()
	{
		for (int i = 0; i < NumDivisions; i++)
		{
			for (int j = 0; j < NumDivisions; j++)
			{
				Partitions[i, j].Clear();
			}
		}
	}

	public bool Contains(Vector2 p)
	{
		int num = (int)((p.x + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int num2 = (int)((p.y + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		num = ((num >= 0) ? ((num > PIndexMax) ? PIndexMax : num) : 0);
		num2 = ((num2 >= 0) ? ((num2 > PIndexMax) ? PIndexMax : num2) : 0);
		return Partitions[num, num2].Contains(p);
	}

	public Vector2 First()
	{
		for (int i = 0; i < NumDivisions; i++)
		{
			for (int j = 0; j < NumDivisions; j++)
			{
				if (Partitions[i, j].Count > 0)
				{
					return Partitions[i, j][0];
				}
			}
		}
		return Vector2.zero;
	}

	public int Count()
	{
		int num = 0;
		for (int i = 0; i < NumDivisions; i++)
		{
			for (int j = 0; j < NumDivisions; j++)
			{
				num += Partitions[i, j].Count;
			}
		}
		return num;
	}

	public List<Vector2> GetLexigraphicList()
	{
		List<Vector2> list = new List<Vector2>(Count());
		SortedList<Vector2, byte> sortedList = new SortedList<Vector2, byte>(LexigraphicComparer.Default);
		for (int i = 0; i < NumDivisions; i++)
		{
			for (int j = 0; j < NumDivisions; j++)
			{
				List<Vector2> list2 = Partitions[i, j];
				for (int k = 0; k < list2.Count; k++)
				{
					sortedList.Add(list2[k], 0);
				}
			}
			for (int l = 0; l < sortedList.Count; l++)
			{
				list.Add(sortedList.Keys[l]);
			}
			sortedList.Clear();
		}
		return list;
	}

	public void Unload()
	{
		for (int i = 0; i < NumDivisions; i++)
		{
			for (int j = 0; j < NumDivisions; j++)
			{
				Partitions[i, j].Clear();
				Partitions[i, j] = null;
			}
		}
		FilteredResult = null;
	}
}
