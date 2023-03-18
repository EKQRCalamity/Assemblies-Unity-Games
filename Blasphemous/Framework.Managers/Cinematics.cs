using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Framework.FrameworkCore;
using Framework.Util;
using Gameplay.UI;
using I2.Loc;
using Rewired;
using Tools.Audio;
using Tools.DataContainer;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Framework.Managers;

public class Cinematics : GameSystem
{
	public delegate void CinematicEvent(CutsceneData cutscene);

	public delegate void CinematicEndEvent(bool cancelled);

	private bool cinematicRunning;

	private Camera cam;

	private VideoPlayer player;

	private EventInstance audio;

	private CutsceneData currentData;

	private Player rewired;

	private bool audioMuted;

	private float StartFade;

	private float EndFade;

	private RawImage videoImage;

	private RenderTexture renderTexture;

	private float oldAudio;

	private const string CINEMATICS_PARAMS_ID = "CINEMATICS";

	private Vector3 lastCamera = Vector3.zero;

	private bool lastCameraValid;

	private bool lastCameraBoundaries;

	private GameObject CurrentPrefabInstance;

	private Coroutine ImageCoroutine;

	private bool cinematicCancelled;

	public bool InSantosCutscene { get; set; }

	public float CinematicVolume
	{
		get
		{
			float volume = -1f;
			float finalvolume = -1f;
			if (audio.isValid())
			{
				audio.getVolume(out volume, out finalvolume);
			}
			return finalvolume;
		}
		set
		{
			if (audio.isValid())
			{
				audio.setVolume(value);
			}
		}
	}

	public event Core.SimpleEvent CinematicStarted;

	public event CinematicEndEvent CinematicEnded;

	public event CinematicEvent VideoStarted;

	public event Core.SimpleEvent VideoEnded;

	public override void Awake()
	{
		rewired = null;
		lastCameraValid = false;
		ImageCoroutine = null;
	}

	public override void Update()
	{
		if (cinematicRunning && currentData != null && currentData.CanBeCancelled && !cinematicCancelled)
		{
			rewired = ReInput.players.GetPlayer(0);
			if (rewired.GetButtonDown(51))
			{
				cinematicCancelled = true;
				EndCutscene(cancelled: true);
			}
		}
	}

	public void StartCutscene(CutsceneData cutscene, bool muteAudio, float fadeStart, float fadeEnd, bool useBackground)
	{
		if (cinematicRunning)
		{
			return;
		}
		cinematicCancelled = false;
		ImageCoroutine = null;
		Log.Trace("Cutscene", "Starting cinematic video.");
		if (!SearchReferences())
		{
			Log.Error("Cutscene", "References were not found.");
			return;
		}
		LocalizeCutscene(cutscene);
		audioMuted = muteAudio;
		if (audioMuted)
		{
			oldAudio = Core.Audio.Ambient.Volume;
			Core.Audio.FadeLevelAudio(0f, 1.5f);
		}
		Core.Input.SetBlocker("CINEMATIC", blocking: true);
		StartFade = fadeStart;
		EndFade = fadeEnd;
		if (StartFade > 0f)
		{
			Core.UI.Fade.Fade(toBlack: true, StartFade, 0f, delegate
			{
				PlayVideo(cutscene, useBackground);
			});
		}
		else
		{
			PlayVideo(cutscene, useBackground);
		}
		if (this.CinematicStarted != null)
		{
			this.CinematicStarted();
		}
	}

	public void EndCutscene(bool cancelled)
	{
		if (!cinematicRunning)
		{
			return;
		}
		if (currentData.cinematicType == CinematicType.Video)
		{
			player.loopPointReached -= OnVideoEnd;
		}
		Log.Trace("Cutscene", "Ending cinematic video.");
		if (this.VideoEnded != null)
		{
			this.VideoEnded();
		}
		if (EndFade > 0f)
		{
			Core.UI.Fade.FadeInOut(EndFade, 0f, delegate
			{
				EndCinematic();
			}, delegate
			{
				if (this.CinematicEnded != null)
				{
					this.CinematicEnded(cancelled);
				}
			});
		}
		else
		{
			EndCinematic();
			if (this.CinematicEnded != null)
			{
				this.CinematicEnded(cancelled);
			}
		}
	}

	public void SetFreeCamera(bool freeCamera)
	{
		if (freeCamera)
		{
			if (!lastCameraValid)
			{
				Core.Logic.CameraManager.ProCamera2D.Reset();
				lastCamera = Core.Logic.CameraManager.ProCamera2D.LocalPosition;
				lastCameraValid = true;
				lastCameraBoundaries = Core.Logic.CameraManager.ProCamera2DNumericBoundaries.UseNumericBoundaries;
			}
			Core.Logic.CameraManager.ProCamera2D.FollowHorizontal = false;
			Core.Logic.CameraManager.ProCamera2D.FollowVertical = false;
			Core.Logic.CameraManager.ProCamera2DNumericBoundaries.UseNumericBoundaries = false;
		}
		else
		{
			if (lastCameraValid)
			{
				Core.Logic.CameraManager.ProCamera2D.MoveCameraInstantlyToPosition(lastCamera);
				lastCameraValid = false;
				Core.Logic.CameraManager.ProCamera2DNumericBoundaries.UseNumericBoundaries = lastCameraBoundaries;
			}
			Core.Logic.CameraManager.ProCamera2D.FollowHorizontal = true;
			Core.Logic.CameraManager.ProCamera2D.FollowVertical = true;
			Core.Logic.CameraManager.ProCamera2D.Reset();
		}
		Core.Input.SetBlocker("FREECAMERA", freeCamera);
	}

	public void SetFreeCameraPosition(Vector3 newPosition)
	{
		Core.Logic.CameraManager.ProCamera2D.MoveCameraInstantlyToPosition(newPosition);
	}

	private void EndCinematic()
	{
		if (ImageCoroutine != null)
		{
			Singleton<Core>.Instance.StopCoroutine(ImageCoroutine);
			ImageCoroutine = null;
		}
		if (audio.isValid())
		{
			Debug.Log("*** EndCinematic, stop audio");
			audio.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			audio.release();
			audio.clearHandle();
			audio = default(EventInstance);
		}
		if (currentData.cinematicType == CinematicType.Video)
		{
			player.Stop();
		}
		if (CurrentPrefabInstance != null)
		{
			UnityEngine.Object.Destroy(CurrentPrefabInstance);
			CurrentPrefabInstance = null;
		}
		cinematicRunning = false;
		CamCinematicMode(cinematicMode: false);
		Core.Input.SetBlocker("CINEMATIC", blocking: false);
		if (audioMuted)
		{
			Core.Audio.FadeLevelAudio(oldAudio, 1.5f);
		}
		videoImage.enabled = false;
		if (currentData.OverwriteReverb)
		{
			Core.Audio.Ambient.StopModifierParams("CINEMATICS");
		}
	}

	private void CamCinematicMode(bool cinematicMode)
	{
		Log.Trace("Cutscene", "Cam cinematic mode is " + cinematicMode);
		float x = cam.transform.position.x;
		float y = cam.transform.position.y;
		if (cinematicMode)
		{
			Core.UI.GameplayUI.gameObject.SetActive(value: false);
			return;
		}
		cam.transform.position = new Vector3(x, y, -10f);
		Core.UI.GameplayUI.gameObject.SetActive(value: true);
	}

	private bool SearchReferences()
	{
		cam = Camera.main;
		if (cam == null)
		{
			return false;
		}
		player = cam.gameObject.GetComponent<VideoPlayer>();
		if (player == null)
		{
			player = cam.gameObject.AddComponent<VideoPlayer>();
		}
		return cam != null && player != null;
	}

	private void PlayVideo(CutsceneData data, bool useSubtitlesBackgound)
	{
		currentData = data;
		audio = default(EventInstance);
		string text = data.foley;
		if (data.foleySpanish.Length > 0 && Core.Localization.GetCurrentAudioLanguageCode().ToUpper() == "ES")
		{
			text = data.foleySpanish;
		}
		if (text.Length > 0)
		{
			Debug.Log("Cutscene: Create audio " + text);
			audio = Core.Audio.CreateEvent(text);
		}
		videoImage = UIController.instance.GetSubtitleWidget().videoRaw;
		if (renderTexture == null)
		{
			renderTexture = new RenderTexture(1920, 1080, 24);
		}
		UIController.instance.GetSubtitleWidget().EnableSubtitleBackground(useSubtitlesBackgound);
		if (data.OverwriteReverb)
		{
			Core.Audio.Ambient.StartModifierParams("CINEMATICS", data.ReverbId, new AudioParamInitialized[0]);
		}
		switch (currentData.cinematicType)
		{
		case CinematicType.Images:
			if (currentData.images.Count > 0)
			{
				videoImage.texture = currentData.images[0].image.texture;
			}
			Core.UI.Fade.ClearFade();
			ImageCoroutine = Singleton<Core>.Instance.StartCoroutine(ShowImagesCutscene());
			break;
		case CinematicType.Video:
			try
			{
				player.clip = data.video;
				player.started += OnVideoStart;
				player.loopPointReached += OnVideoEnd;
				player.playbackSpeed = data.reproductionSpeed;
				player.prepareCompleted += OnVideoPrepared;
				player.playOnAwake = false;
				player.renderMode = VideoRenderMode.RenderTexture;
				videoImage.texture = renderTexture;
				player.targetTexture = renderTexture;
				player.Prepare();
				break;
			}
			catch (NullReferenceException ex)
			{
				Log.Error("Cutscene", "Invalid cutscene data when preparing. Forcing ending. Error: " + ex.ToString());
				break;
			}
		case CinematicType.Animation:
			ShowAnimationCutscene();
			break;
		}
	}

	private void OnVideoPrepared(VideoPlayer source)
	{
		player.prepareCompleted -= OnVideoPrepared;
		try
		{
			if (StartFade > 0f)
			{
				Core.UI.Fade.Fade(toBlack: false, StartFade, 1f, delegate
				{
				});
			}
			player.Play();
			if (audio.isValid())
			{
				audio.start();
			}
			cinematicRunning = true;
			Log.Trace("Cutscene", "Starting video with camera " + cam.name + " and clip " + source.clip.name);
			Singleton<Core>.Instance.StartCoroutine(OnVideoPreparedSecured());
		}
		catch (NullReferenceException ex)
		{
			Log.Error("Cutscene", "Invalid cutscene data when playing. Forcing ending. Error: " + ex.ToString());
		}
	}

	private IEnumerator OnVideoPreparedSecured()
	{
		yield return new WaitForEndOfFrame();
		videoImage.enabled = true;
		if (StartFade == 0f)
		{
			Core.UI.Fade.ClearFade();
		}
	}

	private void OnVideoEnd(VideoPlayer source)
	{
		Log.Trace("Cutscene", "Video End");
		EndCutscene(cancelled: false);
	}

	private void OnVideoStart(VideoPlayer source)
	{
		player.started -= OnVideoStart;
		Log.Trace("Cutscene", "Video Start");
		if (this.VideoStarted != null)
		{
			this.VideoStarted(currentData);
		}
		CamCinematicMode(cinematicMode: true);
	}

	private IEnumerator ShowImagesCutscene()
	{
		videoImage.enabled = true;
		if (StartFade > 0f)
		{
			Core.UI.Fade.Fade(toBlack: false, StartFade, 1f, delegate
			{
			});
		}
		if (audio.isValid())
		{
			audio.start();
		}
		cinematicRunning = true;
		Log.Trace("Cutscene", "Starting image list " + currentData.name);
		OnVideoStart(null);
		foreach (ImageList img in currentData.images)
		{
			videoImage.texture = img.image.texture;
			yield return new WaitForSeconds(img.duration);
		}
		EndCutscene(cancelled: false);
	}

	private void ShowAnimationCutscene()
	{
		videoImage.enabled = false;
		CurrentPrefabInstance = UnityEngine.Object.Instantiate(currentData.AnimationObject);
		Vector3 position = Core.Logic.CameraManager.ProCamera2D.transform.position;
		position.z = 0f;
		CurrentPrefabInstance.transform.position = position;
		string currentAudioLanguageCode = Core.Localization.GetCurrentAudioLanguageCode();
		if (currentData.animationTrigger != null && currentData.animationTrigger.ContainsKey(currentAudioLanguageCode) && currentData.animationTrigger[currentAudioLanguageCode].Count > 0)
		{
			CinematicsAnimator component = CurrentPrefabInstance.GetComponent<CinematicsAnimator>();
			component.SetTriggerList(currentData.animationTrigger[currentAudioLanguageCode]);
		}
		if (StartFade > 0f)
		{
			Core.UI.Fade.Fade(toBlack: false, StartFade, 1f, delegate
			{
			});
		}
		if (audio.isValid())
		{
			audio.start();
		}
		cinematicRunning = true;
		Log.Trace("Cutscene", "Starting animation list " + currentData.name);
		OnVideoStart(null);
	}

	public void OnCinematicsAnimationEnd()
	{
		EndCutscene(cancelled: false);
	}

	public void LocalizeCutscene(CutsceneData data)
	{
		int languageIndexFromCode = Core.Dialog.Language.GetLanguageIndexFromCode(I2.Loc.LocalizationManager.CurrentLanguageCode);
		string subtitleBaseTermName = data.GetSubtitleBaseTermName();
		List<TimeLocalization> list = new List<TimeLocalization>();
		string key = Core.Localization.GetCurrentAudioLanguageCode().ToUpper();
		if (data.subtitlesLocalization.ContainsKey(key))
		{
			list = data.subtitlesLocalization[key];
		}
		int num = 0;
		foreach (SubTitleBlock subtitle in data.subtitles)
		{
			string text = subtitleBaseTermName + "_" + num;
			TermData termData = Core.Dialog.Language.GetTermData(text);
			if (termData == null)
			{
				Debug.LogWarning("Term " + text + " not found in Cutscene Localization");
				continue;
			}
			subtitle.text = termData.Languages[languageIndexFromCode];
			if (num < list.Count)
			{
				subtitle.from = list[num].from;
				subtitle.to = list[num].to;
			}
			num++;
		}
	}
}
