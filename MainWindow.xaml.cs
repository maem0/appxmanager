using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;

namespace AppxManager
{
    public partial class MainWindow : Window
    {
        private string appPath;
        private ObservableCollection<string> logEntries;

        public MainWindow()
        {
            InitializeComponent();
            logEntries = new ObservableCollection<string>();
            LogListBox.ItemsSource = logEntries; 
        }

        private bool isLogVisible = false;

        private void BtnLog_Click(object sender, RoutedEventArgs e)
        {
            isLogVisible = !isLogVisible;
            LogSection.Visibility = isLogVisible ? Visibility.Visible : Visibility.Collapsed;
            LogSplitter.Visibility = isLogVisible ? Visibility.Visible : Visibility.Collapsed;

            if (isLogVisible)
            {
                this.Height += 200;
            }
            else
            {
                this.Height -= 200;
            }
        }


        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "MSIX, MSIXBUNDLE or APPX files (*.msix;*.msixbundle;*.appx)|*.msix;*.appx;*.msixbundle"
            };

            if (dialog.ShowDialog() == true)
            {
                appPath = dialog.FileName;
                AddLog($"Selected file: {appPath}");
            }
        }

        private void Border_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    appPath = files[0];
                    AddLog($"Dragged File: {appPath}");
                }
            }
        }

        private void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(appPath))
            {
                AddLog("Please select or drag a file.");
                return;
            }
            try
            {
                AddLog("Installing...");
                InstallPackage(appPath);
                AddLog("Installation successful.");
            }
            catch (Exception ex)
            {
                AddLog($"Error during installation: {ex.Message}");
            }
        }

        private void InstallPackage(string path)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-Command \"Add-AppxPackage -Path '{path}'\"",
                    Verb = "runas",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception("Failed to install package. Check dependencies or file integrity.");
            }
        }

        private void AddLog(string message)
        {
            txtOutput.Text = message;
            logEntries.Add($"{DateTime.Now:G}: {message}"); 
        }
    }
}
