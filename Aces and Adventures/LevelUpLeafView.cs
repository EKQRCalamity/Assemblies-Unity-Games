using UnityEngine;

public class LevelUpLeafView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/LevelUp/LevelUpLeafView";

	[Header("Leaf")]
	public Material oldLeafMaterial;

	public Material newLeafMaterial;

	public MaterialEvent onMaterialChange;

	private BarFiller _barFiller;

	public BarFiller barFiller => this.CacheComponentInChildren(ref _barFiller);

	public static LevelUpLeafView Create(LevelUpLeaf leaf, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<LevelUpLeafView>().SetData(leaf) as LevelUpLeafView;
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (newTarget is LevelUpLeaf levelUpLeaf)
		{
			onMaterialChange?.Invoke((levelUpLeaf.state == LevelUpLeaf.State.Old) ? oldLeafMaterial : newLeafMaterial);
		}
	}

	protected override void OnDisable()
	{
		if ((bool)this)
		{
			RepoolCard();
		}
		base.OnDisable();
	}
}
