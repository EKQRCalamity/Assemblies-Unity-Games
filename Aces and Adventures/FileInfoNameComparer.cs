using System;
using System.Collections.Generic;
using System.IO;

public class FileInfoNameComparer : IComparer<FileInfo>
{
	public static readonly FileInfoNameComparer Ascending = new FileInfoNameComparer(1);

	public static readonly FileInfoNameComparer Descending = new FileInfoNameComparer(-1);

	private readonly int _direction;

	private FileInfoNameComparer(int direction)
	{
		_direction = direction;
	}

	public int Compare(FileInfo a, FileInfo b)
	{
		int num = string.Compare(a.Name, b.Name, StringComparison.CurrentCulture);
		if (num == 0)
		{
			return b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc);
		}
		return num * _direction;
	}
}
