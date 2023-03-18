using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.FrameworkCore;

[CreateAssetMenu(fileName = "tutorial", menuName = "Blasphemous/Tutorial")]
public class Tutorial : ScriptableObject
{
	[OnValueChanged("OnIdChanged", false)]
	public string id = string.Empty;

	public string description = string.Empty;

	public int order;

	public bool startUnlocked;

	public GameObject prefab;

	[HideInInspector]
	public bool unlocked;

	public void OnIdChanged(string value)
	{
		id = value.Replace(' ', '_').ToUpper();
	}
}
