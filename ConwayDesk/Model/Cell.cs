using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConwayDesk
{
    public class Cell : INotifyPropertyChanged
    {
        private bool _isActive;
        private bool _isHovered;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FillColor));
                }
            }
        }

        public bool IsHovered
        {
            get { return _isHovered; }
            set
            {
                if (_isHovered != value)
                {
                    _isHovered = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Row { get; set; }
        public int Col { get; set; }
        public string FillColor => IsActive ? ColorConstants.ActiveCell.ToString() : ColorConstants.InactiveCell.ToString();


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
