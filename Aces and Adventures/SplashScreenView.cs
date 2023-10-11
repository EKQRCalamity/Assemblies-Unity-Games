using UnityEngine;
using UnityEngine.Events;

public class SplashScreenView : MonoBehaviour
{
	public UnityEvent onFinish;

	private float? _delayedFinish;

	public static SplashScreenView Instance { get; private set; }

	public float masterVolumeMultiplier
	{
		set
		{
			MasterMixManager.Instance.controller.masterVolume.SetMultiplier(this, value);
		}
	}

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		masterVolumeMultiplier = 0f;
		PauseMenu.BlockInput(this);
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private void Update()
	{
		if (_delayedFinish.HasValue && (_delayedFinish -= Time.unscaledDeltaTime) <= 0f)
		{
			_delayedFinish = null;
			Finish();
		}
	}

	private void OnDisable()
	{
		Instance = ((Instance == this) ? null : Instance);
	}

	private void OnDestroy()
	{
		MasterMixManager.Instance.controller.masterVolume.RemoveMultiplier(this);
	}

	public void Finish()
	{
		PauseMenu.UnblockInput(this);
		onFinish?.Invoke();
	}

	public void FinishDelayed(float delayInSeconds)
	{
		_delayedFinish = ((delayInSeconds > 0f) ? delayInSeconds : 0.001f);
	}
}
