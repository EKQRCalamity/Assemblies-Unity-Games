using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.SpikesTrap;

public class SpikeTrap : MonoBehaviour
{
	private Animator deathAnimator;

	private bool spikesAreRaised;

	private BoxCollider2D spikeTrapColllider;

	private bool isVisible;

	private SpriteRenderer spikeRenderer;

	public bool IsVisible => isVisible;

	public bool SpikeAreRaised => spikesAreRaised;

	public BoxCollider2D SpikeTrapCollider => spikeTrapColllider;

	private void Update()
	{
		bool flag = IsVisibleFrom(spikeRenderer, UnityEngine.Camera.main);
		if (isVisible != flag)
		{
			isVisible = flag;
		}
	}

	private void Awake()
	{
		deathAnimator = GetComponent<Animator>();
		spikeTrapColllider = GetComponent<BoxCollider2D>();
		spikeRenderer = GetComponent<SpriteRenderer>();
	}

	public void RiseSpikes(bool rise = true)
	{
		if (rise && !spikesAreRaised)
		{
			spikesAreRaised = true;
			deathAnimator.SetBool("RISE", rise);
		}
		else if (!rise && spikesAreRaised)
		{
			spikesAreRaised = !spikesAreRaised;
			deathAnimator.SetBool("RISE", spikesAreRaised);
		}
	}

	public bool IsVisibleFrom(Renderer renderer, UnityEngine.Camera camera)
	{
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
		return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
	}
}
