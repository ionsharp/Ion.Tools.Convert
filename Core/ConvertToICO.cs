using Ion.Analysis;
using Ion.Core;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Ion.Tools.Convert;

/// <summary>The internal format of an icon. Images larger than 256px are always stored as PNG.</summary>
/// <remarks>https://redketchup.io/icon-converter</remarks>
[Serializable]
public enum InternalIconFormat
{
    BMP4, BMP8, BMP24, BMP32, PNG32
}

[Serializable]
public record class IconSize : Model
{
    public bool Size16 { get => Get(true); set => Set(value); }

    public bool Size24 { get => Get(true); set => Set(value); }

    public bool Size32 { get => Get(true); set => Set(value); }

    public bool Size48 { get => Get(true); set => Set(value); }

    public bool Size64 { get => Get(true); set => Set(value); }

    public bool Size96 { get => Get(true); set => Set(value); }

    public bool Size128 { get => Get(true); set => Set(value); }

    public bool Size256 { get => Get(true); set => Set(value); }

    public bool Size512 { get => Get(true); set => Set(value); }

    public List<int> Sizes
    {
        get
        {
            List<int> result = [];
            if (Size16)
                result.Add(16);
            if (Size24)
                result.Add(24);
            if (Size32)
                result.Add(32);
            if (Size48)
                result.Add(48);
            if (Size64)
                result.Add(64);
            if (Size96)
                result.Add(96);
            if (Size128)
                result.Add(128);
            if (Size256)
                result.Add(256);
            if (Size512)
                result.Add(512);

            return result;
        }
    }
}

[Serializable]
public record class ConvertToICO() : ConvertTo()
{
    enum Group { Resize, Sizes }

    public override TargetTypes Type => TargetTypes.ICO;

    public IconSize Sizes { get => Get<IconSize>(); set => Set(value); }

    public CompositingMode CompositingMode { get => Get(CompositingMode.SourceCopy); set => Set(value); }

    public CompositingQuality CompositingQuality { get => Get(CompositingQuality.HighQuality); set => Set(value); }

    public InterpolationMode InterpolationMode { get => Get(InterpolationMode.HighQualityBicubic); set => Set(value); }

    public SmoothingMode SmoothingMode { get => Get(SmoothingMode.HighQuality); set => Set(value); }

    public PixelOffsetMode PixelOffsetMode { get => Get(PixelOffsetMode.HighQuality); set => Set(value); }

    public WrapMode WrapMode { get => Get(WrapMode.TileFlipXY); set => Set(value); }

    ///

    private bool Convert(Bitmap input, Stream output)
    {
        if (input is null)
            return false;

        int[] sizes = [.. Sizes.Sizes];

        List<MemoryStream> imageStreams = [];
        foreach (int size in sizes)
        {
            var newBitmap = ResizeImage(input, size, size);
            if (newBitmap == null)
                return false;

            var memoryStream = new MemoryStream();
            newBitmap.Save(memoryStream, ImageFormat.Png);
            imageStreams.Add(memoryStream);
        }

        BinaryWriter iconWriter = new(output);
        if (output == null || iconWriter == null)
            return false;

        int offset = 0;

        // 0-1 reserved, 0
        iconWriter.Write((byte)0);
        iconWriter.Write((byte)0);

        // 2-3 image type, 1 = icon, 2 = cursor
        iconWriter.Write((short)1);

        // 4-5 number of images
        iconWriter.Write((short)sizes.Length);

        offset += 6 + (16 * sizes.Length);

        for (int i = 0; i < sizes.Length; i++)
        {
            // image entry 1
            // 0 image width
            iconWriter.Write((byte)sizes[i]);
            // 1 image height
            iconWriter.Write((byte)sizes[i]);

            // 2 number of colors
            iconWriter.Write((byte)0);

            // 3 reserved
            iconWriter.Write((byte)0);

            // 4-5 color planes
            iconWriter.Write((short)0);

            // 6-7 bits per pixel
            iconWriter.Write((short)32);

            // 8-11 size of image data
            iconWriter.Write((int)imageStreams[i].Length);

            // 12-15 offset of image data
            iconWriter.Write((int)offset);

            offset += (int)imageStreams[i].Length;
        }
        for (int i = 0; i < sizes.Length; i++)
        {
            // Write image data; PNG data must contain the whole PNG data file
            iconWriter.Write(imageStreams[i].ToArray());
            imageStreams[i].Close();
        }

        iconWriter.Flush();
        return true;
    }

    private bool Convert(Stream input, Stream output)
    {
        Bitmap inputBitmap = (Bitmap)Bitmap.FromStream(input);
        return Convert(inputBitmap, output);
    }

    ///<remarks>https://stackoverflow.com/questions/1922040/resize-an-image-c-sharp</remarks>
    private Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
    {
        var rect = new Rectangle(0, 0, width, height);

        var result = new Bitmap(width, height);
        result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using (var graphics = Graphics.FromImage(result))
        {
            graphics.CompositingMode
                = CompositingMode;
            graphics.CompositingQuality
                = CompositingQuality;
            graphics.InterpolationMode
                = InterpolationMode;
            graphics.SmoothingMode
                = SmoothingMode;
            graphics.PixelOffsetMode
                = PixelOffsetMode;

            using var attributes = new ImageAttributes();
            attributes.SetWrapMode(WrapMode);
            graphics.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
        }

        return result;
    }

    ///

    protected override void OnConstructed()
    {
        base.OnConstructed();
        Sizes ??= new();
    }

    public override bool CanConvertFrom(string extension)
        => extension == "png";

    public override Result Convert(string oldPath, string newPath)
    {
        using FileStream inputStream = new FileStream(oldPath, FileMode.Open, FileAccess.Read);
        using FileStream outputStream = new FileStream(newPath, FileMode.OpenOrCreate);
        return Convert(inputStream, outputStream);
    }
}