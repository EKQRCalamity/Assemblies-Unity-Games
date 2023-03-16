using System.Collections;
using UnityEngine.SceneManagement;

public class LoadFirstScene : AbstractMonoBehaviour
{
	private void Start()
	{
		SceneManager.LoadScene(0);
	}

	private IEnumerator load_cr()
	{
		yield return null;
	}
}
