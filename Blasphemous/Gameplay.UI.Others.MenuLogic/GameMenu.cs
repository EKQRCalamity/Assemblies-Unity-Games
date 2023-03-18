using System.Collections;
using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic;

public class GameMenu : MonoBehaviour
{
	private CanvasGroup group;

	private float speed = 2f;

	private void Start()
	{
		group = GetComponent<CanvasGroup>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && group.alpha == 0f)
		{
			StartCoroutine(Show());
		}
		else if (Input.GetKeyDown(KeyCode.Escape) && group.alpha == 1f)
		{
			StartCoroutine(Hide());
		}
	}

	private IEnumerator Show()
	{
		while (group.alpha < 1f)
		{
			yield return new WaitForEndOfFrame();
			group.alpha += Time.deltaTime * speed;
		}
		group.interactable = true;
	}

	private IEnumerator Hide()
	{
		while (group.alpha > 0f)
		{
			yield return new WaitForEndOfFrame();
			group.alpha -= Time.deltaTime * speed;
		}
		group.interactable = false;
	}

	public void ImportSouls()
	{
	}
}
