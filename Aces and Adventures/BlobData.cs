using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField]
[ProtoInclude(10, typeof(EBlobData))]
[ProtoInclude(11, typeof(DBlobData))]
public class BlobData : IDataContent
{
	protected const string bytes = "bytes";

	[ProtoMember(1)]
	[UIField(onValueChangedMethod = "_OnEncryptChange")]
	[UIHideIf("_hideEncrypt")]
	protected bool _encrypt;

	[ProtoMember(2)]
	protected byte[] _blobName;

	[ProtoMember(3)]
	protected byte[] _blob;

	[ProtoMember(4)]
	protected byte[] _iv;

	public string tags { get; set; }

	public static string GetBlob<T>(string key) where T : BlobData
	{
		return GetBlob(typeof(T), key);
	}

	public static string GetBlob(Type blobType, string key)
	{
		foreach (ContentRef item in ContentRef.SearchData(blobType))
		{
			if (item.GetDataImmediate() is BlobData blobData && blobData._GetKey() == key)
			{
				return blobData._Decrypt(blobData._blob);
			}
		}
		return null;
	}

	public static string GetBlob(string key)
	{
		return GetBlob<BlobData>(key);
	}

	private string _Decode(byte[] data)
	{
		if (data == null)
		{
			return "";
		}
		return data.ConvertToString().Swizzle();
	}

	private string _Decrypt(byte[] data)
	{
		if (!_encrypt)
		{
			return _Decode(data);
		}
		return data.DecryptToString(_GetBytes(), _iv);
	}

	private string _GetKey()
	{
		return _Decrypt(_blobName);
	}

	private string _GetByteString()
	{
		return GetBlob(GetType(), "bytes");
	}

	private byte[] _GetBytes()
	{
		return _GetByteString().ConvertToBytes();
	}

	public string GetTitle()
	{
		return "Blob";
	}

	public string GetAutomatedDescription()
	{
		return null;
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public void PrepareDataForSave()
	{
	}

	public string GetSaveErrorMessage()
	{
		return null;
	}

	public void OnLoadValidation()
	{
	}
}
