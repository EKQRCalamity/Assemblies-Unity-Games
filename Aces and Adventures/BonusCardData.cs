using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
[Localize]
public class BonusCardData : IDataContent
{
	[ProtoContract]
	[UIField]
	[ProtoInclude(10, typeof(TrackedCriteria))]
	public abstract class ATrackedCriteria
	{
		public event Action<int> onTallyAdjusted;

		protected void _SignalTallyAdjustment(int adjustment)
		{
			this.onTallyAdjusted?.Invoke(adjustment);
		}

		public virtual void Register(GameState state)
		{
		}

		public virtual void Unregister(GameState state)
		{
		}
	}

	[ProtoContract]
	[UIField]
	public class TrackedCriteria : ATrackedCriteria
	{
		[ProtoMember(1, OverwriteList = true)]
		[UIField(collapse = UICollapseType.Open, tooltip = "Determines when <b>Tally Adjustment</b> will be incremented into running tally amount.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Trigger> _triggers;

		[ProtoMember(2, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must be true of combatant which caused trigger in order to actually count towards tally.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.Combatant> _triggeredByConditions;

		[ProtoMember(3, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must be true of combatant which was triggered on in order to actually count towards tally.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.Combatant> _triggeredOnConditions;

		[ProtoMember(4)]
		[UIField(tooltip = "Whenever a valid trigger occurs, this value will be added to running tally amount.", collapse = UICollapseType.Open)]
		[UIDeepValueChange]
		private AAction.DynamicNumber _tallyAdjustment;

		private void _OnTrigger(ReactionContext reactionContext, TargetedReactionFilter reactionFilter, int capturedValue)
		{
			Player player = GameState.Instance.player;
			ActionContext actionContext = new ActionContext(player, null, player).SetCapturedValue(reactionContext.capturedValue);
			if (reactionFilter.IsValid(reactionContext, actionContext) && _triggeredByConditions.All(actionContext.SetTarget(reactionContext.triggeredBy)) && _triggeredOnConditions.All(actionContext.SetTarget(reactionContext.triggeredOn)))
			{
				_SignalTallyAdjustment(_tallyAdjustment.GetValue(actionContext));
			}
		}

		public override void Register(GameState state)
		{
			if (_triggers.IsNullOrEmpty())
			{
				return;
			}
			foreach (AAction.Trigger trigger in _triggers)
			{
				trigger.Register(new ActionContext(state.player, null, state.player));
				trigger.onTrigger += _OnTrigger;
			}
		}

		public override void Unregister(GameState state)
		{
			if (_triggers.IsNullOrEmpty())
			{
				return;
			}
			foreach (AAction.Trigger trigger in _triggers)
			{
				trigger.Unregister(new ActionContext(state.player, null, state.player));
				trigger.onTrigger -= _OnTrigger;
			}
		}

		public override string ToString()
		{
			return "+" + _tallyAdjustment?.ToString() + " " + _triggers.ToStringSmart(" <i>or</i> ").SpaceIfNotEmpty() + (_triggeredByConditions.IsNullOrEmpty() ? "" : ("triggered by " + _triggeredByConditions.ToStringSmart(" & ")).SpaceIfNotEmpty().SizeIfNotEmpty()) + (_triggeredOnConditions.IsNullOrEmpty() ? "" : ("triggered on " + _triggeredOnConditions.ToStringSmart(" & ")).SpaceIfNotEmpty().SizeIfNotEmpty());
		}
	}

	[ProtoContract]
	[UIField]
	[ProtoInclude(5, typeof(Equality))]
	[ProtoInclude(6, typeof(DynamicNumber))]
	[ProtoInclude(7, typeof(Or))]
	[ProtoInclude(8, typeof(More))]
	[ProtoInclude(9, typeof(Ratio))]
	[ProtoInclude(10, typeof(Percent))]
	[ProtoInclude(11, typeof(Conditional))]
	[ProtoInclude(12, typeof(Daily))]
	public abstract class ACriteriaCheck
	{
		[ProtoContract(EnumPassthru = true)]
		public enum TallyType
		{
			Success,
			Fail
		}

		[ProtoContract]
		[UIField("Tally", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Check tally relationally against a dynamic number.")]
		public class Equality : ACriteriaCheck
		{
			[ProtoMember(1)]
			[UIField]
			[DefaultValue(FlagCheckType.GreaterThanOrEqualTo)]
			private FlagCheckType _comparison = FlagCheckType.GreaterThanOrEqualTo;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Open)]
			[UIDeepValueChange]
			private AAction.DynamicNumber _amount;

			[ProtoMember(3)]
			[UIField]
			private TallyType _tallyToCheck;

			public override bool Check(ActionContext context, int tally, int failTally)
			{
				return _comparison.Check((_tallyToCheck == TallyType.Success) ? tally : failTally, _amount.GetValue(context));
			}

			public override string ToString()
			{
				return $"If {EnumUtil.FriendlyName(_tallyToCheck)} tally is {_comparison.GetText()} {_amount}";
			}
		}

		[ProtoContract]
		[UIField("Dynamic Number Check", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Check one dynamic number against another relationally.")]
		public class DynamicNumber : ACriteriaCheck
		{
			[ProtoMember(1)]
			[UIField]
			[DefaultValue(FlagCheckType.GreaterThanOrEqualTo)]
			private FlagCheckType _comparison = FlagCheckType.GreaterThanOrEqualTo;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Open)]
			[UIDeepValueChange]
			private AAction.DynamicNumber _a;

			[ProtoMember(3)]
			[UIField(collapse = UICollapseType.Open)]
			[UIDeepValueChange]
			private AAction.DynamicNumber _b;

			public override bool Check(ActionContext context, int tally, int failTally)
			{
				return _comparison.Check(_a.GetValue(context), _b.GetValue(context));
			}

			public override string ToString()
			{
				return $"If {_a} {_comparison.GetText()} {_b}";
			}
		}

		[ProtoContract]
		[UIField("Or", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Returns true if any of the listed criteria are met.")]
		public class Or : ACriteriaCheck
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open)]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<ACriteriaCheck> _criteria;

			public override bool Check(ActionContext context, int tally, int failTally)
			{
				return _criteria?.Any((ACriteriaCheck c) => c.Check(context, tally, failTally)) ?? false;
			}

			public override string ToString()
			{
				return _criteria?.ToStringSmart(" <b>or</b> ") ?? "";
			}
		}

		[ProtoContract]
		[UIField("More Than", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows checking how much more one tally is to another.")]
		public class More : ACriteriaCheck
		{
			[ProtoMember(1)]
			[UIField(min = 0, max = 100)]
			[UIHorizontalLayout("A", flexibleWidth = 999f)]
			private int _moreByAmount;

			[ProtoMember(2)]
			[UIField(tooltip = "Swap success and failure tallies.")]
			[UIHorizontalLayout("A", flexibleWidth = 1f)]
			private bool _swapTally;

			public override bool Check(ActionContext context, int tally, int failTally)
			{
				int num = (_swapTally ? failTally : tally);
				int num2 = (_swapTally ? tally : failTally);
				return num >= num2 + _moreByAmount;
			}

			public override string ToString()
			{
				string text = (_swapTally ? "Failure" : "Success");
				string text2 = (_swapTally ? "Success" : "Failure");
				if (_moreByAmount != 0)
				{
					return $"{_moreByAmount} more {text} than {text2}";
				}
				return "More " + text + " than " + text2;
			}
		}

		[ProtoContract]
		[UIField("Ratio", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows checking the ratio of one tally to another.")]
		public class Ratio : ACriteriaCheck
		{
			[ProtoMember(1)]
			[UIField(min = 0, max = 10)]
			[DefaultValue(1f)]
			[UIHorizontalLayout("A", flexibleWidth = 999f)]
			private float _ratio = 1f;

			[ProtoMember(2)]
			[UIField(tooltip = "Check if ratio is less than specified.")]
			[UIHorizontalLayout("A", flexibleWidth = 1f)]
			private bool _lessThan;

			[ProtoMember(3)]
			[UIField(tooltip = "Swap success and failure tallies.")]
			[UIHorizontalLayout("A", flexibleWidth = 1f)]
			private bool _swapTally;

			public override bool Check(ActionContext context, int tally, int failTally)
			{
				int num = (_swapTally ? failTally : tally);
				int num2 = (_swapTally ? tally : failTally);
				float num3 = (float)num / ((float)num2).InsureNonZero();
				if (!_lessThan)
				{
					return num3 >= _ratio;
				}
				return num3 <= _ratio;
			}

			public override string ToString()
			{
				string text = (_swapTally ? "Failure" : "Success");
				string text2 = (_swapTally ? "Success" : "Failure");
				return string.Format("Ratio of {0} to {1} is {2} {3}", text, text2, _lessThan ? "<=" : ">=", _ratio);
			}
		}

		[ProtoContract]
		[UIField("Percent", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows checking the percentage of a given tally compared to total of both tallies.")]
		public class Percent : ACriteriaCheck
		{
			[ProtoMember(1)]
			[UIField(min = 0, max = 100)]
			[DefaultValue(50)]
			[UIHorizontalLayout("A", flexibleWidth = 999f)]
			private byte _percentage = 50;

			[ProtoMember(2)]
			[UIField(tooltip = "Check if percentage is less than specified.")]
			[UIHorizontalLayout("A", flexibleWidth = 1f)]
			private bool _lessThan;

			[ProtoMember(3)]
			[UIField(tooltip = "Check percentage of failure instead of success.")]
			[UIHorizontalLayout("A", flexibleWidth = 1f)]
			private bool _checkFailure;

			public override bool Check(ActionContext context, int tally, int failTally)
			{
				float num = (float)(_checkFailure ? failTally : tally) / ((float)(tally + failTally)).InsureNonZero() * 100f;
				if (!_lessThan)
				{
					return num >= (float)(int)_percentage;
				}
				return num <= (float)(int)_percentage;
			}

			public override string ToString()
			{
				return string.Format("Percentage of {0} is {1} {2}%", _checkFailure ? "Failure" : "Success", _lessThan ? "<=" : ">=", _percentage);
			}
		}

		[ProtoContract]
		[UIField("Conditional", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows checking conditions on player at the end of a campaign.")]
		public class Conditional : ACriteriaCheck
		{
			[ProtoMember(1, OverwriteList = true)]
			[UIField(tooltip = "Conditions that must be true of player at the end of a campaign.")]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<AAction.Condition.Actor> _playerConditions;

			public override bool Check(ActionContext context, int tally, int failTally)
			{
				return _playerConditions.All(context);
			}

			public override string ToString()
			{
				return "If " + _playerConditions.ToStringSmart(" & ");
			}
		}

		[ProtoContract]
		[UIField("Daily", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Returns true if player has not completed current daily adventure for the day.")]
		public class Daily : ACriteriaCheck
		{
			public override bool Check(ActionContext context, int tally, int failTally)
			{
				int? dailyLeaderboard = context.gameState.dailyLeaderboard;
				if (dailyLeaderboard.HasValue)
				{
					int valueOrDefault = dailyLeaderboard.GetValueOrDefault();
					return !ProfileManager.progress.games.read.leaderboardProgress.HasCompletedAdventureForDay(valueOrDefault, context.gameState.adventure);
				}
				return false;
			}

			public override string ToString()
			{
				return "If Has Not Completed Daily";
			}
		}

		public abstract bool Check(ActionContext context, int tally, int failTally);
	}

	[ProtoMember(13)]
	[UIField("Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	private LocalizedStringData _nameLocalized;

	[ProtoMember(14)]
	[UIField("Description", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	private LocalizedStringData _descriptionLocalized;

	[ProtoMember(18)]
	[UIField(validateOnChange = true)]
	private DataRef<BonusCardData> _copyExperienceFrom;

	[ProtoMember(3)]
	[UIField(min = 1, max = 250)]
	[DefaultValue(25)]
	[UIHorizontalLayout("XP", flexibleWidth = 999f, expandHeight = false)]
	[UIHideIf("_hideExperience")]
	private int _experience = 25;

	[ProtoMember(4)]
	[UIField(collapse = UICollapseType.Hide)]
	private CroppedImageRef _image = new CroppedImageRef(ImageCategoryType.Ability, AbilityData.Cosmetic.IMAGE_SIZE);

	[ProtoMember(5, OverwriteList = true)]
	[UIField(collapse = UICollapseType.Open, showAddData = false, tooltip = "All criteria in list is tallied into a single combined success tally to be used in checks.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UIHeader("Criteria")]
	[UIMargin(24f, false)]
	private List<ATrackedCriteria> _successCriteria;

	[ProtoMember(6, OverwriteList = true)]
	[UIField(showAddData = false, tooltip = "All criteria in list is tallied into a single combined failure tally to be used in checks.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<ATrackedCriteria> _failureCriteria;

	[ProtoMember(7, OverwriteList = true)]
	[UIField(collapse = UICollapseType.Open, showAddData = false, tooltip = "All checks in list must be true in order to bonus to considered successfully acquired.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UIHeader("Checks")]
	[UIMargin(24f, false)]
	private List<ACriteriaCheck> _successChecks;

	[ProtoMember(8, OverwriteList = true)]
	[UIField(showAddData = false, tooltip = "All checks in list must be true in order to have bonus considered a failure.\n<i>This takes precedence over success checks.</i>")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<ACriteriaCheck> _failureChecks;

	[ProtoMember(9)]
	[UIField(tooltip = "This bonus can only be received once per character.")]
	[UIHorizontalLayout("XP", flexibleWidth = 1f)]
	[UIHideIf("_hideExperience")]
	private bool _oneTimeOnly;

	[ProtoMember(15)]
	[UIField(tooltip = "Used for card material and unlock media.")]
	[UIHorizontalLayout("XP", flexibleWidth = 999f)]
	[UIHideIf("_hideExperience")]
	private AbilityData.Rank _rank;

	[ProtoMember(16)]
	[UIField(order = 2u, tooltip = "Should exactly match the name of the Steam Achievement that is unlocked when this bonus is received.")]
	private string _achievementName;

	[ProtoMember(17)]
	[UIField(order = 1u, tooltip = "Name only visible to developers for organizational purposes.", max = 128)]
	private string _devName;

	[ProtoMember(1)]
	private string _name
	{
		get
		{
			return null;
		}
		set
		{
			if (_nameLocalized == null)
			{
				_nameLocalized = new LocalizedStringData(value);
			}
		}
	}

	[ProtoMember(2)]
	private string _description
	{
		get
		{
			return null;
		}
		set
		{
			if (_descriptionLocalized == null)
			{
				_descriptionLocalized = new LocalizedStringData(value);
			}
		}
	}

	public LocalizedStringData nameLocalized => _nameLocalized ?? (_nameLocalized = new LocalizedStringData(""));

	public string name => nameLocalized;

	public LocalizedStringData descriptionLocalized => _descriptionLocalized ?? (_descriptionLocalized = new LocalizedStringData(""));

	public string description => descriptionLocalized;

	public string achievementName => _achievementName;

	public int experience
	{
		get
		{
			if (!_copyExperienceFrom)
			{
				return _experience;
			}
			return _copyExperienceFrom.data._experience;
		}
	}

	public CroppedImageRef image => _image;

	public List<ATrackedCriteria> successCriteria => _successCriteria ?? (_successCriteria = new List<ATrackedCriteria>());

	public List<ATrackedCriteria> failureCriteria => _failureCriteria ?? (_failureCriteria = new List<ATrackedCriteria>());

	public IEnumerable<ACriteriaCheck> successChecks
	{
		get
		{
			IEnumerable<ACriteriaCheck> enumerable = _successChecks;
			return enumerable ?? Enumerable.Empty<ACriteriaCheck>();
		}
	}

	public IEnumerable<ACriteriaCheck> failureChecks
	{
		get
		{
			IEnumerable<ACriteriaCheck> enumerable = _failureChecks;
			return enumerable ?? Enumerable.Empty<ACriteriaCheck>();
		}
	}

	public bool oneTimeOnly
	{
		get
		{
			if (!_copyExperienceFrom)
			{
				return _oneTimeOnly;
			}
			return _copyExperienceFrom.data._oneTimeOnly;
		}
	}

	public AbilityData.Rank rank
	{
		get
		{
			if (!_copyExperienceFrom)
			{
				return _rank;
			}
			return _copyExperienceFrom.data._rank;
		}
	}

	[ProtoMember(128)]
	public string tags { get; set; }

	private bool _hideExperience => _copyExperienceFrom;

	private bool _copyExperienceFromSpecified => _copyExperienceFrom.ShouldSerialize();

	public async void UnlockAchievement()
	{
		if (_achievementName.HasVisibleCharacter() && Steam.Enabled)
		{
			await Steam.Stats.UnlockAchievement(_achievementName);
		}
	}

	public string GetTitle()
	{
		if (!Application.isEditor || !_devName.HasVisibleCharacter())
		{
			return name;
		}
		return _devName;
	}

	public string GetAutomatedDescription()
	{
		return description;
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public void PrepareDataForSave()
	{
	}

	public string GetSaveErrorMessage()
	{
		if (name.HasVisibleCharacter())
		{
			return null;
		}
		return "Please give bonus a name before saving.";
	}

	public void OnLoadValidation()
	{
	}
}
