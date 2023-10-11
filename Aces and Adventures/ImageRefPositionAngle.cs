using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class ImageRefPositionAngle
{
	[ProtoMember(1)]
	public PositionAngle positionAngle = new PositionAngle(new Vector2(0.5f, 0.5f), MathF.PI / 2f, flipOrthogonalAxis: false);

	public ImageRef imageRef { get; set; }

	public Vector2 position => this;

	public float angle => this;

	public bool flipOrthogonalAxis => positionAngle.flipOrthogonalAxis;

	public Vector2 direction => positionAngle.direction;

	public ImageRefPositionAngle()
	{
	}

	public ImageRefPositionAngle(PositionAngle positionAngle)
	{
		this.positionAngle = positionAngle;
	}

	public static implicit operator PositionAngle(ImageRefPositionAngle a)
	{
		return a.positionAngle;
	}

	public static implicit operator Vector2(ImageRefPositionAngle a)
	{
		return a.positionAngle;
	}

	public static implicit operator float(ImageRefPositionAngle a)
	{
		return a.positionAngle;
	}
}
