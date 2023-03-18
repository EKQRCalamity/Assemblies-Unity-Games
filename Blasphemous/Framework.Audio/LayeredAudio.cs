using System.Collections;
using FMODUnity;
using UnityEngine;

namespace Framework.Audio;

[RequireComponent(typeof(StudioEventEmitter))]
public class LayeredAudio : MonoBehaviour
{
	private bool active;

	private StudioEventEmitter emitter;

	public TextMesh text;

	private void Start()
	{
		emitter = GetComponent<StudioEventEmitter>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!active)
		{
			StartCoroutine(Actions());
			active = true;
		}
	}

	private IEnumerator Actions()
	{
		emitter.Play();
		yield return new WaitForSeconds(10f);
		SetIntensity(1f);
		text.text = "INTENSITY LEVEL: 1";
		yield return new WaitForSeconds(10f);
		SetIntensity(2f);
		text.text = "INTENSITY LEVEL: 2";
		yield return new WaitForSeconds(10f);
		SetIntensity(3f);
		text.text = "INTENSITY LEVEL: 3";
		yield return new WaitForSeconds(10f);
		SetIntensity(4f);
		text.text = "INTENSITY LEVEL: 4";
		yield return new WaitForSeconds(10f);
		SetIntensity(5f);
		text.text = "INTENSITY LEVEL: 5";
		yield return new WaitForSeconds(10f);
		SetIntensity(6f);
		text.text = "INTENSITY LEVEL: 6";
		yield return new WaitForSeconds(10f);
		SetIntensity(7f);
		text.text = "INTENSITY LEVEL: 7";
		yield return new WaitForSeconds(10f);
		SetIntensity(8f);
		text.text = "INTENSITY LEVEL: 8";
		yield return new WaitForSeconds(10f);
		SetEnding();
		text.text = "ENDING PHASE";
	}

	public void SetIntensity(float intensity)
	{
		emitter.SetParameter("Intensity", intensity);
	}

	public void SetEnding()
	{
		emitter.SetParameter("Ending", 1f);
	}
}
