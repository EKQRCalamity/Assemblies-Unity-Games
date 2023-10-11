using ProtoBuf;

[ProtoContract]
public class ImageRefUVCoords
{
	[ProtoMember(1)]
	public UVCoords? uvCoords;

	public ImageRef imageRef { get; set; }
}
