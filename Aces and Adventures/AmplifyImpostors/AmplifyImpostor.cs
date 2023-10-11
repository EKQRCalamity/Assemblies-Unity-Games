using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace AmplifyImpostors;

public class AmplifyImpostor : MonoBehaviour
{
	private const string ShaderGUID = "e82933f4c0eb9ba42aab0739f48efe21";

	private const string DilateGUID = "57c23892d43bc9f458360024c5985405";

	private const string PackerGUID = "31bd3cd74692f384a916d9d7ea87710d";

	private const string ShaderOctaGUID = "572f9be5706148142b8da6e9de53acdb";

	private const string StandardPreset = "e4786beb7716da54dbb02a632681cc37";

	private const string LWPreset = "089f3a2f6b5f48348a48c755f8d9a7a2";

	private const string LWShaderOctaGUID = "94e2ddcdfb3257a43872042f97e2fb01";

	private const string LWShaderGUID = "990451a2073f6994ebf9fd6f90a842b3";

	private const string HDPreset = "47b6b3dcefe0eaf4997acf89caf8c75e";

	private const string HDShaderOctaGUID = "56236dc63ad9b7949b63a27f0ad180b3";

	private const string HDShaderGUID = "175c951fec709c44fa2f26b8ab78b8dd";

	private const string UPreset = "0403878495ffa3c4e9d4bcb3eac9b559";

	private const string UShaderOctaGUID = "83dd8de9a5c14874884f9012def4fdcc";

	private const string UShaderGUID = "da79d698f4bf0164e910ad798d07efdf";

	[SerializeField]
	private AmplifyImpostorAsset m_data;

	[SerializeField]
	private Transform m_rootTransform;

	[SerializeField]
	private LODGroup m_lodGroup;

	[SerializeField]
	private Renderer[] m_renderers;

	public LODReplacement m_lodReplacement = LODReplacement.ReplaceLast;

	[SerializeField]
	public RenderPipelineInUse m_renderPipelineInUse;

	public int m_insertIndex = 1;

	[SerializeField]
	public GameObject m_lastImpostor;

	[SerializeField]
	public string m_folderPath;

	[NonSerialized]
	public string m_impostorName = string.Empty;

	[SerializeField]
	public CutMode m_cutMode;

	[NonSerialized]
	private const float StartXRotation = -90f;

	[NonSerialized]
	private const float StartYRotation = 90f;

	[NonSerialized]
	private const int MinAlphaResolution = 256;

	[NonSerialized]
	private RenderTexture[] m_rtGBuffers;

	[NonSerialized]
	private RenderTexture[] m_alphaGBuffers;

	[NonSerialized]
	private RenderTexture m_trueDepth;

	[NonSerialized]
	public Texture2D m_alphaTex;

	[NonSerialized]
	private float m_xyFitSize;

	[NonSerialized]
	private float m_depthFitSize;

	[NonSerialized]
	private Vector2 m_pixelOffset = Vector2.zero;

	[NonSerialized]
	private Bounds m_originalBound;

	[NonSerialized]
	private Vector3 m_oriPos = Vector3.zero;

	[NonSerialized]
	private Quaternion m_oriRot = Quaternion.identity;

	[NonSerialized]
	private Vector3 m_oriSca = Vector3.one;

	[NonSerialized]
	private const int BlockSize = 65536;

	public AmplifyImpostorAsset Data
	{
		get
		{
			return m_data;
		}
		set
		{
			m_data = value;
		}
	}

	public Transform RootTransform
	{
		get
		{
			return m_rootTransform;
		}
		set
		{
			m_rootTransform = value;
		}
	}

	public LODGroup LodGroup
	{
		get
		{
			return m_lodGroup;
		}
		set
		{
			m_lodGroup = value;
		}
	}

	public Renderer[] Renderers
	{
		get
		{
			return m_renderers;
		}
		set
		{
			m_renderers = value;
		}
	}

	private void GenerateTextures(List<TextureOutput> outputList, bool standardRendering)
	{
		m_rtGBuffers = new RenderTexture[outputList.Count];
		if (standardRendering && m_renderPipelineInUse == RenderPipelineInUse.HD)
		{
			GraphicsFormat format = GraphicsFormat.R8G8B8A8_SRGB;
			GraphicsFormat format2 = GraphicsFormat.R8G8B8A8_UNorm;
			GraphicsFormat format3 = GraphicsFormat.R16G16B16A16_SFloat;
			m_rtGBuffers[0] = new RenderTexture((int)m_data.TexSize.x, (int)m_data.TexSize.y, 16, format);
			m_rtGBuffers[0].Create();
			m_rtGBuffers[1] = new RenderTexture((int)m_data.TexSize.x, (int)m_data.TexSize.y, 16, format2);
			m_rtGBuffers[1].Create();
			m_rtGBuffers[2] = new RenderTexture((int)m_data.TexSize.x, (int)m_data.TexSize.y, 16, format2);
			m_rtGBuffers[2].Create();
			m_rtGBuffers[3] = new RenderTexture((int)m_data.TexSize.x, (int)m_data.TexSize.y, 16, format3);
			m_rtGBuffers[3].Create();
			m_rtGBuffers[4] = new RenderTexture((int)m_data.TexSize.x, (int)m_data.TexSize.y, 16, format2);
			m_rtGBuffers[4].Create();
		}
		else
		{
			for (int i = 0; i < m_rtGBuffers.Length; i++)
			{
				m_rtGBuffers[i] = new RenderTexture((int)m_data.TexSize.x, (int)m_data.TexSize.y, 16, (!outputList[i].SRGB) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
				m_rtGBuffers[i].Create();
			}
		}
		m_trueDepth = new RenderTexture((int)m_data.TexSize.x, (int)m_data.TexSize.y, 16, RenderTextureFormat.Depth);
		m_trueDepth.Create();
	}

	private void GenerateAlphaTextures(List<TextureOutput> outputList)
	{
		m_alphaGBuffers = new RenderTexture[outputList.Count];
		for (int i = 0; i < m_alphaGBuffers.Length; i++)
		{
			m_alphaGBuffers[i] = new RenderTexture(256, 256, 16, (!outputList[i].SRGB) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
			m_alphaGBuffers[i].Create();
		}
		m_trueDepth = new RenderTexture(256, 256, 16, RenderTextureFormat.Depth);
		m_trueDepth.Create();
	}

	private void ClearBuffers()
	{
		RenderTexture.active = null;
		RenderTexture[] rtGBuffers = m_rtGBuffers;
		for (int i = 0; i < rtGBuffers.Length; i++)
		{
			rtGBuffers[i].Release();
		}
		m_rtGBuffers = null;
	}

	private void ClearAlphaBuffers()
	{
		RenderTexture.active = null;
		RenderTexture[] alphaGBuffers = m_alphaGBuffers;
		for (int i = 0; i < alphaGBuffers.Length; i++)
		{
			alphaGBuffers[i].Release();
		}
		m_alphaGBuffers = null;
	}

	public void RenderImpostor(ImpostorType impostorType, int targetAmount, bool impostorMaps = true, bool combinedAlphas = false, bool useMinResolution = false, Shader customShader = null)
	{
		if ((!impostorMaps && !combinedAlphas) || targetAmount <= 0)
		{
			return;
		}
		bool flag = customShader == null;
		Dictionary<Material, Material> dictionary = new Dictionary<Material, Material>();
		CommandBuffer commandBuffer = new CommandBuffer();
		if (impostorMaps)
		{
			commandBuffer.name = "GBufferCatcher";
			RenderTargetIdentifier[] array = new RenderTargetIdentifier[targetAmount];
			for (int i = 0; i < targetAmount; i++)
			{
				array[i] = m_rtGBuffers[i];
			}
			commandBuffer.SetRenderTarget(array, m_trueDepth);
			commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear, 1f);
		}
		CommandBuffer commandBuffer2 = new CommandBuffer();
		if (combinedAlphas)
		{
			commandBuffer2.name = "DepthAlphaCatcher";
			RenderTargetIdentifier[] array2 = new RenderTargetIdentifier[targetAmount];
			for (int j = 0; j < targetAmount; j++)
			{
				array2[j] = m_alphaGBuffers[j];
			}
			commandBuffer2.SetRenderTarget(array2, m_trueDepth);
			commandBuffer2.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear, 1f);
		}
		int horizontalFrames = m_data.HorizontalFrames;
		int num = m_data.HorizontalFrames;
		if (impostorType == ImpostorType.Spherical)
		{
			num = m_data.HorizontalFrames - 1;
			if (m_data.DecoupleAxisFrames)
			{
				num = m_data.VerticalFrames - 1;
			}
		}
		List<MeshFilter> list = new List<MeshFilter>();
		for (int k = 0; k < Renderers.Length; k++)
		{
			if (Renderers[k] == null || !Renderers[k].enabled || Renderers[k].shadowCastingMode == ShadowCastingMode.ShadowsOnly)
			{
				list.Add(null);
				continue;
			}
			MeshFilter component = Renderers[k].GetComponent<MeshFilter>();
			if (component == null || component.sharedMesh == null)
			{
				list.Add(null);
			}
			else
			{
				list.Add(component);
			}
		}
		int count = list.Count;
		for (int l = 0; l < horizontalFrames; l++)
		{
			for (int m = 0; m <= num; m++)
			{
				Bounds bounds = default(Bounds);
				Matrix4x4 cameraRotationMatrix = GetCameraRotationMatrix(impostorType, horizontalFrames, num, l, m);
				for (int n = 0; n < count; n++)
				{
					if (!(list[n] == null))
					{
						if (bounds.size == Vector3.zero)
						{
							bounds = list[n].sharedMesh.bounds.Transform(m_rootTransform.worldToLocalMatrix * Renderers[n].localToWorldMatrix);
						}
						else
						{
							bounds.Encapsulate(list[n].sharedMesh.bounds.Transform(m_rootTransform.worldToLocalMatrix * Renderers[n].localToWorldMatrix));
						}
					}
				}
				if (l == 0 && m == 0)
				{
					m_originalBound = bounds;
				}
				bounds = bounds.Transform(cameraRotationMatrix);
				Matrix4x4 matrix4x = cameraRotationMatrix.inverse * Matrix4x4.LookAt(bounds.center - new Vector3(0f, 0f, m_depthFitSize * 0.5f), bounds.center, Vector3.up);
				float num2 = m_xyFitSize * 0.5f;
				Matrix4x4 matrix4x2 = Matrix4x4.Ortho(0f - num2 + m_pixelOffset.x, num2 + m_pixelOffset.x, 0f - num2 + m_pixelOffset.y, num2 + m_pixelOffset.y, 0f, 0f - m_depthFitSize);
				matrix4x = matrix4x.inverse * m_rootTransform.worldToLocalMatrix;
				if (flag && m_renderPipelineInUse == RenderPipelineInUse.HD)
				{
					matrix4x2 = GL.GetGPUProjectionMatrix(matrix4x2, renderIntoTexture: true);
				}
				if (impostorMaps)
				{
					commandBuffer.SetViewProjectionMatrices(matrix4x, matrix4x2);
					commandBuffer.SetViewport(new Rect(m_data.TexSize.x / (float)horizontalFrames * (float)l, m_data.TexSize.y / (float)(num + ((impostorType == ImpostorType.Spherical) ? 1 : 0)) * (float)m, m_data.TexSize.x / (float)m_data.HorizontalFrames, m_data.TexSize.y / (float)m_data.VerticalFrames));
					if (flag && m_renderPipelineInUse == RenderPipelineInUse.HD)
					{
						BakeHDRPTool.SetupShaderVariableGlobals(matrix4x, matrix4x2, commandBuffer);
						commandBuffer.SetGlobalMatrix("_ViewMatrix", matrix4x);
						commandBuffer.SetGlobalMatrix("_InvViewMatrix", matrix4x.inverse);
						commandBuffer.SetGlobalMatrix("_ProjMatrix", matrix4x2);
						commandBuffer.SetGlobalMatrix("_ViewProjMatrix", matrix4x2 * matrix4x);
						commandBuffer.SetGlobalVector("_WorldSpaceCameraPos", Vector4.zero);
					}
				}
				if (combinedAlphas)
				{
					commandBuffer2.SetViewProjectionMatrices(matrix4x, matrix4x2);
					commandBuffer2.SetViewport(new Rect(0f, 0f, 256f, 256f));
					if (flag && m_renderPipelineInUse == RenderPipelineInUse.HD)
					{
						BakeHDRPTool.SetupShaderVariableGlobals(matrix4x, matrix4x2, commandBuffer2);
						commandBuffer2.SetGlobalMatrix("_ViewMatrix", matrix4x);
						commandBuffer2.SetGlobalMatrix("_InvViewMatrix", matrix4x.inverse);
						commandBuffer2.SetGlobalMatrix("_ProjMatrix", matrix4x2);
						commandBuffer2.SetGlobalMatrix("_ViewProjMatrix", matrix4x2 * matrix4x);
						commandBuffer2.SetGlobalVector("_WorldSpaceCameraPos", Vector4.zero);
					}
				}
				for (int num3 = 0; num3 < count; num3++)
				{
					if (list[num3] == null)
					{
						continue;
					}
					Material[] sharedMaterials = Renderers[num3].sharedMaterials;
					for (int num4 = 0; num4 < sharedMaterials.Length; num4++)
					{
						Material value = null;
						_ = list[num3].sharedMesh;
						int num5 = 0;
						int num6 = 0;
						if (flag)
						{
							value = sharedMaterials[num4];
							num5 = value.FindPass("DEFERRED");
							if (num5 == -1)
							{
								num5 = value.FindPass("Deferred");
							}
							if (num5 == -1)
							{
								num5 = value.FindPass("GBuffer");
							}
							num6 = value.FindPass("DepthOnly");
							if (num5 == -1)
							{
								num5 = 0;
								for (int num7 = 0; num7 < value.passCount; num7++)
								{
									if (value.GetTag("LightMode", searchFallbacks: true).Equals("Deferred"))
									{
										num5 = num7;
										break;
									}
								}
							}
							commandBuffer.EnableShaderKeyword("UNITY_HDR_ON");
						}
						else
						{
							num6 = -1;
							if (!dictionary.TryGetValue(sharedMaterials[num4], out value))
							{
								value = new Material(customShader)
								{
									hideFlags = HideFlags.HideAndDontSave
								};
								dictionary.Add(sharedMaterials[num4], value);
							}
						}
						bool flag2 = Renderers[num3].lightmapIndex > -1;
						bool flag3 = Renderers[num3].realtimeLightmapIndex > -1;
						if ((flag2 || flag3) && !flag)
						{
							commandBuffer.EnableShaderKeyword("LIGHTMAP_ON");
							if (flag2)
							{
								commandBuffer.SetGlobalVector("unity_LightmapST", Renderers[num3].lightmapScaleOffset);
							}
							if (flag3)
							{
								commandBuffer.EnableShaderKeyword("DYNAMICLIGHTMAP_ON");
								commandBuffer.SetGlobalVector("unity_DynamicLightmapST", Renderers[num3].realtimeLightmapScaleOffset);
							}
							else
							{
								commandBuffer.DisableShaderKeyword("DYNAMICLIGHTMAP_ON");
							}
							if (flag2 && flag3)
							{
								commandBuffer.EnableShaderKeyword("DIRLIGHTMAP_COMBINED");
							}
							else
							{
								commandBuffer.DisableShaderKeyword("DIRLIGHTMAP_COMBINED");
							}
						}
						else
						{
							commandBuffer.DisableShaderKeyword("LIGHTMAP_ON");
							commandBuffer.DisableShaderKeyword("DYNAMICLIGHTMAP_ON");
							commandBuffer.DisableShaderKeyword("DIRLIGHTMAP_COMBINED");
						}
						commandBuffer.DisableShaderKeyword("LIGHTPROBE_SH");
						if (impostorMaps)
						{
							if (num6 > -1)
							{
								commandBuffer.DrawRenderer(Renderers[num3], value, num4, num6);
							}
							commandBuffer.DrawRenderer(Renderers[num3], value, num4, num5);
						}
						if (combinedAlphas)
						{
							if (num6 > -1)
							{
								commandBuffer2.DrawRenderer(Renderers[num3], value, num4, num6);
							}
							commandBuffer2.DrawRenderer(Renderers[num3], value, num4, num5);
						}
					}
				}
				if (impostorMaps)
				{
					Graphics.ExecuteCommandBuffer(commandBuffer);
				}
				if (combinedAlphas)
				{
					Graphics.ExecuteCommandBuffer(commandBuffer2);
				}
			}
		}
		list.Clear();
		foreach (KeyValuePair<Material, Material> item in dictionary)
		{
			Material value2 = item.Value;
			if (value2 != null)
			{
				if (!Application.isPlaying)
				{
					UnityEngine.Object.DestroyImmediate(value2);
				}
				value2 = null;
			}
		}
		dictionary.Clear();
		commandBuffer.Release();
		commandBuffer = null;
		commandBuffer2.Release();
		commandBuffer2 = null;
	}

	private Matrix4x4 GetCameraRotationMatrix(ImpostorType impostorType, int hframes, int vframes, int x, int y)
	{
		Matrix4x4 result = Matrix4x4.identity;
		switch (impostorType)
		{
		case ImpostorType.Spherical:
		{
			float num = 0f;
			if (vframes > 0)
			{
				num = 0f - 180f / (float)vframes;
			}
			Quaternion quaternion = Quaternion.Euler(num * (float)y + 90f, 0f, 0f);
			Quaternion quaternion2 = Quaternion.Euler(0f, 360f / (float)hframes * (float)x + -90f, 0f);
			result = Matrix4x4.Rotate(quaternion * quaternion2);
			break;
		}
		case ImpostorType.Octahedron:
		{
			Vector3 vector2 = OctahedronToVector((float)x / ((float)hframes - 1f) * 2f - 1f, (float)y / ((float)vframes - 1f) * 2f - 1f);
			result = Matrix4x4.Rotate(Quaternion.LookRotation(new Vector3(vector2.x * -1f, vector2.z * -1f, vector2.y * -1f), Vector3.up)).inverse;
			break;
		}
		case ImpostorType.HemiOctahedron:
		{
			Vector3 vector = HemiOctahedronToVector((float)x / ((float)hframes - 1f) * 2f - 1f, (float)y / ((float)vframes - 1f) * 2f - 1f);
			result = Matrix4x4.Rotate(Quaternion.LookRotation(new Vector3(vector.x * -1f, vector.z * -1f, vector.y * -1f), Vector3.up)).inverse;
			break;
		}
		}
		return result;
	}

	private Vector3 OctahedronToVector(Vector2 oct)
	{
		Vector3 value = new Vector3(oct.x, oct.y, 1f - Mathf.Abs(oct.x) - Mathf.Abs(oct.y));
		float num = Mathf.Clamp01(0f - value.z);
		value.Set(value.x + ((value.x >= 0f) ? (0f - num) : num), value.y + ((value.y >= 0f) ? (0f - num) : num), value.z);
		return Vector3.Normalize(value);
	}

	private Vector3 OctahedronToVector(float x, float y)
	{
		Vector3 value = new Vector3(x, y, 1f - Mathf.Abs(x) - Mathf.Abs(y));
		float num = Mathf.Clamp01(0f - value.z);
		value.Set(value.x + ((value.x >= 0f) ? (0f - num) : num), value.y + ((value.y >= 0f) ? (0f - num) : num), value.z);
		return Vector3.Normalize(value);
	}

	private Vector3 HemiOctahedronToVector(float x, float y)
	{
		float num = x;
		float num2 = y;
		x = (num + num2) * 0.5f;
		y = (num - num2) * 0.5f;
		return Vector3.Normalize(new Vector3(x, y, 1f - Mathf.Abs(x) - Mathf.Abs(y)));
	}

	public void GenerateAutomaticMesh(AmplifyImpostorAsset data)
	{
		SpriteUtilityEx.GenerateOutline(rect: new Rect(0f, 0f, m_alphaTex.width, m_alphaTex.height), texture: m_alphaTex, detail: data.Tolerance, alphaTolerance: 254, holeDetection: false, paths: out var paths);
		int num = 0;
		for (int i = 0; i < paths.Length; i++)
		{
			num += paths[i].Length;
		}
		data.ShapePoints = new Vector2[num];
		int num2 = 0;
		for (int j = 0; j < paths.Length; j++)
		{
			for (int k = 0; k < paths[j].Length; k++)
			{
				data.ShapePoints[num2] = paths[j][k] + new Vector2((float)m_alphaTex.width * 0.5f, (float)m_alphaTex.height * 0.5f);
				data.ShapePoints[num2] = Vector2.Scale(data.ShapePoints[num2], new Vector2(1f / (float)m_alphaTex.width, 1f / (float)m_alphaTex.height));
				num2++;
			}
		}
		data.ShapePoints = Vector2Ex.ConvexHull(data.ShapePoints);
		data.ShapePoints = Vector2Ex.ReduceVertices(data.ShapePoints, data.MaxVertices);
		data.ShapePoints = Vector2Ex.ScaleAlongNormals(data.ShapePoints, data.NormalScale);
		for (int l = 0; l < data.ShapePoints.Length; l++)
		{
			data.ShapePoints[l].x = Mathf.Clamp01(data.ShapePoints[l].x);
			data.ShapePoints[l].y = Mathf.Clamp01(data.ShapePoints[l].y);
		}
		data.ShapePoints = Vector2Ex.ConvexHull(data.ShapePoints);
		for (int m = 0; m < data.ShapePoints.Length; m++)
		{
			data.ShapePoints[m] = new Vector2(data.ShapePoints[m].x, 1f - data.ShapePoints[m].y);
		}
	}

	public Mesh GenerateMesh(Vector2[] points, Vector3 offset, float width = 1f, float height = 1f, bool invertY = true)
	{
		Vector2[] array = new Vector2[points.Length];
		Vector2[] array2 = new Vector2[points.Length];
		Array.Copy(points, array, points.Length);
		float num = width * 0.5f;
		float num2 = height * 0.5f;
		if (invertY)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Vector2(array[i].x, 1f - array[i].y);
			}
		}
		Array.Copy(array, array2, array.Length);
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = new Vector2(array[j].x * width - num + m_pixelOffset.x, array[j].y * height - num2 + m_pixelOffset.y);
		}
		Triangulator triangulator = new Triangulator(array);
		int[] triangles = triangulator.Triangulate();
		Vector3[] array3 = new Vector3[triangulator.Points.Count];
		for (int k = 0; k < array3.Length; k++)
		{
			array3[k] = new Vector3(triangulator.Points[k].x, triangulator.Points[k].y, 0f);
		}
		Mesh mesh = new Mesh();
		mesh.vertices = array3;
		mesh.uv = array2;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		mesh.bounds = new Bounds(offset, m_originalBound.size);
		return mesh;
	}
}
