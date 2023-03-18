using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BeamLauncher : MonoBehaviour
{
	private RaycastHit2D[] results;

	public float maxRange;

	public LayerMask beamCollisionMask;

	public float distanceBetweenBodySprites;

	public Transform endSprite;

	public List<GameObject> beamParts;

	public GameObject beamBodyPrefab;

	private void Start()
	{
		results = new RaycastHit2D[1];
		CreateBeamBodies();
	}

	private void CreateBeamBodies()
	{
		beamParts = new List<GameObject>();
		int num = Mathf.RoundToInt(maxRange / distanceBetweenBodySprites);
		for (int i = 0; i < num; i++)
		{
			GameObject item = Object.Instantiate(beamBodyPrefab, base.transform);
			beamParts.Add(item);
		}
	}

	private void Update()
	{
		LaunchBeam();
	}

	[Button(ButtonSizes.Small)]
	public void TriggerBeamBodyAnim()
	{
		foreach (GameObject beamPart in beamParts)
		{
			if (beamPart.activeInHierarchy)
			{
				Animator componentInChildren = beamPart.GetComponentInChildren<Animator>();
				if (componentInChildren != null)
				{
					componentInChildren.SetTrigger("BEAM");
				}
			}
		}
	}

	private void LaunchBeam()
	{
		Vector2 zero = Vector2.zero;
		if (Physics2D.RaycastNonAlloc(base.transform.position, base.transform.right, results, maxRange, beamCollisionMask) > 0)
		{
			zero = results[0].point;
			endSprite.position = zero;
			endSprite.gameObject.SetActive(value: true);
		}
		else
		{
			endSprite.gameObject.SetActive(value: false);
			zero = base.transform.position + base.transform.right * maxRange;
		}
		DrawBeam(base.transform.position, zero);
	}

	private void DrawBeam(Vector2 origin, Vector2 end)
	{
		Vector2 vector = end - origin;
		float magnitude = vector.magnitude;
		int num = Mathf.RoundToInt(magnitude / distanceBetweenBodySprites);
		for (int i = 0; i < num; i++)
		{
			Vector2 vector2 = Vector2.Lerp(origin, end, (float)i / (float)num);
			Debug.DrawLine(vector2 - vector.normalized * 0.5f, vector2 + vector.normalized * 0.5f, Color.cyan);
			GameObject gameObject = beamParts[i];
			gameObject.SetActive(value: true);
			gameObject.transform.position = vector2;
			gameObject.transform.right = vector;
		}
		if (num < beamParts.Count)
		{
			for (int j = num; j < beamParts.Count; j++)
			{
				beamParts[j].SetActive(value: false);
			}
		}
	}
}
