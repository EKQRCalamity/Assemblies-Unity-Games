using Sirenix.OdinInspector;
using UnityEngine;

namespace LevelEditor;

public class ChainHook : MonoBehaviour
{
	[BoxGroup("Prefabs", true, false, 0)]
	public GameObject chainLinkPrefab;

	[BoxGroup("Prefabs", true, false, 0)]
	public GameObject lastLinkPrefab;

	[BoxGroup("Links properties", true, false, 0)]
	public float linkVerticalOffset = 0.15f;

	[BoxGroup("Links properties", true, false, 0)]
	public float lastLinkVerticalOffset = -1.3f;

	[BoxGroup("Links properties", true, false, 0)]
	[Range(1f, 50f)]
	public int numLinks = 5;

	[BoxGroup("First Link Motor", true, false, 0)]
	public float motorSpeed = 10f;

	[BoxGroup("First Link Motor", true, false, 0)]
	public float maxMotorForce = 10f;

	[Button("Update Chain", ButtonSizes.Small)]
	private void UpdateChainLink()
	{
		CleanupChain();
		CreateChainLink();
		ApplyMotorToFirstLink();
	}

	private void CleanupChain()
	{
		int childCount = base.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Object.DestroyImmediate(base.transform.GetChild(0).gameObject);
		}
	}

	private void CreateChainLink()
	{
		Vector2 zero = Vector2.zero;
		Rigidbody2D component = GetComponent<Rigidbody2D>();
		for (int i = 0; i < numLinks; i++)
		{
			GameObject gameObject = Object.Instantiate(chainLinkPrefab, base.transform);
			gameObject.transform.localPosition = zero;
			gameObject.name = $"Chain Link #{i:00}";
			zero += Vector2.down * linkVerticalOffset;
			gameObject.GetComponent<HingeJoint2D>().connectedBody = component;
			component = gameObject.GetComponent<Rigidbody2D>();
		}
		if (lastLinkPrefab != null)
		{
			GameObject gameObject2 = Object.Instantiate(lastLinkPrefab, base.transform);
			gameObject2.transform.localPosition = zero + Vector2.down * lastLinkVerticalOffset;
			gameObject2.name = "Last Chain Link";
			gameObject2.GetComponent<HingeJoint2D>().connectedBody = component;
		}
	}

	private void ApplyMotorToFirstLink()
	{
		HingeJoint2D component = base.transform.GetChild(0).GetComponent<HingeJoint2D>();
		JointMotor2D motor = component.motor;
		motor.motorSpeed = motorSpeed;
		motor.maxMotorTorque = maxMotorForce;
		component.motor = motor;
		component.useMotor = true;
	}
}
