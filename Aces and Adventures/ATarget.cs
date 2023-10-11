using System;
using ProtoBuf;

[ProtoContract]
[ProtoInclude(5, typeof(BonusCard))]
[ProtoInclude(6, typeof(ADeck))]
[ProtoInclude(7, typeof(Stone))]
[ProtoInclude(8, typeof(TutorialCard))]
[ProtoInclude(9, typeof(Chip))]
[ProtoInclude(10, typeof(AEntity))]
[ProtoInclude(11, typeof(ResourceCard))]
[ProtoInclude(12, typeof(ButtonCard))]
[ProtoInclude(13, typeof(AdventureTarget))]
[ProtoInclude(14, typeof(TurnOrderSpace))]
[ProtoInclude(15, typeof(AdventureResultCard))]
[ProtoInclude(16, typeof(ExperienceVial))]
[ProtoInclude(17, typeof(CardPack))]
[ProtoInclude(18, typeof(ClassSeal))]
[ProtoInclude(19, typeof(GameStone))]
[ProtoInclude(20, typeof(LevelUpPlant))]
[ProtoInclude(21, typeof(LevelUpLeaf))]
[ProtoInclude(22, typeof(LevelUpReward))]
[ProtoInclude(23, typeof(ProceduralMap))]
[ProtoInclude(24, typeof(MapCompass))]
[ProtoInclude(25, typeof(Ability.RefreshTargetsRegister))]
[ProtoInclude(26, typeof(Leaderboard))]
public abstract class ATarget : Idable<ATarget>, Idable, IRegister, IComparable<ATarget>
{
	protected const int REGISTER_ORDER_BONUS = -1000;

	protected const int REGISTER_ORDER_TUTORIAL = -100;

	protected const int REGISTER_ORDER_PLAYER = 0;

	protected const int REGISTER_ORDER_ABILITY = 10;

	protected const int REGISTER_ORDER_ENEMY = 100;

	[ProtoMember(1)]
	private ushort _id;

	public GameState gameState => GameState.Instance;

	public ATargetView view => ATargetView.GetView(this);

	[ProtoMember(2)]
	public int registerId { get; set; }

	public virtual bool shouldRegisterDuringGameStateInitialization => false;

	public virtual int registerDuringGameStateInitializationOrder => 0;

	public virtual bool canBePooled => false;

	public ushort id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	public ushort tableId { get; set; }

	public virtual void _Register()
	{
	}

	public virtual void _Unregister()
	{
	}

	public virtual int CompareTo(ATarget other)
	{
		return id.CompareTo(other.id);
	}
}
