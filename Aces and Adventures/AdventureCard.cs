using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField]
[UICategorySort(CategorySortType.Appearance)]
[ProtoInclude(4, typeof(RepeatCard))]
[ProtoInclude(5, typeof(ProceduralNodePackCard))]
[ProtoInclude(6, typeof(RandomCard))]
[ProtoInclude(7, typeof(ProceduralNodeCard))]
[ProtoInclude(8, typeof(Blank))]
[ProtoInclude(9, typeof(NewGame))]
[ProtoInclude(10, typeof(Story))]
[ProtoInclude(11, typeof(Encounter))]
[ProtoInclude(12, typeof(Enemy))]
[ProtoInclude(14, typeof(Item))]
[ProtoInclude(15, typeof(DemoOnly))]
[ProtoInclude(16, typeof(Procedural))]
public abstract class AdventureCard
{
	[ProtoContract]
	[UIField(category = "Main")]
	public class Story : AdventureCard
	{
		protected override ATarget _GenerateCard(GameState gameState)
		{
			return new StoryCard();
		}

		public override string ToString()
		{
			return _ToStringPrefix() + "<b>Story</b>" + base.ToString();
		}
	}

	[ProtoContract]
	[UIField(category = "Main")]
	public class Encounter : AdventureCard
	{
		[ProtoMember(1, OverwriteList = true)]
		[UIField(tooltip = "Instructions that take place when this encounter is completed.", maxCount = 10)]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<SelectInstruction> _onCompletedInstructions;

		public List<SelectInstruction> onCompletedInstructions => _onCompletedInstructions;

		protected override ATarget _GenerateCard(GameState gameState)
		{
			return new EncounterCard(this);
		}

		public override string ToString()
		{
			return _ToStringPrefix() + "<b>Encounter</b>" + base.ToString() + ((!_onCompletedInstructions.IsNullOrEmpty()) ? (", <b>When Completed: " + _onCompletedInstructions.ToStringSmart(" & ") + "</b>") : "").SizeIfNotEmpty();
		}
	}

	[ProtoContract]
	[UIField(category = "Main")]
	public class Enemy : AdventureCard
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<EnemyData> _enemy;

		[ProtoMember(2)]
		[UIField(tooltip = "Enemy is not placed into turn order automatically, instead it will be added when some condition is met.")]
		private bool _isAdd;

		public DataRef<EnemyData> enemy
		{
			get
			{
				return _enemy;
			}
			set
			{
				_enemy = value;
			}
		}

		private Enemy _SetEnemy(DataRef<EnemyData> enemyToSet)
		{
			_enemy = enemyToSet;
			return this;
		}

		protected override ATarget _GenerateCard(GameState gameState)
		{
			return new global::Enemy(_enemy, _isAdd);
		}

		public override string ToString()
		{
			return _ToStringPrefix() + "<b>Enemy</b>" + (base.common.name.IsNullOrEmpty() ? (" " + _enemy.GetFriendlyName()) : "") + _isAdd.ToText(" <b>[Add]</b>") + base.ToString();
		}

		protected override void _OnConvertedToNewGameCard(NewGame newGameCard)
		{
			if (!_enemy)
			{
				return;
			}
			using PoolKeepItemListHandle<DataRef<EnemyData>> poolKeepItemListHandle = _enemy.GetUpgradeHierarchy();
			for (int i = 0; i < poolKeepItemListHandle.Count; i++)
			{
				newGameCard.SetOverride(ProtoUtil.Clone(this)._SetEnemy(poolKeepItemListHandle[i]), EnumUtil<NewGameType>.Round(i));
			}
		}
	}

	[ProtoContract]
	[UIField(category = "Main")]
	public class Item : AdventureCard
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open, validateOnChange = true)]
		private DataRef<AbilityData> _ability;

		[ProtoMember(15)]
		[UIField("Cost Description", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
		[UIDeepValueChange]
		private LocalizedStringData _costDescriptionLocalized;

		[ProtoMember(3)]
		[UIField]
		[UIHideIf("_hideIsConsumable")]
		private bool _isConsumable;

		[ProtoMember(4)]
		[UIField]
		[UIHideIf("_hideIsEncounterAbility")]
		private ItemCardType _type;

		[ProtoMember(2)]
		private string _costDescription
		{
			get
			{
				return null;
			}
			set
			{
				if (_costDescriptionLocalized == null)
				{
					_costDescriptionLocalized = new LocalizedStringData(value);
				}
			}
		}

		public DataRef<AbilityData> ability => _ability;

		public string costDescription => _costDescriptionLocalized ?? (_costDescriptionLocalized = new LocalizedStringData(""));

		public bool isConsumable
		{
			get
			{
				if (_isConsumable && (bool)_ability)
				{
					return !_ability.data.type.IsTrait();
				}
				return false;
			}
		}

		public bool isEncounterAbility => _type == ItemCardType.Encounter;

		public ItemCardType type => _type;

		private bool _hideIsConsumable
		{
			get
			{
				if ((bool)_ability)
				{
					return _ability.data.type.IsTrait();
				}
				return true;
			}
		}

		private bool _hideIsEncounterAbility => !_ability;

		private Item _ClearDataForItemCard()
		{
			_costDescriptionLocalized?.ClearRawText();
			_common = null;
			return this;
		}

		protected override ATarget _GenerateCard(GameState gameState)
		{
			return new ItemCard(ProtoUtil.Clone(this)._ClearDataForItemCard(), gameState.player);
		}

		public override string ToString()
		{
			return _ToStringPrefix() + "<b>" + isConsumable.ToText("Consumable ") + ((isEncounterAbility && !_hideIsEncounterAbility) ? ("Encounter " + _ability.data.type.IsTrait().ToText("Condition", "Ability")) : EnumUtil.FriendlyName(_type)) + "</b>" + base.common.name.IsNullOrEmpty().ToText(" " + _ability.GetFriendlyName()) + (_ability ? (" <size=66%>\"" + _ability.data.description.Replace("\n", ", ") + "\"</size>") : "") + costDescription.PreSpaceIfNotEmpty().SizeIfNotEmpty().ItalicIfNotEmpty() + base.ToString();
		}
	}

	[ProtoContract]
	[UIField(category = "Conditional")]
	public class DemoOnly : AdventureCard
	{
		[ProtoMember(1)]
		[UIField(" ", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Hide)]
		[UIDeepValueChange]
		private AdventureCard _card;

		protected override bool _hideShowCommon => true;

		public override IEnumerable<ATarget> GenerateCards(GameState gameState)
		{
			if (!IOUtil.IsDemo)
			{
				return Enumerable.Empty<ATarget>();
			}
			return _card.GenerateCards(gameState);
		}

		protected override Common _GetCommon(GameState gameState)
		{
			return _card?.common;
		}

		public override string ToString()
		{
			return "<b>[DEMO ONLY]</b>: " + (_card?.ToString() ?? "NULL");
		}
	}

	[ProtoContract]
	[UIField(category = "Conditional")]
	public class NewGame : AdventureCard
	{
		[ProtoContract]
		[UIField]
		public class Override
		{
			[ProtoMember(1)]
			[UIField(validateOnChange = true, onValueChangedMethod = "_OnOverrideChange", tooltip = "Hold <b>Alt</b> while enabling to clear localization table links of cloned card.")]
			[UIHorizontalLayout("A", preferredWidth = 1f, flexibleWidth = 0f, minWidth = 150f)]
			private bool _override;

			[ProtoMember(2)]
			[UIField(" ", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
			[UIHideIf("_hideCard")]
			[UIHorizontalLayout("A", preferredWidth = 1f, flexibleWidth = 999f)]
			[UIDeepValueChange]
			private AdventureCard _card;

			public bool enabled
			{
				get
				{
					return _override;
				}
				set
				{
					_override = value;
				}
			}

			private bool _hideCard => !_override;

			public Override()
			{
			}

			public Override(AdventureCard card)
			{
				_override = true;
				_card = card;
			}

			public static implicit operator bool(Override o)
			{
				return o?._override ?? false;
			}

			public static implicit operator AdventureCard(Override o)
			{
				if (!o)
				{
					return null;
				}
				return o._card;
			}

			private void _OnOverrideChange()
			{
				if (!_override || _card == null || !InputManager.I[KeyModifiers.Alt])
				{
					return;
				}
				foreach (LocalizedStringData item in ReflectionUtil.GetValuesFromUI<LocalizedStringData>(_card))
				{
					_ = item;
				}
			}
		}

		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Hide)]
		[UIHeader("Winter")]
		private Override _winter;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Hide)]
		[UIHeader("Fall")]
		private Override _fall;

		[ProtoMember(3)]
		[UIField(collapse = UICollapseType.Hide)]
		[UIHeader("Summer")]
		private Override _summer;

		[ProtoMember(4)]
		[UIField(collapse = UICollapseType.Hide)]
		[UIHeader("Spring")]
		private Override _spring;

		private AdventureCard this[NewGameType type] => _GetOverride(type);

		protected override bool _hideShowCommon => true;

		private bool _winterSpecified => _winter;

		private bool _fallSpecified => _fall;

		private bool _summerSpecified => _summer;

		private bool _springSpecified => _spring;

		public NewGame()
		{
		}

		public NewGame(AdventureCard spring)
		{
			_spring = new Override(spring);
		}

		private Override _GetOverride(NewGameType type)
		{
			return type switch
			{
				NewGameType.Summer => _summer, 
				NewGameType.Fall => _fall, 
				NewGameType.Winter => _winter, 
				_ => _spring, 
			};
		}

		private AdventureCard _GetAdventureCard(NewGameType type)
		{
			foreach (NewGameType item in EnumUtil.EnumerateDescending(type))
			{
				AdventureCard adventureCard = this[item];
				if (adventureCard != null)
				{
					return adventureCard;
				}
			}
			return null;
		}

		public override IEnumerable<ATarget> GenerateCards(GameState gameState)
		{
			return _GetAdventureCard(gameState.game.data.newGameType)?.GenerateCards(gameState) ?? Enumerable.Empty<ATarget>();
		}

		protected override Common _GetCommon(GameState gameState)
		{
			return _GetAdventureCard(gameState.game.data.newGameType)?._GetCommon(gameState);
		}

		public override string ToString()
		{
			return "<b>[NEW GAME]</b>: " + (_GetAdventureCard(EnumUtil<NewGameType>.Max)?.ToString() ?? "NULL");
		}

		public bool HasEmptySlot()
		{
			return EnumUtil<NewGameType>.Values.Any((NewGameType newGame) => this[newGame] == null);
		}

		public AdventureCard GetOverride(NewGameType newGame)
		{
			return this[newGame];
		}

		public Override SetOverride(AdventureCard card, NewGameType newGame)
		{
			Override @override = new Override(card);
			switch (newGame)
			{
			case NewGameType.Spring:
				_spring = @override;
				break;
			case NewGameType.Summer:
				_summer = @override;
				break;
			case NewGameType.Fall:
				_fall = @override;
				break;
			case NewGameType.Winter:
				_winter = @override;
				break;
			default:
				throw new ArgumentOutOfRangeException("newGame", newGame, null);
			}
			return @override;
		}

		private void OnValidateUI()
		{
			NewGameType[] values = EnumUtil<NewGameType>.Values;
			foreach (NewGameType newGameType in values)
			{
				if (!_GetOverride(newGameType))
				{
					AdventureCard adventureCard = _GetAdventureCard(newGameType);
					if (adventureCard != null)
					{
						SetOverride(ProtoUtil.Clone(adventureCard), newGameType).enabled = false;
					}
				}
			}
		}
	}

	[ProtoContract]
	[UIField(category = "Conditional")]
	public class Blank : AdventureCard
	{
		protected override bool _hideShowCommon => true;

		protected override ATarget _GenerateCard(GameState gameState)
		{
			return null;
		}

		protected override Common _GetCommon(GameState gameState)
		{
			return null;
		}

		public override string ToString()
		{
			return "[BLANK]";
		}
	}

	[ProtoContract]
	[UIField("Procedural Graph", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Procedural")]
	public class Procedural : AdventureCard
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<ProceduralGraphData> _graph;

		private bool _graphSpecified => _graph.ShouldSerialize();

		protected override ATarget _GenerateCard(GameState gameState)
		{
			return new ProceduralCard(_graph);
		}

		public override string ToString()
		{
			return _ToStringPrefix() + "<b>Procedural: " + _graph.GetFriendlyName() + "</b>" + base.ToString();
		}
	}

	[ProtoContract]
	[UIField("Procedural Node Reference", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Generates cards contained by the Procedural Node.", category = "Procedural")]
	public class ProceduralNodeCard : AdventureCard
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<ProceduralNodeData> _node;

		[ProtoMember(2, OverwriteList = true)]
		private List<ATarget> _cards;

		protected override bool _hideShowCommon => true;

		private bool _nodeSpecified => _node.ShouldSerialize();

		private ProceduralNodeCard()
		{
		}

		public ProceduralNodeCard(DataRef<ProceduralNodeData> node)
		{
			_node = node;
		}

		public override void OnGraphGenerated(GameState state)
		{
			if ((bool)_node)
			{
				_cards = new List<ATarget>(GenerateCards(state));
				_node = null;
			}
		}

		public override IEnumerable<ATarget> GenerateCards(GameState state)
		{
			if (!_cards.IsNullOrEmpty())
			{
				return _cards.CloneTargets();
			}
			PoolKeepItemListHandle<ATarget> poolKeepItemListHandle = Pools.UseKeepItemList(_node ? _node.data.cards.GenerateCards(state) : Enumerable.Empty<ATarget>());
			if (poolKeepItemListHandle.value.FirstOrDefault() is IAdventureCard adventureCard)
			{
				adventureCard.adventureCardCommon.AddAboutToDrawInstructions(_node.data.onSelectInstructions.Select(ProtoUtil.Clone));
			}
			return poolKeepItemListHandle.AsEnumerable();
		}

		public override string ToString()
		{
			return _ToStringPrefix() + "<b>Procedural Node: " + _node.GetFriendlyName() + "</b>" + base.ToString();
		}
	}

	[ProtoContract]
	[UIField(category = "Procedural")]
	public class RandomCard : AdventureCard
	{
		[ProtoMember(1, OverwriteList = true)]
		[UIField(maxCount = 0, collapse = UICollapseType.Open)]
		[UIFieldCollectionItem]
		[UIFieldKey(flexibleWidth = 3f)]
		[UIFieldValue(min = 0.01f, max = 100, defaultValue = 1, flexibleWidth = 0.2f)]
		[UIDeepValueChange]
		private Dictionary<AdventureCard, float> _cardWeights;

		[ProtoMember(2, OverwriteList = true)]
		private List<ATarget> _cards;

		protected override bool _hideShowCommon => true;

		public override void OnGraphGenerated(GameState state)
		{
			if (!_cardWeights.IsNullOrEmpty())
			{
				_cards = new List<ATarget>(GenerateCards(state));
				_cardWeights = null;
			}
		}

		public override IEnumerable<ATarget> GenerateCards(GameState state)
		{
			object obj = _cards?.CloneTargets();
			if (obj == null)
			{
				if (_cardWeights.IsNullOrEmpty())
				{
					return Enumerable.Empty<ATarget>();
				}
				obj = _cardWeights.RandomKey(state.random).GenerateCards(state);
			}
			return (IEnumerable<ATarget>)obj;
		}

		public override string ToString()
		{
			string obj = _cardWeights?.Keys.FirstOrDefault()?.ToString() ?? "NULL";
			Dictionary<AdventureCard, float> cardWeights = _cardWeights;
			return "<b>Random:</b> " + obj + ((cardWeights != null && cardWeights.Count > 1) ? $" & {_cardWeights.Count - 1} other(s)" : "").SizeIfNotEmpty() + base.ToString();
		}
	}

	[ProtoContract]
	[UIField("Procedural Pack Reference", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows randomly selecting a Procedural Node Data from a Procedural Node Pack to generate cards from.", category = "Procedural")]
	public class ProceduralNodePackCard : AdventureCard
	{
		[ProtoMember(1, OverwriteList = true)]
		[UIField(maxCount = 0, collapse = UICollapseType.Open)]
		[UIFieldCollectionItem]
		[UIFieldValue(min = 0.01f, max = 100, defaultValue = 1)]
		[UIDeepValueChange]
		private Dictionary<DataRef<ProceduralNodePackData>, float> _packWeights;

		[ProtoMember(2, OverwriteList = true)]
		private List<ATarget> _cards;

		protected override bool _hideShowCommon => true;

		public override void OnGraphGenerated(GameState state)
		{
			if (!_packWeights.IsNullOrEmpty())
			{
				_cards = new List<ATarget>(GenerateCards(state));
				_packWeights = null;
			}
		}

		public override IEnumerable<ATarget> GenerateCards(GameState state)
		{
			object obj = _cards?.CloneTargets();
			if (obj == null)
			{
				if (!_packWeights.IsNullOrEmpty())
				{
					Dictionary<DataRef<ProceduralNodePackData>, ProceduralWeight> packs = _packWeights.ToDictionary((KeyValuePair<DataRef<ProceduralNodePackData>, float> p) => p.Key, (KeyValuePair<DataRef<ProceduralNodePackData>, float> p) => new ProceduralWeight(p.Value));
					ProceduralNodePack.Selection selection = new ProceduralNodePack(null, null, packs).GetSelection(state.random, state.graph);
					ProceduralGraph graph = state.graph;
					if (graph == null || graph.PreventRepeatedNodeDataAddTrue(selection.node))
					{
						obj = new ProceduralNodeCard(selection.node).GenerateCards(state);
						goto IL_00c6;
					}
				}
				return Enumerable.Empty<ATarget>();
			}
			goto IL_00c6;
			IL_00c6:
			return (IEnumerable<ATarget>)obj;
		}

		public override string ToString()
		{
			string[] obj = new string[6]
			{
				_ToStringPrefix(),
				"<b>Procedural Pack: ",
				_packWeights?.Keys.FirstOrDefault().GetFriendlyName(),
				"</b>",
				null,
				null
			};
			Dictionary<DataRef<ProceduralNodePackData>, float> packWeights = _packWeights;
			obj[4] = ((packWeights != null && packWeights.Count > 1) ? $" & {_packWeights.Count - 1} other(s)" : "").SizeIfNotEmpty();
			obj[5] = base.ToString();
			return string.Concat(obj);
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Allows repeating a card a random number of times.", category = "Procedural")]
	public class RepeatCard : AdventureCard
	{
		[ProtoContract(EnumPassthru = true)]
		public enum DrawInstructionType
		{
			None,
			Draw,
			PickOne,
			PickFlexible
		}

		[ProtoContract(EnumPassthru = true)]
		private enum Count
		{
			Zero,
			One,
			Two,
			Three,
			Four,
			Five
		}

		[ProtoMember(1, OverwriteList = true)]
		[UIField(collapse = UICollapseType.Open)]
		[UIFieldCollectionItem]
		[UIFieldValue(defaultValue = 1f, min = 0.01f, max = 100f)]
		[UIDeepValueChange]
		private Dictionary<Count, float> _repeatCounts;

		[ProtoMember(2)]
		[UIField]
		private DrawInstructionType _drawInstructions;

		[ProtoMember(3)]
		[UIField(collapse = UICollapseType.Hide)]
		[UIHeader("Card To Repeat")]
		private AdventureCard _cardToRepeat;

		protected override bool _hideShowCommon => true;

		public override IEnumerable<ATarget> GenerateCards(GameState state)
		{
			PoolKeepItemListHandle<ATarget> poolKeepItemListHandle = Pools.UseKeepItemList<ATarget>();
			int num = (int)((!_repeatCounts.IsNullOrEmpty()) ? _repeatCounts.RandomKey(state.random) : Count.Zero);
			for (int i = 0; i < num; i++)
			{
				foreach (ATarget item in _cardToRepeat.GenerateCards(state))
				{
					poolKeepItemListHandle.Add(item);
				}
			}
			SelectInstruction instruction = _drawInstructions.GetInstruction((byte)num);
			if (instruction != null && poolKeepItemListHandle.value.FirstOrDefault() is IAdventureCard adventureCard)
			{
				adventureCard.adventureCardCommon.AddAboutToDrawInstruction(instruction);
			}
			return poolKeepItemListHandle.AsEnumerable();
		}

		public override string ToString()
		{
			return string.Format("<b>Repeat {0}[{1}]:</b> {2}", (_drawInstructions != 0) ? (EnumUtil.FriendlyName(_drawInstructions) + " ") : "", _repeatCounts?.Keys.Select((Count k) => (int)k).ToStringSmart(), _cardToRepeat);
		}
	}

	[ProtoContract(EnumPassthru = true)]
	public enum Pile
	{
		Draw,
		SelectionHand,
		ActiveHand,
		TurnOrder,
		ItemActHand,
		ItemTraitHand,
		Discard
	}

	[Flags]
	public enum Piles
	{
		Draw = 1,
		SelectionHand = 2,
		ActiveHand = 4,
		TurnOrder = 8,
		ItemActHand = 0x10,
		ItemTraitHand = 0x20,
		Discard = 0x40
	}

	[ProtoContract]
	[UIField]
	public class Common
	{
		private static readonly char[] DESCRIPTION_SPLIT = new char[1] { '\n' };

		[ProtoMember(14)]
		[UIField("Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
		[UIDeepValueChange]
		private LocalizedStringData _nameLocalized;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Hide)]
		private CroppedImageRef _image = new CroppedImageRef(ImageCategoryType.Adventure, IMAGE_SIZE, ImageCategoryFlags.Ability | ImageCategoryFlags.Enemy);

		[ProtoMember(15)]
		[UIField("Description", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
		[UIDeepValueChange]
		private LocalizedStringData _descriptionLocalized;

		[ProtoMember(5)]
		[UIField(filter = AudioCategoryType.Adventure)]
		private AudioRef _narration;

		[ProtoMember(7)]
		[UIField(min = 0, max = 10, stepSize = 0.01f, tooltip = "Delay before text begins typing after narration audio begins.")]
		[UIHorizontalLayout("Time")]
		private float _narrationStart;

		[ProtoMember(8)]
		[UIField(min = 0, max = 100, stepSize = 0.01f, tooltip = "Time in narration audio where voice is finished and text typing should be complete.")]
		[UIHorizontalLayout("Time")]
		[DefaultValue(100)]
		private float _narrationEnd = 100f;

		[ProtoMember(6, OverwriteList = true)]
		[UIField(tooltip = "Instructions that take place when this card is drawn.", maxCount = 10)]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<SelectInstruction> _onDrawnInstructions;

		[ProtoMember(4, OverwriteList = true)]
		[UIField("On Selected Instructions", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Instructions that take place when this card is selected.", collapse = UICollapseType.Open, maxCount = 10)]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<SelectInstruction> _onSelectInstructions;

		[ProtoMember(9, OverwriteList = true)]
		private List<SelectInstruction> _onAboutToDrawInstructions;

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

		[ProtoMember(3)]
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

		public CroppedImageRef image => _image;

		public AudioRef narration => _narration;

		public float narrationStart => _narrationStart;

		public float narrationDurationAdjustment
		{
			get
			{
				if (!_narration)
				{
					return 0f;
				}
				return Math.Min(_narrationEnd, _narration.audioClip.length) - _narration.audioClip.length;
			}
		}

		public string description => _descriptionLocalized ?? (_descriptionLocalized = new LocalizedStringData(""));

		public List<SelectInstruction> selectInstructions
		{
			get
			{
				if (_onSelectInstructions.IsNullOrEmpty())
				{
					return DEFAULT_SELECT_INSTRUCTIONS;
				}
				return _onSelectInstructions;
			}
		}

		public IEnumerable<SelectInstruction> aboutToDrawInstructions
		{
			get
			{
				IEnumerable<SelectInstruction> onAboutToDrawInstructions = _onAboutToDrawInstructions;
				return onAboutToDrawInstructions ?? Enumerable.Empty<SelectInstruction>();
			}
		}

		private bool _imageSpecified => _image;

		private bool _narrationSpecified => _narration;

		public CanBeSelectedResult CanBeSelected(GameState gameState)
		{
			if (!_onSelectInstructions.IsNullOrEmpty())
			{
				foreach (SelectInstruction onSelectInstruction in _onSelectInstructions)
				{
					CanBeSelectedResult canBeSelectedResult = onSelectInstruction.CanBeSelected(gameState);
					if (!canBeSelectedResult)
					{
						return canBeSelectedResult;
					}
				}
			}
			return AbilityPreventedBy.Nothing;
		}

		public IEnumerable<GameStep> GetOnDrawnSteps(GameState gameState)
		{
			if (_onDrawnInstructions.IsNullOrEmpty())
			{
				yield break;
			}
			foreach (SelectInstruction onDrawnInstruction in _onDrawnInstructions)
			{
				foreach (GameStep gameStep in onDrawnInstruction.GetGameSteps(gameState))
				{
					yield return gameStep;
				}
			}
		}

		public Common AddAboutToDrawInstructions(IEnumerable<SelectInstruction> instructions)
		{
			if (instructions != null)
			{
				foreach (SelectInstruction instruction in instructions)
				{
					(_onAboutToDrawInstructions ?? (_onAboutToDrawInstructions = new List<SelectInstruction>())).Add(instruction);
				}
				return this;
			}
			return this;
		}

		public Common AddAboutToDrawInstruction(SelectInstruction instruction, int? insertIndex = null)
		{
			if (instruction != null)
			{
				(_onAboutToDrawInstructions ?? (_onAboutToDrawInstructions = new List<SelectInstruction>())).Insert(insertIndex ?? _onAboutToDrawInstructions.Count, instruction);
			}
			return this;
		}

		public void ClearAboutToDrawInstructions()
		{
			_onAboutToDrawInstructions = null;
		}

		public Common ClearGeneratedData()
		{
			_nameLocalized?.ClearRawText();
			_descriptionLocalized?.ClearRawText();
			return this;
		}

		public override string ToString()
		{
			return _onDrawnInstructions.ToStringSmart(" & ").BoldIfNotEmpty().SpaceIfNotEmpty()
				.SizeIfNotEmpty() + name.SpaceIfNotEmpty() + (((!description.IsNullOrEmpty()) ? ("\"<i>" + description.Split(DESCRIPTION_SPLIT, 2)[0].MaxLengthOf(128) + "</i>\" ") : "") + ((!_onSelectInstructions.IsNullOrEmpty()) ? ("<b>" + _onSelectInstructions.ToStringSmart(" & ") + "</b>") : "")).SizeIfNotEmpty();
		}

		private string _GetLocalizedKeyLabel()
		{
			return name;
		}
	}

	[ProtoContract]
	[UIField]
	[ProtoInclude(1, typeof(BlankInstruction))]
	[ProtoInclude(2, typeof(NewGameInstruction))]
	[ProtoInclude(3, typeof(SetLighting))]
	[ProtoInclude(4, typeof(PlayAmbient))]
	[ProtoInclude(5, typeof(Draw))]
	[ProtoInclude(6, typeof(Discard))]
	[ProtoInclude(7, typeof(Pick))]
	[ProtoInclude(8, typeof(TopDeck))]
	[ProtoInclude(9, typeof(Choice))]
	[ProtoInclude(10, typeof(Action))]
	[ProtoInclude(11, typeof(LevelUp))]
	[ProtoInclude(12, typeof(ConditionalInstruction))]
	[ProtoInclude(13, typeof(Rest))]
	[ProtoInclude(14, typeof(PlaySound))]
	[ProtoInclude(15, typeof(PlayMusic))]
	[ProtoInclude(16, typeof(Encounter))]
	[ProtoInclude(17, typeof(GiveMana))]
	[ProtoInclude(18, typeof(SetProceduralPhase))]
	[ProtoInclude(19, typeof(CombatMusic))]
	[ProtoInclude(20, typeof(Wait))]
	[ProtoInclude(21, typeof(Group))]
	[ProtoInclude(22, typeof(AddAbility))]
	[ProtoInclude(23, typeof(UpgradeAbility))]
	[ProtoInclude(24, typeof(RemoveAbility))]
	[ProtoInclude(25, typeof(CopyAbility))]
	[ProtoInclude(26, typeof(ReduceAbilityCost))]
	[ProtoInclude(27, typeof(ProceduralGraph))]
	[ProtoInclude(1000, typeof(ExitGame))]
	public abstract class SelectInstruction
	{
		[ProtoContract]
		[UIField]
		public class Draw : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField(min = 1, max = 10)]
			[DefaultValue(1)]
			private int _drawCount = 1;

			[ProtoMember(2)]
			[UIField(tooltip = "Allows you to override the default pile a card would be drawn into.\n<i>Default in most cases is hand, but for things like enemies it is turn order.</i>")]
			private Pile? _drawToPile;

			private Draw()
			{
			}

			public Draw(int drawCount)
			{
				_drawCount = drawCount;
			}

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				for (int x = 0; x < _drawCount; x++)
				{
					yield return state.adventureDeck.DrawStepAdventure(_drawToPile);
				}
			}

			public override string ToString()
			{
				return $"Draw {_drawCount}" + _drawToPile.HasValue.ToText(" into " + EnumUtil.FriendlyName(_drawToPile));
			}
		}

		[ProtoContract]
		[UIField]
		public class Discard : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField(min = 1, max = 100)]
			[DefaultValue(1)]
			private int _discardCount = 1;

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				IdDeck<Pile, ATarget> adventureDeck = state.adventureDeck;
				int discardCount = _discardCount;
				Pile? drawTo = Pile.Discard;
				yield return adventureDeck.DrawStep(discardCount, null, drawTo);
			}

			public override string ToString()
			{
				return $"Discard {_discardCount}";
			}
		}

		[ProtoContract]
		[UIField]
		public class Pick : SelectInstruction
		{
			private static readonly RangeByte DEFAULT_RANGE = new RangeByte(1, 2, 1, 10, 0, 0);

			[ProtoMember(1)]
			[UIField]
			private RangeByte _pickRange = DEFAULT_RANGE;

			[ProtoMember(2)]
			[UIField(tooltip = "Allows player to pick less cards than the minimum pick range.")]
			private bool _flexiblePickCount;

			private bool _pickRangeSpecified => _pickRange != DEFAULT_RANGE;

			private Pick()
			{
			}

			public Pick(byte min, byte max, bool flexiblePickCount)
			{
				_pickRange.min = min;
				_pickRange.max = max;
				_flexiblePickCount = flexiblePickCount;
			}

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				for (int x = 0; x < _pickRange.max; x++)
				{
					yield return state.adventureDeck.DrawStepAdventure(state.adventureDeck.handPile);
				}
				yield return new GameStepAdventurePick(_pickRange.min, _flexiblePickCount, _pickRange.max);
			}

			public override string ToString()
			{
				return $"Draw {_pickRange.max}, pick {_pickRange.min}" + _flexiblePickCount.ToText(" (Flexible)");
			}
		}

		[ProtoContract]
		[UIField]
		public class TopDeck : SelectInstruction
		{
			[ProtoContract]
			[UIField]
			public class ConditionInstructionPair
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Open)]
				[UIDeepValueChange]
				public TopDeckCondition condition;

				[ProtoMember(2, OverwriteList = true)]
				[UIField(collapse = UICollapseType.Open)]
				[UIFieldCollectionItem]
				[UIDeepValueChange]
				public List<SelectInstruction> instructions;

				public override string ToString()
				{
					return string.Format("{0} {1}", condition, instructions.ToStringSmart(" & ").SizeIfNotEmpty());
				}
			}

			public class ConditionInstructionStep : GameStep
			{
				public ConditionInstructionPair pair;

				public override void Start()
				{
					if (!pair.condition.IsValid(new ActionContext(base.state.player, null)))
					{
						return;
					}
					foreach (SelectInstruction instruction in pair.instructions)
					{
						foreach (GameStep gameStep in instruction.GetGameSteps(base.state))
						{
							AppendStep(gameStep);
						}
					}
				}
			}

			[ProtoMember(1)]
			[UIField]
			[UIDeepValueChange]
			private TopDeckInstruction _topDeck;

			[ProtoMember(2)]
			[UIField]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<ConditionInstructionPair> _selectionInstructions;

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return _topDeck.GetGameStep(new ActionContext(state.player, null));
				yield return new GameStepWait(0.75f);
				foreach (ConditionInstructionPair selectionInstruction in _selectionInstructions)
				{
					if (!selectionInstruction.instructions.IsNullOrEmpty())
					{
						yield return new ConditionInstructionStep
						{
							pair = selectionInstruction
						};
					}
				}
				yield return new GameStepGeneric
				{
					onStart = delegate
					{
						TargetLineView.RemoveAll(TargetLineTags.TopDeck);
						state.player.resourceDeck.TransferPile(ResourceCard.Pile.TopDeckHand, ResourceCard.Pile.DiscardPile);
					}
				};
			}

			public override string ToString()
			{
				return $"{_topDeck}: {_selectionInstructions.ToStringSmart()}";
			}
		}

		[ProtoContract]
		public class Choice : SelectInstruction
		{
			[ProtoContract]
			[UIField]
			public class ChoiceInstructionPair
			{
				[ProtoMember(1)]
				[UIField]
				public string choice;

				[ProtoMember(2, OverwriteList = true)]
				[UIField(collapse = UICollapseType.Open)]
				[UIFieldCollectionItem]
				[UIDeepValueChange]
				public List<SelectInstruction> instructions;

				public override string ToString()
				{
					return choice + " " + instructions.ToStringSmart(" & ").SizeIfNotEmpty();
				}
			}

			public class ChoiceInstructionStep : GameStep
			{
				public GameStepStringChoice choice;

				public ChoiceInstructionPair pair;

				public override void Start()
				{
					if (!choice.choice.Equals(pair.choice))
					{
						return;
					}
					foreach (SelectInstruction instruction in pair.instructions)
					{
						foreach (GameStep gameStep in instruction.GetGameSteps(base.state))
						{
							AppendStep(gameStep);
						}
					}
				}
			}

			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open)]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<ChoiceInstructionPair> _choices;

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				GameStepStringChoice choice = new GameStepStringChoice(_choices.Select((ChoiceInstructionPair c) => c.choice).ToArray());
				yield return choice;
				foreach (ChoiceInstructionPair choice2 in _choices)
				{
					if (!choice2.instructions.IsNullOrEmpty())
					{
						yield return new ChoiceInstructionStep
						{
							choice = choice,
							pair = choice2
						};
					}
				}
			}

			public override string ToString()
			{
				return "Choose: " + _choices.ToStringSmart(" ");
			}
		}

		[ProtoContract]
		[UIField]
		public class Action : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Hide)]
			private AAction _action;

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				return new GameStepGrouper(_action.GetActGameSteps(new ActionContext(state.player, null, state.player)));
			}

			public override string ToString()
			{
				return $"Action: {_action}";
			}
		}

		[ProtoContract]
		[UIField]
		public class Rest : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField(tooltip = "Only <b>chosen</b> rests include level up rest enhancements. Any rest which is presented in a <b>Pick</b> instruction is automatically counted as chosen.")]
			private bool? _isChosenOverride;

			private bool isChosen => _isChosenOverride ?? IsBeingChosen;

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepRest(isChosen);
				if (isChosen)
				{
					int num = Math.Min(state.parameters.restAbilityDrawCount, state.player.abilityHandSpace);
					if (num > 0)
					{
						yield return state.abilityDeck.DrawStep(num);
					}
					if (state.parameters.restMulliganEnabled)
					{
						yield return new GameStepDiscardResourceChoice(state.parameters.mulliganCount, DiscardReason.Mulligan);
						yield return new GameStepWait(0.333f, null, canSkip: false);
					}
					if (state.parameters.restMulliganAbilityEnabled)
					{
						yield return new GameStepDiscardAbilityChoice(state.parameters.mulliganCount, DiscardReason.Mulligan);
						yield return new GameStepWait(0.333f, null, canSkip: false);
					}
				}
			}

			public override string ToString()
			{
				bool? isChosenOverride = _isChosenOverride;
				object obj;
				if (isChosenOverride.HasValue)
				{
					bool valueOrDefault = isChosenOverride.GetValueOrDefault();
					obj = valueOrDefault.ToText("Chosen ", "!Chosen ");
				}
				else
				{
					obj = "";
				}
				return (string)obj + "Rest";
			}
		}

		[ProtoContract]
		[UIField]
		public class LevelUp : SelectInstruction
		{
			public override CanBeSelectedResult CanBeSelected(GameState state)
			{
				return (state.player.heroDeck.Count(HeroDeckPile.Draw) <= 0) ? AbilityPreventedBy.MaxLevel : AbilityPreventedBy.Nothing;
			}

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepLevelUp();
			}

			public override string ToString()
			{
				return "Level Up";
			}
		}

		[ProtoContract]
		[UIField("Conditional", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		public class ConditionalInstruction : SelectInstruction
		{
			private class ConditionalStep : GameStep
			{
				private ConditionalInstruction _instruction;

				public ConditionalStep(ConditionalInstruction instruction)
				{
					_instruction = instruction;
				}

				public override void Start()
				{
					if (!_instruction._IsValid(base.state))
					{
						CancelGroup();
					}
				}
			}

			[ProtoMember(1, OverwriteList = true)]
			[UIField]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<AAction.Condition.Actor> _conditions;

			[ProtoMember(2, OverwriteList = true)]
			[UIField]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<SelectInstruction> _instructions;

			[ProtoMember(3)]
			[UIField]
			private bool _preventsSelectionWhenFalse;

			private IEnumerable<GameStep> _GetGameSteps(GameState state)
			{
				yield return new ConditionalStep(this);
				if (_instructions.IsNullOrEmpty())
				{
					yield break;
				}
				foreach (SelectInstruction instruction in _instructions)
				{
					foreach (GameStep gameStep in instruction.GetGameSteps(state))
					{
						yield return gameStep;
					}
				}
			}

			private bool _IsValid(GameState state)
			{
				return _conditions.All(new ActionContext(state.player, null, state.player));
			}

			public override CanBeSelectedResult CanBeSelected(GameState state)
			{
				return (!_preventsSelectionWhenFalse) ? new AbilityPreventedBy?(AbilityPreventedBy.Nothing) : _conditions.GetAbilityPreventedBy(new ActionContext(state.player, null, state.player));
			}

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				return new GameStepGrouper(_GetGameSteps(state));
			}

			public override string ToString()
			{
				return _conditions.ToStringSmart(" & ") + "<size=66%> then do </size>" + _instructions.ToStringSmart(" & ").SpaceIfNotEmpty() + _preventsSelectionWhenFalse.ToText("(Prevents Selection)".SizeIfNotEmpty());
			}
		}

		[ProtoContract]
		[UIField]
		public class PlaySound : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField(filter = AudioCategoryType.Adventure)]
			private AudioRef _sound;

			private bool _soundSpecified => _sound.ShouldSerialize();

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				if (_soundSpecified)
				{
					yield return new GameStepSound(_sound);
				}
			}

			public override string ToString()
			{
				return "Play Sound " + _sound.GetFriendlyName();
			}
		}

		[ProtoContract]
		[UIField]
		public class PlayMusic : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField(validateOnChange = true)]
			private MusicPlayType _command;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Open)]
			[UIHideIf("_hideMusic")]
			private DataRef<MusicData> _music;

			[ProtoMember(3)]
			[UIField(min = 0.1f, max = 1f)]
			[UIHideIf("_hideMusic")]
			[DefaultValue(0.5f)]
			private float _volume = 0.5f;

			private bool _hideMusic => _command == MusicPlayType.Stop;

			private bool _musicSpecified
			{
				get
				{
					if (!_hideMusic)
					{
						return _music.ShouldSerialize();
					}
					return false;
				}
			}

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepGenericSimple(delegate
				{
					state.lastMusicInstruction = this;
				});
				yield return new GameStepMusic(_command, _music, _volume);
			}

			public override string ToString()
			{
				if (!_hideMusic)
				{
					return EnumUtil.FriendlyName(_command) + " Music: " + _music.GetFriendlyName();
				}
				return "Stop Music";
			}
		}

		[ProtoContract]
		[UIField]
		public class PlayAmbient : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField(validateOnChange = true)]
			[DefaultValue(MusicPlayType.Resume)]
			private MusicPlayType _command = MusicPlayType.Resume;

			[ProtoMember(2)]
			[UIField(filter = AudioCategoryType.Ambient, collapse = UICollapseType.Open)]
			[UIHideIf("_hideAmbient")]
			private AudioRef _ambient;

			[ProtoMember(3)]
			[UIField(min = 0.1f, max = 1f)]
			[UIHideIf("_hideAmbient")]
			[DefaultValue(0.5f)]
			private float _volume = 0.5f;

			private bool _hideAmbient => _command == MusicPlayType.Stop;

			private bool _ambientSpecified
			{
				get
				{
					if (!_hideAmbient)
					{
						return _ambient.ShouldSerialize();
					}
					return false;
				}
			}

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepGenericSimple(delegate
				{
					state.lastAmbientInstruction = this;
				});
				yield return new GameStepAmbient(_command, _ambient, _volume);
			}

			public override string ToString()
			{
				if (!_hideAmbient)
				{
					return EnumUtil.FriendlyName(_command) + " Ambiance: " + _ambient.GetFriendlyName();
				}
				return "Stop Ambiance";
			}
		}

		[ProtoContract]
		[UIField]
		public class SetLighting : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open)]
			private DataRef<LightingData> _lighting;

			private bool _lightingSpecified => _lighting.ShouldSerialize();

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepGenericSimple(delegate
				{
					state.lastLightingInstruction = this;
				});
				if (_lightingSpecified)
				{
					yield return GameStepLighting.Create(_lighting.data);
				}
			}

			public override string ToString()
			{
				return "Set Lighting: " + _lighting.GetFriendlyName();
			}
		}

		[ProtoContract]
		[UIField("New Game", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		public class NewGameInstruction : SelectInstruction
		{
			[ProtoContract]
			[UIField]
			public class Override
			{
				[ProtoMember(1)]
				[UIField(validateOnChange = true)]
				[UIHorizontalLayout("A", preferredWidth = 1f, flexibleWidth = 0f, minWidth = 150f)]
				private bool _override;

				[ProtoMember(2)]
				[UIField]
				[UIHideIf("_hideCard")]
				[UIHorizontalLayout("A", preferredWidth = 1f, flexibleWidth = 999f)]
				[UIDeepValueChange]
				private SelectInstruction _instruction;

				private bool _hideCard => !_override;

				public static implicit operator bool(Override o)
				{
					return o?._override ?? false;
				}

				public static implicit operator SelectInstruction(Override o)
				{
					if (!o)
					{
						return null;
					}
					return o._instruction;
				}
			}

			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Hide)]
			[UIHeader("Winter")]
			private Override _winter;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Hide)]
			[UIHeader("Fall")]
			private Override _fall;

			[ProtoMember(3)]
			[UIField(collapse = UICollapseType.Hide)]
			[UIHeader("Summer")]
			private Override _summer;

			[ProtoMember(4)]
			[UIField(collapse = UICollapseType.Hide)]
			[UIHeader("Spring")]
			private Override _spring;

			private SelectInstruction this[NewGameType type] => _GetOverride(type);

			private bool _winterSpecified => _winter;

			private bool _fallSpecified => _fall;

			private bool _summerSpecified => _summer;

			private bool _springSpecified => _spring;

			private Override _GetOverride(NewGameType type)
			{
				return type switch
				{
					NewGameType.Summer => _summer, 
					NewGameType.Fall => _fall, 
					NewGameType.Winter => _winter, 
					_ => _spring, 
				};
			}

			private SelectInstruction _GetInstruction(NewGameType type)
			{
				foreach (NewGameType item in EnumUtil.EnumerateDescending(type))
				{
					SelectInstruction selectInstruction = this[item];
					if (selectInstruction != null)
					{
						return selectInstruction;
					}
				}
				return null;
			}

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				return _GetInstruction(state.game.data.newGameType)?.GetGameSteps(state) ?? Enumerable.Empty<GameStep>();
			}

			public override string ToString()
			{
				return "<b>(NEW GAME)</b>: " + (_GetInstruction(EnumUtil<NewGameType>.Max)?.ToString() ?? "NULL");
			}
		}

		[ProtoContract]
		[UIField("Blank", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		public class BlankInstruction : SelectInstruction
		{
			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield break;
			}

			public override string ToString()
			{
				return "[BLANK]";
			}
		}

		[ProtoContract]
		[UIField]
		public class ExitGame : SelectInstruction
		{
			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepGeneric
				{
					onStart = GameUtil.ExitApplication
				};
			}

			public override string ToString()
			{
				return "Exit Game";
			}
		}

		[ProtoContract]
		public class Encounter : SelectInstruction
		{
			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepEncounter();
			}
		}

		[ProtoContract]
		[UIField]
		public class GiveMana : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField(min = 0, max = 100)]
			[DefaultValue(25)]
			private int _mana = 25;

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepGenericSimple(delegate
				{
					state.experience += _mana;
				});
			}

			public override string ToString()
			{
				return $"Give {_mana} <b>Mana</b>";
			}
		}

		[ProtoContract]
		[UIField]
		public class ProceduralGraph : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField]
			private DataRef<ProceduralGraphData> _graph;

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepProceduralMap(_graph);
			}

			public override string ToString()
			{
				return "Procedural Graph: <b>" + _graph.GetFriendlyName() + "</b>";
			}
		}

		[ProtoContract]
		[UIField]
		public class SetProceduralPhase : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField]
			private ProceduralPhaseType _phase;

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepGenericSimple(delegate
				{
					state.proceduralPhase = _phase;
				});
			}

			public override string ToString()
			{
				return "Set <b>Phase</b> To " + EnumUtil.FriendlyName(_phase);
			}
		}

		[ProtoContract]
		public class CombatMusic : SelectInstruction
		{
			[ProtoMember(1)]
			private ProceduralEncounterDifficulty _encounterDifficulty;

			[ProtoMember(2)]
			private ProceduralPhaseType? _phase;

			private CombatMusic()
			{
			}

			public CombatMusic(ProceduralEncounterDifficulty encounterDifficulty, ProceduralPhaseType? phase = null)
			{
				_encounterDifficulty = encounterDifficulty;
				_phase = phase;
			}

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				foreach (GameStep gameStep in ContentRef.Defaults.audio.GetCombatMusicInstruction((int)_encounterDifficulty + (int)(state.proceduralPhase ?? _phase.GetValueOrDefault())).GetGameSteps(state))
				{
					yield return gameStep;
				}
			}
		}

		[ProtoContract]
		[UIField]
		public class Wait : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField(min = 0.1f, max = 2f, tooltip = "Wait time in seconds.")]
			[DefaultValue(0.75f)]
			private float _wait = 0.75f;

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepWait(_wait, null, canSkip: false);
			}

			public override string ToString()
			{
				return $"Wait {_wait}(s)";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Use to allow grouping actions together for inherited targeting.")]
		public class Group : SelectInstruction
		{
			[ProtoMember(1, OverwriteList = true)]
			[UIField]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<SelectInstruction> _instructions;

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				return new GameStepGrouper(_instructions.GetSteps(state));
			}

			public override string ToString()
			{
				return "Group(" + _instructions.ToStringSmart() + ")";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Present the choice to add one ability card from a random selection to your deck.")]
		public class AddAbility : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField]
			[DefaultValue(AddAbilityTypes.Standard | AddAbilityTypes.Buff | AddAbilityTypes.Debuff | AddAbilityTypes.Summon)]
			private AddAbilityTypes _abilityTypes = AddAbilityTypes.Standard | AddAbilityTypes.Buff | AddAbilityTypes.Debuff | AddAbilityTypes.Summon;

			[ProtoMember(2)]
			[UIField(min = 1, max = 5)]
			[DefaultValue(3)]
			private int _count = 3;

			[ProtoMember(3)]
			[UIField(min = 0, max = 100)]
			private byte _survivalAbilityChance;

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepAddAbility(_abilityTypes, _count, (float)(int)_survivalAbilityChance * 0.01f);
			}

			public override string ToString()
			{
				return string.Format("Add Ability <size=66%>({0}: {1}{2})</size>", _count, EnumUtil.FriendlyName(_abilityTypes), (_survivalAbilityChance > 0) ? $", Survival Ability {_survivalAbilityChance}%" : "");
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Present the choice to upgrade one ability card from your deck.")]
		public class UpgradeAbility : SelectInstruction
		{
			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepUpgradeAbility();
			}

			public override string ToString()
			{
				return "Upgrade Ability";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Present the choice to remove an ability card from your deck.")]
		public class RemoveAbility : SelectInstruction
		{
			[ProtoMember(1)]
			[UIField(min = 1, max = 10)]
			[DefaultValue(1)]
			private int _count = 1;

			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepRemoveAbility(_count);
			}

			public override string ToString()
			{
				return $"Remove {_count} Ability";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Present the choice to copy one ability card from your deck.")]
		public class CopyAbility : SelectInstruction
		{
			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepCopyAbility();
			}

			public override string ToString()
			{
				return "Copy Ability";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Present the choice to reduce card cost of one ability from your deck.")]
		public class ReduceAbilityCost : SelectInstruction
		{
			public override IEnumerable<GameStep> GetGameSteps(GameState state)
			{
				yield return new GameStepReduceAbilityCost();
			}

			public override string ToString()
			{
				return "Reduce Ability Cost";
			}
		}

		public virtual CanBeSelectedResult CanBeSelected(GameState state)
		{
			return AbilityPreventedBy.Nothing;
		}

		public abstract IEnumerable<GameStep> GetGameSteps(GameState state);
	}

	private const string CAT_MAIN = "Main";

	private const string CAT_CONDITIONAL = "Conditional";

	private const string CAT_PROCEDURAL = "Procedural";

	public static readonly Ushort2 IMAGE_SIZE = new Ushort2(425, 305);

	private static readonly List<SelectInstruction> DEFAULT_SELECT_INSTRUCTIONS = new List<SelectInstruction>();

	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIHideIf("_hideCommon")]
	protected Common _common;

	public static bool IsBeingChosen { get; set; }

	public Common common => _common ?? (_common = new Common());

	protected virtual bool _hideShowCommon => false;

	protected bool _commonSpecified => !_hideShowCommon;

	protected string _ToStringPrefix()
	{
		return "";
	}

	protected virtual ATarget _GenerateCard(GameState gameState)
	{
		return null;
	}

	public virtual IEnumerable<ATarget> GenerateCards(GameState state)
	{
		if (_GenerateCard(state) is IAdventureCard adventureCard)
		{
			yield return adventureCard.SetCommon(_ProcessGeneratedCommon(ProtoUtil.Clone(_GetCommon(state)))).adventureCard;
		}
	}

	protected virtual Common _GetCommon(GameState gameState)
	{
		return common;
	}

	protected virtual Common _ProcessGeneratedCommon(Common generatedCommon)
	{
		return generatedCommon?.ClearGeneratedData();
	}

	public override string ToString()
	{
		return _common?.ToString().PreSpaceIfNotEmpty() ?? "";
	}

	protected virtual void _OnConvertedToNewGameCard(NewGame newGameCard)
	{
	}

	public virtual void OnGraphGenerated(GameState state)
	{
	}

	public void ReplaceWithNewGameVersion(AdventureData activeEdit, int indexOf)
	{
		NewGame newGame = new NewGame(ProtoUtil.Clone(this));
		_OnConvertedToNewGameCard(newGame);
		activeEdit.ReplaceAtIndex(newGame, indexOf);
	}
}
