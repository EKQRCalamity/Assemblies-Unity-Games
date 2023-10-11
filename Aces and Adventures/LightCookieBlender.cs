using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
[RequireComponent(typeof(Light))]
public class LightCookieBlender : MonoBehaviour
{
	private static int TEXTURE = Shader.PropertyToID("_Texture");

	private static int INTENSITY = Shader.PropertyToID("_Intensity");

	private static int BLEND = Shader.PropertyToID("_BlendWeight");

	[Range(0.01f, 10f)]
	public float blendTime = 1f;

	public AnimationCurve blendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public CustomRenderTexture cookieRenderTexture;

	private VideoPlayer _videoPlayer;

	private Light _light;

	private float _intensity = -1f;

	private float _blend = -1f;

	private float _playbackSpeed = 1f;

	private bool _loop;

	private VideoClip _nextVideoClip;

	private LightInterpolator.Target<float> _nextIntensity;

	private LightInterpolator.Target<float> _nextPlaybackSpeed;

	public VideoPlayer videoPlayer => this.CacheComponent(ref _videoPlayer);

	public Light light => this.CacheComponent(ref _light);

	public VideoClip videoClip
	{
		get
		{
			return videoPlayer.clip;
		}
		private set
		{
			if (!(videoClip == value))
			{
				videoPlayer.clip = value;
				if ((bool)videoClip)
				{
					videoPlayer.Play();
				}
			}
		}
	}

	public VideoClip nextVideoClip
	{
		get
		{
			return _nextVideoClip;
		}
		private set
		{
			_nextVideoClip = value;
		}
	}

	public float intensity
	{
		get
		{
			return _intensity;
		}
		private set
		{
			if (SetPropertyUtility.SetStruct(ref _intensity, value))
			{
				cookieRenderTexture.material.SetFloat(INTENSITY, value);
			}
		}
	}

	public float blend
	{
		get
		{
			return _blend;
		}
		private set
		{
			if (SetPropertyUtility.SetStruct(ref _blend, Mathf.Clamp01(value)))
			{
				cookieRenderTexture.material.SetFloat(BLEND, blendCurve.Evaluate(_blend));
			}
		}
	}

	public float playbackSpeed
	{
		get
		{
			return _playbackSpeed;
		}
		set
		{
			_playbackSpeed = value;
		}
	}

	public bool isCrossfading
	{
		get
		{
			if ((bool)videoClip && (bool)nextVideoClip)
			{
				return videoClip != nextVideoClip;
			}
			return false;
		}
	}

	private void OnEnable()
	{
		intensity = 1f;
		blend = 0f;
		light.cookie = cookieRenderTexture;
		videoPlayer.enabled = true;
		playbackSpeed = 1f;
		cookieRenderTexture.Update();
	}

	private void Update()
	{
		float num = Time.deltaTime * isCrossfading.ToFloat(2f, 1f) / blendTime;
		if (videoClip != _nextVideoClip)
		{
			blend -= num;
			if (blend <= 0f)
			{
				videoClip = nextVideoClip;
				if (!videoClip)
				{
					base.enabled = false;
				}
				else
				{
					if ((bool)_nextIntensity)
					{
						intensity = _nextIntensity.Finish();
					}
					if ((bool)_nextPlaybackSpeed)
					{
						playbackSpeed = _nextPlaybackSpeed.Finish();
					}
					videoPlayer.isLooping = _loop;
				}
			}
		}
		else
		{
			blend += num;
			if ((bool)_nextIntensity)
			{
				intensity = (_nextIntensity.GetSample(blendCurve, blendTime, out var sample) ? _nextIntensity.Finish() : Mathf.Lerp(_nextIntensity.start, _nextIntensity, sample));
			}
			if ((bool)_nextPlaybackSpeed)
			{
				playbackSpeed = (_nextPlaybackSpeed.GetSample(blendCurve, blendTime, out var sample2) ? _nextPlaybackSpeed.Finish() : Mathf.Lerp(_nextPlaybackSpeed.start, _nextPlaybackSpeed, sample2));
			}
		}
		if (light.cookie == cookieRenderTexture)
		{
			cookieRenderTexture.Update();
		}
		videoPlayer.playbackSpeed = playbackSpeed * Time.timeScale;
	}

	private void OnDisable()
	{
		intensity = 1f;
		blend = 0f;
		light.cookie = null;
		videoPlayer.enabled = false;
	}

	public void SetTarget(VideoClip targetClip, float targetIntensity = 1f, float targetPlaybackSpeed = 1f, bool loop = true)
	{
		base.enabled |= targetClip;
		nextVideoClip = targetClip;
		_nextIntensity = new LightInterpolator.Target<float>(intensity, targetIntensity);
		_nextPlaybackSpeed = new LightInterpolator.Target<float>(playbackSpeed, targetPlaybackSpeed);
		_loop = loop;
		if (!videoClip && (bool)nextVideoClip)
		{
			videoPlayer.isLooping = loop;
			videoClip = nextVideoClip;
		}
	}
}
