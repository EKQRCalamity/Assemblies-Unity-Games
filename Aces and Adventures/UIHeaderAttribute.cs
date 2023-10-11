using System;
using UnityEngine;

public class UIHeaderAttribute : Attribute
{
	private byte _a = 128;

	public string header { get; set; }

	public byte r { get; set; }

	public byte g { get; set; }

	public byte b { get; set; }

	public byte a
	{
		get
		{
			return _a;
		}
		set
		{
			_a = value;
		}
	}

	public Color32 tint => new Color32(r, g, b, a);

	public UIHeaderAttribute(string header)
	{
		this.header = header;
	}
}
