using Ion.Core;
using System.IO;

namespace Ion.Tools.Convert
{
    public record class MainViewModel : Model
    {
        private readonly List<String> Extensions = [ "ico" ];

        public MainViewModel() : base() { }

        public Int32 ConvertFrom { get => Get(0); set => Set(value); }

        public Int32 ConvertTo { get => Get(0); set => Set(value); }

        public String FilePathOld { get => Get(""); set => Set(value); }

        public String FilePathNew { get => Get(""); set => Set(value); }

        public String FolderPathNew { get => Get(""); set => Set(value); }

        ///

        public override void OnSetProperty(PropertySetEventArgs e)
        {
            base.OnSetProperty(e);
            switch (e.PropertyName)
            {
                case nameof(ConvertFrom):
                case nameof(ConvertTo):
                case nameof(FilePathOld):
                case nameof(FolderPathNew):
                    FilePathNew = Path.Combine(FolderPathNew, Path.GetFileNameWithoutExtension(FilePathOld) + "." + Extensions[ConvertTo]);
                    break;
            }
        }

        public void Do()
        {
            var doer = new ConvertToICO();
            doer.Convert(FilePathOld, FilePathNew);
        }
    }
}
