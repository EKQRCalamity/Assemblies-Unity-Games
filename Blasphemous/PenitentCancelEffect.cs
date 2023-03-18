using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using UnityEngine;

public class PenitentCancelEffect : MonoBehaviour
{
	public GameObject cancelVFX;

	public MasterShaderEffects shaderEffects;

	public Vector2 Offset;

	private void Start()
	{
		if (cancelVFX != null)
		{
			PoolManager.Instance.CreatePool(cancelVFX, 3);
		}
	}

	public void PlayCancelEffect()
	{
		PoolManager.Instance.ReuseObject(cancelVFX, GetSpawnPosition(), base.transform.rotation);
		if ((bool)shaderEffects)
		{
			shaderEffects.TriggerColorFlash();
		}
	}

	private Vector2 GetSpawnPosition()
	{
		Vector2 offset = Offset;
		offset.x = ((Core.Logic.Penitent.Status.Orientation != EntityOrientation.Left) ? Offset.x : (0f - Offset.x));
		return (Vector2)base.transform.position + offset;
	}
}
