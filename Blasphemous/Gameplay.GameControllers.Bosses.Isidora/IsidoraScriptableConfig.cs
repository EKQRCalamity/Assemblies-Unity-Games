using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Isidora;

[CreateAssetMenu(menuName = "Blasphemous/Bosses/Isidora/IsidoraAttacksConfig", fileName = "IsidoraAttacksConfig")]
public class IsidoraScriptableConfig : SerializedScriptableObject
{
	[Serializable]
	public struct IsidoraAttackConfig
	{
		[TableColumnWidth(200)]
		public IsidoraBehaviour.ISIDORA_ATTACKS attackID;

		[VerticalGroup("Active", 0)]
		[TableColumnWidth(125)]
		[LabelText("100% HP%")]
		[LabelWidth(80f)]
		public bool activeFirstThird;

		[VerticalGroup("Active", 0)]
		[TableColumnWidth(125)]
		[LabelText("<66% HP%")]
		[LabelWidth(80f)]
		public bool activeSecondThird;

		[VerticalGroup("Active", 0)]
		[TableColumnWidth(125)]
		[LabelText("<33% HP%")]
		[LabelWidth(80f)]
		public bool activeThirdPart;

		[VerticalGroup("Enabled", 0)]
		[TableColumnWidth(60)]
		[HideLabel]
		public bool active;

		[TableColumnWidth(80)]
		[VerticalGroup("Recovery", 0)]
		[HideLabel]
		[SuffixLabel("seconds", false, Overlay = true)]
		public float recoverySeconds1;

		[TableColumnWidth(80)]
		[VerticalGroup("Recovery", 0)]
		[HideLabel]
		[SuffixLabel("seconds", false, Overlay = true)]
		public float recoverySeconds2;

		[TableColumnWidth(80)]
		[VerticalGroup("Recovery", 0)]
		[HideLabel]
		[SuffixLabel("seconds", false, Overlay = true)]
		public float recoverySeconds3;

		[VerticalGroup("Repetitions", 0)]
		[TableColumnWidth(80)]
		[HideLabel]
		public int repetitions1;

		[VerticalGroup("Repetitions", 0)]
		[TableColumnWidth(80)]
		[HideLabel]
		public int repetitions2;

		[VerticalGroup("Repetitions", 0)]
		[TableColumnWidth(80)]
		[HideLabel]
		public int repetitions3;

		[TableColumnWidth(100)]
		[ProgressBar(0.0, 5.0, 0.15f, 0.47f, 0.74f)]
		[VerticalGroup("Weight", 0)]
		[HideLabel]
		public float weight1;

		[TableColumnWidth(100)]
		[ProgressBar(0.0, 5.0, 0.15f, 0.47f, 0.74f)]
		[VerticalGroup("Weight", 0)]
		[HideLabel]
		public float weight2;

		[TableColumnWidth(100)]
		[ProgressBar(0.0, 5.0, 0.15f, 0.47f, 0.74f)]
		[VerticalGroup("Weight", 0)]
		[HideLabel]
		public float weight3;

		[TableColumnWidth(200)]
		[DrawWithUnity]
		public List<IsidoraBehaviour.ISIDORA_ATTACKS> alwaysFollowedBy;

		[TableColumnWidth(200)]
		[DrawWithUnity]
		public List<IsidoraBehaviour.ISIDORA_ATTACKS> cantBeFollowedBy;
	}

	[Serializable]
	public struct HorizontalDashConfig
	{
		[FoldoutGroup("Horizontal dash", 0)]
		public float anticipationBeforeDash;

		[FoldoutGroup("Horizontal dash", 0)]
		public float dashDuration;

		[FoldoutGroup("Horizontal dash", 0)]
		public float shoryukenDuration;

		[FoldoutGroup("Horizontal dash", 0)]
		public float floatingDownDuration;
	}

	[TableList]
	public List<IsidoraAttackConfig> attacksConfig;

	[OdinSerialize]
	[FoldoutGroup("Debug", 0)]
	public Dictionary<KeyCode, IsidoraBehaviour.ISIDORA_ATTACKS> debugActions;

	[Header("Timing settings")]
	public HorizontalDashConfig horDashConfig;

	public HorizontalDashConfig invisibleHorDashConfig;

	public List<IsidoraBehaviour.ISIDORA_ATTACKS> GetAttackIds(bool onlyActive, bool useHP, float hpPercentage)
	{
		List<IsidoraBehaviour.ISIDORA_ATTACKS> list = new List<IsidoraBehaviour.ISIDORA_ATTACKS>();
		foreach (IsidoraAttackConfig item in attacksConfig)
		{
			if (ShouldReturnAttack(item, onlyActive, useHP, hpPercentage))
			{
				list.Add(item.attackID);
			}
		}
		return list;
	}

	public int GetAttackRepetitions(IsidoraBehaviour.ISIDORA_ATTACKS atk, bool useHP, float hpPercentage)
	{
		IsidoraAttackConfig attackConfig = GetAttackConfig(atk);
		if (useHP)
		{
			if (hpPercentage < 0.33f)
			{
				return attackConfig.repetitions3;
			}
			if (hpPercentage < 0.66f)
			{
				return attackConfig.repetitions2;
			}
		}
		return attackConfig.repetitions1;
	}

	public float GetAttackRecoverySeconds(IsidoraBehaviour.ISIDORA_ATTACKS atk, bool useHP, float hpPercentage)
	{
		IsidoraAttackConfig attackConfig = GetAttackConfig(atk);
		if (useHP)
		{
			if (hpPercentage < 0.33f)
			{
				return attackConfig.recoverySeconds3;
			}
			if (hpPercentage < 0.66f)
			{
				return attackConfig.recoverySeconds2;
			}
		}
		return attackConfig.recoverySeconds1;
	}

	public List<float> GetFilteredAttacksWeights(List<IsidoraBehaviour.ISIDORA_ATTACKS> filteredAtks, bool useHP, float hpPercentage)
	{
		List<float> list = new List<float>();
		foreach (IsidoraBehaviour.ISIDORA_ATTACKS filteredAtk in filteredAtks)
		{
			if (useHP)
			{
				if (hpPercentage < 0.33f)
				{
					list.Add(GetAttackConfig(filteredAtk).weight3);
				}
				else if (hpPercentage < 0.66f)
				{
					list.Add(GetAttackConfig(filteredAtk).weight2);
				}
				else
				{
					list.Add(GetAttackConfig(filteredAtk).weight1);
				}
			}
			else
			{
				list.Add(GetAttackConfig(filteredAtk).weight1);
			}
		}
		return list;
	}

	public IsidoraAttackConfig GetAttackConfig(IsidoraBehaviour.ISIDORA_ATTACKS atk)
	{
		return attacksConfig.Find((IsidoraAttackConfig x) => x.attackID == atk);
	}

	private bool ShouldReturnAttack(IsidoraAttackConfig atk, bool onlyActive, bool useHP, float hpPercentage)
	{
		bool flag = !onlyActive || atk.active;
		if (useHP)
		{
			flag = flag && IsActiveInHpSection(atk, hpPercentage);
		}
		return flag;
	}

	private bool IsActiveInHpSection(IsidoraAttackConfig atk, float hpPercentage)
	{
		if (hpPercentage < 0.33f)
		{
			return atk.activeThirdPart;
		}
		if (hpPercentage < 0.66f)
		{
			return atk.activeSecondThird;
		}
		return atk.activeFirstThird;
	}
}
