using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity.BlobShadow;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class EntityShadow : Trait
{
	[Range(0f, 0.5f)]
	public float ShadowXOffset = 0.15f;

	[Range(0f, 0.5f)]
	public float ShadowYOffset = 0.15f;

	public BlobShadow BlobShadow { get; private set; }

	protected override void OnStart()
	{
		if (!Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE))
		{
			AssignBlobShadow();
			base.EntityOwner.FlagChanged += OnFlagChanged;
		}
	}

	private void OnFlagChanged(string key, bool active)
	{
		base.enabled = key.Equals("HIDE_SHADOW");
	}

	protected override void OnTraitDestroy()
	{
		RemoveBlobShadow();
	}

	private void AssignBlobShadow()
	{
		GameObject blowShadow = Core.Logic.CurrentLevelConfig.BlobShadowManager.GetBlowShadow(base.transform.position);
		if (!(blowShadow == null))
		{
			BlobShadow component = blowShadow.GetComponent<BlobShadow>();
			BlobShadow = component;
			BlobShadow.SetEntity(base.EntityOwner);
		}
	}

	public void RemoveBlobShadow()
	{
		BlobShadowManager blobShadowManager = Core.Logic.CurrentLevelConfig.BlobShadowManager;
		if (blobShadowManager != null && BlobShadow != null)
		{
			blobShadowManager.StoreBlobShadow(BlobShadow.gameObject);
		}
	}
}
