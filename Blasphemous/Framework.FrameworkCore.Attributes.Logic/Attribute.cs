using System.Collections.Generic;
using System.Collections.ObjectModel;
using Framework.Managers;

namespace Framework.FrameworkCore.Attributes.Logic;

public class Attribute : BaseAttribute
{
	private List<RawBonus> _rawBonuses;

	private List<FinalBonus> _finalBonuses;

	protected float _bonusValue;

	protected float _finalValue;

	protected float _initialValue;

	protected float _upgradeValue;

	public float PermanetBonus { get; private set; }

	public virtual float Final => CalculateValue();

	public float Bonus
	{
		get
		{
			CalculateValue();
			return _bonusValue;
		}
		set
		{
			_bonusValue = value;
		}
	}

	public Attribute(float baseValue, float upgradeValue, float baseMultiplier = 1f)
		: base(baseValue, baseMultiplier)
	{
		_rawBonuses = new List<RawBonus>();
		_finalBonuses = new List<FinalBonus>();
		_finalValue = baseValue;
		_bonusValue = 0f;
		PermanetBonus = 0f;
		_initialValue = baseValue;
		_upgradeValue = upgradeValue;
	}

	public virtual bool IsVariable()
	{
		return false;
	}

	public virtual bool CallArchivementWhenUpgrade()
	{
		return false;
	}

	public void ResetBonus()
	{
		_rawBonuses.Clear();
		_finalBonuses.Clear();
	}

	public void Upgrade()
	{
		PermanetBonus += _upgradeValue;
		if (CallArchivementWhenUpgrade())
		{
			Core.AchievementsManager.CheckProgressToAC46();
		}
	}

	public int GetUpgrades()
	{
		return (int)(PermanetBonus / _upgradeValue);
	}

	public void ResetUpgrades()
	{
		PermanetBonus = 0f;
	}

	public void SetPermanentBonus(float value)
	{
		PermanetBonus = value;
	}

	public void AddRawBonus(RawBonus bonus)
	{
		_rawBonuses.Add(bonus);
	}

	public void AddFinalBonus(FinalBonus bonus)
	{
		_finalBonuses.Add(bonus);
	}

	public void RemoveRawBonus(RawBonus bonus)
	{
		if (_rawBonuses.Contains(bonus))
		{
			_rawBonuses.Remove(bonus);
		}
	}

	public void RemoveFinalBonus(FinalBonus bonus)
	{
		if (_finalBonuses.Contains(bonus))
		{
			_finalBonuses.Remove(bonus);
		}
	}

	protected void ApplyRawBonuses()
	{
		float num = 0f;
		float num2 = 1f;
		for (int i = 0; i < _rawBonuses.Count; i++)
		{
			num += _rawBonuses[i].Base;
			num2 *= _rawBonuses[i].Multiplier;
		}
		_bonusValue = num;
		_finalValue += num;
		_finalValue *= num2;
	}

	protected void ApplyFinalBonuses()
	{
		float num = 0f;
		float num2 = 1f;
		for (int i = 0; i < _finalBonuses.Count; i++)
		{
			num += _finalBonuses[i].Base;
			num2 *= _finalBonuses[i].Multiplier;
		}
		_bonusValue += num;
		_finalValue += num;
		_finalValue *= num2;
	}

	public ReadOnlyCollection<RawBonus> GetRawBonus()
	{
		return _rawBonuses.AsReadOnly();
	}

	public float CalculateValue()
	{
		_finalValue = base.Base;
		ApplyRawBonuses();
		ApplyFinalBonuses();
		_bonusValue += PermanetBonus;
		_finalValue += PermanetBonus;
		return _finalValue;
	}

	public void ConsoleSet(float newValue)
	{
		base.Base = newValue;
	}
}
