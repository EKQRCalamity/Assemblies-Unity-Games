using UnityEngine;

public class PlayerRecolorHandler : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer chaliceRenderer;

	private void OnEnable()
	{
		EventManager.Instance.AddListener<ChaliceRecolorEvent>(chaliceRecolorEventHandler);
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveListener<ChaliceRecolorEvent>(chaliceRecolorEventHandler);
	}

	private void chaliceRecolorEventHandler(ChaliceRecolorEvent e)
	{
		SetChaliceRecolorEnabled(chaliceRenderer.sharedMaterial, e.enabled);
	}

	public static void SetChaliceRecolorEnabled(Material sharedMaterial, bool enabled)
	{
		sharedMaterial.SetFloat("_RecolorFactor", enabled ? 1 : 0);
	}
}
