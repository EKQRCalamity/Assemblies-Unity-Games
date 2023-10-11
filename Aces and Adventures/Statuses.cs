using System;
using ProtoBuf;

[ProtoContract]
public class Statuses
{
	[ProtoMember(1)]
	private int _cannotAttackCount;

	[ProtoMember(2)]
	private int _cannotUntapCount;

	[ProtoMember(3)]
	private int _canBeReducedToZeroDefenseCount;

	[ProtoMember(4)]
	private int _redrawAttackCount;

	[ProtoMember(5)]
	private int _guardCount;

	[ProtoMember(6)]
	private int _stealthCount;

	[ProtoMember(7)]
	private int _safeAttackCount;

	[ProtoMember(8)]
	private int _abilityGuardCount;

	[ProtoMember(9)]
	private int _abilityStealthCount;

	[ProtoMember(10)]
	private int _pacifistCount;

	[ProtoMember(11)]
	private int _canReduceEnemyDefenseToZeroCount;

	private BBool _cannotAttack;

	private BBool _cannotUntap;

	private BBool _canBeReducedToZeroDefense;

	private BBool _redrawAttack;

	private BBool _guard;

	private BBool _stealth;

	private BBool _safeAttack;

	private BBool _abilityGuard;

	private BBool _abilityStealth;

	private BBool _pacifist;

	private BBool _canReduceEnemyDefenseToZero;

	public BBool cannotAttack => _cannotAttack ?? (_cannotAttack = new BBool(_cannotAttackCount > 0));

	public BBool cannotUntap => _cannotUntap ?? (_cannotUntap = new BBool(_cannotUntapCount > 0));

	public BBool canBeReducedToZeroDefense => _canBeReducedToZeroDefense ?? (_canBeReducedToZeroDefense = new BBool(_canBeReducedToZeroDefenseCount > 0));

	public BBool redrawAttack => _redrawAttack ?? (_redrawAttack = new BBool(_redrawAttackCount > 0));

	public BBool guard => _guard ?? (_guard = new BBool(_guardCount > 0));

	public BBool stealth => _stealth ?? (_stealth = new BBool(_stealthCount > 0));

	public BBool safeAttack => _safeAttack ?? (_safeAttack = new BBool(_safeAttackCount > 0));

	public BBool abilityGuard => _abilityGuard ?? (_abilityGuard = new BBool(_abilityGuardCount > 0));

	public BBool abilityStealth => _abilityStealth ?? (_abilityStealth = new BBool(_abilityStealthCount > 0));

	public BBool pacifist => _pacifist ?? (_pacifist = new BBool(_pacifistCount > 0));

	public BBool canReduceEnemyDefenseToZero => _canReduceEnemyDefenseToZero ?? (_canReduceEnemyDefenseToZero = new BBool(_canReduceEnemyDefenseToZeroCount > 0));

	public int this[StatusType status]
	{
		get
		{
			return status switch
			{
				StatusType.CannotAttack => _cannotAttackCount, 
				StatusType.CannotUntap => _cannotUntapCount, 
				StatusType.CanBeReducedToZeroDefense => _canBeReducedToZeroDefenseCount, 
				StatusType.RedrawAttack => _redrawAttackCount, 
				StatusType.Guard => _guardCount, 
				StatusType.Stealth => _stealthCount, 
				StatusType.SafeAttack => _safeAttackCount, 
				StatusType.AbilityGuard => _abilityGuardCount, 
				StatusType.AbilityStealth => _abilityStealthCount, 
				StatusType.Pacifist => _pacifistCount, 
				StatusType.CanReduceEnemyDefenseToZero => _canReduceEnemyDefenseToZeroCount, 
				_ => 0, 
			};
		}
		set
		{
			switch (status)
			{
			case StatusType.CannotAttack:
				cannotAttack.value = (_cannotAttackCount = value) > 0;
				break;
			case StatusType.CannotUntap:
				cannotUntap.value = (_cannotUntapCount = value) > 0;
				break;
			case StatusType.CanBeReducedToZeroDefense:
				canBeReducedToZeroDefense.value = (_canBeReducedToZeroDefenseCount = value) > 0;
				break;
			case StatusType.RedrawAttack:
				redrawAttack.value = (_redrawAttackCount = value) > 0;
				break;
			case StatusType.Guard:
				guard.value = (_guardCount = value) > 0;
				break;
			case StatusType.Stealth:
				stealth.value = (_stealthCount = value) > 0;
				break;
			case StatusType.SafeAttack:
				safeAttack.value = (_safeAttackCount = value) > 0;
				break;
			case StatusType.AbilityGuard:
				abilityGuard.value = (_abilityGuardCount = value) > 0;
				break;
			case StatusType.AbilityStealth:
				abilityStealth.value = (_abilityStealthCount = value) > 0;
				break;
			case StatusType.Pacifist:
				pacifist.value = (_pacifistCount = value) > 0;
				break;
			case StatusType.CanReduceEnemyDefenseToZero:
				canReduceEnemyDefenseToZero.value = (_canReduceEnemyDefenseToZeroCount = value) > 0;
				break;
			default:
				throw new ArgumentOutOfRangeException("status", status, null);
			}
		}
	}
}
