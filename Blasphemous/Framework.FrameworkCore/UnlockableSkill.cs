using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.FrameworkCore;

[CreateAssetMenu(fileName = "skill", menuName = "Blasphemous/Unlockable Skill")]
public class UnlockableSkill : ScriptableObject, ILocalizable
{
	[OnValueChanged("OnIdChanged", false)]
	public string id = string.Empty;

	public string caption = string.Empty;

	[TextArea(3, 10)]
	public string description = string.Empty;

	public string instructions = string.Empty;

	public int tier;

	public int cost = 500;

	public bool unlocked;

	[ValueDropdown("SkillsValues")]
	public string parentSkill = string.Empty;

	public Sprite bigImage;

	public Sprite smallImage;

	public Sprite smallImageBuyed;

	private const string NO_DEPENDENCY = "NO DEPENDENCY";

	public void OnIdChanged(string value)
	{
		id = value.Replace(' ', '_').ToUpper();
	}

	public string GetParentSkill()
	{
		if (parentSkill == "NO DEPENDENCY")
		{
			return string.Empty;
		}
		return parentSkill;
	}

	public string GetBaseTranslationID()
	{
		return "UnlockableSkill/" + id;
	}

	private IList<string> SkillsValues()
	{
		return null;
	}
}
