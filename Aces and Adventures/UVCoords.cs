using ProtoBuf;
using UnityEngine;

[ProtoContract]
public struct UVCoords
{
	public static readonly UVCoords Default = new UVCoords(Vector2.zero, Vector2.one);

	public static readonly UVCoords Min = new UVCoords(new Vector2(float.MinValue, float.MinValue), new Vector2(float.MinValue, float.MinValue));

	[ProtoMember(1, IsRequired = true)]
	public Vector2 min;

	[ProtoMember(2, IsRequired = true)]
	public Vector2 max;

	public Vector2 size => max - min;

	public Vector2 aspect => size / size.AbsMax().InsureNonZero();

	public Vector2 center => (min + max) * 0.5f;

	public static UVCoords FromAspectRatio(Vector2 dimensions)
	{
		Vector2 vector = dimensions.Abs() / dimensions.AbsMax().InsureNonZero() * 0.5f;
		return new UVCoords(new Vector2(0.5f, 0.5f) - vector, new Vector2(0.5f, 0.5f) + vector);
	}

	public static UVCoords FromPreferredAndActualSize(Vector2 preferredSize, Vector2 actualSize)
	{
		return FromAspectRatio(preferredSize.normalized.Multiply(actualSize.normalized.Inverse().InsureNonZero()));
	}

	public static UVCoords FromPreferredAndActualSize(Int2 preferredSize, Int2 actualSize)
	{
		return FromPreferredAndActualSize(preferredSize.ToVector2(), actualSize.ToVector2());
	}

	public UVCoords(Vector2 min, Vector2 max)
	{
		this.min = min;
		this.max = max;
	}

	public UVCoords(Rect rect)
		: this(rect.min, rect.max)
	{
	}

	public Rect ToRect()
	{
		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	}

	public Bounds ToBounds()
	{
		Rect rect = ToRect();
		return new Bounds(rect.center.Unproject(AxisType.Z), rect.size.Unproject(AxisType.Z));
	}

	public UVCoords FitIntoRange(UVCoords range)
	{
		return this + (Vector2.Max(Vector2.zero, range.min - min) - Vector2.Max(Vector2.zero, max - range.max));
	}

	public UVCoords Scale(Vector2 scale)
	{
		Vector2 vector = center;
		Vector2 v = size * 0.5f;
		return new UVCoords(vector - v.Multiply(scale), vector + v.Multiply(scale));
	}

	public UVCoords SetSize(Vector2 newSize)
	{
		Vector2 vector = center;
		Vector2 vector2 = newSize * 0.5f;
		return new UVCoords(vector - vector2, vector + vector2);
	}

	public Int4 ToPixelRect(Int2 pixelDimensions)
	{
		Vector2 multipleOf = new Vector2(1f / (float)pixelDimensions.x, 1f / (float)pixelDimensions.y);
		Vector2 multiplier = new Vector2(pixelDimensions.x, pixelDimensions.y);
		Int2 @int = new Int2(size.RoundToNearestMultipleOf(multipleOf).Multiply(multiplier));
		Int2 int2 = new Int2(min.RoundToNearestMultipleOf(multipleOf).Multiply(multiplier));
		Int2 int3 = new Int2(max.RoundToNearestMultipleOf(multipleOf).Multiply(multiplier));
		return new Int4(int2.x, pixelDimensions.y - int3.y, @int.x, @int.y);
	}

	public static implicit operator Rect(UVCoords uv)
	{
		return uv.ToRect();
	}

	public static implicit operator Bounds(UVCoords uv)
	{
		return uv.ToBounds();
	}

	public static implicit operator UVCoords(Rect rect)
	{
		return new UVCoords(rect);
	}

	public static implicit operator Vector4(UVCoords uv)
	{
		return new Vector4(uv.min.x, uv.min.y, uv.max.x, uv.max.y);
	}

	public static implicit operator UVCoords(Vector4 v)
	{
		return new UVCoords(new Vector2(v.x, v.y), new Vector2(v.z, v.w));
	}

	public static UVCoords operator +(Vector2 v, UVCoords uv)
	{
		return new UVCoords(uv.min + v, uv.max + v);
	}

	public static UVCoords operator +(UVCoords uv, Vector2 v)
	{
		return new UVCoords(uv.min + v, uv.max + v);
	}
}
