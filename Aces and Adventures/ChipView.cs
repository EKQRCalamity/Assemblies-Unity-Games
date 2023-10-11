using UnityEngine;

public class ChipView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/ChipView";

	[Header("Chip")]
	public MaterialEvent onMaterialChange;

	public MeshEvent onMeshChange;

	public Chip chip
	{
		get
		{
			return (Chip)base.target;
		}
		set
		{
			base.target = value;
		}
	}

	public static ChipView Create(Chip chip, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<ChipView>()._SetData(chip);
	}

	private ChipView _SetData(Chip chipTarget)
	{
		chip = chipTarget;
		return this;
	}

	private void _SetSkin(TokenSkinType skin, ChipType chipType)
	{
		TokenSkin.Data data = EnumUtil.GetResourceBlueprint(skin).GetComponent<TokenSkin>()[chipType];
		onMaterialChange?.Invoke(data.material);
		onMeshChange?.Invoke(data.mesh);
	}

	private void _OnTokenSkinChange(TokenSkinType skin)
	{
		_SetSkin(skin, chip?.type ?? ChipType.Attack);
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (oldTarget is Chip)
		{
			ProfileOptions.CosmeticOptions.OnTokenChange -= _OnTokenSkinChange;
		}
		if (newTarget is Chip)
		{
			_OnTokenSkinChange(ProfileManager.options.cosmetic.token);
			ProfileOptions.CosmeticOptions.OnTokenChange += _OnTokenSkinChange;
		}
	}
}
