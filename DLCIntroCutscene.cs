using System.Collections;
using UnityEngine;

public class DLCIntroCutscene : DLCGenericCutscene
{
	[SerializeField]
	private GameObject canvas;

	[SerializeField]
	private GameObject cameraPos;

	[SerializeField]
	private Animator BGanim;

	[SerializeField]
	private Transform astralPlaneController;

	[SerializeField]
	private Transform[] astralPlanePositions;

	[SerializeField]
	private float screen4BGScrollSpeed = 0.1f;

	[SerializeField]
	private float screen4ForestScrollStartSpeed = 325f;

	private float screen4ForestScrollSpeed;

	[SerializeField]
	private GameObject[] screen4Characters;

	[SerializeField]
	private GameObject screen4Forest;

	[SerializeField]
	private GameObject screen4Clouds;

	[SerializeField]
	private GameObject screen4FG;

	[SerializeField]
	private GameObject screen4ScrollEnd;

	[SerializeField]
	private SpriteRenderer screen4ScrollStart;

	[SerializeField]
	private SpriteRenderer screen4EndLoopBack;

	[SerializeField]
	[Range(-1f, 7f)]
	private int fastForward = -1;

	protected override void Start()
	{
		base.Start();
		allowScreenSkip = true;
		BGanim.speed = screen4BGScrollSpeed;
		screen4ForestScrollSpeed = screen4ForestScrollStartSpeed;
		AudioManager.PlayLoop("sfx_dlc_intro_oceanamb_loop");
	}

	protected override void OnScreenSkip()
	{
		StartCoroutine(skip_title_cr());
	}

	private IEnumerator skip_title_cr()
	{
		allowScreenSkip = false;
		IrisIn();
		yield return CupheadTime.WaitForSeconds(this, 0.9f);
		screens[curScreen].Play("End");
	}

	protected override void OnScreenAdvance(int which)
	{
		if (which == 0)
		{
			canvas.SetActive(value: true);
			AudioManager.StartBGMAlternate(0);
			AudioManager.Stop("sfx_dlc_intro_oceanamb_loop");
		}
		if (which < astralPlanePositions.Length && (bool)astralPlanePositions[which])
		{
			astralPlaneController.position = astralPlanePositions[which].position;
			astralPlaneController.localScale = astralPlanePositions[which].localScale;
		}
	}

	protected override void OnContinue()
	{
		allowScreenSkip = false;
		if (curScreen == 3 && !BGanim.GetBool("End"))
		{
			BGanim.SetBool("End", value: true);
			StartCoroutine(slow_down_bg_cr());
		}
	}

	private IEnumerator slow_down_bg_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		float end = screen4BGScrollSpeed / 2f;
		while (!BGanim.GetCurrentAnimatorStateInfo(0).IsName("End") && !BGanim.GetCurrentAnimatorStateInfo(0).IsName("AltEnd"))
		{
			yield return null;
		}
		GameObject[] array = screen4Characters;
		foreach (GameObject gameObject in array)
		{
			gameObject.transform.parent = screen4ScrollEnd.transform;
		}
		while (BGanim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
		{
			BGanim.speed = EaseUtils.EaseOutSine(screen4BGScrollSpeed, end, BGanim.GetCurrentAnimatorStateInfo(0).normalizedTime);
			screen4ForestScrollSpeed = screen4ForestScrollStartSpeed * (BGanim.speed / screen4BGScrollSpeed);
			GameObject[] array2 = screen4Characters;
			foreach (GameObject gameObject2 in array2)
			{
				gameObject2.transform.localPosition += Vector3.right * 6.4f;
			}
			yield return wait;
		}
		screen4ForestScrollSpeed = 0f;
		yield return screens[curScreen].WaitForAnimationToStart(this, "holdforBG");
		screens[curScreen].SetTrigger("Continue");
		while (screen4Characters[2].transform.localPosition.x < 1420f)
		{
			GameObject[] array3 = screen4Characters;
			foreach (GameObject gameObject3 in array3)
			{
				gameObject3.transform.localPosition += Vector3.right * 6.4f;
			}
			yield return wait;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (curScreen == 0)
		{
			CupheadCutsceneCamera.Current.SetPosition(cameraPos.transform.position);
		}
		else
		{
			CupheadCutsceneCamera.Current.SetPosition(Vector3.zero);
		}
		if (curScreen == 3)
		{
			screen4Forest.transform.localPosition += Vector3.left * screen4ForestScrollSpeed * CupheadTime.Delta;
			screen4Clouds.transform.localPosition += Vector3.left * screen4ForestScrollSpeed * CupheadTime.Delta * 0.5f;
			screen4FG.transform.localPosition += Vector3.left * screen4ForestScrollSpeed * CupheadTime.Delta * 1.5f;
		}
	}

	private void LateUpdate()
	{
		if (screen4Forest.transform.localPosition.x < -2560f)
		{
			screen4Forest.transform.localPosition += Vector3.right * 1280f;
		}
		if (screen4Clouds.transform.localPosition.x < -2560f)
		{
			screen4Clouds.transform.localPosition += Vector3.right * 1280f;
		}
		if (screen4FG.transform.localPosition.x < -5156f)
		{
			screen4FG.transform.localPosition += Vector3.right * 4767f;
		}
		if (screen4ScrollStart.transform.position.x < -1600f)
		{
			screen4ScrollStart.enabled = false;
			screen4EndLoopBack.enabled = true;
		}
	}
}
