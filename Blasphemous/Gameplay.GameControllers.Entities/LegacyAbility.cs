using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class LegacyAbility : MonoBehaviour
{
	[Header("Ability Settings")]
	[SerializeField]
	[Tooltip("Identificator of the ability. If the ID matches a wiredaction the ability will be executed on the stabilished key event.")]
	private string abilityID = string.Empty;

	[SerializeField]
	[Tooltip("Percentaje reduction of the cooldown base")]
	private float cooldownBase;

	private float cooldownCount;

	protected Entity entity;

	private void Start()
	{
		entity = base.transform.parent.GetComponent<Entity>();
	}

	protected virtual void OnAbilityStart()
	{
	}

	protected virtual void OnAbilityUpdate()
	{
	}

	protected virtual void OnAbilityEnd()
	{
	}

	private void Update()
	{
		UpdateCooldown();
		if (IsControlPressed())
		{
			Use();
		}
	}

	private void UpdateCooldown()
	{
		if (cooldownCount > 0f)
		{
			cooldownCount -= Time.deltaTime;
			OnAbilityUpdate();
		}
		else if (cooldownCount < 0f)
		{
			cooldownCount = 0f;
			OnAbilityEnd();
		}
	}

	public bool IsControlPressed()
	{
		return false;
	}

	public bool HasCooldown()
	{
		return cooldownCount > 0f;
	}

	public void Use()
	{
		if (!HasCooldown())
		{
			StartCooldown();
			OnAbilityStart();
		}
	}

	public void ResetCooldown()
	{
		cooldownCount = 0f;
	}

	public void StartCooldown()
	{
		cooldownCount = cooldownBase;
	}
}
