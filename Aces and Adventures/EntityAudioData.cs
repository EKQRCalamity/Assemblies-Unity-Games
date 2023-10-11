using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField]
public class EntityAudioData : IDataContent
{
	[ProtoContract]
	[UIField]
	public class AttackAudio
	{
		[ProtoMember(1)]
		[UIField]
		private SoundPack _oneCardAttack = new SoundPack(AudioCategoryType.Attack, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(2)]
		[UIField]
		private SoundPack _twoCardAttack = new SoundPack(AudioCategoryType.Attack, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(3)]
		[UIField]
		private SoundPack _threeCardAttack = new SoundPack(AudioCategoryType.Attack, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(4)]
		[UIField]
		private SoundPack _fourCardAttack = new SoundPack(AudioCategoryType.Attack, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(5)]
		[UIField]
		private SoundPack _fiveCardAttack = new SoundPack(AudioCategoryType.Attack, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(6)]
		[UIField]
		private SoundPack _defense = new SoundPack(AudioCategoryType.Attack, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		public SoundPack defense
		{
			get
			{
				if (!_defense)
				{
					return null;
				}
				return _defense;
			}
		}

		private SoundPack this[int cardCount] => cardCount switch
		{
			1 => _oneCardAttack, 
			2 => _twoCardAttack, 
			3 => _threeCardAttack, 
			4 => _fourCardAttack, 
			5 => _fiveCardAttack, 
			_ => _oneCardAttack, 
		};

		public SoundPack Attack(int cardCount)
		{
			for (int num = Math.Min(5, cardCount); num >= 1; num--)
			{
				if ((bool)this[num])
				{
					return this[num];
				}
			}
			return null;
		}
	}

	[ProtoContract]
	[UIField]
	public class HurtAudio
	{
		[ProtoMember(1)]
		[UIField]
		private SoundPack _hurt = new SoundPack(AudioCategoryType.Hurt, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(2)]
		[UIField]
		private SoundPack _criticallyHurt = new SoundPack(AudioCategoryType.CriticallyHurt, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(3)]
		[UIField]
		private SoundPack _death = new SoundPack(AudioCategoryType.Death, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		public SoundPack hurt
		{
			get
			{
				if (!_hurt)
				{
					return null;
				}
				return _hurt;
			}
		}

		public SoundPack criticallyHurt
		{
			get
			{
				if (!_criticallyHurt)
				{
					return hurt;
				}
				return _criticallyHurt;
			}
		}

		public SoundPack death
		{
			get
			{
				if (!_death)
				{
					return criticallyHurt;
				}
				return _death;
			}
		}
	}

	[ProtoContract]
	[UIField]
	public class AdditionalAudio
	{
		[ProtoMember(1)]
		[UIField]
		private SoundPack _turnStart = new SoundPack(AudioCategoryType.TurnStart, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(2)]
		[UIField]
		private SoundPack _friendlyWords = new SoundPack(AudioCategoryType.FriendlyWords, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(3)]
		[UIField]
		private SoundPack _hostileWords = new SoundPack(AudioCategoryType.HostileWords, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(4)]
		[UIField]
		private SoundPack _encounterStart = new SoundPack(AudioCategoryType.TurnStart, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		public SoundPack turnStart
		{
			get
			{
				if (!_turnStart)
				{
					return null;
				}
				return _turnStart;
			}
		}

		public SoundPack friendlyWords
		{
			get
			{
				if (!_friendlyWords)
				{
					return null;
				}
				return _friendlyWords;
			}
		}

		public SoundPack hostileWords
		{
			get
			{
				if (!_hostileWords)
				{
					return null;
				}
				return _hostileWords;
			}
		}

		public SoundPack encounterStart
		{
			get
			{
				if (!_encounterStart)
				{
					return null;
				}
				return _encounterStart;
			}
		}
	}

	[ProtoContract]
	[UIField]
	public class CharacterAudio
	{
		[ProtoContract]
		[UIField]
		public class Victory
		{
			[ProtoMember(1)]
			[UIField]
			private SoundPack _normal = new SoundPack(AudioCategoryType.Victory, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			[ProtoMember(2)]
			[UIField]
			private SoundPack _good = new SoundPack(AudioCategoryType.Victory, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			[ProtoMember(3)]
			[UIField]
			private SoundPack _best = new SoundPack(AudioCategoryType.Victory, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			public SoundPack normal => _normal;

			public SoundPack good
			{
				get
				{
					if (!_good)
					{
						return normal;
					}
					return _good;
				}
			}

			public SoundPack best
			{
				get
				{
					if (!_best)
					{
						return good;
					}
					return _best;
				}
			}

			public SoundPack this[AdventureCompletionRank rank] => rank switch
			{
				AdventureCompletionRank.SPlus => best, 
				AdventureCompletionRank.S => best, 
				AdventureCompletionRank.A => good, 
				_ => normal, 
			};
		}

		[ProtoContract]
		[UIField]
		public class Error
		{
			[ProtoMember(1)]
			[UIField]
			private SoundPack _generic = new SoundPack(AudioCategoryType.Error, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			[ProtoMember(2)]
			[UIField]
			private SoundPack _outOfAttacks = new SoundPack(AudioCategoryType.Error, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			[ProtoMember(3)]
			[UIField]
			private SoundPack _notEnoughResources = new SoundPack(AudioCategoryType.Error, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			[ProtoMember(4)]
			[UIField]
			private SoundPack _notEnoughCards = new SoundPack(AudioCategoryType.Error, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			[ProtoMember(5)]
			[UIField]
			private SoundPack _cannotUseNow = new SoundPack(AudioCategoryType.Error, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			[ProtoMember(6)]
			[UIField]
			private SoundPack _noTarget = new SoundPack(AudioCategoryType.Error, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			[ProtoMember(7)]
			[UIField]
			private SoundPack _reaction = new SoundPack(AudioCategoryType.Error, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			[ProtoMember(8)]
			[UIField]
			private SoundPack _invalidHand = new SoundPack(AudioCategoryType.Error, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			[ProtoMember(9)]
			[UIField]
			private SoundPack _invalidAttack = new SoundPack(AudioCategoryType.Error, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			[ProtoMember(10)]
			[UIField]
			private SoundPack _matchAttackCardCount = new SoundPack(AudioCategoryType.Error, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			[ProtoMember(11)]
			[UIField]
			private SoundPack _outOfUses = new SoundPack(AudioCategoryType.Error, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

			public SoundPack generic => _generic;

			public SoundPack outOfAttacks
			{
				get
				{
					if (!_outOfAttacks)
					{
						return generic;
					}
					return _outOfAttacks;
				}
			}

			public SoundPack notEnoughResources
			{
				get
				{
					if (!_notEnoughResources)
					{
						return generic;
					}
					return _notEnoughResources;
				}
			}

			public SoundPack notEnoughCards
			{
				get
				{
					if (!_notEnoughCards)
					{
						return notEnoughResources;
					}
					return _notEnoughCards;
				}
			}

			public SoundPack cannotUseNow
			{
				get
				{
					if (!_cannotUseNow)
					{
						return generic;
					}
					return _cannotUseNow;
				}
			}

			public SoundPack noTarget
			{
				get
				{
					if (!_noTarget)
					{
						return generic;
					}
					return _noTarget;
				}
			}

			public SoundPack reaction
			{
				get
				{
					if (!_reaction)
					{
						return cannotUseNow;
					}
					return _reaction;
				}
			}

			public SoundPack invalidHand
			{
				get
				{
					if (!_invalidHand)
					{
						return generic;
					}
					return _invalidHand;
				}
			}

			public SoundPack invalidAttack
			{
				get
				{
					if (!_invalidAttack)
					{
						return generic;
					}
					return _invalidAttack;
				}
			}

			public SoundPack matchAttackCardCount
			{
				get
				{
					if (!_matchAttackCardCount)
					{
						return generic;
					}
					return _matchAttackCardCount;
				}
			}

			public SoundPack outOfUses
			{
				get
				{
					if (!_outOfUses)
					{
						return generic;
					}
					return _outOfUses;
				}
			}

			public SoundPack this[CanAttackResult.PreventedBy preventedBy, bool isAttack = true]
			{
				get
				{
					if (isAttack)
					{
						return (!preventedBy.IsInvalidHand()) ? (preventedBy switch
						{
							CanAttackResult.PreventedBy.OutOfAttacks => outOfAttacks, 
							CanAttackResult.PreventedBy.Guard => invalidAttack, 
							CanAttackResult.PreventedBy.Stealth => invalidAttack, 
							_ => generic, 
						}) : invalidAttack;
					}
					return (preventedBy != CanAttackResult.PreventedBy.InvalidDefenseHandCount) ? generic : matchAttackCardCount;
				}
			}

			public SoundPack this[AbilityPreventedBy preventedBy]
			{
				get
				{
					switch (preventedBy)
					{
					case AbilityPreventedBy.OutOfUsesThisRound:
						return outOfUses;
					case AbilityPreventedBy.LackingResources:
						return notEnoughResources;
					case AbilityPreventedBy.AdditionalCardsInActionHand:
						return invalidHand;
					case AbilityPreventedBy.NotEnoughAbilities:
						return notEnoughCards;
					case AbilityPreventedBy.Guard:
					case AbilityPreventedBy.Stealth:
						return generic;
					default:
						if (preventedBy.IsCardResource())
						{
							return notEnoughCards;
						}
						if (preventedBy.IsResource())
						{
							return notEnoughResources;
						}
						if (preventedBy.IsAvailability())
						{
							return cannotUseNow;
						}
						if (preventedBy.IsReaction())
						{
							return reaction;
						}
						if (preventedBy.IsTargeting())
						{
							return noTarget;
						}
						return generic;
					}
				}
			}

			public SoundPack this[EndTurnPreventedBy preventedBy] => generic;
		}

		[ProtoMember(1)]
		[UIField]
		private SoundPack _select = new SoundPack(AudioCategoryType.Select, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(2)]
		[UIField]
		private SoundPack _findItem = new SoundPack(AudioCategoryType.FindItem, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(3)]
		[UIField]
		private SoundPack _buyItem = new SoundPack(AudioCategoryType.BuyItem, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(4)]
		[UIField]
		private SoundPack _restore = new SoundPack(AudioCategoryType.Restore, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(5)]
		[UIField]
		private SoundPack _level = new SoundPack(AudioCategoryType.Level, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(6)]
		[UIField]
		private Victory _victory;

		[ProtoMember(7)]
		[UIField]
		private Error _error;

		public SoundPack select => _select;

		public SoundPack findItem => _findItem;

		public SoundPack buyItem => _buyItem;

		public SoundPack restore => _restore;

		public SoundPack level => _level;

		public Victory victory => _victory ?? (_victory = new Victory());

		public Error error => _error ?? (_error = new Error());
	}

	public static readonly EntityAudioData Default = new EntityAudioData();

	[ProtoMember(1)]
	[UIField]
	[UICategory("Main")]
	private string _name;

	[ProtoMember(2)]
	[UIField]
	[UICategory("Main")]
	private SoundPack _grunt = new SoundPack(AudioCategoryType.Grunt, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

	[ProtoMember(3)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Attack")]
	private AttackAudio _attack;

	[ProtoMember(4)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Hurt")]
	private HurtAudio _hurt;

	[ProtoMember(5)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Additional")]
	private AdditionalAudio _additional;

	[ProtoMember(6)]
	[UIField(validateOnChange = true)]
	[UICategory("Character")]
	private bool _characterAudioEnabled;

	[ProtoMember(7)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIHideIf("_hideCharacter")]
	[UICategory("Character")]
	private CharacterAudio _character;

	[ProtoMember(15)]
	public string tags { get; set; }

	private AttackAudio attackAudio => _attack ?? (_attack = new AttackAudio());

	private HurtAudio hurtAudio => _hurt ?? (_hurt = new HurtAudio());

	private AdditionalAudio additionalAudio => _additional ?? (_additional = new AdditionalAudio());

	public CharacterAudio character => _character ?? (_character = new CharacterAudio());

	public SoundPack grunt => _grunt;

	public SoundPack defense => attackAudio.defense ?? grunt;

	public SoundPack hurt => hurtAudio.hurt ?? grunt;

	public SoundPack criticallyHurt => hurtAudio.criticallyHurt ?? grunt;

	public SoundPack death => hurtAudio.death ?? grunt;

	public SoundPack turnStart => additionalAudio.turnStart ?? grunt;

	public SoundPack encounterStart => additionalAudio.encounterStart ?? turnStart;

	public SoundPack friendlyWords => additionalAudio.friendlyWords ?? grunt;

	public SoundPack hostileWords => additionalAudio.hostileWords ?? Attack(1) ?? grunt;

	private bool _hideCharacter => !_characterAudioEnabled;

	private bool _characterSpecified => _characterAudioEnabled;

	public SoundPack Hurt(int damage)
	{
		if (damage >= 4)
		{
			return criticallyHurt;
		}
		if (damage <= 0)
		{
			return null;
		}
		return hurt;
	}

	public SoundPack Attack(int cardCount)
	{
		return attackAudio.Attack(cardCount) ?? grunt;
	}

	public string GetTitle()
	{
		return _name ?? "Unnamed Entity Audio Data";
	}

	public string GetAutomatedDescription()
	{
		return null;
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public void PrepareDataForSave()
	{
		foreach (SoundPack item in ReflectionUtil.FindAllInstances<SoundPack>(this))
		{
			item.PrepareDataForSave();
		}
	}

	public string GetSaveErrorMessage()
	{
		return null;
	}

	public void OnLoadValidation()
	{
	}
}
