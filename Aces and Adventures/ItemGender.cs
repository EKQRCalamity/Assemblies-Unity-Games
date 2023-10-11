using System;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

[Serializable]
[Metadata(AllowedTypes = MetadataType.StringTableEntry)]
public class ItemGender : IMetadataEnum<ItemGender.Gender>, IMetadata, IMetadataVariable, IVariable
{
	[UIField(tooltip = "Male or Female.\nUsed by smart text to correctly respond to grammatical changes based on gender.")]
	public enum Gender
	{
		Male,
		Female
	}

	public Gender gender;

	public string VariableName => "gender";

	public Gender value
	{
		get
		{
			return gender;
		}
		set
		{
			gender = value;
		}
	}

	public object GetSourceValue(ISelectorInfo _)
	{
		return gender;
	}
}
