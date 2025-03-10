using System.Collections.ObjectModel;
using System.Windows;

namespace QuizzApp
{
    public partial class Result : Window
    {
        private ObservableCollection<UserScore> _userScores;

        public Result(ObservableCollection<UserScore> userScores)
        {
            InitializeComponent();
            _userScores = userScores;
            ResultDataGrid.ItemsSource = _userScores;
        }

        private void OnClearAllClicked(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn xóa tất cả các thành tích không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _userScores.Clear();
                Leaderboard.ClearScores();
                MessageBox.Show("Tất cả các thành tích đã được xóa.");
            }
        }
    }
}