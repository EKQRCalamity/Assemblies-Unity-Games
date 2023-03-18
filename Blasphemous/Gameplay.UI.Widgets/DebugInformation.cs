using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gameplay.UI.Widgets;

public class DebugInformation : MonoBehaviour
{
	public Text currentSceneLabel;

	public Text currentCoordinatesLabel;

	public bool showCursor;

	private bool isDebugBuild;

	private void Awake()
	{
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		LevelManager.OnMenuLoaded += OnMenuLoaded;
		base.gameObject.SetActive(value: false);
		isDebugBuild = Debug.isDebugBuild;
		DisableWhenRetailMode();
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		LevelManager.OnMenuLoaded -= OnMenuLoaded;
	}

	private void OnMenuLoaded()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		base.gameObject.SetActive(value: true);
		string text = SceneManager.GetActiveScene().name.ToUpper();
		if (currentSceneLabel != null)
		{
			currentSceneLabel.text = text;
		}
	}

	private void Update()
	{
		if (Core.ready)
		{
			Cursor.visible = false;
			if (currentCoordinatesLabel != null && Core.Logic.Penitent != null)
			{
				currentCoordinatesLabel.text = ((Vector2)Core.Logic.Penitent.transform.position).ToString();
			}
			Cursor.visible = isDebugBuild && showCursor;
		}
	}

	private void DisableWhenRetailMode()
	{
		if ((bool)currentCoordinatesLabel)
		{
			currentCoordinatesLabel.gameObject.SetActive(isDebugBuild);
		}
		if ((bool)currentSceneLabel)
		{
			currentSceneLabel.gameObject.SetActive(isDebugBuild);
		}
	}
}
