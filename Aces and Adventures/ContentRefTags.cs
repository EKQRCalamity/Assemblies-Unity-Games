using ProtoBuf;

[ProtoContract]
[UIField]
public class ContentRefTags
{
	[ProtoMember(1)]
	[UIField(view = "UI/Input Field Multiline", max = 128, collapse = UICollapseType.Open)]
	public string tags;

	public ContentRefTags()
	{
	}

	public ContentRefTags(string tags)
	{
		this.tags = tags;
	}

	public static implicit operator string(ContentRefTags cRefTags)
	{
		return cRefTags?.tags;
	}
}
