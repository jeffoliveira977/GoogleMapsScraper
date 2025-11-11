using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

namespace GoogleMapsScraper
{
    public class LeadsData : INotifyPropertyChanged
    {
        private string? _name;
        private string? _address;
        private string? _phone;
        private string? _email;
        private string? _url;
        private string? _domain;
        private string? _facebook;
        private string? _instagram;
        private string? _twitter;
        private string? _tiktok;
        private string? _youtube;
        private string? _rating;
        private string? _categories;
        private string? _cnpj;

        public string? Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }

        public string? Categories
        {
            get => _categories;
            set { if (_categories != value) { _categories = value; OnPropertyChanged(); } }
        }

        public string? Address
        {
            get => _address;
            set { if (_address != value) { _address = value; OnPropertyChanged(); } }
        }

        public string? Phone
        {
            get => _phone;
            set { if (_phone != value) { _phone = value; OnPropertyChanged(); } }
        }

        public string? Email
        {
            get => _email;
            set { if (_email != value) { _email = value; OnPropertyChanged(); } }
        }

        public string? Url
        {
            get => _url;
            set { if (_url != value) { _url = value; OnPropertyChanged(); } }
        }

        public string? Domain
        {
            get => _domain;
            set { if (_domain != value) { _domain = value; OnPropertyChanged(); } }
        }

        public string? Facebook
        {
            get => _facebook;
            set { if (_facebook != value) { _facebook = value; OnPropertyChanged(); } }
        }

        public string? Instagram
        {
            get => _instagram;
            set { if (_instagram != value) { _instagram = value; OnPropertyChanged(); } }
        }

        public string? Tiktok
        {
            get => _tiktok;
            set { if (_tiktok != value) { _tiktok = value; OnPropertyChanged(); } }
        }

        public string? Twitter
        {
            get => _twitter;
            set { if (_twitter != value) { _twitter = value; OnPropertyChanged(); } }
        }

        public string? Youtube
        {
            get => _youtube;
            set { if (_youtube != value) { _youtube = value; OnPropertyChanged(); } }
        }

        public string? Rating
        {
            get => _rating;
            set { if (_rating != value) { _rating = value; OnPropertyChanged(); } }
        }

        public string? Cnpj
        {
            get => _cnpj;
            set { if (_cnpj != value) { _cnpj = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}