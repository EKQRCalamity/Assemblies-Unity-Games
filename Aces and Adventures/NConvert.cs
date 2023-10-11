using System.IO;

public static class NConvert
{
	private const string NCONVERT = "nconvert";

	public static readonly string[] IMPORT_IMAGE_FORMATS = new string[15]
	{
		".jpg", ".jpeg", ".jp2", ".j2k", ".jpx", ".png", ".psd", ".tiff", ".tif", ".bmp",
		".cpt", ".psp", ".dds", ".tga", ".jfif"
	};

	public const int MAX_IMPORT_RES = 2048;

	public static string Convert(string format, string inputPath, string outputPath = null, int maxResolution = 2048, bool preserveAspect = true, NConvertResizeSide resizeSide = NConvertResizeSide.longest, NConvertResizeType resizeType = NConvertResizeType.lanczos, NConvertResizeFlag flag = NConvertResizeFlag.decr, bool overwrite = true, Int2? maxDimensions = null)
	{
		outputPath = outputPath ?? Path.ChangeExtension(inputPath, "." + format);
		string text = "-out " + format;
		text = text + " -o \"" + outputPath + "\"";
		if (overwrite)
		{
			text += " -overwrite";
		}
		if (maxResolution > 0)
		{
			if (preserveAspect)
			{
				text += " -ratio";
			}
			text = text + " -rtype " + resizeType;
			text = text + " -rflag " + flag;
			text += " -resize ";
			text = (maxDimensions.HasValue ? (text + maxDimensions.Value.x + " " + maxDimensions.Value.y) : ((resizeSide == NConvertResizeSide.both) ? (text + maxResolution + " " + maxResolution) : (text + resizeSide.ToString() + " " + maxResolution)));
		}
		text = text + " \"" + inputPath + "\"";
		ProcessUtil.Run("nconvert", text);
		return outputPath;
	}

	public static void CropResizeAndCanvas(string path, int x, int y, int width, int height, int maxResolution, int canvasX, int canvasY, byte r, byte g, byte b, NConvertCanvasResizeOrigin origin = NConvertCanvasResizeOrigin.center)
	{
		string args = StringUtil.Build(" ", "-out png", "-o \"" + path + "\"", "-overwrite", "-crop", x, y, width, height, "-ratio", "-rtype", NConvertResizeType.lanczos, "-rflag", NConvertResizeFlag.decr, "-resize", NConvertResizeSide.longest, maxResolution, "-canvas", canvasX, canvasY, origin, "-bgcolor", r, g, b, "\"" + path + "\"");
		ProcessUtil.Run("nconvert", args);
	}

	public static void CropAndResize(string inputPath, string outputPath, int x, int y, int width, int height, int maxResolution)
	{
		string args = StringUtil.Build(" ", "-out png", "-o \"" + outputPath + "\"", "-overwrite", "-crop", x, y, width, height, "-ratio", "-rtype", NConvertResizeType.lanczos, "-rflag", NConvertResizeFlag.decr, "-resize", NConvertResizeSide.longest, maxResolution, "\"" + inputPath + "\"");
		ProcessUtil.Run("nconvert", args);
	}

	public static void Resize(string path, int maxResolution)
	{
		string args = StringUtil.Build(" ", "-o \"" + path + "\"", "-overwrite", "-ratio", "-rtype", NConvertResizeType.lanczos, "-rflag", NConvertResizeFlag.decr, "-resize", NConvertResizeSide.longest, maxResolution, "\"" + path + "\"");
		ProcessUtil.Run("nconvert", args);
	}

	public static bool ResizeIfNecessary(string path, int maxResolution)
	{
		bool num = GetImageInfo(path).MaxDimension() > maxResolution;
		if (num)
		{
			Resize(path, maxResolution);
		}
		return num;
	}

	public static ImageInfo GetImageInfo(string filepath)
	{
		return new ImageInfo(ProcessUtil.Run("nconvert", "-info \"" + filepath + "\"", redirectStandardInput: false, redirectStandardOutput: true).standardOutput);
	}
}
