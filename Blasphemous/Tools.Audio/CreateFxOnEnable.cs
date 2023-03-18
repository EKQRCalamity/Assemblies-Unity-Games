using System.Collections;
using FMODUnity;
using Framework.Managers;
using UnityEngine;

namespace Tools.Audio;

public class CreateFxOnEnable : MonoBehaviour
{
	public int n;

	public GameObject toInstantiate;

	public float delay = 0.1f;

	public Vector2 offset;

	[EventRef]
	public string fxOneshotSound;

	private void Awake()
	{
		if (toInstantiate != null && n > 0)
		{
			PoolManager.Instance.CreatePool(toInstantiate, n);
		}
	}

	private void OnEnable()
	{
		StartCoroutine(DelayedInstantiation());
	}

	private IEnumerator DelayedInstantiation()
	{
		yield return new WaitForSeconds(delay);
		InstantiateStuff();
	}

	private void InstantiateStuff()
	{
		if (fxOneshotSound != string.Empty)
		{
			Core.Audio.PlayOneShot(fxOneshotSound);
		}
		if (toInstantiate != null)
		{
			GameObject gameObject = PoolManager.Instance.ReuseObject(toInstantiate, (Vector2)base.transform.position + offset, base.transform.rotation).GameObject;
			gameObject.transform.localScale = base.transform.localScale;
		}
	}
}
