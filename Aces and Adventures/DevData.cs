using System;
using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/DevData")]
public class DevData : ScriptableObject
{
	[Serializable]
	[ProtoContract]
	[UIField]
	public class UnlockData
	{
		[ProtoMember(1)]
		[UIField(tooltip = "Unlock all NG+ games.")]
		public bool games;

		[ProtoMember(2)]
		[UIField(tooltip = "Unlock all adventures for all unlocked games.")]
		public bool adventures;

		[ProtoMember(3)]
		[UIField(tooltip = "Unlock all playable characters.")]
		public bool characters;

		[ProtoMember(4)]
		[UIField(tooltip = "Unlock all abilities.")]
		public bool abilities;

		[ProtoMember(5)]
		[UIField(tooltip = "Exclude abilities without images from deck creation.")]
		public bool hideAbilitiesWithoutImage;
	}

	[Serializable]
	[ProtoContract]
	[UIField]
	public class OverridesData
	{
		[SerializeField]
		[Range(-1f, 30f)]
		[ProtoMember(1)]
		[UIField(tooltip = "Override the current level of all playable characters. Leave at -1 to not override.\n<i>Set before sitting down at game table.</i>", min = -1, max = 30, onValueChangedMethod = "_OnLevelOverrideChange")]
		[DefaultValue(-1)]
		protected int _levelOverride = -1;

		[SerializeField]
		[Range(-1f, 1000f)]
		[ProtoMember(2)]
		[UIField(tooltip = "Override the amount of mana that would be gathered when you complete an adventure. Leave at -1 to not override.", min = -1, max = 1000)]
		[DefaultValue(-1)]
		protected int _adventureExperienceOverride = -1;

		public int? levelOverride
		{
			get
			{
				if (_levelOverride < 0)
				{
					return null;
				}
				return _levelOverride;
			}
		}

		public int? adventureExperienceOverride
		{
			get
			{
				if (_adventureExperienceOverride < 0)
				{
					return null;
				}
				return _adventureExperienceOverride;
			}
		}

		private void _OnLevelOverrideChange()
		{
			ProfileOptions.DevOptions.OnLevelChange?.Invoke(levelOverride.GetValueOrDefault());
		}
	}

	private static DevData _Default;

	private static readonly ResourceBlueprint<DevData> _DevData = "DEV";

	[SerializeField]
	protected UnlockData _unlocks;

	[SerializeField]
	protected OverridesData _overrides;

	public static DevData Default => _Default ?? (_Default = ScriptableObject.CreateInstance<DevData>());

	private static DevData Data
	{
		get
		{
			DevData devData = ProfileManager.options.devData;
			if ((object)devData != null)
			{
				return devData;
			}
			return Default;
		}
	}

	public static UnlockData Unlocks => Data.unlocks;

	public static OverridesData Overrides => Data.overrides;

	public UnlockData unlocks => _unlocks ?? (_unlocks = new UnlockData());

	public OverridesData overrides => _overrides ?? (_overrides = new OverridesData());

	public DevData SetData(UnlockData unlockData, OverridesData overrideData)
	{
		_unlocks = unlockData;
		_overrides = overrideData;
		return this;
	}
}
