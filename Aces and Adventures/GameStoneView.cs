using UnityEngine;

public class GameStoneView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/Rewards/GameStoneView";

	[Header("Game Stone")]
	public NewGameTypeMaterials materials;

	public MaterialEvent onMaterialChange;

	public static GameStoneView Create(GameStone gameStone, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<GameStoneView>().SetData(gameStone) as GameStoneView;
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (newTarget is GameStone gameStone)
		{
			onMaterialChange?.Invoke(materials[gameStone.game.data.specialGameType] ?? materials[gameStone.game.data.newGameType]);
		}
	}
}
