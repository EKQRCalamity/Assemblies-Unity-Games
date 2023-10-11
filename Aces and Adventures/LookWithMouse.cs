using UnityEngine;

public class LookWithMouse : MonoBehaviour
{
	public float mouseSensitivity = 100f;

	public Transform playerBody;

	private float xRotation;

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update()
	{
		float num = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
		float num2 = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
		xRotation -= num2;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);
		base.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		playerBody.Rotate(Vector3.up * num);
	}
}
