using System;
using System.Collections.Generic;
using System.IO;

public class FileInfoRecentComparer : IComparer<FileInfo>
{
	public static readonly FileInfoRecentComparer Recent = new FileInfoRecentComparer(1);

	public static readonly FileInfoRecentComparer Old = new FileInfoRecentComparer(-1);

	private readonly int _direction;

	private FileInfoRecentComparer(int direction)
	{
		_direction = direction;
	}

	public int Compare(FileInfo a, FileInfo b)
	{
		int num = b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc);
		if (num == 0)
		{
			return string.Compare(a.Name, b.Name, StringComparison.CurrentCulture);
		}
		return num * _direction;
	}
}
