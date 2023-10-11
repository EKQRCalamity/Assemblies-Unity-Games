using UnityEngine;

public class LevelUpPlantView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/LevelUp/LevelUpPlantView";

	[Header("Level Up Plant")]
	public LevelUpLeafDeckLayout leafs;

	public FloatEvent onRebirthOneChange;

	public FloatEvent onRebirthTwoChange;

	protected override bool _shouldCacheInputColliderOnAwake => false;

	public static LevelUpPlantView Create(LevelUpPlant plant, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<LevelUpPlantView>().SetData(plant) as LevelUpPlantView;
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (newTarget is LevelUpPlant levelUpPlant)
		{
			leafs.deck = levelUpPlant.leafs;
			onRebirthOneChange?.Invoke((levelUpPlant.rebirth >= 1) ? 1 : 0);
			onRebirthTwoChange?.Invoke((levelUpPlant.rebirth >= 2) ? 1 : 0);
		}
	}
}
