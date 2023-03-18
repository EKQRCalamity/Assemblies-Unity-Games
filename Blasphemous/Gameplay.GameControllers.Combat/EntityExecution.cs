using System.Collections;
using System.Linq;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using Tools.Level.Interactables;
using UnityEngine;

namespace Gameplay.GameControllers.Combat;

public class EntityExecution : Trait
{
	public GameObject ExecutionPrefab;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	protected string ExecutionAwareness;

	private GameObject execution;

	public Vector2 ExecutionPosition { get; private set; }

	protected override void OnStart()
	{
		base.OnStart();
		if (ExecutionPrefab == null)
		{
			Debug.LogError("An Execution Prefab Is Needed!");
		}
	}

	public void InstantiateExecution()
	{
		Core.Audio.PlaySfx(ExecutionAwareness);
		execution = Object.Instantiate(ExecutionPrefab, base.EntityOwner.transform.position, Quaternion.identity);
		ExecutionPosition = new Vector3(execution.transform.position.x, execution.transform.position.y);
		execution.GetComponentInChildren<Execution>().ExecutedEntity = base.EntityOwner as Enemy;
		SpriteRenderer spriteRenderer = execution.GetComponentsInChildren<SpriteRenderer>().First((SpriteRenderer x) => x.gameObject.CompareTag("Interactable"));
		spriteRenderer.flipX = Core.Logic.Penitent.transform.position.x < execution.transform.position.x;
	}

	[Button(ButtonSizes.Small)]
	public void DestroyExecution()
	{
		if (!(execution == null))
		{
			StartCoroutine(DestroyExecutionCoroutine());
		}
	}

	private IEnumerator DestroyExecutionCoroutine()
	{
		Execution e = execution.GetComponentInChildren<Execution>();
		yield return new WaitForSeconds(e.SafeTimeThreshold + 0.1f);
		Hit hit = new Hit
		{
			DamageAmount = 100f
		};
		if (e != null)
		{
			e.Damage(hit);
		}
	}
}
