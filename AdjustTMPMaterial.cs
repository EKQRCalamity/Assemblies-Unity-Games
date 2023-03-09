using System;
using TMPro;
using UnityEngine;

public class AdjustTMPMaterial : MonoBehaviour
{
	[Serializable]
	public struct MaterialData
	{
		public Localization.Languages language;

		public string materialName;
	}

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private Material defaultMaterial;

	[SerializeField]
	private MaterialData[] materials;

	private Localization.Languages previousLanguage;

	private bool initialSetupComplete;

	private void Update()
	{
		if (!initialSetupComplete || Localization.language != previousLanguage)
		{
			initialSetupComplete = true;
			previousLanguage = Localization.language;
			Localization.Languages language = Localization.language;
			Material material = getMaterial(language);
			if (material != null)
			{
				text.fontMaterial = material;
			}
		}
	}

	private Material getMaterial(Localization.Languages language)
	{
		MaterialData[] array = materials;
		for (int i = 0; i < array.Length; i++)
		{
			MaterialData materialData = array[i];
			if (materialData.language == language)
			{
				return FontLoader.GetTMPMaterial(materialData.materialName);
			}
		}
		return defaultMaterial;
	}
}
