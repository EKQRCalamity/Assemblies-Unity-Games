using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BonusCard : ATarget, IAdventureCard
{
	private static readonly List<BonusCardData.ATrackedCriteria> EMPTY_CRITERIA = new List<BonusCardData.ATrackedCriteria>();

	[ProtoMember(1)]
	private DataRef<BonusCardData> _dataRef;

	[ProtoMember(2, OverwriteList = true)]
	private List<BonusCardData.ATrackedCriteria> _successCriteria;

	[ProtoMember(3, OverwriteList = true)]
	private List<BonusCardData.ATrackedCriteria> _failureCriteria;

	[ProtoMember(4)]
	private int _tally;

	[ProtoMember(5)]
	private int _failTally;

	[ProtoMember(6)]
	private bool? _isNew;

	public DataRef<BonusCardData> dataRef => _dataRef;

	private BonusCardData _data => _dataRef.data;

	private List<BonusCardData.ATrackedCriteria> successCriteria
	{
		get
		{
			if (_data.successCriteria.Count <= 0)
			{
				return EMPTY_CRITERIA;
			}
			return _successCriteria ?? (_successCriteria = ProtoUtil.Clone(_data.successCriteria));
		}
	}

	private List<BonusCardData.ATrackedCriteria> failureCriteria
	{
		get
		{
			if (_data.failureCriteria.Count <= 0)
			{
				return EMPTY_CRITERIA;
			}
			return _failureCriteria ?? (_failureCriteria = ProtoUtil.Clone(_data.failureCriteria));
		}
	}

	public bool isNew
	{
		get
		{
			bool valueOrDefault = _isNew.GetValueOrDefault();
			if (!_isNew.HasValue)
			{
				GameState obj = base.gameState;
				valueOrDefault = (obj == null || !obj.dailyLeaderboardIsValid) && !ProfileManager.progress.characters.read[base.gameState].HasUnlockedBonus(_dataRef);
				_isNew = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool oneTimeOnly => _data.oneTimeOnly;

	public int experience => _data.experience * (isNew && !oneTimeOnly).ToInt(2, 1);

	public AbilityData.Rank rank => _data.rank;

	public override int registerDuringGameStateInitializationOrder => -1000;

	public override bool shouldRegisterDuringGameStateInitialization => true;

	public ATarget adventureCard => this;

	public AdventureCard.Pile pileToTransferToOnDraw => AdventureCard.Pile.TurnOrder;

	public AdventureCard.Pile pileToTransferToOnSelect => AdventureCard.Pile.Discard;

	public GameStep selectTransferStep => null;

	public AdventureCard.Common adventureCardCommon
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string name => _data.name;

	public string description => _data.description;

	public CroppedImageRef image => _data.image;

	public IEnumerable<GameStep> selectedGameSteps
	{
		get
		{
			yield break;
		}
	}

	public ResourceBlueprint<GameObject> blueprint => BonusCardView.Blueprint;

	private bool _dataRefSpecified => _dataRef.ShouldSerialize();

	private BonusCard()
	{
	}

	public BonusCard(DataRef<BonusCardData> dataRef)
	{
		_dataRef = dataRef;
	}

	private void _OnTallyAdjusted(int adjustment)
	{
		_tally += adjustment;
	}

	private void _OnFailTallyAdjusted(int adjustment)
	{
		_failTally += adjustment;
	}

	public override void _Register()
	{
		foreach (BonusCardData.ATrackedCriteria successCriterion in successCriteria)
		{
			successCriterion.Register(base.gameState);
			successCriterion.onTallyAdjusted += _OnTallyAdjusted;
		}
		foreach (BonusCardData.ATrackedCriteria failureCriterion in failureCriteria)
		{
			failureCriterion.Register(base.gameState);
			failureCriterion.onTallyAdjusted += _OnFailTallyAdjusted;
		}
	}

	public override void _Unregister()
	{
		foreach (BonusCardData.ATrackedCriteria successCriterion in successCriteria)
		{
			successCriterion.Unregister(base.gameState);
			successCriterion.onTallyAdjusted -= _OnTallyAdjusted;
		}
		foreach (BonusCardData.ATrackedCriteria failureCriterion in failureCriteria)
		{
			failureCriterion.Unregister(base.gameState);
			failureCriterion.onTallyAdjusted -= _OnFailTallyAdjusted;
		}
	}

	public bool IsSuccessful()
	{
		if (_data.failureChecks.Any() && _data.failureChecks.All((BonusCardData.ACriteriaCheck check) => check.Check(new ActionContext(base.gameState.player, null, base.gameState.player), _tally, _failTally)))
		{
			return false;
		}
		return _data.successChecks.All((BonusCardData.ACriteriaCheck check) => check.Check(new ActionContext(base.gameState.player, null, base.gameState.player), _tally, _failTally));
	}

	public void Unlock()
	{
		base.gameState.experience += experience;
		ProfileManager.progress.characters.write[base.gameState].UnlockBonus(dataRef);
		_data.UnlockAchievement();
	}
}
