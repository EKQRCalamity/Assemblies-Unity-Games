using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class AwaitExtensions
{
	private static async Task<UnityWebRequest> _SendWebRequest(Func<UnityWebRequest> createWebRequest, float timeout, int maxAttempts, Func<bool> retry = null)
	{
		while (maxAttempts > 0)
		{
			UnityWebRequest request = createWebRequest();
			float timeoutRemaining = timeout;
			ulong previousDownloadedBytes = 0uL;
			Func<bool> condition = delegate
			{
				if (timeout > 0f && !SetPropertyUtility.SetStruct(ref previousDownloadedBytes, request.downloadedBytes))
				{
					if ((timeoutRemaining -= Time.unscaledDeltaTime) <= 0f)
					{
						request.Abort();
					}
				}
				else
				{
					timeoutRemaining = timeout;
				}
				return request.isDone || request.error.HasVisibleCharacter();
			};
			request.SendWebRequest();
			await new AwaitCondition(condition);
			if (!request.error.HasVisibleCharacter())
			{
				return request;
			}
			if (retry == null || !retry())
			{
				int num = maxAttempts - 1;
				maxAttempts = num;
			}
			Debug.LogWarning($"_SendWebRequest(URL = {request.url}, timeout = {timeout}, attemptsRemaining = {maxAttempts}): Error = {request.error}, Bytes downloaded = {previousDownloadedBytes}");
			request.Dispose();
		}
		return null;
	}

	public static async Task<Texture2D> GetTextureAsync(string uri, bool nonReadable, TextureWrapMode wrapMode = TextureWrapMode.Clamp, FilterMode filterMode = FilterMode.Trilinear, int anisoLevel = 16, float timeout = 0f, int maxAttempts = 1, Func<bool> retry = null)
	{
		UnityWebRequest requestTexture = await _SendWebRequest(() => UnityWebRequestTexture.GetTexture(uri, nonReadable), timeout, maxAttempts, retry);
		try
		{
			if (requestTexture == null)
			{
				return null;
			}
			await new AwaitCondition(() => requestTexture.downloadHandler.isDone);
			Texture2D content = DownloadHandlerTexture.GetContent(requestTexture);
			if (!content)
			{
				return null;
			}
			content.wrapMode = wrapMode;
			content.filterMode = filterMode;
			content.anisoLevel = anisoLevel;
			return content;
		}
		finally
		{
			if (requestTexture != null)
			{
				((IDisposable)requestTexture).Dispose();
			}
		}
	}

	public static async Task<AudioClip> GetAudioClipAsync(this UnityWebRequest requestAudioClip)
	{
		using (requestAudioClip)
		{
			await requestAudioClip.SendWebRequest();
			await new AwaitCondition(() => requestAudioClip.downloadHandler.isDone);
			return DownloadHandlerAudioClip.GetContent(requestAudioClip);
		}
	}

	public static async Task<byte[]> GetDataAsync(this UnityWebRequest request)
	{
		using (request)
		{
			await request.SendWebRequest();
			await new AwaitCondition(() => request.downloadHandler.isDone);
			return request.downloadHandler.data;
		}
	}

	public static async Task<byte[]> LoadBytesFromPngAsync(string uriToPng)
	{
		Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, mipChain: false, linear: true);
		Texture2D tex = texture;
		tex.LoadImage(await UnityWebRequest.Get(uriToPng).GetDataAsync());
		byte[] result = IOUtil.LoadBytesFromPng(texture);
		UnityEngine.Object.Destroy(texture);
		return result;
	}

	public static TaskAwaiter<T[]> GetAwaiter<T>(this IEnumerable<Task<T>> tasks)
	{
		return Task.WhenAll(tasks).GetAwaiter();
	}

	public static TaskAwaiter GetAwaiter(this IEnumerable<Task> tasks)
	{
		return Task.WhenAll(tasks).GetAwaiter();
	}

	public static TaskAwaiter<object> GetAwaiter(this YieldInstruction yieldInstruction)
	{
		return new AwaitCoroutine<object>(yieldInstruction).GetAwaiter();
	}

	public static TaskAwaiter<object> GetAwaiter(this CustomYieldInstruction customYieldExtension)
	{
		return new AwaitCoroutine<object>(customYieldExtension).GetAwaiter();
	}

	public static TaskAwaiter<object> GetAwaiter(this Job job)
	{
		TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
		job.OnStopRunning(delegate(Job j)
		{
			taskCompletionSource.SetResult(j.result);
		});
		return taskCompletionSource.Task.GetAwaiter();
	}

	public static TaskAwaiter GetAwaiter(this Department department)
	{
		return new AwaitCondition(() => Job.NumberOfJobsRunningInDepartment(department) == 0).GetAwaiter();
	}
}
