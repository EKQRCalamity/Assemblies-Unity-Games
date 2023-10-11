using UnityEngine;

public class LevelUpRewardView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/LevelUp/LevelUpRewardView";

	[Header("Level Up Reward")]
	public StringEvent onNameChange;

	public StringEvent onDescriptionChange;

	public Texture2DEvent onImageChange;

	public RectEvent onImageUVChange;

	public MaterialEvent onCardFrontChange;

	public MaterialEvent onCardBackChange;

	public static LevelUpRewardView Create(LevelUpReward reward, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<LevelUpRewardView>().SetData(reward) as LevelUpRewardView;
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		LevelUpReward levelUpReward = newTarget as LevelUpReward;
		if (levelUpReward == null)
		{
			return;
		}
		onNameChange?.InvokeLocalized(this, () => levelUpReward.data.name);
		onDescriptionChange?.InvokeLocalized(this, () => levelUpReward.data.description);
		if ((bool)levelUpReward.data.image)
		{
			levelUpReward.data.image.image.GetTexture2D(delegate(Texture2D texture)
			{
				this.InvokeIfAlive(onImageChange, texture);
			});
			onImageUVChange?.Invoke(levelUpReward.data.image);
		}
		onCardFrontChange?.Invoke(AbilityCardSkins.Default[levelUpReward.gameState.levelUp.selectedClass].cardFronts[AbilityData.Rank.Normal]);
		onCardBackChange?.Invoke(AbilityCardSkins.Default[levelUpReward.gameState.levelUp.selectedClass].cardBack);
	}
}
