using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Input;

namespace QuizzApp
{
    public partial class ExamWindow : Window
    {
        private ObservableCollection<Question> _questions;
        private ObservableCollection<Question> _answeredQuestions = new ObservableCollection<Question>();
        private ObservableCollection<UserScore> _userScores;
        private int _currentQuestionIndex = 0;
        private DispatcherTimer _timer;
        private DateTime _startTime;
        private string _userName;

        public ExamWindow(string userName)
        {
            InitializeComponent();
            _userName = userName;
            LoadQuestionsFromExcel();
            ShuffleQuestions();
            DisplayCurrentQuestion();
            DataContext = this;
            StartTimer();
            _userScores = Leaderboard.LoadScores();
        }

        public void DisplayLeaderboard()
        {
            var sortedScores = _userScores
                .OrderByDescending(us => us.Score)
                .ThenBy(us => us.TimeTaken)
                .ToList();

            Console.WriteLine("Leaderboard:");
            Console.WriteLine("Rank\tName\tScore\tTime Taken");

            for (int i = 0; i < sortedScores.Count; i++)
            {
                var userScore = sortedScores[i];
                Console.WriteLine($"{i + 1}\t{userScore.UserName}\t{userScore.Score}\t{userScore.TimeTaken}");
            }
        }

        private void LoadQuestionsFromExcel()
        {
            _questions = new ObservableCollection<Question>();
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo("question1.xlsx")))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var questionText = worksheet.Cells[row, 1].Text;
                    var answers = new List<Choice>
                    {
                        new Choice { Text = worksheet.Cells[row, 2].Text ,ChoiceNum = ChoiceNumber.A},
                        new Choice { Text = worksheet.Cells[row, 3].Text ,ChoiceNum = ChoiceNumber.B},
                        new Choice { Text = worksheet.Cells[row, 4].Text ,ChoiceNum = ChoiceNumber.C},
                        new Choice { Text = worksheet.Cells[row, 5].Text ,ChoiceNum = ChoiceNumber.D}
                    };
                    var correctAnswer = worksheet.Cells[row, 6].Text;
                    var explanation = worksheet.Cells[row, 6].Text;

                    _questions.Add(new Question
                    {
                        Text = questionText,
                        Option = answers,
                        CorrectChoice = (ChoiceNumber)Enum.Parse(typeof(ChoiceNumber), correctAnswer),
                        Explanation = explanation
                    });
                }
            }
        }

        private void ShuffleQuestions()
        {
            var random = new Random();
            _questions = new ObservableCollection<Question>(_questions.OrderBy(q => random.Next()));
        }

        private void DisplayCurrentQuestion()
        {
            if (_currentQuestionIndex < _questions.Count)
            {
                var currentQuestion = _questions[_currentQuestionIndex];

                QuestionTextBlock.Inlines.Clear();

                Run boldRun = new Run($"Câu hỏi {_currentQuestionIndex + 1}. ")
                {
                    FontWeight = FontWeights.Bold
                };

                Run normalRun = new Run(currentQuestion.Text);

                QuestionTextBlock.Inlines.Add(boldRun);
                QuestionTextBlock.Inlines.Add(normalRun);

                AnswerTextBlockA.Text = currentQuestion.Option[0].Text;
                AnswerTextBlockB.Text = currentQuestion.Option[1].Text;
                AnswerTextBlockC.Text = currentQuestion.Option[2].Text;
                AnswerTextBlockD.Text = currentQuestion.Option[3].Text;
            }
            else
            {
                MessageBox.Show("You have completed the exam!");
                this.Close();
            }
        }
        private void AnsweredQuestionsListBox_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null) return;

            var selectedQuestion = listBox.SelectedItem as Question;
            if (selectedQuestion == null) return;

            MessageBox.Show($"Giải thích: {selectedQuestion.Explanation}\nĐáp án đúng: {selectedQuestion.CorrectChoice}", "Giải thích và Đáp án đúng");
        }

        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            ChoiceNumber selectedAnswer = SelectedButtonToChoiceNum(button);
            AddToAnsweredQuestionTable(selectedAnswer);
        }

        private void AddToAnsweredQuestionTable(ChoiceNumber selectedAnswer)
        {
            if (_questions == null) { return; }
            var currentQuestion = _questions[_currentQuestionIndex];

            if (!_answeredQuestions.Contains(currentQuestion))
            {
                currentQuestion.SelectedChoice = selectedAnswer;
                currentQuestion.IsCorrectChoice = (selectedAnswer == currentQuestion.CorrectChoice);
                _answeredQuestions.Add(currentQuestion);
            }
        }

        public ChoiceNumber SelectedButtonToChoiceNum(Button button)
        {
            var buttonName = (string)button.Content;
            return (ChoiceNumber)Enum.Parse(typeof(ChoiceNumber), buttonName);
        }

        private void Before_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuestionIndex > 0)
            {
                _currentQuestionIndex--;
                DisplayCurrentQuestion();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            _currentQuestionIndex++;
            if (_currentQuestionIndex < _questions.Count)
            {
                DisplayCurrentQuestion();
            }
            else
            {
                MessageBox.Show("You have reached the end of the questions. Please submit your answers.");
            }
        }

        public void Submit_Click(object sender, RoutedEventArgs e)
        {
            int correctAnswersCount = 0;

            foreach (var question in _answeredQuestions)
            {
                if (question.SelectedChoice == question.CorrectChoice)
                {
                    correctAnswersCount++;
                }
            }

            var elapsedTime = DateTime.Now - _startTime;
            var userScore = new UserScore(_userName, correctAnswersCount, elapsedTime);
            _userScores.Add(userScore);

            Leaderboard.SaveScores(_userScores);

            MessageBox.Show($"You have answered {correctAnswersCount} out of {_questions.Count} questions correctly.");
            DisplayLeaderboard();
            var resultWindow = new Result(_userScores);
            resultWindow.Show();

            this.Close();
        }

        public ObservableCollection<Question> AnsweredQuestions
        {
            get { return _answeredQuestions; }
        }

        private void StartTimer()
        {
            _startTime = DateTime.Now;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var elapsedTime = DateTime.Now - _startTime;
            TimerTextBlock.Text = $"Time: {elapsedTime:hh\\:mm\\:ss}";
        }
    }

    public enum ChoiceNumber
    {
        A,
        B,
        C,
        D,
    }

    public class Question : INotifyPropertyChanged
    {
        private bool _isCorrectChoice;
        public string Text { get; set; }
        public List<Choice> Option { get; set; }
        public ChoiceNumber CorrectChoice { get; set; }
        public ChoiceNumber SelectedChoice { get; set; }
        public string Explanation { get; set; }

        public bool IsCorrectChoice
        {
            get { return _isCorrectChoice; }
            set
            {
                _isCorrectChoice = value;
                OnPropertyChanged(nameof(IsCorrectChoice));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Choice
    {
        public string Text { get; set; }
        public ChoiceNumber ChoiceNum { get; set; }
    }
}

public class UserScore
{
    public string UserName { get; set; }
    public int Score { get; set; }
    public TimeSpan TimeTaken { get; set; }

    public UserScore(string userName, int score, TimeSpan timeTaken)
    {
        UserName = userName;
        Score = score;
        TimeTaken = timeTaken;
    }
}