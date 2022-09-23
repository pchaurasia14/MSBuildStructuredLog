
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using StructuredLogViewer;

namespace Microsoft.Build.Logging.StructuredLogger
{
    public class WelcomeScreen : ObservableObject
    {
        private IEnumerable<string> recentLogs;
        public IEnumerable<string> RecentLogs => recentLogs ?? (recentLogs = SettingsService.GetRecentLogFiles());

        private IEnumerable<string> recentProjects;
        public IEnumerable<string> RecentProjects => recentProjects ?? (recentProjects = SettingsService.GetRecentProjects());

        public bool ShowRecentLogs => RecentLogs.Any();
        public bool ShowRecentProjects => RecentProjects.Any();

        public event Action<string> RecentLogSelected;
        public event Action<string> RecentProjectSelected;
        public event Action OpenProjectRequested;
        public event Action OpenLogFileRequested;
        public event Action<string, string> CompareLogFilesRequested;
        public event Action OpenFile1DialogWindow;
        public event Action OpenFile2DialogWindow;

        private string version = GetVersion();
        public string Version
        {
            get => version;
            set => SetField(ref version, value);
        }

        private string message;
        public string Message
        {
            get => message;
            set => SetField(ref message, value);
        }

        private static string GetVersion()
        {
            return $"Version {ThisAssembly.AssemblyInformationalVersion}";
        }

        private string selectedLog;
        public string SelectedLog
        {
            get => selectedLog;

            set
            {
                if (value == null)
                {
                    return;
                }

                selectedLog = value;

                if (!File.Exists(value))
                {
                    DialogService.ShowMessageBox($"File {value} doesn't exist.");
                    SettingsService.RemoveRecentLogFile(value);
                    recentLogs = null;
                    RaisePropertyChanged(nameof(RecentLogs));
                    RaisePropertyChanged(nameof(ShowRecentLogs));
                    return;
                }

                RecentLogSelected?.Invoke(value);
            }
        }

        private string selectedProject;
        public string SelectedProject
        {
            get => selectedProject;

            set
            {
                if (value == null)
                {
                    return;
                }

                selectedProject = value;

                if (!File.Exists(value))
                {
                    DialogService.ShowMessageBox($"Project {value} doesn't exist.");
                    SettingsService.RemoveRecentProject(value);
                    recentProjects = null;
                    RaisePropertyChanged(nameof(RecentProjects));
                    RaisePropertyChanged(nameof(ShowRecentProjects));
                    return;
                }

                RecentProjectSelected?.Invoke(value);
            }
        }

        private ICommand openProjectCommand;
        public ICommand OpenProjectCommand => openProjectCommand ?? (openProjectCommand = new Command(OpenProject));
        private void OpenProject() => OpenProjectRequested?.Invoke();

        private ICommand openLogFileCommand;
        public ICommand OpenLogFileCommand => openLogFileCommand ?? (openLogFileCommand = new Command(OpenLogFile));
        private void OpenLogFile() => OpenLogFileRequested?.Invoke();


        private ICommand compareLogFilesCommand;
        public ICommand CompareLogFilesCommand => compareLogFilesCommand ?? (compareLogFilesCommand = new Command(CompareLogFiles));
        private void CompareLogFiles() => CompareLogFilesRequested?.Invoke(FileToCompare1, FileToCompare2);

        public bool EnableLogFileComparison => !string.IsNullOrEmpty(FileToCompare1) && !string.IsNullOrEmpty(FileToCompare2);

        private string fileToCompare1 = @"E:\fhl\CalcDemo.xml";
        // @"C:\Users\pchaurasia\Downloads\fhl-logs\fhl\CalcDemo.binlog";
        public string FileToCompare1
        {
            get => fileToCompare1;
            set
            {
                if (!File.Exists(value))
                {
                    DialogService.ShowMessageBox($"File {value} doesn't exist.");
                    fileToCompare1 = null;
                    RaisePropertyChanged(nameof(FileToCompare1));
                    RaisePropertyChanged(nameof(EnableLogFileComparison));
                    return;
                }

                fileToCompare1 = value;
                RaisePropertyChanged(nameof(FileToCompare1));
                RaisePropertyChanged(nameof(EnableLogFileComparison));

            }
        }

        private string fileToCompare2 = @"E:\fhl\CalcDemo-badxaml.xml";
// C:/Users/pchaurasia/Git/dotnet/wpftestartifacts/Tests.Release.x64/FeatureTests/Text/Data/CFFRasterizerP1_1.xml";//@"C:\Users\pchaurasia\Downloads\fhl-logs\fhl\CalcDemo-badsyntax.binlog";
        public string FileToCompare2
        {
            get => fileToCompare2;
            set
            {
                if (!File.Exists(value))
                {
                    DialogService.ShowMessageBox($"File {value} doesn't exist.");

                    fileToCompare2 = null;
                    RaisePropertyChanged(nameof(FileToCompare2));
                    RaisePropertyChanged(nameof(EnableLogFileComparison));

                    return;
                }

                fileToCompare2 = value;
                RaisePropertyChanged(nameof(FileToCompare2));
                RaisePropertyChanged(nameof(EnableLogFileComparison));

            }
        }

        public ICommand setFileToCompare1;

        private ICommand toggleCompareDialog;
        public ICommand ToggleCompareDialogCommand => toggleCompareDialog ?? (toggleCompareDialog = new Command(ToggleCompareDialog));
        private void ToggleCompareDialog() => ShowCompareDialog = !ShowCompareDialog;
        
        private bool showCompareDialog;
        public bool ShowCompareDialog
        {
            get => showCompareDialog;
            set
            {
                showCompareDialog = value;
                RaisePropertyChanged(nameof(ShowCompareDialog));
            }
        }

        private ICommand openFile1DialogCommand;
        public ICommand OpenFile1DialogCommand => openFile1DialogCommand ??= new Command(OpenFile1Dialog);
        private void OpenFile1Dialog() => OpenFile1DialogWindow?.Invoke();

        private ICommand openFile2DialogCommand;
        public ICommand OpenFile2DialogCommand => openFile2DialogCommand ??= new Command(OpenFile2Dialog);
        private void OpenFile2Dialog() => OpenFile2DialogWindow?.Invoke();

        //public event Action<string> OpenFileToCompare;
        //private ICommand openFileXXDialogCommand;
        //public ICommand OpenFileXXDialogCommand => openFileXXDialogCommand ??= new Command<string>(OpenFileDialog);

        //private void OpenFileDialog(string obj) => OpenFileToCompare?.Invoke(obj);

        public bool EnableVirtualization
        {
            get => SettingsService.EnableTreeViewVirtualization;
            set => SettingsService.EnableTreeViewVirtualization = value;
        }

        public bool MarkResultsInTree
        {
            get => SettingsService.MarkResultsInTree;
            set => SettingsService.MarkResultsInTree = value;
        }

        public bool UseDarkTheme
        {
            get => SettingsService.UseDarkTheme;
            set
            {
                SettingsService.UseDarkTheme = value;
                RaisePropertyChanged();
            }
        }
    }
}
