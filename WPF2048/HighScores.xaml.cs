using System.Windows;
using System.Windows.Input;

namespace WPF2048
{
    /// <summary>
    /// Interaction logic for HighScores.xaml
    /// </summary>
    public partial class HighScores : Window
    {
        public HighScores(HighScoreData highScore)
        {
            InitializeComponent();

            HighScore = highScore;

            lblDateTime.Text = highScore.DateTime.ToString();
            lblScore.Text = highScore.Score.ToString();

            IsCancelled = false;
            canClose = false;

            txtName.Focus();
            txtName.SelectionStart = 0;
            txtName.SelectionLength = txtName.Text.Length;
        }

        public HighScoreData HighScore { get; set; }
        public bool IsCancelled { get; set; }

        bool canClose;

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void txtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveData();
            }
        }

        void SaveData()
        {
            if (txtName.Text != "")
            {
                HighScore.Name = txtName.Text;
                DataIO.WriteHighScore(HighScore);
                canClose = true;
                Close();
            }
            else
            {
                MessageBox.Show("Your Name cannot be empty.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!canClose)
                if (MessageBoxResult.No == MessageBox.Show("Discard?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Information))
                {
                    e.Cancel = true;
                }
                else
                    IsCancelled = true;
        }
    }
}
