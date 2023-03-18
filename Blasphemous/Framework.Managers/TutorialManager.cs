using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Framework.FrameworkCore;
using Gameplay.UI;
using UnityEngine;

namespace Framework.Managers;

public class TutorialManager : GameSystem, PersistentInterface
{
	[Serializable]
	public class TutorialPersistenceData : PersistentManager.PersistentData
	{
		public Dictionary<string, bool> Tutorials = new Dictionary<string, bool>();

		public bool TutorialsEnabled = true;

		public TutorialPersistenceData()
			: base("ID_TUTORIAL")
		{
		}
	}

	private const string TUTORIAL_RESOUCE_DIR = "Tutorial/";

	public bool TutorialsEnabled = true;

	private Dictionary<string, Tutorial> tutorials = new Dictionary<string, Tutorial>();

	private bool IsShowwingTutorial;

	private const string PERSITENT_ID = "ID_TUTORIAL";

	public override void Start()
	{
		LoadAllTutorials();
		IsShowwingTutorial = false;
	}

	private void LoadAllTutorials()
	{
		Tutorial[] array = Resources.LoadAll<Tutorial>("Tutorial/");
		tutorials.Clear();
		Tutorial[] array2 = array;
		foreach (Tutorial tutorial in array2)
		{
			tutorial.unlocked = tutorial.startUnlocked;
			tutorials[tutorial.id] = tutorial;
		}
		Log.Debug("Tutorial", tutorials.Count + " tutorials loaded succesfully.");
	}

	public override void AllInitialized()
	{
		Core.Persistence.AddPersistentManager(this);
	}

	public bool AnyTutorialIsUnlocked()
	{
		return GetUnlockedTutorials().Count > 0;
	}

	public List<Tutorial> GetUnlockedTutorials()
	{
		return (from x in tutorials.Values
			where x.unlocked
			orderby x.order
			select x).ToList();
	}

	public void UnlockTutorial(string id)
	{
		if (tutorials.ContainsKey(id))
		{
			tutorials[id].unlocked = true;
		}
	}

	public bool IsTutorialUnlocked(string id)
	{
		bool result = false;
		if (tutorials.ContainsKey(id))
		{
			result = tutorials[id].unlocked;
		}
		return result;
	}

	public IEnumerator ShowTutorial(string id, bool blockPlayer = true)
	{
		if (!TutorialsEnabled)
		{
			yield return null;
		}
		else if (tutorials.ContainsKey(id))
		{
			while (IsShowwingTutorial)
			{
				yield return null;
			}
			IsShowwingTutorial = true;
			if (blockPlayer)
			{
				Core.Input.SetBlocker("TUTORIAL", blocking: true);
				Core.Logic.PauseGame();
			}
			Tutorial tut = tutorials[id];
			tut.unlocked = true;
			GameObject uiroot = UIController.instance.GetTutorialRoot();
			IEnumerator enumerator = uiroot.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform transform = (Transform)enumerator.Current;
					UnityEngine.Object.Destroy(transform.gameObject);
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable2 = (disposable = enumerator as IDisposable);
				if (disposable != null)
				{
					disposable2.Dispose();
				}
			}
			GameObject tutObj = UnityEngine.Object.Instantiate(tut.prefab, Vector3.zero, Quaternion.identity, uiroot.transform);
			tutObj.transform.localPosition = Vector3.zero;
			TutorialWidget widget = tutObj.GetComponent<TutorialWidget>();
			widget.ShowInGame();
			CanvasGroup gr = uiroot.GetComponent<CanvasGroup>();
			gr.alpha = 0f;
			uiroot.SetActive(value: true);
			DOTween.defaultTimeScaleIndependent = true;
			DOTween.To(() => gr.alpha, delegate(float x)
			{
				gr.alpha = x;
			}, 1f, 1f);
			while (!widget.WantToExit)
			{
				yield return null;
			}
			TweenerCore<float, float, FloatOptions> teen = DOTween.To(() => gr.alpha, delegate(float x)
			{
				gr.alpha = x;
			}, 0f, 1f);
			yield return new WaitForSecondsRealtime(0.5f);
			if (blockPlayer)
			{
				Core.Input.SetBlocker("TUTORIAL", blocking: false);
				Core.Logic.ResumeGame();
			}
			uiroot.SetActive(value: false);
			DOTween.defaultTimeScaleIndependent = false;
		}
		IsShowwingTutorial = false;
		yield return null;
	}

	public int GetOrder()
	{
		return 0;
	}

	public string GetPersistenID()
	{
		return "ID_TUTORIAL";
	}

	public void ResetPersistence()
	{
		LoadAllTutorials();
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		TutorialPersistenceData tutorialPersistenceData = new TutorialPersistenceData();
		foreach (Tutorial value in tutorials.Values)
		{
			tutorialPersistenceData.Tutorials[value.id] = value.unlocked;
		}
		tutorialPersistenceData.TutorialsEnabled = TutorialsEnabled;
		return tutorialPersistenceData;
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		TutorialPersistenceData tutorialPersistenceData = (TutorialPersistenceData)data;
		foreach (KeyValuePair<string, bool> tutorial in tutorialPersistenceData.Tutorials)
		{
			if (tutorials.ContainsKey(tutorial.Key))
			{
				tutorials[tutorial.Key].unlocked = tutorial.Value;
			}
		}
	}
}
