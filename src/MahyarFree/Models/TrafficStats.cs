using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MahyarFree.Models
{
    public class TrafficStats : INotifyPropertyChanged
    {
        private long _totalDownload;
        private long _totalUpload;
        private double _currentDownloadSpeed;
        private double _currentUploadSpeed;

        public long TotalDownload
        {
            get => _totalDownload;
            set { _totalDownload = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalDownloadDisplay)); }
        }

        public long TotalUpload
        {
            get => _totalUpload;
            set { _totalUpload = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalUploadDisplay)); }
        }

        public double CurrentDownloadSpeed
        {
            get => _currentDownloadSpeed;
            set { _currentDownloadSpeed = value; OnPropertyChanged(); OnPropertyChanged(nameof(DownloadSpeedDisplay)); }
        }

        public double CurrentUploadSpeed
        {
            get => _currentUploadSpeed;
            set { _currentUploadSpeed = value; OnPropertyChanged(); OnPropertyChanged(nameof(UploadSpeedDisplay)); }
        }

        public string TotalDownloadDisplay => FormatBytes(_totalDownload);
        public string TotalUploadDisplay => FormatBytes(_totalUpload);
        public string DownloadSpeedDisplay => $"{FormatBytes((long)_currentDownloadSpeed)}/s";
        public string UploadSpeedDisplay => $"{FormatBytes((long)_currentUploadSpeed)}/s";

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
