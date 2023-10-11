using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Card Layout/CardDimensionSettings")]
public class CardDimensionSettings : ScriptableObject
{
	public const float WIDTH = 0.0635f;

	public const float HEIGHT = 0.0889f;

	public const float THICKNESS = 0.00024f;

	public float width = 0.0635f;

	public float height = 0.0889f;

	public float thickness = 0.00024f;

	[Range(0.1f, 10f)]
	public float scale = 1f;

	public float scaledWidth => width * scale;

	public float scaledHeight => height * scale;

	public float scaledThickness => thickness * scale;

	public float scaledAverage => (width + height) * 0.5f * scale;

	public Vector3 scaleVector => scale.ToVector3();

	public float inverseScale => 1f / scale.InsureNonZero();

	public float max => Mathf.Max(Mathf.Max(width, height), thickness) * scale;

	public float this[AxisType axis] => GetDimension(axis);

	public float GetDimension(AxisType axis)
	{
		return axis switch
		{
			AxisType.X => scaledWidth, 
			AxisType.Z => scaledHeight, 
			_ => scaledThickness, 
		};
	}

	public static implicit operator Vector3(CardDimensionSettings size)
	{
		if (!(size != null))
		{
			return new Vector3(0.0635f, 0.00024f, 0.0889f);
		}
		return new Vector3(size.scaledWidth, size.scaledThickness, size.scaledHeight);
	}

	public static implicit operator Vector2(CardDimensionSettings size)
	{
		if (!(size != null))
		{
			return new Vector2(0.0635f, 0.0889f);
		}
		return new Vector2(size.scaledWidth, size.scaledHeight);
	}
}
