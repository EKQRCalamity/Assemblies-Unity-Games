using System.Collections.Generic;
using Framework.Managers;
using Framework.Penitences;
using Gameplay.UI.Others.MenuLogic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class PenitenceSlot : MonoBehaviour
{
	[SerializeField]
	[BoxGroup("Elements", true, false, 0)]
	private GameObject childElement;

	private Dictionary<string, SelectSaveSlots.PenitenceData> allPenitences = new Dictionary<string, SelectSaveSlots.PenitenceData>();

	private void Awake()
	{
		if (childElement != null)
		{
			childElement.gameObject.SetActive(value: false);
		}
	}

	public void UpdateFromSavegameData(PenitenceManager.PenitencePersistenceData data)
	{
		DeleteElements();
		if (data == null || data.allPenitences == null)
		{
			return;
		}
		foreach (IPenitence allPenitence in data.allPenitences)
		{
			if (allPenitence == null)
			{
				continue;
			}
			if (allPenitences.ContainsKey(allPenitence.Id))
			{
				bool flag = data.currentPenitence != null && data.currentPenitence.Id == allPenitence.Id;
				if (allPenitence.Completed || flag || allPenitence.Abandoned)
				{
					SelectSaveSlots.PenitenceData penitenceData = allPenitences[allPenitence.Id];
					Sprite sprite = null;
					CreateElement(sprite: allPenitence.Completed ? penitenceData.Completed : ((!flag) ? penitenceData.Missing : penitenceData.InProgress), name: allPenitence.Id);
				}
			}
			else
			{
				Debug.LogError("PenitenceSlot: Error penitence " + allPenitence.Id + " not found in config");
			}
		}
	}

	public void SetPenitenceConfig(List<SelectSaveSlots.PenitenceData> data)
	{
		allPenitences = new Dictionary<string, SelectSaveSlots.PenitenceData>();
		foreach (SelectSaveSlots.PenitenceData datum in data)
		{
			allPenitences[datum.id] = datum;
		}
	}

	private void DeleteElements()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in childElement.transform.parent)
		{
			if (item.gameObject.activeSelf)
			{
				list.Add(item.gameObject);
			}
		}
		foreach (GameObject item2 in list)
		{
			Object.Destroy(item2);
		}
	}

	private void CreateElement(string name, Sprite sprite)
	{
		GameObject gameObject = Object.Instantiate(childElement, childElement.transform.parent);
		gameObject.name = "Penitence_" + name;
		gameObject.SetActive(value: true);
		Image component = gameObject.GetComponent<Image>();
		component.sprite = sprite;
	}
}
