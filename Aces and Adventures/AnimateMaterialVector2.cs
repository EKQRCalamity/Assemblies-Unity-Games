using UnityEngine;

public class AnimateMaterialVector2 : AnimateMaterialVector
{
	[Header("Texture")]
	public string textureName = "_MainTex";

	public SetTexturePropertyType textureProperty;

	public bool forceFindTexture;

	public override void CacheInitialValues()
	{
		if (!string.IsNullOrEmpty(textureName))
		{
			material = GetComponent<Renderer>().material;
			propertyFound = forceFindTexture || (bool)material.GetTexture(textureName);
			if (!propertyFound && textureName[0] != '_')
			{
				textureName = "_" + textureName;
				propertyFound = material.GetTexture(textureName);
			}
			if (!propertyFound)
			{
				Debug.LogError("~AnimateMaterialVector2: Failed to find [Texture] named [" + textureName + "] in material [" + material.name + "] of GameObject [" + base.gameObject.name + "].");
			}
			else
			{
				initialValue = ((textureProperty == SetTexturePropertyType.Offset) ? material.GetTextureOffset(textureName) : material.GetTextureScale(textureName));
			}
		}
		else
		{
			base.CacheInitialValues();
		}
	}

	protected override void UniqueUpdate(float t)
	{
		if (string.IsNullOrEmpty(textureName))
		{
			base.UniqueUpdate(t);
		}
		else if (propertyFound)
		{
			Vector4 value = GetValue(t);
			switch (textureProperty)
			{
			case SetTexturePropertyType.Offset:
				material.SetTextureOffset(textureName, value);
				break;
			case SetTexturePropertyType.Scale:
				material.SetTextureScale(textureName, value);
				break;
			}
		}
	}
}
