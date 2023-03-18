using System.Collections;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.UI.Others.Disclaimer;

public class Disclaimer : MonoBehaviour
{
	public float waitTime;

	private void Awake()
	{
		StartCoroutine(GoToMainMenu());
	}

	public IEnumerator GoToMainMenu()
	{
		yield return new WaitForSeconds(waitTime);
		Core.Logic.LoadMenuScene();
	}
}
