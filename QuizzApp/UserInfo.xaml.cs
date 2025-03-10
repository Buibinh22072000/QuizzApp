using System;
using System.Windows;
using System.Windows.Media;

namespace QuizzApp
{
    public partial class UserInfo : Window
    {
        public string UserName { get; private set; }

        public UserInfo()
        {
            InitializeComponent();
        }

        private void RemoveText(object sender, EventArgs e)
        {
            if (NameTextBox.Text == "")
            {
                NameTextBox.Text = "";
                NameTextBox.Foreground = Brushes.Black;
            }
        }

        private void AddText(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                NameTextBox.Text = "";
                NameTextBox.Foreground = Brushes.Gray;
            }
        }

        private void StartExam_Click(object sender, RoutedEventArgs e)
        {
            UserName = NameTextBox.Text;
            if (!string.IsNullOrEmpty(UserName))
            {
                var examWindow = new ExamWindow(UserName);
                examWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Vui lòng nhập họ và tên.");
            }
        }

        private void Score_Click(object sender, RoutedEventArgs e)
        {
            var scoreWindow = new Result(Leaderboard.LoadScores());
            scoreWindow.Show();
        }
    }
}