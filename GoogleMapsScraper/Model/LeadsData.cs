using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

namespace GoogleMapsScraper.Model
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
            set => SetProperty(ref _name, value);
        }

        public string? Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public string? Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string? Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string? Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string? Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        public string? Domain
        {
            get => _domain;
            set => SetProperty(ref _domain, value);
        }

        public string? Facebook
        {
            get => _facebook;
            set => SetProperty(ref _facebook, value);
        }

        public string? Instagram
        {
            get => _instagram;
            set => SetProperty(ref _instagram, value);
        }

        public string? Tiktok
        {
            get => _tiktok;
            set => SetProperty(ref _tiktok, value);
        }

        public string? Twitter
        {
            get => _twitter;
            set => SetProperty(ref _twitter, value);
        }

        public string? Youtube
        {
            get => _youtube;
            set => SetProperty(ref _youtube, value);
        }

        public string? Rating
        {
            get => _rating;
            set => SetProperty(ref _rating, value);
        }

        public string? Cnpj
        {
            get => _cnpj;
            set => SetProperty(ref _cnpj, value);
        }
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}