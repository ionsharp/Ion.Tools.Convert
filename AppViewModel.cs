using Ion.Analysis;
using Ion.Core;
using Ion.Input;
using Ion.Storage;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Ion.Tools.Convert;

public record class AppViewModel() : AppToolViewModel()
{
    private readonly List<String> extensions = [ "ico" ];

    private bool isDoing = false;

    /// <see cref="Region.Property"/>

    public Int32 ConvertFrom { get => Get(0); set => Set(value); }

    public Int32 ConvertTo { get => Get(0); set => Set(value); }

    public String FilePathOld { get => Get(""); set => Set(value); }

    public String FilePathNew { get => Get(""); set => Set(value); }

    public String FolderPathNew { get => Get(""); set => Set(value); }

    public Result Result { get => Get(default(Result)); set => Set(value); }

    public override string Title => nameof(Convert);

    ///

    private void SetFilePathNew()
    {
        var filePathNew = "";

        try
        {
            var fileName = $"{System.IO.Path.GetFileNameWithoutExtension(FilePathOld)}.{extensions[(int)ConvertTo]}";

            if (System.IO.Directory.Exists(FolderPathNew))
            {
                filePathNew = System.IO.Path.Combine(FolderPathNew, fileName);
            }
            else
            {
                filePathNew = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FilePathOld), fileName);
            }

            filePathNew = Storage.FilePath.CloneName(filePathNew, Storage.FilePath.DefaultCloneFormat, System.IO.File.Exists);
            FilePathNew = filePathNew;
        }
        catch { FilePathNew = ""; }
    }

    public override void OnSettingProperty(PropertySettingEventArgs e)
    {
        base.OnSettingProperty(e);
        switch (e.PropertyName)
        {
            case nameof(FilePathOld):
            case nameof(FolderPathNew):
                e.NewValue = e.NewValue is null ? null : new string(e.NewValue.ToString().Where(i => !Path.GetInvalidPathChars().Contains(i)).ToArray());
                break;
        }
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(ConvertFrom):
            case nameof(ConvertTo):
            case nameof(FilePathOld):
            case nameof(FolderPathNew):
                SetFilePathNew();
                Result = null;
                break;
        }
    }

    async public void Do()
    {
        if (isDoing) { return; }
        isDoing = true;

        var result = await Dialog.ShowProgress("Converting...", "Converting. Please wait...", i =>
        {
            var doer = new ConvertToICO();
            doer.Convert(FilePathOld, FilePathNew);

        }, TimeSpan.FromSeconds(3), true);

        SetFilePathNew();

        Result = result is Success ? new Success("File converted!") : result;
        isDoing = false;
    }

    public ICommand DoCommand => Commands[nameof(DoCommand)] ??= new RelayCommand(() => Do(), () => true);
}
