using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
[ProtoInclude(5, typeof(UnlockClassReward))]
[ProtoInclude(6, typeof(UnlockGameReward))]
[ProtoInclude(7, typeof(NewGameUnlock))]
[ProtoInclude(8, typeof(Nothing))]
[ProtoInclude(9, typeof(UnlockAdventure))]
[ProtoInclude(10, typeof(DemoOnly))]
public abstract class AReward
{
	[ProtoContract]
	[UIField("Unlock Class", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	public class UnlockClassReward : AReward
	{
		[ProtoMember(1)]
		[UIField]
		private DataRef<CharacterData> _characterToUnlock;

		public DataRef<CharacterData> characterToUnlock => _characterToUnlock;

		private UnlockClassReward()
		{
		}

		public UnlockClassReward(DataRef<CharacterData> characterData)
		{
			_characterToUnlock = characterData;
		}

		public override bool ShouldUnlock()
		{
			return ProfileManager.progress.characters.read.IsLocked(_characterToUnlock);
		}

		public override GameStepGroupRewards.AGameStepUnlock GetUnlockStep()
		{
			return new GameStepGroupRewards.GameStepUnlockClass(_characterToUnlock);
		}

		public override string ToString()
		{
			return "Unlock " + _characterToUnlock.GetFriendlyName();
		}
	}

	[ProtoContract]
	[UIField("Unlock Game", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	public class UnlockGameReward : AReward
	{
		[ProtoMember(1)]
		[UIField]
		private DataRef<GameData> _gameToUnlock;

		[ProtoMember(2)]
		[UIField]
		[DefaultValue(true)]
		private bool _selectGameOnUnlock = true;

		public DataRef<GameData> gameToUnlock => _gameToUnlock;

		public override bool ShouldUnlock()
		{
			return !ProfileManager.progress.games.read.IsUnlocked(_gameToUnlock);
		}

		public override GameStepGroupRewards.AGameStepUnlock GetUnlockStep()
		{
			return new GameStepGroupRewards.GameStepUnlockGame(_gameToUnlock, _selectGameOnUnlock);
		}

		public override string ToString()
		{
			return "Unlock <b>" + _gameToUnlock.GetFriendlyName() + "</b>" + _selectGameOnUnlock.ToText(" & Select");
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Allows unlocking an adventure from another game, ignoring normal unlock rules of having to beat previous adventures.")]
	public class UnlockAdventure : AReward
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<GameData> _game;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<AdventureData> _adventureToUnlock;

		public override bool ShouldUnlock()
		{
			if (ProfileManager.progress.games.read.IsUnlocked(_game))
			{
				return !ProfileManager.progress.games.read.IsUnlocked(_game, _adventureToUnlock);
			}
			return true;
		}

		public override GameStepGroupRewards.AGameStepUnlock GetUnlockStep()
		{
			return new GameStepGroupRewards.GameStepUnlockAdventure(_game, _adventureToUnlock);
		}

		public override string ToString()
		{
			return "Unlock <b>" + _adventureToUnlock.GetFriendlyName() + "</b> in " + _game.GetFriendlyName();
		}
	}

	[ProtoContract]
	[UIField("New Game", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows setting reward based on current New Game setting.")]
	public class NewGameUnlock : AReward
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
			[UIField(" ", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
			[UIHideIf("_hideReward")]
			[UIHorizontalLayout("A", preferredWidth = 1f, flexibleWidth = 999f)]
			[UIDeepValueChange]
			private AReward _reward;

			private bool _hideReward => !_override;

			public static implicit operator bool(Override o)
			{
				return o?._override ?? false;
			}

			public static implicit operator AReward(Override o)
			{
				if (!o)
				{
					return null;
				}
				return o._reward;
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

		private AReward this[NewGameType type] => type switch
		{
			NewGameType.Summer => _summer, 
			NewGameType.Fall => _fall, 
			NewGameType.Winter => _winter, 
			_ => _spring, 
		};

		private bool _winterSpecified => _winter;

		private bool _fallSpecified => _fall;

		private bool _summerSpecified => _summer;

		private bool _springSpecified => _spring;

		public override bool ShouldUnlock()
		{
			return _GetReward(GameState.NewGame)?.ShouldUnlock() ?? false;
		}

		public override GameStepGroupRewards.AGameStepUnlock GetUnlockStep()
		{
			return _GetReward(GameState.NewGame)?.GetUnlockStep();
		}

		public override string ToString()
		{
			return "<b>[NEW GAME]</b>: " + (_GetReward(EnumUtil<NewGameType>.Max)?.ToString() ?? "NULL");
		}

		private AReward _GetReward(NewGameType type)
		{
			foreach (NewGameType item in EnumUtil.EnumerateDescending(type))
			{
				AReward aReward = this[item];
				if (aReward != null)
				{
					return aReward;
				}
			}
			return null;
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Used to empty a slot in a New Game unlock.")]
	public class Nothing : AReward
	{
		public override bool ShouldUnlock()
		{
			return false;
		}

		public override GameStepGroupRewards.AGameStepUnlock GetUnlockStep()
		{
			return null;
		}

		public override string ToString()
		{
			return "Nothing";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Allows giving, or not giving, a reward based on whether or not game is in demo version.")]
	public class DemoOnly : AReward
	{
		[ProtoMember(1)]
		[UIField]
		[UIDeepValueChange]
		private AReward _reward;

		[ProtoMember(2)]
		[UIField]
		[DefaultValue(true)]
		private bool _demoOnly = true;

		public override bool ShouldUnlock()
		{
			if (_demoOnly == IOUtil.IsDemo)
			{
				return _reward?.ShouldUnlock() ?? false;
			}
			return false;
		}

		public override GameStepGroupRewards.AGameStepUnlock GetUnlockStep()
		{
			return _reward?.GetUnlockStep();
		}

		public override string ToString()
		{
			return "<b>" + _demoOnly.ToText("DEMO", "!DEMO") + "</b>: " + _reward;
		}
	}

	public abstract bool ShouldUnlock();

	public abstract GameStepGroupRewards.AGameStepUnlock GetUnlockStep();
}
