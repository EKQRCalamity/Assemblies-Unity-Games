using System.Collections;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

public class MultipleVFX : MonoBehaviour
{
	public SimpleVFX vfx;

	public int number;

	public float delayBetweenEffects;

	public float range;

	public Vector2 offset;

	private void Start()
	{
		PoolManager.Instance.CreatePool(vfx.gameObject, number);
	}

	[Button("Test spawn", ButtonSizes.Small)]
	public void Play()
	{
		StartCoroutine(SpawnVFX());
	}

	private IEnumerator SpawnVFX()
	{
		for (int i = 0; i < number; i++)
		{
			Vector2 p = (Vector2)base.transform.position + offset + new Vector2(Random.Range(0f - range, range), Random.Range(0f - range, range));
			PoolManager.Instance.ReuseObject(vfx.gameObject, p, Quaternion.identity);
			yield return new WaitForSeconds(delayBetweenEffects);
		}
	}
}
