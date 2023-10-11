using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract(IgnoreListHandling = true)]
public struct SRect
{
	public static readonly SRect Zero = new SRect(Short2.Zero, Short2.Zero);

	public static readonly SRect All = new SRect(Short2.MinValue, Short2.MaxValue);

	[ProtoMember(1)]
	public readonly Short2 min;

	[ProtoMember(2)]
	public readonly Short2 max;

	public short left => min.x;

	public short right => max.x;

	public short bottom => min.y;

	public short top => max.y;

	public Short2 center => new Short2((min.x + max.x) / 2, (min.y + max.y) / 2);

	public int width => max.x - min.x + 1;

	public int height => max.y - min.y + 1;

	public int area => width * height;

	public Short2 size => new Short2(width, height);

	public Vector2 extents => new Vector2((float)width * 0.5f, (float)height * 0.5f);

	public int this[int index]
	{
		get
		{
			if (index != 0)
			{
				return height;
			}
			return width;
		}
	}

	public static SRect FromPoints(params Short2[] points)
	{
		return FromPoints((IEnumerable<Short2>)points);
	}

	public static SRect FromPoints(IEnumerable<Short2> points)
	{
		short num = short.MaxValue;
		short num2 = short.MinValue;
		short num3 = short.MaxValue;
		short num4 = short.MinValue;
		foreach (Short2 point in points)
		{
			num = ((point.x < num) ? point.x : num);
			num2 = ((point.x > num2) ? point.x : num2);
			num3 = ((point.y < num3) ? point.y : num3);
			num4 = ((point.y > num4) ? point.y : num4);
		}
		if (num > num2 || num3 > num4)
		{
			return default(SRect);
		}
		return new SRect(new Short2(num, num3), new Short2(num2, num4));
	}

	public SRect(IEnumerable<Short2> points)
	{
		short num = short.MaxValue;
		short num2 = short.MinValue;
		short num3 = short.MaxValue;
		short num4 = short.MinValue;
		foreach (Short2 point in points)
		{
			num = ((point.x < num) ? point.x : num);
			num2 = ((point.x > num2) ? point.x : num2);
			num3 = ((point.y < num3) ? point.y : num3);
			num4 = ((point.y > num4) ? point.y : num4);
		}
		min = new Short2(num, num3);
		max = new Short2(num2, num4);
	}

	public SRect(Short2 min, Short2 max)
	{
		this.min = min;
		this.max = max;
	}

	public SRect(Couple<Short2, Short2> points)
		: this(points.a.Min(points.b), points.a.Max(points.b))
	{
	}

	public bool Intersects(SRect r)
	{
		if (min.x > r.max.x || max.x < r.min.x || min.y > r.max.y || max.y < r.min.y)
		{
			return this == r;
		}
		return true;
	}

	public SRect Intersection(SRect r)
	{
		short x = ((min.x > r.min.x) ? min.x : r.min.x);
		short x2 = ((max.x < r.max.x) ? max.x : r.max.x);
		short y = ((min.y > r.min.y) ? min.y : r.min.y);
		return new SRect(max: new Short2(x2, (max.y < r.max.y) ? max.y : r.max.y), min: new Short2(x, y));
	}

	public bool Contains(SRect r)
	{
		if (min.x <= r.min.x && max.x >= r.max.x && min.y <= r.min.y)
		{
			return max.y >= r.max.y;
		}
		return false;
	}

	public bool Contains(Short2 p)
	{
		if (p.x >= min.x && p.y >= min.y && p.x <= max.x)
		{
			return p.y <= max.y;
		}
		return false;
	}

	public SRect Translate(Short2 t)
	{
		return new SRect(new Short2(min.x + t.x, min.y + t.y), new Short2(max.x + t.x, max.y + t.y));
	}

	public SRect Add(SRect r)
	{
		short x = ((min.x < r.min.x) ? min.x : r.min.x);
		short x2 = ((max.x > r.max.x) ? max.x : r.max.x);
		short y = ((min.y < r.min.y) ? min.y : r.min.y);
		return new SRect(max: new Short2(x2, (max.y > r.max.y) ? max.y : r.max.y), min: new Short2(x, y));
	}

	public SRect Add(Short2 p)
	{
		return Add(p.ToSRect());
	}

	public SRect[] Subtract(SRect r)
	{
		SRect sRect = Intersection(r);
		if (sRect == this)
		{
			return new SRect[1] { Zero };
		}
		if (sRect.area <= 0)
		{
			return new SRect[1] { this };
		}
		int num = ((sRect.min.x != min.x) ? 1 : 0);
		int num2 = ((sRect.max.x != max.x) ? 1 : 0);
		int num3 = ((sRect.min.y != min.y) ? 1 : 0);
		int num4 = ((sRect.max.y != max.y) ? 1 : 0);
		int num5 = 0;
		SRect[] array = new SRect[num + num2 + num3 + num4];
		if (num2 == 1)
		{
			array[num5++] = new SRect(new Short2(sRect.max.x, min.y), max);
		}
		if (num4 == 1)
		{
			array[num5++] = new SRect(new Short2(sRect.min.x, sRect.max.y), new Short2(sRect.max.x, max.y));
		}
		if (num == 1)
		{
			array[num5++] = new SRect(min, new Short2(sRect.min.x, max.y));
		}
		if (num3 == 1)
		{
			array[num5] = new SRect(new Short2(sRect.min.x, min.y), new Short2(sRect.max.x, sRect.min.y));
		}
		return array;
	}

	public SRect[] Subtract(Short2 p)
	{
		return Subtract(p.ToSRect());
	}

	public SRect Pad(Short2 padAmount)
	{
		return new SRect(min - padAmount, max + padAmount);
	}

	public SRect[] Invert()
	{
		return All.Subtract(this);
	}

	public int MaxComponentDistanceToPoint(Short2 point)
	{
		int num = 0;
		if (point.x < min.x)
		{
			num = min.x - point.x;
		}
		else if (point.x > max.x)
		{
			num = point.x - max.x;
		}
		if (point.y < min.y)
		{
			num = Math.Max(num, min.y - point.y);
		}
		else if (point.y > max.y)
		{
			num = Math.Max(num, point.y - max.y);
		}
		return num;
	}

	public int ManhattenDistance(SRect rect)
	{
		int num = 0;
		if (max.x < rect.min.x)
		{
			num += rect.min.x - max.x;
		}
		else if (rect.max.x < min.x)
		{
			num += min.x - rect.max.x;
		}
		if (max.y < rect.min.y)
		{
			num += rect.min.y - max.y;
		}
		else if (rect.max.y < min.y)
		{
			num += min.y - rect.max.y;
		}
		return num;
	}

	public SRect Orient(byte orientation)
	{
		return orientation switch
		{
			0 => this, 
			1 => new SRect(new Short2(-max.y, min.x), new Short2(-min.y, max.x)), 
			2 => new SRect(new Short2(-max.x, -max.y), new Short2(-min.x, -min.y)), 
			3 => new SRect(new Short2(min.y, -max.x), new Short2(max.y, -min.x)), 
			_ => this, 
		};
	}

	public SRect Unorient(byte orientation)
	{
		return Orient((byte)(4 - orientation));
	}

	public Short2[] GetIndices()
	{
		int num = height;
		Short2[] array = new Short2[width * num];
		for (short num2 = min.x; num2 <= max.x; num2 = (short)(num2 + 1))
		{
			for (short num3 = min.y; num3 <= max.y; num3 = (short)(num3 + 1))
			{
				array[(num2 - min.x) * num + (num3 - min.y)] = new Short2(num2, num3);
			}
		}
		return array;
	}

	public IEnumerable<Short2> Indices()
	{
		for (short x = min.x; x <= max.x; x = (short)(x + 1))
		{
			for (short y = min.y; y <= max.y; y = (short)(y + 1))
			{
				yield return new Short2(x, y);
			}
		}
	}

	public IEnumerable<IEnumerable<Short2>> IndicesByRowsDescending()
	{
		for (short y = max.y; y >= min.y; y = (short)(y - 1))
		{
			yield return _IndicesOfRow(y);
		}
	}

	private IEnumerable<Short2> _IndicesOfRow(short yValueOfRow)
	{
		for (short x = min.x; x <= max.x; x = (short)(x + 1))
		{
			yield return new Short2(x, yValueOfRow);
		}
	}

	public Rect ToRect()
	{
		return new Rect(min.x, min.y, width, height);
	}

	public Rect ToBoundingRect()
	{
		return new Rect((float)min.x - 0.5f, (float)min.y - 0.5f, width, height);
	}

	public Bounds ToBounds(float yPosition = 0f, float depth = float.MaxValue)
	{
		Vector3 vector = new Vector3((float)(min.x + max.x) * 0.5f, yPosition, (float)(min.y + max.y) * 0.5f);
		Vector3 vector2 = new Vector3(width, depth, height);
		return new Bounds(vector, vector2);
	}

	public int GreaterAxis()
	{
		if (max.x - min.x < max.y - min.y)
		{
			return 1;
		}
		return 0;
	}

	public int MinDimension()
	{
		int num = max.x - min.x;
		int num2 = max.y - min.y;
		if (num >= num2)
		{
			return num2;
		}
		return num;
	}

	public int MaxDimension()
	{
		int num = max.x - min.x;
		int num2 = max.y - min.y;
		if (num <= num2)
		{
			return num2;
		}
		return num;
	}

	public IEnumerable<Short2> CenterStripIndices()
	{
		int num = GreaterAxis();
		int num2 = 1 - num;
		int num3 = this[num];
		int num4 = this[num2];
		Short2 @short = default(Short2).SetAxis(num, (short)((num4 > 2) ? 1 : ((num3 - 1) / 2))).SetAxis(num2, (short)((num4 - 1) / 2));
		return Pad(-@short).Indices();
	}

	public SRect? Inlay(SRect r)
	{
		Short2? @short = InlayOffset(r);
		if (!@short.HasValue)
		{
			return null;
		}
		r = r.Translate(@short.Value);
		return r;
	}

	public Short2? InlayOffset(SRect r)
	{
		if (r.width > width || r.height > height)
		{
			return null;
		}
		Short2 @short = (min - r.min).Clamp(0);
		Short2 short2 = (r.max - max).Clamp(0);
		return @short - short2;
	}

	public bool IsWithinEdgeProjections(Vector2 v)
	{
		return ToBoundingRect().IsWithinEdgeProjections(v);
	}

	public static bool operator ==(SRect a, SRect b)
	{
		if (a.min.x == b.min.x && a.min.y == b.min.y && a.max.x == b.max.x)
		{
			return a.max.y == b.max.y;
		}
		return false;
	}

	public static bool operator !=(SRect a, SRect b)
	{
		return !(a == b);
	}

	public static SRect operator +(SRect r, Short2 t)
	{
		return r.Translate(t);
	}

	public static SRect operator -(SRect r, Short2 t)
	{
		return r.Translate(-t);
	}

	public static SRect operator *(SRect r, int multiplier)
	{
		Short2 @short = new Short2(r.min.x * multiplier, r.min.y * multiplier);
		return new SRect(@short, new Short2(@short.x + (r.max.x - r.min.x + 1) * multiplier - 1, @short.y + (r.max.y - r.min.y + 1) * multiplier - 1));
	}

	public override bool Equals(object obj)
	{
		if (obj is SRect)
		{
			return this == (SRect)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return min.x ^ (min.y << 8) ^ (max.x << 16) ^ (max.y << 24);
	}

	public override string ToString()
	{
		Short2 @short = min;
		string text = @short.ToString();
		@short = max;
		return text + "," + @short.ToString();
	}
}
