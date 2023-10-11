using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField]
public struct AdditionalResourceCosts : IEquatable<AdditionalResourceCosts>
{
	public const byte MAX = 5;

	[ProtoMember(1)]
	[UIField(min = 0, max = 5)]
	[UIHorizontalLayout("Top")]
	private byte _hp;

	[ProtoMember(2)]
	[UIField(min = 0, max = 5)]
	[UIHorizontalLayout("Top")]
	private byte _shield;

	[ProtoMember(3)]
	[UIField(min = 0, max = 5)]
	[UIHorizontalLayout("Bottom")]
	private byte _attack;

	[ProtoMember(4)]
	[UIField(min = 0, max = 5)]
	[UIHorizontalLayout("Bottom")]
	private byte _heroAbility;

	public byte hp => _hp;

	public byte shield => _shield;

	public byte attack => _attack;

	public byte heroAbility => _heroAbility;

	public bool isFree => !this;

	public AdditionalResourceCosts(byte hp, byte shield, byte attack, byte heroAbility)
	{
		_hp = hp;
		_shield = shield;
		_attack = attack;
		_heroAbility = heroAbility;
	}

	public IEnumerable<AbilityPreventedBy> GetAbilityPreventedBys()
	{
		for (int x3 = 0; x3 < _hp; x3++)
		{
			yield return AbilityPreventedBy.ResourceHP;
		}
		for (int x3 = 0; x3 < _shield; x3++)
		{
			yield return AbilityPreventedBy.ResourceShield;
		}
		for (int x3 = 0; x3 < _attack; x3++)
		{
			yield return AbilityPreventedBy.ResourceAttack;
		}
	}

	public AbilityPreventedBy? HasResources(ACombatant combatant)
	{
		if (_hp > 0 && (int)combatant.HP < _hp + 1)
		{
			return AbilityPreventedBy.ResourceHP;
		}
		if (_shield > 0 && (int)combatant.shield < _shield)
		{
			return AbilityPreventedBy.ResourceShield;
		}
		if (_heroAbility > 0 && combatant is Player player && (int)player.numberOfHeroAbilities < _heroAbility)
		{
			return AbilityPreventedBy.ResourceHeroAbility;
		}
		if (_attack > 0 && (int)combatant.numberOfAttacks < _attack)
		{
			return AbilityPreventedBy.ResourceAttack;
		}
		return null;
	}

	public void ConsumeResources(ACombatant combatant)
	{
		if (_hp > 0)
		{
			combatant.HP.value -= Math.Max(0, Math.Min(_hp, (int)combatant.HP - 1));
		}
		if (_shield > 0)
		{
			combatant.shield.value -= Math.Min(_shield, combatant.shield);
		}
		if (_attack > 0)
		{
			combatant.numberOfAttacks.value -= Math.Min(_attack, combatant.numberOfAttacks);
		}
		if (_heroAbility > 0 && combatant is Player player)
		{
			player.numberOfHeroAbilities.value -= Math.Min(_heroAbility, player.numberOfHeroAbilities);
		}
	}

	public IEnumerable<string> GetCostStrings()
	{
		if (_hp > 0)
		{
			yield return $"{_hp} HP";
		}
		if (_shield > 0)
		{
			yield return $"{_shield} Shield";
		}
		if (_attack > 0)
		{
			yield return $"{_attack} Attack";
		}
		if (_heroAbility > 0)
		{
			yield return $"{_heroAbility} Hero Ability";
		}
	}

	public IEnumerable<string> GetSearchStrings()
	{
		if (_hp > 0)
		{
			yield return "hp";
			yield return ResourceCostIconType.HP.GetTooltip().Localize();
		}
		if (_shield > 0)
		{
			yield return ResourceCostIconType.Shield.GetTooltip().Localize();
		}
		if (_attack > 0)
		{
			yield return ResourceCostIconType.Attack.GetTooltip().Localize();
		}
	}

	public static implicit operator bool(AdditionalResourceCosts costs)
	{
		if (costs._hp <= 0 && costs._shield <= 0 && costs._attack <= 0)
		{
			return costs._heroAbility > 0;
		}
		return true;
	}

	public static bool operator ==(AdditionalResourceCosts a, AdditionalResourceCosts b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(AdditionalResourceCosts a, AdditionalResourceCosts b)
	{
		return !a.Equals(b);
	}

	public static AdditionalResourceCosts operator +(AdditionalResourceCosts a, AdditionalResourceCosts b)
	{
		return new AdditionalResourceCosts((byte)(a.hp + b.hp), (byte)(a.shield + b.shield), (byte)(a.attack + b.attack), (byte)(a.heroAbility + b.heroAbility));
	}

	public override string ToString()
	{
		return ((_hp > 0).ToText("& " + _hp + " HP ") + (_shield > 0).ToText(" & " + _shield + " Shield ") + (_attack > 0).ToText(" & " + _attack + " Attack ") + (_heroAbility > 0).ToText(" & " + _heroAbility + " Hero Ability")).Trim();
	}

	public bool Equals(AdditionalResourceCosts other)
	{
		if (_hp == other._hp && _shield == other._shield && _attack == other._attack)
		{
			return _heroAbility == other._heroAbility;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is AdditionalResourceCosts other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _hp ^ (_shield << 8) ^ (_attack << 16) ^ (_heroAbility << 24);
	}
}
