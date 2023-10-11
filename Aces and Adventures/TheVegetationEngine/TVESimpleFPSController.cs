using UnityEngine;

namespace TheVegetationEngine;

[RequireComponent(typeof(CharacterController))]
public class TVESimpleFPSController : MonoBehaviour
{
	public float walkingSpeed = 2f;

	public float lookSpeed = 2f;

	public float lookXLimit = 45f;

	[Space(10f)]
	public Camera playerCamera;

	private CharacterController characterController;

	private float rotationX;

	private void Start()
	{
		characterController = GetComponent<CharacterController>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		float num = 1f;
		if (Input.GetKey(KeyCode.LeftShift))
		{
			num = 3f;
		}
		Vector3 vector = base.transform.TransformDirection(Vector3.forward);
		Vector3 vector2 = base.transform.TransformDirection(Vector3.right);
		Vector3 vector3 = vector * walkingSpeed * num * Input.GetAxis("Vertical") + vector2 * walkingSpeed * num * Input.GetAxis("Horizontal");
		if (!characterController.isGrounded)
		{
			vector3 += Physics.gravity;
		}
		characterController.Move(vector3 * Time.deltaTime);
		rotationX += (0f - Input.GetAxis("Mouse Y")) * lookSpeed;
		rotationX = Mathf.Clamp(rotationX, 0f - lookXLimit, lookXLimit);
		playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
		base.transform.rotation *= Quaternion.Euler(0f, Input.GetAxis("Mouse X") * lookSpeed, 0f);
	}
}
