using Ion.Analysis;
using Ion.Core;

namespace Ion.Tools.Convert;

public abstract record class ConvertTo() : Model()
{
    public abstract TargetTypes Type { get; }

    public abstract bool CanConvertFrom(string extension);

    public abstract Result Convert(string oldPath, string newPath);
}