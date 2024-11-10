using INVApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using INVApp.Services;

namespace INVApp.ViewModels
{
    public class RestorePointViewModel : BaseViewModel
    {
        private readonly DatabaseConfigService _databaseConfigService;
        private const string BackupFolderName = "DatabaseBackups";

        public ObservableCollection<RestorePointItem> RestorePoints { get; set; } = new ObservableCollection<RestorePointItem>();

        public ICommand LoadRestorePointsCommand { get; }
        public ICommand RestoreDatabaseCommand { get; }
        public ICommand DeleteRestorePointCommand { get; }
        public ICommand ExitCommand { get; }

        public RestorePointViewModel(DatabaseConfigService databaseConfigService)
        {
            _databaseConfigService = databaseConfigService;
            LoadRestorePointsCommand = new Command(async () => await LoadRestorePointsAsync());
            RestoreDatabaseCommand = new Command<string>(async (filePath) => await RestoreDatabaseAsync(filePath));
            DeleteRestorePointCommand = new Command<string>(async (filePath) => await DeleteRestorePointAsync(filePath));
            ExitCommand = new Command(async () => await ExitAsync());

        }

        public async Task LoadRestorePointsAsync()
        {
            try
            {
                // Clear current restore points list
                RestorePoints.Clear();

                string backupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), BackupFolderName);

                if (Directory.Exists(backupFolderPath))
                {
                    var backupFiles = Directory.GetFiles(backupFolderPath, "*.db");

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Debug", $"Found {backupFiles.Length} backup files in directory.", "OK");
                    });

                    foreach (var file in backupFiles)
                    {
                        var fileInfo = new FileInfo(file);

                        // Check if the file is already in the RestorePoints collection
                        if (!RestorePoints.Any(rp => rp.FilePath == fileInfo.FullName))
                        {
                            RestorePoints.Add(new RestorePointItem
                            {
                                FileName = fileInfo.Name,
                                FilePath = fileInfo.FullName,
                                Timestamp = fileInfo.CreationTime
                            });
                        }
                        else
                        {
                            await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                await Application.Current.MainPage.DisplayAlert("Debug", $"Duplicate detected: {fileInfo.FullName}", "OK");
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load restore points: {ex.Message}", "OK");
            }
        }

        public async Task RestoreDatabaseAsync(string filePath)
        {
            try
            {
                bool isConfirmed = await Application.Current.MainPage.DisplayAlert("Confirm", "Are you sure you want to restore the database from this backup?", "Yes", "No");

                if (isConfirmed)
                {
                    // Restore the database from the selected file
                    await _databaseConfigService.RestoreDatabaseFromBackupAsync(filePath);
                    await Application.Current.MainPage.DisplayAlert("Success", "Database restored successfully!", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to restore database: {ex.Message}", "OK");
            }
        }

        public async Task DeleteRestorePointAsync(string filePath)
        {
            try
            {
                bool isConfirmed = await Application.Current.MainPage.DisplayAlert("Confirm", "Are you sure you want to delete this restore point?", "Yes", "No");

                if (isConfirmed && File.Exists(filePath))
                {
                    File.Delete(filePath);
                    RestorePoints.Remove(RestorePoints.FirstOrDefault(r => r.FilePath == filePath));
                    await Application.Current.MainPage.DisplayAlert("Success", "Restore point deleted successfully!", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete restore point: {ex.Message}", "OK");
            }

        }

        private async Task ExitAsync()
        {
            await Shell.Current.GoToAsync(".."); // Go back to the previous page (settings page)
        }
    }
}
