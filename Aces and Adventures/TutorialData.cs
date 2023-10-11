using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
[Localize]
public class TutorialData : IDataContent
{
	[ProtoContract]
	[UIField]
	public class TargetLineData
	{
		[ProtoMember(1)]
		[UIField(tooltip = "Place on tutorial card that the target line will start.")]
		[DefaultValue(CardTarget.Description)]
		[UIHorizontalLayout("T")]
		private CardTarget _beginFrom = CardTarget.Description;

		[ProtoMember(2)]
		[UIField(tooltip = "Place on targets that target line will end.")]
		[DefaultValue(CardTarget.Name)]
		[UIHorizontalLayout("T")]
		private CardTarget _endAt = CardTarget.Name;

		[ProtoMember(4)]
		[UIField(min = -180, max = 180, tooltip = "The rotation to apply to the end of the target line.")]
		[DefaultValue(45)]
		private int _endRotation = 45;

		[ProtoMember(5)]
		[UIField(min = -0.1f, max = 0.1f, view = "UI/Input Field Standard", tooltip = "The offset to apply to the end of the target line.")]
		[DefaultValue(0.0025f)]
		public float _endOffset = 0.0025f;

		[ProtoMember(3, OverwriteList = true)]
		[UIField(tooltip = "The targets that should have a target line aimed at them.", filterMethod = "_FilterTargetingAction")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private AAction _targetingAction;

		public CardTarget beginFrom => _beginFrom;

		public CardTarget endAt => _endAt;

		public AAction targetingAction => _targetingAction;

		public Quaternion endRotation => Quaternion.Euler(_endRotation, 0f, 0f);

		public Vector3 endOffset => new Vector3(0f, 0f, _endOffset);

		public override string ToString()
		{
			return "<size=66%>From " + EnumUtil.FriendlyName(_beginFrom).SizeIfNotEmpty(100) + " of tutorial card to " + EnumUtil.FriendlyName(_endAt).SizeIfNotEmpty(100) + " of " + _targetingAction?.ToString().Replace("Target ", "").SizeIfNotEmpty(100) + "</size>";
		}

		private bool _FilterTargetingAction(Type type)
		{
			if (!(type == typeof(TargetCombatantAction)) && !(type == typeof(TargetResourceAction)) && !(type == typeof(TargetPlayerAction)) && !(type == typeof(TargetAbilityAction)) && !(type == typeof(TargetSpaceAction)))
			{
				return !(type == typeof(TargetStoneAction));
			}
			return false;
		}
	}

	private const string CAT_MAIN = "Main";

	private const string CAT_TARGET = "Targets";

	[ProtoMember(13)]
	[UIField("Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
	[UICategory("Main")]
	[UIDeepValueChange]
	private LocalizedStringData _nameLocalized;

	[ProtoMember(14)]
	[UIField("Description", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
	[UICategory("Main")]
	[UIDeepValueChange]
	private LocalizedStringData _descriptionLocalized;

	[ProtoMember(3)]
	[UIField(filter = AudioCategoryType.Adventure)]
	[UICategory("Main")]
	private AudioRef _narration;

	[ProtoMember(4, OverwriteList = true)]
	[UIField(tooltip = "If any of the followings triggers are met, the tutorial is drawn and shown to the player.", collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UICategory("Main")]
	private List<AAction.Trigger> _triggers;

	[ProtoMember(5, OverwriteList = true)]
	[UIField(tooltip = "Target lines to be generated from the tutorial card to other cards on the table.", collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UICategory("Targets")]
	private List<TargetLineData> _targetLines;

	[ProtoMember(6, OverwriteList = true)]
	[UIField(tooltip = "If any of the following triggers are met, the tutorial is unregistered and won't be shown to the player.", collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UICategory("Main")]
	private List<AAction.Trigger> _antiTriggers;

	[ProtoMember(7, OverwriteList = true)]
	[UIField(tooltip = "All of the following conditions must be true for the player in order for the tutorial to trigger", collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UICategory("Main")]
	private List<AAction.Condition.Actor> _playerConditions;

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

	public string name => _nameLocalized ?? (_nameLocalized = new LocalizedStringData(""));

	public string description => _descriptionLocalized ?? (_descriptionLocalized = new LocalizedStringData(""));

	public AudioRef narration => _narration;

	public List<AAction.Trigger> triggers => _triggers ?? (_triggers = new List<AAction.Trigger>());

	public List<TargetLineData> targetLines => _targetLines ?? (_targetLines = new List<TargetLineData>());

	public List<AAction.Trigger> antiTriggers => _antiTriggers ?? (_antiTriggers = new List<AAction.Trigger>());

	public List<AAction.Condition.Actor> playerConditions => _playerConditions ?? (_playerConditions = new List<AAction.Condition.Actor>());

	[ProtoMember(15)]
	public string tags { get; set; }

	private bool _narrationSpecified => _narration;

	public string GetTitle()
	{
		return name;
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
		if (!name.HasVisibleCharacter())
		{
			return "Please give tutorial data a name before saving.";
		}
		return null;
	}

	public void OnLoadValidation()
	{
	}

	public static implicit operator bool(TutorialData data)
	{
		if (data != null && !data._triggers.IsNullOrEmpty())
		{
			return data.description.HasVisibleCharacter();
		}
		return false;
	}
}
