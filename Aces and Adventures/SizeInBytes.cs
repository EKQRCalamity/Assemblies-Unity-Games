using System;
using System.Runtime.InteropServices;

public static class SizeInBytes
{
	private static class SizeOfCache<T>
	{
		public static readonly int SizeOf;

		static SizeOfCache()
		{
			SizeOf = Marshal.SizeOf(typeof(T).IsEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T));
		}
	}

	public static int Get<T>() where T : struct
	{
		return SizeOfCache<T>.SizeOf;
	}

	public static int Get<T>(T value) where T : struct
	{
		return SizeOfCache<T>.SizeOf;
	}
}
