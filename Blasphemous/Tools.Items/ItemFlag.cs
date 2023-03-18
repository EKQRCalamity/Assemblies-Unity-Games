using Framework.Inventory;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Items;

public class ItemFlag : ObjectEffect
{
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private string flagName;

	protected override bool OnApplyEffect()
	{
		Core.Events.SetFlag(flagName, b: true);
		return true;
	}

	protected override void OnRemoveEffect()
	{
		Core.Events.SetFlag(flagName, b: false);
	}
}
