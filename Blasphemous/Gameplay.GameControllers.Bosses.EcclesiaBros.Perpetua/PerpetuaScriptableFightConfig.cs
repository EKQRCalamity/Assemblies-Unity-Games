using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Perpetua;

[CreateAssetMenu(menuName = "Blasphemous/Bosses/PerpetuaConfig")]
public class PerpetuaScriptableFightConfig : ScriptableObject
{
	[FoldoutGroup("Attacks", 0)]
	public List<PerpetuaAttackConfig> attackList;

	public PerpetuaAttackConfig GetAttack(PerpetuaBehaviour.Perpetua_ATTACKS atk)
	{
		return attackList.Find((PerpetuaAttackConfig x) => x.attack == atk);
	}
}
