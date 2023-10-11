using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class WebRequestTextureCache : MonoBehaviour
{
	private static WebRequestTextureCache _Instance;

	private long _maxCachedMemory = 100000000L;

	private long _usedMemory;

	private Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

	private Dictionary<string, HashSet<Behaviour>> _activeRequests = new Dictionary<string, HashSet<Behaviour>>(StringComparer.OrdinalIgnoreCase);

	public static WebRequestTextureCache Instance => ManagerUtil.GetSingletonInstance(ref _Instance);

	public static async Task<Texture2D> RequestAsync(Behaviour requester, string uri)
	{
		return await Instance._RequestAsync(requester, uri);
	}

	public static void ReleaseRequest(Behaviour requester, string uri)
	{
		Instance._ReleaseRequest(requester, uri);
	}

	public static void ClearCache(bool clearEvenIfInUse = true)
	{
		if (clearEvenIfInUse)
		{
			Instance._activeRequests.Clear();
		}
		Instance._ClearUnused();
	}

	private void _RemoveFromCache(string uri)
	{
		if (_cache.ContainsKey(uri))
		{
			Texture2D texture2D = _cache[uri];
			_usedMemory -= texture2D.GetMemoryUsed();
			_cache.Remove(uri);
			UnityEngine.Object.Destroy(texture2D);
		}
	}

	private void _SetIntoCache(string uri, Texture2D texture)
	{
		_RemoveFromCache(uri);
		_cache[uri] = texture;
		_usedMemory += texture.GetMemoryUsed();
	}

	private async Task<Texture2D> _SendRequestAsync(string uri)
	{
		Texture2D texture2D = await AwaitExtensions.GetTextureAsync(uri, nonReadable: true, TextureWrapMode.Clamp, FilterMode.Trilinear, 16, ProfileManager.options.game.ugc.advanced.previewTimeout, 1, () => _activeRequests.ContainsKey(uri));
		if ((bool)texture2D)
		{
			_SetIntoCache(uri, texture2D);
		}
		return texture2D;
	}

	private async Task<Texture2D> _RequestAsync(Behaviour requester, string uri)
	{
		if (!_activeRequests.ContainsKey(uri))
		{
			_activeRequests.Add(uri, new HashSet<Behaviour>());
		}
		_activeRequests[uri].Add(requester);
		if (_cache.ContainsKey(uri))
		{
			return _cache[uri];
		}
		return (await (Job.GetJob(this, uri) ?? Job.Process(_SendRequestAsync(uri).AsEnumerator()).BeginTracking(this, uri))) as Texture2D;
	}

	private void _ReleaseRequest(Behaviour requester, string uri)
	{
		if (_activeRequests.ContainsKey(uri) && _activeRequests[uri].Remove(requester) && _activeRequests[uri].Count == 0)
		{
			_activeRequests.Remove(uri);
		}
	}

	private void _ClearInactiveRequests()
	{
		foreach (KeyValuePair<string, HashSet<Behaviour>> item in _activeRequests.EnumeratePairsSafe())
		{
			foreach (Behaviour item2 in item.Value.EnumerateSafe())
			{
				if (!item2.IsActiveAndEnabled())
				{
					item.Value.Remove(item2);
					if (item.Value.Count == 0)
					{
						_activeRequests.Remove(item.Key);
					}
				}
			}
		}
	}

	private void _ClearUnused()
	{
		foreach (KeyValuePair<string, Texture2D> item in _cache.EnumeratePairsSafe())
		{
			if (!_activeRequests.ContainsKey(item.Key))
			{
				_RemoveFromCache(item.Key);
			}
		}
	}

	private void Awake()
	{
		_maxCachedMemory = Math.Min(Math.Max((long)((double)SystemInfo.graphicsMemorySize * Math.Pow(10.0, 5.0) * 0.5), (long)Math.Pow(10.0, 7.0)), (long)Math.Pow(10.0, 9.0));
		Debug.Log("WebRequestTextureCache: Max Cached Memory = " + _maxCachedMemory);
	}

	private void Update()
	{
		_ClearInactiveRequests();
		if (_usedMemory > _maxCachedMemory)
		{
			_ClearUnused();
		}
	}
}
