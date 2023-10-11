using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField("Top Deck", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Top Deck")]
public class TopDeckAction : AAction
{
	[ProtoContract]
	[UIField]
	public struct TopDeckConditionActionPair
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open)]
		[UIDeepValueChange]
		public TopDeckCondition condition;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Open)]
		[UIDeepValueChange]
		public AAction action;

		[ProtoMember(3)]
		[UIField]
		public TopDeckResult result;

		public override string ToString()
		{
			return ((result != TopDeckResult.None) ? ("<size=66%>" + EnumUtil.FriendlyName(result) + "</size> ") : "") + $"{condition}: {action}";
		}
	}

	public static readonly Target TARGET = new Target();

	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	private TopDeckInstruction _topDeck;

	[ProtoMember(2, OverwriteList = true)]
	[UIField(collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<TopDeckConditionActionPair> _actions;

	[ProtoMember(3)]
	[UIField]
	[DefaultValue(TopDeckResult.Failure)]
	private TopDeckResult _defaultResult = TopDeckResult.Failure;

	public TopDeckInstruction topDeck => _topDeck ?? (_topDeck = new TopDeckInstruction.Draw());

	public List<TopDeckConditionActionPair> actions => _actions ?? (_actions = new List<TopDeckConditionActionPair>());

	public override Target target => _actions?.FirstOrDefault().action?.target ?? TARGET;

	protected override bool _ShouldAct(ActionContext context)
	{
		if (!topDeck.ShouldAct(context))
		{
			if (base.isTicking)
			{
				return context.state != ActionContext.State.Tick;
			}
			return false;
		}
		return true;
	}

	private IEnumerable<GameStep> _GetActGameSteps(ActionContext context)
	{
		if (isApplied)
		{
			foreach (GameStep actGameStep in base.GetActGameSteps(context))
			{
				yield return actGameStep;
			}
			yield break;
		}
		yield return topDeck.GetGameStep(context);
		yield return new GameStepTopDeckInstructionComplete(this, context, _defaultResult);
		foreach (TopDeckConditionActionPair action in actions)
		{
			foreach (GameStep item in new GameStepGroupTopDeckAct(this, context, action))
			{
				yield return item;
			}
		}
		yield return new GameStepTopDeckComplete(this, context, _defaultResult);
	}

	public override IEnumerable<GameStep> GetActGameSteps(ActionContext context)
	{
		foreach (GameStep item in new GameStepGrouper(_GetActGameSteps(context)))
		{
			yield return item;
		}
	}

	private IEnumerable<GameStep> _GetTickGameSteps(ActionContext context)
	{
		yield return topDeck.GetGameStep(context);
		yield return new GameStepTopDeckInstructionComplete(this, context, _defaultResult);
		foreach (TopDeckConditionActionPair action in actions)
		{
			foreach (GameStep item in new GameStepGroupTopDeckTick(this, context, action))
			{
				yield return item;
			}
		}
		yield return new GameStepTopDeckComplete(this, context, _defaultResult);
	}

	public override IEnumerable<GameStep> GetTickGameSteps(ActionContext context)
	{
		foreach (GameStep item in new GameStepGrouper(_GetTickGameSteps(context)))
		{
			yield return item;
		}
	}

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		foreach (AbilityKeyword keyword2 in topDeck.GetKeywords())
		{
			yield return keyword2;
		}
		foreach (TopDeckConditionActionPair action in actions)
		{
			foreach (AbilityKeyword keyword3 in action.action.GetKeywords(abilityData))
			{
				yield return keyword3;
			}
		}
	}

	protected override string _ToStringUnique()
	{
		return _topDeck?.ToString() + " " + _actions.ToStringSmart(" & ") + "<size=66%> Otherwise " + EnumUtil.FriendlyName(_defaultResult) + "</size>";
	}

	protected override string _GetTargetString()
	{
		return "";
	}
}
