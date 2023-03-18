using Gameplay.GameControllers.Camera;
using UnityEngine;

[ExecuteInEditMode]
public class PIDI_2DReflection : MonoBehaviour
{
	[HideInInspector]
	public Camera tempCamera;

	[HideInInspector]
	private Camera secondCam;

	[HideInInspector]
	public RenderTexture rt;

	[HideInInspector]
	public RenderTexture mask;

	public Shader parallaxInternal;

	public bool advancedParallax;

	public Camera[] cameras;

	[Range(1f, 5f)]
	public int downScaleValue = 1;

	public float surfaceLevel = -99f;

	public bool improvedReflection;

	public Color refColor = Color.white;

	public Color backgroundColor = Color.white;

	public LayerMask renderLayers;

	public LayerMask drawOverLayers;

	public bool alphaBackground;

	public bool srpMode;

	public Camera backgroundCam;

	public Camera foregroundCam;

	public bool isLocalReflection;

	public float waterSurfaceLine;

	public Vector2 horizontalLimits;

	public Transform reflectAsLocal;

	private Vector3 targetPos;

	private Material blitMat;

	private Camera cam;

	private void OnEnable()
	{
		if (!parallaxInternal)
		{
		}
		if (!blitMat)
		{
			blitMat = new Material(parallaxInternal);
			blitMat.hideFlags = HideFlags.DontSave;
		}
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if (component != null)
		{
			component.enabled = true;
		}
	}

	private void Start()
	{
		targetPos = base.transform.localPosition;
	}

	private void OnDrawGizmosSelected()
	{
		if (isLocalReflection)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(new Vector3(base.transform.position.x - 100f, waterSurfaceLine, base.transform.position.z), new Vector3(base.transform.position.x + 100f, waterSurfaceLine, base.transform.position.z));
			Gizmos.color = Color.red;
			Gizmos.DrawLine(new Vector3(horizontalLimits.x, base.transform.position.y + 100f, base.transform.position.z), new Vector3(horizontalLimits.x, base.transform.position.y - 100f, base.transform.position.z));
			Gizmos.DrawLine(new Vector3(horizontalLimits.y, base.transform.position.y + 100f, base.transform.position.z), new Vector3(horizontalLimits.y, base.transform.position.y - 100f, base.transform.position.z));
			Gizmos.color = Color.white;
		}
	}

	private void LateUpdate()
	{
		if (Application.isPlaying && isLocalReflection && (bool)reflectAsLocal)
		{
			base.transform.localPosition = targetPos;
			base.transform.position = new Vector3(Mathf.Clamp(reflectAsLocal.position.x, horizontalLimits.x, horizontalLimits.y), Mathf.Clamp(base.transform.position.y, float.NegativeInfinity, waterSurfaceLine), reflectAsLocal.position.z);
		}
	}

	private void OnWillRenderObject()
	{
		if (srpMode)
		{
			return;
		}
		if (!tempCamera)
		{
			tempCamera = new GameObject("TempPIDI2D_Camera", typeof(Camera)).GetComponent<Camera>();
			tempCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
			tempCamera.enabled = false;
		}
		float num = 1f / (float)downScaleValue;
		Camera current = Camera.current;
		if (GetComponent<Renderer>().sharedMaterial.HasProperty("_BackgroundReflection"))
		{
			current = foregroundCam;
		}
		if (current.name == "TempPIDI2D_Camera" || current.GetComponent<CameraManager>() == null)
		{
			return;
		}
		if (!rt)
		{
			rt = new RenderTexture((int)((float)current.pixelWidth * num), (int)((float)current.pixelHeight * num), 0);
		}
		else if ((int)((float)current.pixelWidth * num) != rt.width || (int)((float)current.pixelHeight * num) != rt.height)
		{
			Object.DestroyImmediate(rt);
			rt = new RenderTexture((int)((float)current.pixelWidth * num), (int)((float)current.pixelHeight * num), 0);
		}
		if (!mask)
		{
			mask = new RenderTexture((int)((float)current.pixelWidth * num), (int)((float)current.pixelHeight * num), 0);
		}
		else if ((int)((float)current.pixelWidth * num) != rt.width || (int)((float)current.pixelHeight * num) != rt.height)
		{
			Object.DestroyImmediate(rt);
			mask = new RenderTexture((int)((float)current.pixelWidth * num), (int)((float)current.pixelHeight * num), 0);
		}
		if (!current)
		{
			return;
		}
		tempCamera.transform.position = current.transform.position;
		tempCamera.transform.rotation = current.transform.rotation;
		if (improvedReflection)
		{
			float y = (tempCamera.ScreenToWorldPoint(new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 10f)) - tempCamera.ScreenToWorldPoint(new Vector3((float)Screen.width * 0.5f, 0f, 10f))).y;
			tempCamera.transform.position = new Vector3(tempCamera.transform.position.x, base.transform.position.y + y, tempCamera.transform.position.z);
			GetComponent<Renderer>().sharedMaterial.SetFloat("_BetaReflections", 1f);
		}
		else
		{
			GetComponent<Renderer>().sharedMaterial.SetFloat("_BetaReflections", 0f);
		}
		tempCamera.orthographic = current.orthographic;
		tempCamera.orthographicSize = current.orthographicSize;
		tempCamera.fieldOfView = current.fieldOfView;
		tempCamera.aspect = current.aspect;
		tempCamera.cullingMask = (int)renderLayers & -17;
		tempCamera.targetTexture = rt;
		tempCamera.clearFlags = ((current.clearFlags != CameraClearFlags.Nothing && current.clearFlags != CameraClearFlags.Depth) ? current.clearFlags : CameraClearFlags.Color);
		tempCamera.backgroundColor = current.backgroundColor;
		tempCamera.backgroundColor = ((!alphaBackground && !GetComponent<Renderer>().sharedMaterial.HasProperty("_BackgroundReflection")) ? current.backgroundColor : Color.clear);
		tempCamera.allowHDR = current.allowHDR;
		tempCamera.allowMSAA = current.allowMSAA;
		if (GetComponent<Renderer>().sharedMaterial.HasProperty("_Reflection2D"))
		{
			if (!advancedParallax)
			{
				tempCamera.Render();
			}
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			GetComponent<Renderer>().GetPropertyBlock(materialPropertyBlock);
			if (surfaceLevel == -99f)
			{
				surfaceLevel = GetComponent<Renderer>().sharedMaterial.GetFloat("_SurfaceLevel");
			}
			if (refColor == Color.clear)
			{
				refColor = GetComponent<Renderer>().sharedMaterial.GetColor("_Color");
			}
			if (backgroundColor == Color.clear && GetComponent<Renderer>().sharedMaterial.HasProperty("_BackgroundReflection"))
			{
				backgroundColor = GetComponent<Renderer>().sharedMaterial.GetColor("_ColorB");
			}
			if (!advancedParallax)
			{
				materialPropertyBlock.SetTexture("_Reflection2D", rt);
			}
			if (!advancedParallax && GetComponent<Renderer>().sharedMaterial.HasProperty("_ReflectionMask"))
			{
				tempCamera.clearFlags = CameraClearFlags.Color;
				tempCamera.backgroundColor = Color.clear;
				tempCamera.cullingMask = drawOverLayers;
				tempCamera.targetTexture = mask;
				tempCamera.transform.position = current.transform.position;
				tempCamera.Render();
				materialPropertyBlock.SetTexture("_ReflectionMask", mask);
			}
			materialPropertyBlock.SetColor("_Color", refColor);
			if (!advancedParallax && GetComponent<Renderer>().sharedMaterial.HasProperty("_BackgroundReflection"))
			{
				materialPropertyBlock.SetColor("_ColorB", backgroundColor);
				current = backgroundCam;
				if ((bool)current)
				{
					tempCamera.transform.position = current.transform.position;
					tempCamera.transform.rotation = current.transform.rotation;
					tempCamera.orthographic = current.orthographic;
					tempCamera.orthographicSize = current.orthographicSize;
					tempCamera.fieldOfView = current.fieldOfView;
					tempCamera.aspect = current.aspect;
					tempCamera.cullingMask = (int)drawOverLayers & -17;
					tempCamera.targetTexture = rt;
					tempCamera.clearFlags = ((current.clearFlags != CameraClearFlags.Nothing && current.clearFlags != CameraClearFlags.Depth) ? current.clearFlags : CameraClearFlags.Color);
					tempCamera.backgroundColor = current.backgroundColor;
					tempCamera.backgroundColor = ((!alphaBackground) ? current.backgroundColor : Color.clear);
					tempCamera.allowHDR = current.allowHDR;
					tempCamera.allowMSAA = current.allowMSAA;
					tempCamera.targetTexture = mask;
					tempCamera.transform.position = current.transform.position;
					tempCamera.Render();
					materialPropertyBlock.SetTexture("_BackgroundReflection", mask);
				}
			}
			if (advancedParallax)
			{
				RenderTexture temporary = RenderTexture.GetTemporary(Screen.width, Screen.height);
				RenderTexture temporary2 = RenderTexture.GetTemporary(Screen.width, Screen.height);
				for (int i = 0; i < cameras.Length; i++)
				{
					if ((bool)cameras[i])
					{
						Camera camera = cameras[i];
						tempCamera.transform.position = camera.transform.position;
						tempCamera.transform.rotation = camera.transform.rotation;
						tempCamera.orthographic = camera.orthographic;
						tempCamera.orthographicSize = camera.orthographicSize;
						tempCamera.fieldOfView = camera.fieldOfView;
						tempCamera.aspect = camera.aspect;
						if (improvedReflection)
						{
							float y2 = (tempCamera.ScreenToWorldPoint(new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 10f)) - tempCamera.ScreenToWorldPoint(new Vector3((float)Screen.width * 0.5f, 0f, 10f))).y;
							tempCamera.transform.position = new Vector3(tempCamera.transform.position.x, base.transform.position.y + y2, tempCamera.transform.position.z);
							blitMat.SetFloat("_BetaReflections", 1f);
						}
						else
						{
							blitMat.SetFloat("_BetaReflections", 0f);
						}
						tempCamera.cullingMask = camera.cullingMask & -17;
						tempCamera.clearFlags = CameraClearFlags.Skybox;
						tempCamera.backgroundColor = ((i <= 0) ? camera.backgroundColor : Color.clear);
						tempCamera.depth = camera.depth;
						tempCamera.allowHDR = camera.allowHDR;
						tempCamera.allowMSAA = camera.allowMSAA;
						tempCamera.targetTexture = ((i <= 0) ? temporary : temporary2);
						tempCamera.Render();
						blitMat.SetTexture("_SecondReflection", (i <= 0) ? temporary : temporary2);
						Graphics.Blit(temporary, temporary, blitMat);
					}
				}
				Graphics.Blit(temporary, rt);
				RenderTexture.ReleaseTemporary(temporary);
				RenderTexture.ReleaseTemporary(temporary2);
				materialPropertyBlock.SetTexture("_Reflection2D", rt);
			}
			materialPropertyBlock.SetFloat("_AlphaBackground", (!alphaBackground) ? 1 : 0);
			materialPropertyBlock.SetFloat("_SurfaceLevel", surfaceLevel);
			GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
		}
		tempCamera.targetTexture = null;
	}

	public void SetLocalReflectionConfig(LocalReflectionConfig config)
	{
		waterSurfaceLine = config.groundLevel;
		horizontalLimits.x = config.LeftSideLimit;
		horizontalLimits.y = config.RightSideLimit;
	}

	public void SRPUpdate(Camera cam)
	{
		if (!tempCamera)
		{
			tempCamera = new GameObject("TempPIDI2D_Camera", typeof(Camera)).GetComponent<Camera>();
			tempCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
			tempCamera.enabled = false;
		}
		float num = 1f / (float)downScaleValue;
		Camera camera = cam;
		if (GetComponent<Renderer>().sharedMaterial.HasProperty("_BackgroundReflection"))
		{
			camera = foregroundCam;
		}
		if (!camera || camera.name == "TempPIDI2D_Camera")
		{
			return;
		}
		if (!rt)
		{
			rt = new RenderTexture((int)((float)camera.pixelWidth * num), (int)((float)camera.pixelHeight * num), 0);
		}
		else if ((int)((float)camera.pixelWidth * num) != rt.width || (int)((float)camera.pixelHeight * num) != rt.height)
		{
			Object.DestroyImmediate(rt);
			rt = new RenderTexture((int)((float)camera.pixelWidth * num), (int)((float)camera.pixelHeight * num), 0);
		}
		if (!mask)
		{
			mask = new RenderTexture((int)((float)camera.pixelWidth * num), (int)((float)camera.pixelHeight * num), 0);
		}
		else if ((int)((float)camera.pixelWidth * num) != rt.width || (int)((float)camera.pixelHeight * num) != rt.height)
		{
			Object.DestroyImmediate(rt);
			mask = new RenderTexture((int)((float)camera.pixelWidth * num), (int)((float)camera.pixelHeight * num), 0);
		}
		if (!camera)
		{
			return;
		}
		tempCamera.transform.position = camera.transform.position;
		tempCamera.transform.rotation = camera.transform.rotation;
		tempCamera.orthographic = camera.orthographic;
		tempCamera.orthographicSize = camera.orthographicSize;
		tempCamera.fieldOfView = camera.fieldOfView;
		tempCamera.aspect = camera.aspect;
		tempCamera.cullingMask = (int)renderLayers & -17;
		tempCamera.targetTexture = rt;
		tempCamera.clearFlags = ((camera.clearFlags != CameraClearFlags.Nothing && camera.clearFlags != CameraClearFlags.Depth) ? camera.clearFlags : CameraClearFlags.Color);
		tempCamera.backgroundColor = camera.backgroundColor;
		tempCamera.backgroundColor = ((!alphaBackground && !GetComponent<Renderer>().sharedMaterial.HasProperty("_BackgroundReflection")) ? camera.backgroundColor : Color.clear);
		tempCamera.allowHDR = camera.allowHDR;
		tempCamera.allowMSAA = camera.allowMSAA;
		if (GetComponent<Renderer>().sharedMaterial.HasProperty("_Reflection2D"))
		{
			tempCamera.Render();
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			GetComponent<Renderer>().GetPropertyBlock(materialPropertyBlock);
			if (surfaceLevel == -99f)
			{
				surfaceLevel = GetComponent<Renderer>().sharedMaterial.GetFloat("_SurfaceLevel");
			}
			if (refColor == Color.clear)
			{
				refColor = GetComponent<Renderer>().sharedMaterial.GetColor("_Color");
			}
			if (backgroundColor == Color.clear && GetComponent<Renderer>().sharedMaterial.HasProperty("_BackgroundReflection"))
			{
				backgroundColor = GetComponent<Renderer>().sharedMaterial.GetColor("_ColorB");
			}
			materialPropertyBlock.SetTexture("_Reflection2D", rt);
			if (GetComponent<Renderer>().sharedMaterial.HasProperty("_ReflectionMask"))
			{
				tempCamera.clearFlags = CameraClearFlags.Color;
				tempCamera.backgroundColor = Color.clear;
				tempCamera.cullingMask = drawOverLayers;
				tempCamera.targetTexture = mask;
				tempCamera.transform.position = camera.transform.position;
				tempCamera.Render();
				materialPropertyBlock.SetTexture("_ReflectionMask", mask);
			}
			materialPropertyBlock.SetColor("_Color", refColor);
			if (GetComponent<Renderer>().sharedMaterial.HasProperty("_BackgroundReflection"))
			{
				materialPropertyBlock.SetColor("_ColorB", backgroundColor);
				camera = backgroundCam;
				if ((bool)camera)
				{
					tempCamera.transform.position = camera.transform.position;
					tempCamera.transform.rotation = camera.transform.rotation;
					tempCamera.orthographic = camera.orthographic;
					tempCamera.orthographicSize = camera.orthographicSize;
					tempCamera.fieldOfView = camera.fieldOfView;
					tempCamera.aspect = camera.aspect;
					tempCamera.cullingMask = (int)drawOverLayers & -17;
					tempCamera.targetTexture = rt;
					tempCamera.clearFlags = ((camera.clearFlags != CameraClearFlags.Nothing && camera.clearFlags != CameraClearFlags.Depth) ? camera.clearFlags : CameraClearFlags.Color);
					tempCamera.backgroundColor = camera.backgroundColor;
					tempCamera.backgroundColor = ((!alphaBackground) ? camera.backgroundColor : Color.clear);
					tempCamera.allowHDR = camera.allowHDR;
					tempCamera.allowMSAA = camera.allowMSAA;
					tempCamera.targetTexture = mask;
					tempCamera.transform.position = camera.transform.position;
					tempCamera.Render();
					materialPropertyBlock.SetTexture("_BackgroundReflection", mask);
				}
			}
			materialPropertyBlock.SetFloat("_AlphaBackground", (!alphaBackground) ? 1 : 0);
			materialPropertyBlock.SetFloat("_SurfaceLevel", surfaceLevel);
			GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
		}
		tempCamera.targetTexture = null;
	}

	private void OnDestroy()
	{
		if ((bool)rt)
		{
			if (!Application.isPlaying)
			{
				Object.DestroyImmediate(rt);
			}
			else
			{
				Object.Destroy(rt);
			}
		}
		if ((bool)tempCamera)
		{
			if (!Application.isPlaying)
			{
				Object.DestroyImmediate(tempCamera.gameObject);
			}
			else
			{
				Object.Destroy(tempCamera.gameObject);
			}
		}
	}

	private void OnDisable()
	{
		if ((bool)rt)
		{
			if (!Application.isPlaying)
			{
				Object.DestroyImmediate(rt);
			}
			else
			{
				Object.Destroy(rt);
			}
		}
		if ((bool)tempCamera)
		{
			if (!Application.isPlaying)
			{
				Object.DestroyImmediate(tempCamera.gameObject);
			}
			else
			{
				Object.Destroy(tempCamera.gameObject);
			}
		}
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if (component != null)
		{
			component.enabled = false;
		}
	}
}
