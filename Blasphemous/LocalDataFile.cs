using System;
using System.IO;
using System.Text;
using Framework.Managers;
using FullSerializer;
using Tools;
using UnityEngine;

public class LocalDataFile
{
	private fsSerializer serializer = new fsSerializer();

	public ILocalData Data;

	public LocalDataFile(ILocalData data)
	{
		Data = data;
		LoadData();
	}

	public bool SaveData()
	{
		string saveGameFile = GetSaveGameFile();
		Debug.Log("* Saving file " + saveGameFile);
		fsData data;
		fsResult fsResult = serializer.TrySerialize(Data, out data);
		if (fsResult.Failed)
		{
			Debug.LogError("** Saving file error: " + fsResult.FormattedMessages);
			return false;
		}
		string s = fsJsonPrinter.CompressedJson(data);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		string encryptedData = Convert.ToBase64String(bytes);
		FileTools.SaveSecure(saveGameFile, encryptedData);
		return true;
	}

	public bool LoadData()
	{
		string saveGameFile = GetSaveGameFile();
		Debug.Log("* Loading file " + saveGameFile);
		bool flag = true;
		string input = string.Empty;
		Data.Clean();
		try
		{
			string s = File.ReadAllText(saveGameFile);
			byte[] bytes = Convert.FromBase64String(s);
			input = Encoding.UTF8.GetString(bytes);
		}
		catch (Exception)
		{
			flag = false;
		}
		if (flag)
		{
			fsData data;
			fsResult fsResult = fsJsonParser.Parse(input, out data);
			if (fsResult.Failed)
			{
				Debug.LogError("** Loading file parsing error: " + fsResult.FormattedMessages);
				flag = false;
			}
			else
			{
				try
				{
					fsResult = serializer.TryDeserialize(data, ref Data);
				}
				catch (Exception ex2)
				{
					Debug.LogError("** Loading file deserialization exception: " + ex2.Message);
					flag = false;
				}
				finally
				{
					if (fsResult.Failed)
					{
						Debug.LogError("** Loading file deserialization error: " + fsResult.FormattedMessages);
						flag = false;
					}
				}
			}
		}
		return flag;
	}

	private string GetSaveGameFile()
	{
		return PersistentManager.GetPathAppSettings(Data.GetFileName());
	}
}
