using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField]
[Localize]
public class LevelUpData : IDataContent
{
	[ProtoMember(13)]
	[UIField("Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	private LocalizedStringData _nameLocalized;

	[ProtoMember(14)]
	[UIField("Description", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	private LocalizedStringData _descriptionLocalized;

	[ProtoMember(3)]
	[UIField(collapse = UICollapseType.Hide)]
	private CroppedImageRef _image = new CroppedImageRef(ImageCategoryType.Ability, AbilityData.Cosmetic.IMAGE_SIZE);

	[ProtoMember(4, OverwriteList = true)]
	[UIField(collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	private List<CharacterData.ALevelUp> _levelUps;

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

	[ProtoMember(2)]
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

	public string description => _descriptionLocalized ?? (_descriptionLocalized = new LocalizedStringData(""));

	public CroppedImageRef image => _image;

	public IEnumerable<CharacterData.ALevelUp> levelUps
	{
		get
		{
			IEnumerable<CharacterData.ALevelUp> enumerable = _levelUps;
			return enumerable ?? Enumerable.Empty<CharacterData.ALevelUp>();
		}
	}

	[ProtoMember(15)]
	public string tags { get; set; }

	private bool _imageSpecified => _image;

	public ATarget GenerateCard(PlayerClass characterClass)
	{
		return levelUps.Select((CharacterData.ALevelUp levelUp) => levelUp.GenerateCard(characterClass)).FirstOrDefault((ATarget card) => card != null) ?? new LevelUpReward(this);
	}

	public string GetTitle()
	{
		return name;
	}

	public string GetAutomatedDescription()
	{
		return description;
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
