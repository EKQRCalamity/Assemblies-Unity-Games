using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree<T>
{
	private Func<T, bool> FilterCheck;

	public readonly List<T>[,] Partitions;

	public List<T> FilteredResult;

	private HashSet<T> HashFilter;

	private float[] PartitionDepthLengths;

	private float[] PartitionDepthWidths;

	private int MaxPartitionDepthLevel;

	private int NumDivisions;

	private int PIndexMax;

	private float MainPartitionLengthX;

	private float PartitionGrainSizeX;

	private float HalfPartitionGrainSizeX;

	private float PartitionGrainSizeReciprocalX;

	private float HalfMainPartitionLengthX;

	private float MainPartitionLengthY;

	private float PartitionGrainSizeY;

	private float PartitionGrainSizeReciprocalY;

	private float HalfMainPartitionLengthY;

	private float CenterY;

	public readonly Rect bounds;

	private Action<T, ushort, ushort> _addToPartions;

	private Action<T> _removeFromTree;

	public QuadTree(Rect bounds, float desiredGrainSize, Func<T, bool> filterCheck = null)
	{
		this.bounds = bounds;
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
		Partitions = new List<T>[NumDivisions, NumDivisions];
		for (int i = 0; i < NumDivisions; i++)
		{
			for (int j = 0; j < NumDivisions; j++)
			{
				Partitions[i, j] = new List<T>();
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
		FilteredResult = new List<T>();
		HashFilter = new HashSet<T>();
		FilterCheck = filterCheck;
		Vector2 center = bounds.center;
		HalfMainPartitionLengthX -= center.x;
		HalfMainPartitionLengthY -= center.y;
		CenterY = center.y;
		if (typeof(IQuadTreeObject<T>).IsAssignableFrom(typeof(T)))
		{
			_addToPartions = delegate(T obj, ushort x, ushort y)
			{
				(obj as IQuadTreeObject<T>).AddToPartition(x, y);
			};
			_removeFromTree = delegate(T obj)
			{
				(obj as IQuadTreeObject<T>).RemoveFromTree(this);
			};
		}
		else
		{
			_addToPartions = delegate
			{
			};
			_removeFromTree = delegate
			{
			};
		}
	}

	public IEnumerable<T> Filter(SRect rect)
	{
		int num = (int)(((float)rect.min.x + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int maxXIndex = (int)(((float)rect.max.x + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int minYIndex = (int)(((float)rect.min.y + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		int maxYIndex = (int)(((float)rect.max.y + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		num = ((num > 0) ? num : 0);
		maxXIndex = ((maxXIndex < PIndexMax) ? maxXIndex : PIndexMax);
		minYIndex = ((minYIndex > 0) ? minYIndex : 0);
		maxYIndex = ((maxYIndex < PIndexMax) ? maxYIndex : PIndexMax);
		HashSet<T> hash = new HashSet<T>();
		for (int x = num; x <= maxXIndex; x++)
		{
			for (int y = minYIndex; y <= maxYIndex; y++)
			{
				List<T> objects = Partitions[x, y];
				for (int z = 0; z < objects.Count; z++)
				{
					T val = objects[z];
					if (hash.Add(val) && (FilterCheck == null || FilterCheck(val)))
					{
						yield return val;
					}
				}
			}
		}
	}

	public void Filter(Vector2 position, float radius)
	{
		FilteredResult.Clear();
		HashFilter.Clear();
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
				List<T> list = Partitions[i, j];
				for (int k = 0; k < list.Count; k++)
				{
					T val5 = list[k];
					if (HashFilter.Add(val5) && (FilterCheck == null || FilterCheck(val5)))
					{
						FilteredResult.Add(val5);
					}
				}
			}
		}
	}

	public void Filter(Vector2 position, Vector2 direction, float length, float width)
	{
		FilteredResult.Clear();
		HashFilter.Clear();
		Vector2 partitionPos = new Vector2(0f - HalfMainPartitionLengthX, CenterY);
		FilterRect(ref position, ref direction, length, width, partitionPos, 0);
	}

	public void Filter(Vector2 startPosition, Vector2 endPosition, float width)
	{
		Vector2 direction = default(Vector2);
		direction.x = endPosition.x - startPosition.x;
		direction.y = endPosition.y - startPosition.y;
		float num = (float)Math.Sqrt(direction.x * direction.x + direction.y * direction.y) + MathUtil.Epsilon;
		direction.x /= num;
		direction.y /= num;
		Filter(startPosition, direction, num + width, width);
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
		List<T> list = Partitions[value, value2];
		for (int i = 0; i < list.Count; i++)
		{
			T val = list[i];
			if (HashFilter.Add(val) && (FilterCheck == null || FilterCheck(val)))
			{
				FilteredResult.Add(val);
			}
		}
	}

	public void Add(T obj, SRect rect)
	{
		int num = (int)(((float)rect.min.x + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int num2 = (int)(((float)rect.max.x + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int num3 = (int)(((float)rect.min.y + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		int num4 = (int)(((float)rect.max.y + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		num = ((num > 0) ? num : 0);
		num2 = ((num2 < PIndexMax) ? num2 : PIndexMax);
		num3 = ((num3 > 0) ? num3 : 0);
		num4 = ((num4 < PIndexMax) ? num4 : PIndexMax);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				Partitions[i, j].Add(obj);
			}
		}
	}

	public void Remove(T obj, SRect rect)
	{
		int num = (int)(((float)rect.min.x + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int num2 = (int)(((float)rect.max.x + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
		int num3 = (int)(((float)rect.min.y + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		int num4 = (int)(((float)rect.max.y + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
		num = ((num > 0) ? num : 0);
		num2 = ((num2 < PIndexMax) ? num2 : PIndexMax);
		num3 = ((num3 > 0) ? num3 : 0);
		num4 = ((num4 < PIndexMax) ? num4 : PIndexMax);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				Partitions[i, j].Remove(obj);
			}
		}
	}

	public void Add(T obj, Vector2 position, float radius)
	{
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
				Partitions[i, j].Add(obj);
				_addToPartions(obj, (ushort)i, (ushort)j);
			}
		}
	}

	public void Add(T obj, Vector2 position, Vector2 dir, float length, float width)
	{
		Vector2 partitionPos = new Vector2(0f - HalfMainPartitionLengthX, CenterY);
		AddRect(obj, ref position, ref dir, length, width, partitionPos, 0);
	}

	protected void AddRect(T obj, ref Vector2 pos, ref Vector2 dir, float length, float width, Vector2 partitionPos, int partitionDepth)
	{
		float num = PartitionDepthLengths[partitionDepth];
		float num2 = PartitionDepthWidths[partitionDepth];
		float num3 = num * 0.5f;
		float num4 = num2 * 0.5f;
		if (MathUtil.RectRectSAT(pos, dir, length, width, partitionPos, Vector2.right, num, num2))
		{
			if (partitionDepth < MaxPartitionDepthLevel)
			{
				partitionDepth++;
				AddRect(obj, ref pos, ref dir, length, width, new Vector2(partitionPos.x, partitionPos.y - num4), partitionDepth);
				AddRect(obj, ref pos, ref dir, length, width, new Vector2(partitionPos.x + num3, partitionPos.y - num4), partitionDepth);
				AddRect(obj, ref pos, ref dir, length, width, new Vector2(partitionPos.x, partitionPos.y + num4), partitionDepth);
				AddRect(obj, ref pos, ref dir, length, width, new Vector2(partitionPos.x + num3, partitionPos.y + num4), partitionDepth);
			}
			else
			{
				partitionPos.x += HalfPartitionGrainSizeX;
				int value = (int)((partitionPos.x + HalfMainPartitionLengthX) * PartitionGrainSizeReciprocalX);
				int value2 = (int)((partitionPos.y + HalfMainPartitionLengthY) * PartitionGrainSizeReciprocalY);
				value = Mathf.Clamp(value, 0, PIndexMax);
				value2 = Mathf.Clamp(value2, 0, PIndexMax);
				Partitions[value, value2].Add(obj);
				_addToPartions(obj, (ushort)value, (ushort)value2);
			}
		}
	}

	public void Remove(T obj)
	{
		_removeFromTree(obj);
	}

	public IEnumerable<T> UniqueObjects()
	{
		HashSet<T> hash = new HashSet<T>();
		for (int x = 0; x < Partitions.GetLength(0); x++)
		{
			for (int y = 0; y < Partitions.GetLength(1); y++)
			{
				for (int z = 0; z < Partitions[x, y].Count; z++)
				{
					if (hash.Add(Partitions[x, y][z]))
					{
						yield return Partitions[x, y][z];
					}
				}
			}
		}
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
		FilterCheck = null;
		FilteredResult = null;
		HashFilter = null;
	}
}
