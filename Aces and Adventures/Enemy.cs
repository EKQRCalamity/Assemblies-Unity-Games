using System.Linq;
using ProtoBuf;

[ProtoContract]
public class Enemy : ACombatant
{
	private static TextBuilder _Builder;

	[ProtoMember(1)]
	private DataRef<EnemyData> _enemyDataRef;

	[ProtoMember(2)]
	private bool _isAdd;

	private static TextBuilder Builder => _Builder ?? (_Builder = new TextBuilder(clearOnToString: true));

	public EnemyData enemyData => _enemyDataRef.data;

	public DataRef<EnemyData> enemyDataRef => _enemyDataRef;

	public EnemyCardView enemyCard => base.view as EnemyCardView;

	public override string name
	{
		get
		{
			if (!base.name.IsNullOrEmpty())
			{
				return base.name;
			}
			return enemyData.name;
		}
	}

	public override string description => GetTraitDescription();

	public override CroppedImageRef image
	{
		get
		{
			if (!base.image)
			{
				return enemyData?.cosmetic.image;
			}
			return base.image;
		}
	}

	public override AdventureCard.Pile pileToTransferToOnDraw
	{
		get
		{
			if (!_isAdd)
			{
				return base.pileToTransferToOnDraw;
			}
			return AdventureCard.Pile.ActiveHand;
		}
	}

	public override bool canBePooled => true;

	public override Faction faction => Faction.Enemy;

	public override IdDeck<ResourceCard.Pile, ResourceCard> resourceDeck => base.gameState.enemyResourceDeck;

	public override IdDeck<Ability.Pile, Ability> abilityDeck => null;

	public override CombatantData combatantData => enemyData;

	public override int registerDuringGameStateInitializationOrder => 100;

	private Enemy()
	{
	}

	public Enemy(DataRef<EnemyData> enemyDataRef, bool isAdd)
	{
		_enemyDataRef = enemyDataRef;
		_isAdd = isAdd;
		_Initialize(enemyData);
		base.gameState?.SignalCombatantAdded(this);
	}

	public override int GetOffenseAgainst(ACombatant defender, bool shouldTriggerMedia = false)
	{
		return base.gameState.ProcessEnemyCombatStat(this, defender, base.GetOffenseAgainst(defender, shouldTriggerMedia), shouldTriggerMedia, 1);
	}

	public override int GetDefenseAgainst(ACombatant attacker, bool shouldTriggerMedia = false)
	{
		return base.gameState.ProcessEnemyCombatStat(attacker, this, base.GetDefenseAgainst(attacker, shouldTriggerMedia), shouldTriggerMedia, (!base.statuses.canBeReducedToZeroDefense && !(attacker?.statuses.canReduceEnemyDefenseToZero)) ? 1 : 0);
	}

	public override AGameStepTurn GetTurnStep()
	{
		return new GameStepTurnEnemy(this);
	}

	public override GameStep GetDefenseStep()
	{
		return new GameStepPresentDefenseEnemy();
	}

	public string GetTraitDescription(bool fadeOutInactiveTraits = true)
	{
		using PoolKeepItemListHandle<DataRef<AbilityData>> poolKeepItemListHandle2 = Pools.UseKeepItemList(from trait in Traits().AsEnumerable()
			select trait.value.dataRef);
		using PoolKeepItemListHandle<DataRef<AbilityData>> poolKeepItemListHandle = Pools.UseKeepItemList(combatantData.traits);
		using PoolKeepItemHashSetHandle<DataRef<AbilityData>> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(poolKeepItemListHandle.value);
		foreach (DataRef<AbilityData> item in poolKeepItemListHandle2.value)
		{
			poolKeepItemHashSetHandle.Add(item);
		}
		bool flag = poolKeepItemHashSetHandle.Count <= 3;
		string text = flag.ToText("\n", ", ");
		foreach (DataRef<AbilityData> item2 in poolKeepItemHashSetHandle.value)
		{
			bool flag2 = !poolKeepItemListHandle2.value.Contains(item2) && fadeOutInactiveTraits;
			bool num = !poolKeepItemListHandle.value.Contains(item2);
			bool permanent = item2.data.permanent;
			if (flag2)
			{
				Builder.StrikeThrough().Alpha(200);
			}
			if (permanent)
			{
				Builder.Bold();
			}
			if (num)
			{
				Builder.Italic();
			}
			if (flag)
			{
				Builder.NoBreak();
			}
			Builder.Append(item2.data.name);
			if (flag)
			{
				Builder.EndNoBreak();
			}
			if (num)
			{
				Builder.EndItalic();
			}
			if (permanent)
			{
				Builder.EndBold();
			}
			if (flag2)
			{
				Builder.EndStrikeThrough().EndAlpha();
			}
			Builder.Append(text);
		}
		if (Builder.length > 0)
		{
			Builder.RemoveFromEnd(text.Length);
		}
		if (!base.description.IsNullOrEmpty())
		{
			Builder.Append(((Builder.length > 0) ? "\n" : "") + "<size=80%><align=center>\"<i>" + base.description + "</i>\"</align></size>");
		}
		return Builder;
	}
}
