using UnityEngine;

public class CutsceneGUI : AbstractMonoBehaviour
{
	public const string PATH = "UI/Cutscene_UI";

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	public CutscenePauseGUI pause;

	[Space(10f)]
	[SerializeField]
	private CupheadUICamera uiCameraPrefab;

	private CupheadUICamera uiCamera;

	public static CutsceneGUI Current { get; private set; }

	public Canvas Canvas => canvas;

	protected override void Awake()
	{
		base.Awake();
		Current = this;
	}

	private void Start()
	{
		uiCamera = Object.Instantiate(uiCameraPrefab);
		uiCamera.transform.SetParent(base.transform);
		uiCamera.transform.ResetLocalTransforms();
		canvas.worldCamera = uiCamera.camera;
	}

	private void OnDestroy()
	{
		if (Current == this)
		{
			Current = null;
		}
	}

	public void CutseneInit()
	{
		pause.Init(checkIfDead: false);
	}

	protected virtual void CutsceneSnapshot()
	{
		AudioManager.HandleSnapshot(AudioManager.Snapshots.Cutscene.ToString(), 0.15f);
	}
}
