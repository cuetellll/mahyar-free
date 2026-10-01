using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace MahyarFree.Models
{
    public class ProxyConfig : INotifyPropertyChanged
    {
        private long _ping = -1;
        private bool _isSelected;
        private bool _isTesting;
        private string _name = string.Empty;
        private string _protocol = string.Empty;
        private string _address = string.Empty;
        private int _port;

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string RawUri { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; } = DateTime.Now;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Protocol
        {
            get => _protocol;
            set { _protocol = value; OnPropertyChanged(); }
        }

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        public int Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged(); }
        }

        public long Ping
        {
            get => _ping;
            set { _ping = value; OnPropertyChanged(); OnPropertyChanged(nameof(PingDisplay)); OnPropertyChanged(nameof(PingColor)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public bool IsTesting
        {
            get => _isTesting;
            set { _isTesting = value; OnPropertyChanged(); }
        }

        [JsonIgnore]
        public string PingDisplay => _ping < 0 ? "--" : $"{_ping} ms";

        [JsonIgnore]
        public string PingColor
        {
            get
            {
                if (_ping < 0) return "#6E76A8";
                if (_ping < 100) return "#00E676";
                if (_ping < 250) return "#FFB300";
                return "#FF5252";
            }
        }

        [JsonIgnore]
        public string ProtocolIcon
        {
            get
            {
                return _protocol.ToLower() switch
                {
                    "vmess" => "V",
                    "vless" => "L",
                    "trojan" => "T",
                    "ss" or "shadowsocks" => "S",
                    " hysteria" or "hysteria2" => "H",
                    _ => "?"
                };
            }
        }

        [JsonIgnore]
        public string ProtocolColor
        {
            get
            {
                return _protocol.ToLower() switch
                {
                    "vmess" => "#7C4DFF",
                    "vless" => "#536DFE",
                    "trojan" => "#FF5252",
                    "ss" or "shadowsocks" => "#00E5FF",
                    "hysteria" or "hysteria2" => "#FFB300",
                    _ => "#6E76A8"
                };
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
