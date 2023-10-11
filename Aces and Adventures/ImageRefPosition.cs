using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class ImageRefPosition
{
	[ProtoMember(1)]
	private bool _enabled;

	[ProtoMember(2)]
	private Alpha2 _position = Alpha2.Center;

	public bool enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
		}
	}

	public Vector2 position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = new Alpha2(value);
		}
	}

	public ImageRef imageRef { get; set; }

	private bool _positionSpecified
	{
		get
		{
			if (_enabled)
			{
				return _position != Alpha2.Center;
			}
			return false;
		}
	}

	public ImageRefPosition()
	{
	}

	public ImageRefPosition(bool enabled, Alpha2 position)
	{
		_enabled = enabled;
		_position = position;
	}

	public static implicit operator bool(ImageRefPosition a)
	{
		return a._enabled;
	}

	public static implicit operator Vector2(ImageRefPosition a)
	{
		return a._position;
	}

	public static implicit operator Alpha2?(ImageRefPosition a)
	{
		if (!a)
		{
			return null;
		}
		return a._position;
	}
}
