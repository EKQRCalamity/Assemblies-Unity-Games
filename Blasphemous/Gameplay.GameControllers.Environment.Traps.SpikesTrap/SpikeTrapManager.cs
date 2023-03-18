using System.Collections;
using System.Linq;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.SpikesTrap;

public class SpikeTrapManager : MonoBehaviour
{
	public enum RiseMode
	{
		Carrousel,
		Alternate,
		Simultaneous
	}

	public bool horizontalOrder;

	public bool spikeTrapEnabled;

	public SpikeTrap[] spikeTraps;

	public RiseMode riseMode;

	public float delayTime;

	protected float deltaDelayTime;

	protected bool isRise;

	public float interval = 0.15f;

	protected bool outSpread;

	protected bool oddRising;

	protected int spikesCicleCounter;

	private void Start()
	{
		OrderSpikesTraps();
		isRise = false;
		spikesCicleCounter = 0;
	}

	private void Update()
	{
		deltaDelayTime += Time.deltaTime;
		switch (riseMode)
		{
		case RiseMode.Carrousel:
			runCarrousel();
			break;
		case RiseMode.Alternate:
			runAlternate();
			break;
		case RiseMode.Simultaneous:
			runSimultaneous();
			break;
		}
	}

	public void OrderSpikesTraps()
	{
		if (spikeTraps.Length <= 0)
		{
			return;
		}
		if (horizontalOrder)
		{
			spikeTraps = spikeTraps.OrderBy((SpikeTrap singleSpikeTrap) => singleSpikeTrap.transform.position.x).ToArray();
		}
		else
		{
			spikeTraps = spikeTraps.OrderBy((SpikeTrap singleSpikeTrap) => singleSpikeTrap.transform.position.y).ToArray();
		}
	}

	private IEnumerator Rise(float delayTime, bool rise = true)
	{
		for (int i = 0; i < spikeTraps.Length; i++)
		{
			spikeTraps[i].RiseSpikes(rise);
			yield return new WaitForSeconds(delayTime);
		}
		if (riseMode == RiseMode.Carrousel)
		{
			deltaDelayTime = 0f;
			outSpread = !outSpread;
		}
		isRise = rise;
	}

	private IEnumerator RiseOdd(float delayTime, bool rise = true)
	{
		isRise = rise;
		spikesCicleCounter++;
		int spikesTurn = spikesCicleCounter % 4;
		for (int i = 0; i < spikeTraps.Length; i++)
		{
			switch (spikesTurn)
			{
			case 0:
				if (i % 2 != 0)
				{
					spikeTraps[i].RiseSpikes();
				}
				break;
			case 1:
				if (i % 2 != 0)
				{
					spikeTraps[i].RiseSpikes(rise: false);
				}
				break;
			case 2:
				if (i % 2 == 0)
				{
					spikeTraps[i].RiseSpikes();
				}
				break;
			case 3:
				if (i % 2 == 0)
				{
					spikeTraps[i].RiseSpikes(rise: false);
				}
				break;
			}
			yield return new WaitForSeconds(delayTime);
		}
	}

	protected void runCarrousel()
	{
		if (deltaDelayTime >= delayTime && !outSpread)
		{
			outSpread = true;
			StartCoroutine(Rise(interval, !isRise));
		}
	}

	protected void runAlternate()
	{
		if (deltaDelayTime >= delayTime)
		{
			deltaDelayTime = 0f;
			StartCoroutine(RiseOdd(0f, !isRise));
		}
	}

	protected void runSimultaneous()
	{
		if (deltaDelayTime >= delayTime)
		{
			deltaDelayTime = 0f;
			StartCoroutine(Rise(0f, !isRise));
		}
	}
}
