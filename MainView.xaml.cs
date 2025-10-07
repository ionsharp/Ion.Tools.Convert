using Microsoft.Win32;
using System.Windows;

namespace Ion.Tools.Convert;

public partial class MainView : Window
{
    private MainViewModel viewModel = new();

    public MainView()
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void onBrowseFile(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Multiselect = false,
            Title = "Select file",
        };

        if (dialog.ShowDialog() == true)
        {
            string selectedPath = dialog.FileName;
            viewModel.FilePathOld = selectedPath;
        }
    }

    private void onBrowseFolder(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Title = "Select folder",
        };

        if (dialog.ShowDialog() == true)
        {
            string selectedPath = dialog.FolderName;
            viewModel.FolderPathNew = selectedPath;
        }
    }

    private void onDo(object sender, RoutedEventArgs e)
    {
        viewModel.Do();
    }
}