using System;
using UnityEngine;

namespace PygmyMonkey.AdvancedBuilder;

[Serializable]
public class AppParameters : ScriptableObject
{
	private static AppParameters mInstance;

	[SerializeField]
	private string m_releaseType = string.Empty;

	[SerializeField]
	private string m_platformType = string.Empty;

	[SerializeField]
	private string m_distributionPlatform = string.Empty;

	[SerializeField]
	private string m_platformArchitecture = string.Empty;

	[SerializeField]
	private string m_textureCompression = string.Empty;

	[SerializeField]
	private string m_productName = string.Empty;

	[SerializeField]
	private string m_bundleIdentifier = string.Empty;

	[SerializeField]
	private string m_bundleVersion = string.Empty;

	[SerializeField]
	private int m_buildNumber;

	public static AppParameters Get
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = (AppParameters)Resources.Load("AppParameters", typeof(AppParameters));
				if (mInstance == null)
				{
					throw new Exception("We could not find the ScriptableObject AppParameters.asset inside the folder 'PygmyMonkey/AdvancedBuilder/Resources/'");
				}
			}
			return mInstance;
		}
	}

	public string releaseType => m_releaseType;

	public string platformType => m_platformType;

	public string distributionPlatform => m_distributionPlatform;

	public string platformArchitecture => m_platformArchitecture;

	public string textureCompression => m_textureCompression;

	public string productName => m_productName;

	public string bundleIdentifier => m_bundleIdentifier;

	public string bundleVersion => m_bundleVersion;

	public int buildNumber => m_buildNumber;

	public void updateParameters(string releaseType, string platformType, string distributionPlatform, string platformArchitecture, string textureCompression, string productName, string bundleIdentifier, string bundleVersion, int buildNumber)
	{
		m_releaseType = releaseType;
		m_platformType = platformType;
		m_distributionPlatform = distributionPlatform;
		m_platformArchitecture = platformArchitecture;
		m_textureCompression = textureCompression;
		m_productName = productName;
		m_bundleIdentifier = bundleIdentifier;
		m_bundleVersion = bundleVersion;
		m_buildNumber = buildNumber;
	}
}
