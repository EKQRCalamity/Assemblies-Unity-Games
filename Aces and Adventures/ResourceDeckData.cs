using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract(SkipConstructor = true)]
[UIField]
public class ResourceDeckData : IDataContent
{
	[ProtoMember(1)]
	[UIField]
	private string _name;

	[ProtoMember(2, OverwriteList = true)]
	[UIField(maxCount = 52, collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	private List<PlayingCardType> _cards;

	public IEnumerable<PlayingCardType> cards => _cards?.AsEnumerable().Reverse();

	[ProtoMember(15)]
	public string tags { get; set; }

	public ResourceDeckData()
	{
		_cards = new List<PlayingCardType>(EnumUtil<PlayingCardType>.Values);
	}

	public string GetTitle()
	{
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
		if (_name.HasVisibleCharacter())
		{
			return null;
		}
		return "Please enter a name before saving.";
	}

	public void OnLoadValidation()
	{
	}

	[UIField]
	private void _Shuffle()
	{
		_cards?.Shuffle(new Random());
		UIGeneratorType.Validate(this);
	}
}
