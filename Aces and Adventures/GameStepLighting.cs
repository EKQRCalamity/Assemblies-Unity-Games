using UnityEngine;
using UnityEngine.Video;

public class GameStepLighting : GameStep
{
	public Color32? color;

	public int? intensity;

	public Quaternion? rotation;

	public VideoClip cookieClip;

	public float cookieIntensity;

	public float cookiePlaybackSpeed;

	public bool loopCookie;

	public LightingData.SpotlightData spotlightData;

	public LightingData.FogData fogData;

	public float indirectLightingMultiplier;

	public Transform target;

	public static GameStepLighting Create(LightingData lightingData, Transform target = null)
	{
		return new GameStepLighting(ProfileManager.options.video.postProcessing.animatedLighting ? lightingData : ContentRef.Defaults.lighting.adventure.data, target);
	}

	private GameStepLighting(Color32? color = null, int? intensity = null, Quaternion? rotation = null)
	{
		this.color = color;
		this.intensity = intensity;
		this.rotation = rotation;
	}

	public GameStepLighting(LightingData lightingData, Transform target = null)
		: this(lightingData.filter, lightingData.intensity, lightingData.rotation)
	{
		cookieClip = lightingData.cookieClip;
		cookieIntensity = lightingData.cookieIntensity;
		cookiePlaybackSpeed = lightingData.cookiePlaybackSpeed;
		loopCookie = lightingData.loopCookie;
		spotlightData = lightingData.spotlight;
		fogData = lightingData.fog;
		indirectLightingMultiplier = lightingData.indirectLightingMultiplier;
		this.target = target;
	}

	public override void Start()
	{
		base.manager.mainLight.GetOrAddComponent<LightInterpolator>().SetTarget(color, intensity, rotation, 1f, target);
		base.manager.mainLight.GetOrAddComponent<LightCookieBlender>().SetTarget(cookieClip, cookieIntensity, cookiePlaybackSpeed, loopCookie);
		VolumetricInterpolator orAddComponent = base.manager.mainLight.GetOrAddComponent<VolumetricInterpolator>();
		if ((bool)fogData)
		{
			orAddComponent.SetTarget(base.manager.adventureCamera.gameObject, fogData.density, fogData.filter);
		}
		else
		{
			orAddComponent.SetTarget(base.manager.adventureCamera.gameObject, 0f);
		}
		SpotlightInterpolator orAddComponent2 = base.manager.spotLight.GetOrAddComponent<SpotlightInterpolator>();
		if ((bool)spotlightData)
		{
			orAddComponent2.SetTarget(spotlightData.filter, spotlightData.intensity, spotlightData.rotation, spotlightData.distance, spotlightData.coneAngle, spotlightData.innerConeAnglePercent, spotlightData.range, spotlightData.positionAnimation, spotlightData.rotationAnimation, spotlightData.intensityAnimation, target, spotlightData.shadows && ProfileManager.options.video.postProcessing.shadows);
		}
		else
		{
			orAddComponent2.SetTarget(0);
		}
		base.manager.mainLight.GetOrAddComponent<IndirectLightInterpolator>().SetTarget(base.manager.adventureCamera.gameObject, indirectLightingMultiplier);
	}
}
