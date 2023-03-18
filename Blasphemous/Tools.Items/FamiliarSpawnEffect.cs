using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Familiar;
using Gameplay.GameControllers.Penitent;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tools.Items;

public class FamiliarSpawnEffect : ObjectEffect
{
	[FormerlySerializedAs("Familiar")]
	public GameObject FamiliarPrefab;

	public Vector2 Offset;

	private Familiar FamiliarEntity { get; set; }

	private Vector3 GetPosition
	{
		get
		{
			Penitent penitent = Core.Logic.Penitent;
			return penitent.transform.position + (Vector3)Offset;
		}
	}

	protected override bool OnApplyEffect()
	{
		if (!Core.Logic.IsMenuScene())
		{
			InstantiateFamiliar(GetPosition);
		}
		return FamiliarEntity != null;
	}

	protected override void OnRemoveEffect()
	{
		base.OnRemoveEffect();
		if ((bool)FamiliarEntity)
		{
			FamiliarEntity.Dispose();
		}
	}

	private void InstantiateFamiliar(Vector3 position)
	{
		if (!(FamiliarPrefab == null))
		{
			GameObject gameObject = Object.Instantiate(FamiliarPrefab, position, Quaternion.identity);
			FamiliarEntity = gameObject.GetComponentInChildren<Familiar>();
			if ((bool)FamiliarEntity)
			{
				FamiliarEntity.Owner = Core.Logic.Penitent;
			}
		}
	}
}
