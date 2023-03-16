using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class ThreadedLoader
{
	public class LoadOperation : DLCManager.AssetBundleLoadWaitInstruction
	{
		public string path;

		public byte[] data;

		public void SetComplete(AssetBundle bundle)
		{
			complete = true;
			base.assetBundle = bundle;
		}
	}

	private MonoBehaviour coroutineParent;

	private List<LoadOperation> operationQueue = new List<LoadOperation>();

	private bool busy;

	private bool threadBusy;

	public ThreadedLoader(MonoBehaviour coroutineParent)
	{
		this.coroutineParent = coroutineParent;
	}

	public LoadOperation LoadAssetBundle(string path)
	{
		LoadOperation loadOperation = new LoadOperation();
		loadOperation.path = path;
		if (busy)
		{
			operationQueue.Add(loadOperation);
			return loadOperation;
		}
		startLoad(loadOperation);
		return loadOperation;
	}

	private void startLoad(LoadOperation operation)
	{
		busy = true;
		threadBusy = true;
		coroutineParent.StartCoroutine(threadWait_cr(operation));
		Thread thread = new Thread((ThreadStart)delegate
		{
			loadData(operation);
		});
		thread.Start();
	}

	private IEnumerator threadWait_cr(LoadOperation operation)
	{
		while (threadBusy)
		{
			yield return null;
		}
		AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(operation.data);
		yield return request;
		operation.SetComplete(request.assetBundle);
		busy = false;
		if (operationQueue.Count > 0)
		{
			LoadOperation operation2 = operationQueue[0];
			operationQueue.RemoveAt(0);
			startLoad(operation2);
		}
	}

	private void loadData(LoadOperation operation)
	{
		operation.data = File.ReadAllBytes(operation.path);
		threadBusy = false;
	}
}
