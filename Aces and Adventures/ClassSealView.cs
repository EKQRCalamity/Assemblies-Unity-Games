using UnityEngine;

public class ClassSealView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/Rewards/ClassSealView";

	[Header("Class Seal")]
	public MaterialEvent onMaterialChange;

	public static ClassSealView Create(ClassSeal classSeal, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<ClassSealView>().SetData(classSeal) as ClassSealView;
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (newTarget is ClassSeal classSeal)
		{
			onMaterialChange?.Invoke(CharacterCardSkins.Default[classSeal.characterClass].seal);
		}
	}
}
