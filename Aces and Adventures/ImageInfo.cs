using System;
using System.Collections.Generic;
using System.Linq;

public struct ImageInfo
{
	public readonly int width;

	public readonly int height;

	public ImageInfo(Queue<string> info)
	{
		width = 0;
		height = 0;
		while (info.Count != 0)
		{
			string text = info.Dequeue().TrimStart(' ');
			if (text.StartsWith("Width", StringComparison.OrdinalIgnoreCase))
			{
				width = int.Parse(new string(text.Where(char.IsDigit).ToArray()));
			}
			else if (text.StartsWith("Height", StringComparison.OrdinalIgnoreCase))
			{
				height = int.Parse(new string(text.Where(char.IsDigit).ToArray()));
			}
		}
	}

	public int MaxDimension()
	{
		return Math.Max(width, height);
	}

	public override string ToString()
	{
		return $"Width: {width}, Height: {height}";
	}

	public static implicit operator Int2(ImageInfo imageInfo)
	{
		return new Int2(imageInfo.width, imageInfo.height);
	}
}
