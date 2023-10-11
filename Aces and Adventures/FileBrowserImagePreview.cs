using UnityEngine;

public class FileBrowserImagePreview : MonoBehaviour
{
	public Texture2DEvent onTextureChange;

	public StringEvent onNameChange;

	public StringEvent onResolutionChange;

	private Texture2D _texture;

	public Texture2D texture
	{
		get
		{
			return _texture;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _texture, value))
			{
				_OnTextureChange();
			}
		}
	}

	private void _OnTextureChange()
	{
		onTextureChange.Invoke(texture);
		if ((bool)texture)
		{
			onNameChange.Invoke(texture.name);
		}
		if ((bool)texture)
		{
			onResolutionChange.Invoke($"{texture.width} x {texture.height}");
		}
	}

	private void OnDisable()
	{
		texture = null;
	}
}
