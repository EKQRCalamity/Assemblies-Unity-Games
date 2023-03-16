using System;

public class AssetLoaderOption
{
	[Flags]
	public enum Type
	{
		None = 0,
		PersistInCache = 1,
		DontDestroyOnUnload = 2,
		PersistInCacheTagged = 4
	}

	public Type type { get; private set; }

	public object context { get; private set; }

	public AssetLoaderOption(Type option, object context)
	{
		type = option;
		this.context = context;
	}

	public static AssetLoaderOption None()
	{
		return new AssetLoaderOption(Type.None, null);
	}

	public static AssetLoaderOption PersistInCache()
	{
		return new AssetLoaderOption(Type.PersistInCache, null);
	}

	public static AssetLoaderOption DontDestroyOnUnload()
	{
		return new AssetLoaderOption(Type.DontDestroyOnUnload, null);
	}

	public static AssetLoaderOption PersistInCacheTagged(string tag)
	{
		return new AssetLoaderOption(Type.PersistInCacheTagged, tag);
	}
}
