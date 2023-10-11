using System.Collections.Generic;
using UnityEngine;

public class StoneView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/StoneView";

	private static Dictionary<StoneType, Material> _Materials;

	[Header("Stone")]
	public MaterialEvent onMaterialChange;

	public FloatEvent onScaleChange;

	private static Dictionary<StoneType, Material> Materials => _Materials ?? (_Materials = ReflectionUtil.CreateEnumResourceMap<StoneType, Material>("GameState/Stones"));

	public Stone stone
	{
		get
		{
			return (Stone)base.target;
		}
		set
		{
			base.target = value;
		}
	}

	public static StoneView Create(Stone stone, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<StoneView>()._SetData(stone);
	}

	private StoneView _SetData(Stone stoneTarget)
	{
		stone = stoneTarget;
		return this;
	}

	private void _OnCardChanged()
	{
		onMaterialChange?.Invoke(Materials[stone]);
		onScaleChange?.Invoke(stone.type.GetScale());
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (newTarget != null)
		{
			_OnCardChanged();
		}
	}
}
