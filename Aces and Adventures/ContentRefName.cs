using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
public class ContentRefName
{
	[ProtoMember(1)]
	[UIField(dynamicInitMethod = "_InitName")]
	public string name;

	[ProtoMember(2)]
	[UIField]
	[DefaultValue(32)]
	public byte maxLength = 32;

	public ContentRefName()
	{
	}

	public ContentRefName(string name)
	{
		this.name = name;
	}

	public ContentRefName(string name, byte maxLength)
		: this(name)
	{
		this.maxLength = maxLength;
	}

	public static implicit operator string(ContentRefName cRefName)
	{
		return cRefName?.name;
	}

	private void _InitName(UIFieldAttribute uiField)
	{
		uiField.max = maxLength;
	}
}
