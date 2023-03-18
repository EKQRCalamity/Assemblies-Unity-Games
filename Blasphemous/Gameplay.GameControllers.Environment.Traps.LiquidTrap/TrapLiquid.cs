using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.LiquidTrap;

public class TrapLiquid : MonoBehaviour
{
	[FoldoutGroup("References", 0)]
	public List<Animator> bodyAnimators;

	[FoldoutGroup("References", 0)]
	public GameObject liquidBodyPrefab;

	[FoldoutGroup("References", 0)]
	public Transform origin;

	[FoldoutGroup("Animation settings", 0)]
	public float fallDelay = 0.3f;

	[FoldoutGroup("Animation settings", 0)]
	public float widenDelay = 0.3f;

	[FoldoutGroup("Animation settings", 0)]
	public float stretchDelay = 0.3f;

	[FoldoutGroup("Animation settings", 0)]
	public float liquidBodyHeight = 1f;

	[FoldoutGroup("Design", 0)]
	public LayerMask groundLayerMask;

	public Animator trapanimator;

	private RaycastHit2D[] results = new RaycastHit2D[1];

	private void ClearAllOriginChildren()
	{
		while (origin.childCount > 0)
		{
			Object.DestroyImmediate(origin.GetChild(0).gameObject);
		}
		bodyAnimators.Clear();
	}

	[FoldoutGroup("Design", 0)]
	[Button("AUTO CONFIGURE", ButtonSizes.Medium)]
	public void AutoConfigureLength()
	{
		ClearAllOriginChildren();
		int num = Physics2D.RaycastNonAlloc(origin.position, Vector2.down, results, 100f, groundLayerMask);
		if (num > 0)
		{
			Vector2 point = results[0].point;
			float num2 = Vector2.Distance(origin.position, point);
			int num3 = Mathf.RoundToInt(num2 / liquidBodyHeight);
			for (int i = 0; i < num3; i++)
			{
				GameObject gameObject = Object.Instantiate(liquidBodyPrefab, origin);
				gameObject.transform.position = origin.position + Vector3.down * ((float)i * liquidBodyHeight);
				bodyAnimators.Add(gameObject.GetComponent<Animator>());
			}
		}
	}

	private void CreateBodyParts()
	{
	}

	[Button("ACTIVATE", ButtonSizes.Small)]
	public void ActivateTrap()
	{
		trapanimator.SetBool("ATTACK", value: true);
		TestStartToFall();
		TestToWiden();
	}

	[Button("DEACTIVATE", ButtonSizes.Small)]
	public void DeactivateTrap()
	{
		trapanimator.SetBool("ATTACK", value: false);
		TestStartToFall();
		TestToWiden();
	}

	[FoldoutGroup("Test animation", 0)]
	[Button(ButtonSizes.Small)]
	public void TestStartToFall()
	{
		TriggerAllDelayed("FALL", fallDelay);
	}

	[FoldoutGroup("Test animation", 0)]
	[Button(ButtonSizes.Small)]
	public void TestToWiden()
	{
		TriggerAllDelayed("WIDEN", widenDelay);
	}

	[FoldoutGroup("Test animation", 0)]
	[Button(ButtonSizes.Small)]
	public void TestToStretch()
	{
		TriggerAllDelayed("STRETCH", stretchDelay);
	}

	public void TriggerAllDelayed(string trigger, float delay)
	{
		for (int i = 0; i < bodyAnimators.Count; i++)
		{
			SetTriggerDelayed(delay * (float)i, bodyAnimators[i], trigger);
		}
	}

	public void SetTriggerDelayed(float seconds, Animator a, string trigger)
	{
		StartCoroutine(SetTriggerDelayedCoroutine(seconds, a, trigger));
	}

	private IEnumerator SetTriggerDelayedCoroutine(float seconds, Animator a, string trigger)
	{
		yield return new WaitForSeconds(seconds);
		a.SetTrigger(trigger);
	}
}
