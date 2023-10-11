using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField]
public class CombatMediaData : IDataContent
{
	public static readonly CombatMediaData Default = new CombatMediaData();

	[ProtoMember(1)]
	[UIField]
	private string _name;

	[ProtoMember(2)]
	[UIField]
	private ProjectileMediaPack _oneCardAttack;

	[ProtoMember(3)]
	[UIField]
	private ProjectileMediaPack _twoCardAttack;

	[ProtoMember(4)]
	[UIField]
	private ProjectileMediaPack _threeCardAttack;

	[ProtoMember(5)]
	[UIField]
	private ProjectileMediaPack _fourCardAttack;

	[ProtoMember(6)]
	[UIField]
	private ProjectileMediaPack _fiveCardAttack;

	[ProtoMember(7)]
	[UIField]
	private ProjectileMediaPack _defense;

	[ProtoMember(15)]
	public string tags { get; set; }

	public ProjectileMediaPack defense => _defense;

	private ProjectileMediaPack this[int cardCount] => cardCount switch
	{
		1 => _oneCardAttack, 
		2 => _twoCardAttack, 
		3 => _threeCardAttack, 
		4 => _fourCardAttack, 
		5 => _fiveCardAttack, 
		_ => _fiveCardAttack, 
	};

	public ProjectileMediaPack Attack(int cardCount)
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

	public string GetTitle()
	{
		if (!_name.HasVisibleCharacter())
		{
			return "Unnamed Combat Media Data";
		}
		return _name;
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
	}

	public string GetSaveErrorMessage()
	{
		return null;
	}

	public void OnLoadValidation()
	{
	}
}
