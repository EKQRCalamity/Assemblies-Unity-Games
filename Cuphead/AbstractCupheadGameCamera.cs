using System.Collections;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public abstract class AbstractCupheadGameCamera : AbstractCupheadCamera
{
	public delegate void OnShakeHandler(float amount, float time);

	private enum FloatState
	{
		Stop,
		Float
	}

	private Vector3 _shakeAdd;

	private Vector3 _floatAdd;

	protected Vector3 _position;

	protected BlurOptimized _blurEffect;

	private float _zoom = 1f;

	private IEnumerator shakeCoroutine;

	private float shakeAmount;

	private FloatState floatState;

	private IEnumerator floatCoroutine;

	private IEnumerator zoomCoroutine;

	private const float BLUR_TIME_START = 0.15f;

	private const float BLUR_TIME_END = 0.15f;

	private IEnumerator _blurCoroutine;

	private float _currentBlurAmount;

	private float maxBlur = 1.2f;

	public float zoom
	{
		get
		{
			return _zoom;
		}
		protected set
		{
			_zoom = value;
			base.camera.orthographicSize = OrthographicSize / _zoom;
		}
	}

	public bool isShaking { get; private set; }

	public abstract float OrthographicSize { get; }

	public float Width => 1.7777778f * OrthographicSize * zoom;

	public float Height => OrthographicSize * zoom;

	public float Top => base.camera.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)).y;

	public event OnShakeHandler OnShakeEvent;

	protected override void Awake()
	{
		base.Awake();
		base.camera.orthographicSize = OrthographicSize;
		_blurEffect = GetComponent<BlurOptimized>();
		_blurEffect.enabled = false;
		base.camera.clearFlags = CameraClearFlags.Color;
	}

	protected void Move()
	{
		base.transform.position = _position + _shakeAdd + _floatAdd;
	}

	public void Shake(float amount, float time, bool bypassEvent = false)
	{
		isShaking = true;
		ResetShake();
		shakeAmount = amount;
		if (!bypassEvent && this.OnShakeEvent != null)
		{
			this.OnShakeEvent(amount, time);
		}
		shakeCoroutine = falloffShake_cr(amount, time);
		StartCoroutine(shakeCoroutine);
	}

	public void StartShake(float amount)
	{
		ResetShake();
		shakeAmount = amount;
		shakeCoroutine = endlessShake_cr(amount);
		StartCoroutine(shakeCoroutine);
	}

	public void EndShake(float time)
	{
		ResetShake();
		if (time > 0f)
		{
			shakeCoroutine = falloffShake_cr(shakeAmount, time);
			StartCoroutine(shakeCoroutine);
		}
	}

	public void StartSmoothShake(float amount, float period, int octaves)
	{
		ResetShake();
		shakeCoroutine = endlessSmoothShake_cr(amount, period, octaves);
		StartCoroutine(shakeCoroutine);
	}

	public void ResetShake()
	{
		if (shakeCoroutine != null)
		{
			StopCoroutine(shakeCoroutine);
		}
		shakeCoroutine = null;
		_shakeAdd = Vector3.zero;
	}

	private IEnumerator falloffShake_cr(float amount, float time)
	{
		float t = 0f;
		float halfAmount = amount / 2f;
		while (t < time)
		{
			float val = 1f - t / time;
			shakeAmount = amount * val;
			float x = Random.Range(0f - halfAmount, halfAmount);
			float y = Random.Range(0f - halfAmount, halfAmount);
			_shakeAdd = new Vector3(x * val, y * val, 0f);
			t += (float)CupheadTime.Delta;
			yield return null;
			if (PauseManager.state == PauseManager.State.Paused)
			{
				while (PauseManager.state == PauseManager.State.Paused)
				{
					yield return null;
				}
			}
		}
		ResetShake();
		isShaking = false;
	}

	private IEnumerator endlessShake_cr(float amount)
	{
		float halfAmount = amount / 2f;
		while (true)
		{
			float x = Random.Range(0f - halfAmount, halfAmount);
			float y = Random.Range(0f - halfAmount, halfAmount);
			_shakeAdd = new Vector3(x, y, 0f);
			yield return null;
			if (PauseManager.state == PauseManager.State.Paused)
			{
				while (PauseManager.state == PauseManager.State.Paused)
				{
					yield return null;
				}
			}
		}
	}

	private IEnumerator endlessSmoothShake_cr(float amount, float period, int octaves)
	{
		float t = 0f;
		while (true)
		{
			t += (float)CupheadTime.Delta;
			float x = 0f;
			float y = 0f;
			float scale2 = 1f;
			for (int i = 0; i < octaves; i++)
			{
				scale2 = Mathf.Pow(2f, i);
				x += Mathf.PerlinNoise((t + 1000f) * scale2 / period, 0f) * amount / scale2;
				y += Mathf.PerlinNoise((t + 2000f) * scale2 / period, 0f) * amount / scale2;
			}
			_shakeAdd.x = x;
			_shakeAdd.y = y;
			_shakeAdd.z = 0f;
			yield return null;
			if (PauseManager.state == PauseManager.State.Paused)
			{
				while (PauseManager.state == PauseManager.State.Paused)
				{
					yield return null;
				}
			}
		}
	}

	public void StartFloat(float amount, float time)
	{
		ResetFloat();
		floatCoroutine = float_cr(amount, time);
		StartCoroutine(floatCoroutine);
	}

	public void EndFloat()
	{
		ResetFloat();
	}

	public void ResetFloat()
	{
		if (floatCoroutine != null)
		{
			StopCoroutine(floatCoroutine);
		}
		floatCoroutine = null;
		_floatAdd = Vector3.zero;
	}

	public void SetManualFloat(Vector3 value)
	{
		_floatAdd = value;
	}

	private IEnumerator float_cr(float amount, float time)
	{
		floatState = FloatState.Float;
		float t3 = 0f;
		float bottom = 0f;
		while (true)
		{
			t3 = 0f;
			while (t3 < time)
			{
				float val = t3 / time;
				float y = EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, bottom, amount, val);
				_floatAdd = new Vector3(0f, y, 0f);
				t3 += (float)CupheadTime.Delta;
				yield return null;
			}
			t3 = 0f;
			while (t3 < time)
			{
				float val2 = t3 / time;
				float y2 = EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, amount, bottom, val2);
				_floatAdd = new Vector3(0f, y2, 0f);
				t3 += (float)CupheadTime.Delta;
				yield return null;
			}
			if (floatState == FloatState.Stop)
			{
				ResetFloat();
			}
		}
	}

	public void Zoom(float newZoom, float time, EaseUtils.EaseType ease)
	{
		StopZoom();
		zoomCoroutine = zoom_cr(newZoom, time, ease);
		StartCoroutine(zoomCoroutine);
	}

	private void StopZoom()
	{
		if (zoomCoroutine != null)
		{
			StopCoroutine(zoomCoroutine);
		}
		zoomCoroutine = null;
	}

	private IEnumerator zoom_cr(float newZoom, float time, EaseUtils.EaseType ease)
	{
		float oldZoom = zoom;
		float t = 0f;
		while (t < time)
		{
			float val = t / time;
			zoom = EaseUtils.Ease(ease, oldZoom, newZoom, val);
			t += (float)CupheadTime.Delta;
			yield return null;
			if (PauseManager.state == PauseManager.State.Paused)
			{
				while (PauseManager.state == PauseManager.State.Paused)
				{
					yield return null;
				}
			}
		}
		zoom = newZoom;
		yield return null;
	}

	public void StartBlur()
	{
		maxBlur = 1.2f;
		StartBlurCoroutine(2f, 0.15f, disableBlurWhenComplete: false);
	}

	public void StartBlur(float time)
	{
		maxBlur = 1.2f;
		StartBlurCoroutine(2f, time, disableBlurWhenComplete: false);
	}

	public void StartBlur(float time, float amount)
	{
		maxBlur = amount;
		StartBlurCoroutine(2f, time, disableBlurWhenComplete: false);
	}

	public void EndBlur()
	{
		maxBlur = 1.2f;
		StartBlurCoroutine(0f, 0.15f, disableBlurWhenComplete: true);
	}

	public void EndBlur(float time)
	{
		maxBlur = 1.2f;
		StartBlurCoroutine(0f, time, disableBlurWhenComplete: true);
	}

	public void EndBlur(float time, float amount)
	{
		maxBlur = amount;
		StartBlurCoroutine(0f, time, disableBlurWhenComplete: true);
	}

	private void StartBlurCoroutine(float amount, float time, bool disableBlurWhenComplete)
	{
		StopBlurCoroutine();
		_blurCoroutine = blur_cr(amount, time, disableBlurWhenComplete);
		StartCoroutine(_blurCoroutine);
	}

	private void StopBlurCoroutine()
	{
		if (_blurCoroutine != null)
		{
			StopCoroutine(_blurCoroutine);
		}
		_blurCoroutine = null;
	}

	private void UpdateBlur()
	{
		_blurEffect.blurSize = Mathf.Lerp(0f, maxBlur, _currentBlurAmount);
	}

	protected IEnumerator blur_cr(float end, float time, bool disableBlurWhenComplete)
	{
		float start = _currentBlurAmount;
		_blurEffect.enabled = true;
		float t = 0f;
		while (t < time)
		{
			float val = t / time;
			_currentBlurAmount = Mathf.Lerp(start, end, val);
			UpdateBlur();
			t += Time.deltaTime;
			yield return null;
		}
		_currentBlurAmount = end;
		UpdateBlur();
		yield return null;
		if (disableBlurWhenComplete)
		{
			_blurEffect.enabled = false;
		}
	}
}
