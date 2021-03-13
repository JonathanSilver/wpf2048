using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading;

namespace WPF2048
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    AddElement(0, i, j);
                }
            }

            b.Completed += b_Completed;

            s.Completed += s_Completed;

            timer.Interval = TimeSpan.FromSeconds(0.4);
            timer.Tick += timer_Tick;
        }

        HighScoreData HighScore = null;
        HighScores HighScoresWindow = null;
        bool CanShowHighScoreWindow;

        Element[,] list = new Element[4, 4];
        long score;
        long Score { get { return score; } set { score = value; tbScore.Text = score.ToString(); } }
        int status;

        Random r = new Random();
        int[] number = new int[] { 2, 4 };

        Storyboard s = new Storyboard();
        Storyboard b = new Storyboard();

        long up;
        long down;
        long left;
        long right;
        long UpMove { get { return up; } set { up = value; tbUp.Text = value.ToString(); } }
        long DownMove { get { return down; } set { down = value; tbDown.Text = value.ToString(); } }
        long LeftMove { get { return left; } set { left = value; tbLeft.Text = value.ToString(); } }
        long RightMove { get { return right; } set { right = value; tbRight.Text = value.ToString(); } }

        int currentMove;
        bool isAutoPlay;
        bool IsAutoPlay { get { return isAutoPlay; } set { isAutoPlay = value; if (isAutoPlay) { lblAutoPlay.Text = "STOP PLAY"; } else { lblAutoPlay.Text = "AUTO PLAY"; } } }

        DispatcherTimer timer = new DispatcherTimer();

        public void NewGame()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] != null)
                    {
                        board.Children.Remove(list[i, j]);
                    }
                    list[i, j] = null;
                }
            }
            GenerateNewNumber();
            GenerateNewNumber();
            Score = 0;
            status = 0;
            lblInfo.Visibility = Visibility.Hidden;
            CanShowHighScoreWindow = true;

            UpMove = 0;
            DownMove = 0;
            LeftMove = 0;
            RightMove = 0;

            IsAutoPlay = false;
            timer.Stop();
        }

        public void AddElement(int value, int x, int y)
        {
            Element ele = new Element();
            ele.Value = value;
            ele.SetValue(Canvas.LeftProperty, (double)(x * 50 + 2));
            ele.SetValue(Canvas.TopProperty, (double)(y * 50 + 2));
            board.Children.Add(ele);
        }

        private int NewNumber()
        {
            return number[r.Next(0, 2)];
        }

        public void GenerateNewNumber()
        {
            int x = 0, y = 0;
            do
            {
                x = r.Next(0, 4);
                y = r.Next(0, 4);
            }
            while (list[x, y] != null);
            list[x, y] = new Element() { Value = NewNumber() };
            list[x, y].Height = 0;
            list[x, y].Width = 0;
            list[x, y].SetValue(Canvas.LeftProperty, (double)(x * 50 + 2));
            list[x, y].SetValue(Canvas.TopProperty, (double)(y * 50 + 2));
            board.Children.Add(list[x, y]);

            DoubleAnimation dh = new DoubleAnimation();
            dh.To = 46;
            dh.Duration = TimeSpan.FromSeconds(0.3);
            Storyboard.SetTarget(dh, list[x, y]);
            Storyboard.SetTargetProperty(dh, new PropertyPath(Element.HeightProperty));
            s.Children.Add(dh);
            DoubleAnimation dw = new DoubleAnimation();
            dw.To = 46;
            dw.Duration = TimeSpan.FromSeconds(0.3);
            Storyboard.SetTarget(dw, list[x, y]);
            Storyboard.SetTargetProperty(dw, new PropertyPath(Element.WidthProperty));
            s.Children.Add(dw);
            status = 1;
            s.Begin();
        }

        private bool IsRowEmpty(int y)
        {
            for (int i = 0; i < 4; i++)
            {
                if (list[i, y] != null)
                    return false;
            }
            return true;
        }

        private bool IsColumnEmpty(int x)
        {
            for (int j = 0; j < 4; j++)
            {
                if (list[x, j] != null)
                    return false;
            }
            return true;
        }

        private bool IsFull()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] == null)
                        return false;
                }
            }
            return true;
        }

        public bool CanMove()
        {
            return CanMoveLeftRight() || CanMoveUpDown();
        }

        public bool CanMoveLeftRight()
        {
            if (IsFull())
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (list[i, j].Value == list[i + 1, j].Value)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            else
                return true;
        }

        public bool CanMoveUpDown()
        {
            if (IsFull())
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (list[i, j].Value == list[i, j + 1].Value)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            else
                return true;
        }

        public bool Win()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] != null && list[i, j].Value == 2048)
                        return true;
                }
            }
            return false;
        }

        public void MoveLeft()
        {
            for (int j = 0; j < 4; j++)
                if (!IsRowEmpty(j))
                    for (int i = 0; i < 4; i++)
                    {
                        int k = -1;
                        for (int l = i + 1; l < 4; l++)
                            if (list[l, j] != null)
                            {
                                k = l;
                                break;
                            }
                        if (k > i)
                            if (list[i, j] == null)
                                if (i == 0)
                                {
                                    list[i, j] = list[k, j];
                                    list[k, j] = null;
                                    DoubleAnimation d = new DoubleAnimation();
                                    d.To = i * 50 + 2;
                                    d.Duration = TimeSpan.FromSeconds(0.3);
                                    Storyboard.SetTarget(d, list[i, j]);
                                    Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.LeftProperty));
                                    s.Children.Add(d);
                                }
                                else
                                    if (list[i - 1, j].Value == list[k, j].Value && list[i - 1, j].Element2 == null)
                                    {
                                        list[i - 1, j].Element2 = list[k, j];
                                        list[k, j] = null;
                                        DoubleAnimation d = new DoubleAnimation();
                                        d.To = (i - 1) * 50 + 2;
                                        d.Duration = TimeSpan.FromSeconds(0.3);
                                        Storyboard.SetTarget(d, list[i - 1, j].Element2);
                                        Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.LeftProperty));
                                        s.Children.Add(d);
                                        i--;
                                    }
                                    else
                                    {
                                        list[i, j] = list[k, j];
                                        list[k, j] = null;
                                        DoubleAnimation d = new DoubleAnimation();
                                        d.To = i * 50 + 2;
                                        d.Duration = TimeSpan.FromSeconds(0.3);
                                        Storyboard.SetTarget(d, list[i, j]);
                                        Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.LeftProperty));
                                        s.Children.Add(d);
                                    }
                            else
                            {
                                if (list[i, j].Value == list[k, j].Value)
                                {
                                    list[i, j].Element2 = list[k, j];
                                    list[k, j] = null;
                                    DoubleAnimation d = new DoubleAnimation();
                                    d.To = i * 50 + 2;
                                    d.Duration = TimeSpan.FromSeconds(0.3);
                                    Storyboard.SetTarget(d, list[i, j].Element2);
                                    Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.LeftProperty));
                                    s.Children.Add(d);
                                }
                            }
                        else
                            break;
                    }
            status = 1;
            s.Begin();
            LeftMove++;
        }

        public void MoveUp()
        {
            for (int i = 0; i < 4; i++)
                if (!IsColumnEmpty(i))
                    for (int j = 0; j < 4; j++)
                    {
                        int k = -1;
                        for (int l = j + 1; l < 4; l++)
                            if (list[i, l] != null)
                            {
                                k = l;
                                break;
                            }
                        if (k > j)
                            if (list[i, j] == null)
                            {
                                if (j == 0)
                                {
                                    list[i, j] = list[i, k];
                                    list[i, k] = null;
                                    DoubleAnimation d = new DoubleAnimation();
                                    d.To = j * 50 + 2;
                                    d.Duration = TimeSpan.FromSeconds(0.3);
                                    Storyboard.SetTarget(d, list[i, j]);
                                    Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.TopProperty));
                                    s.Children.Add(d);
                                }
                                else
                                    if (list[i, j - 1].Value == list[i, k].Value && list[i, j - 1].Element2 == null)
                                    {
                                        list[i, j - 1].Element2 = list[i, k];
                                        list[i, k] = null;
                                        DoubleAnimation d = new DoubleAnimation();
                                        d.To = (j - 1) * 50 + 2;
                                        d.Duration = TimeSpan.FromSeconds(0.3);
                                        Storyboard.SetTarget(d, list[i, j - 1].Element2);
                                        Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.TopProperty));
                                        s.Children.Add(d);
                                        j--;
                                    }
                                    else
                                    {
                                        list[i, j] = list[i, k];
                                        list[i, k] = null;
                                        DoubleAnimation d = new DoubleAnimation();
                                        d.To = j * 50 + 2;
                                        d.Duration = TimeSpan.FromSeconds(0.3);
                                        Storyboard.SetTarget(d, list[i, j]);
                                        Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.TopProperty));
                                        s.Children.Add(d);
                                    }
                            }
                            else
                            {
                                if (list[i, j].Value == list[i, k].Value)
                                {
                                    list[i, j].Element2 = list[i, k];
                                    list[i, k] = null;
                                    DoubleAnimation d = new DoubleAnimation();
                                    d.To = j * 50 + 2;
                                    d.Duration = TimeSpan.FromSeconds(0.3);
                                    Storyboard.SetTarget(d, list[i, j].Element2);
                                    Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.TopProperty));
                                    s.Children.Add(d);
                                }
                            }
                        else
                            break;
                    }
            status = 1;
            s.Begin();
            UpMove++;
        }

        public void MoveRight()
        {
            for (int j = 0; j < 4; j++)
                if (!IsRowEmpty(j))
                    for (int i = 3; i >= 0; i--)
                    {
                        int k = -1;
                        for (int l = i - 1; l >= 0; l--)
                            if (list[l, j] != null)
                            {
                                k = l;
                                break;
                            }
                        if (k >= 0 && k < i)
                            if (list[i, j] == null)
                            {
                                if (i == 3)
                                {
                                    list[i, j] = list[k, j];
                                    list[k, j] = null;
                                    DoubleAnimation d = new DoubleAnimation();
                                    d.To = i * 50 + 2;
                                    d.Duration = TimeSpan.FromSeconds(0.3);
                                    Storyboard.SetTarget(d, list[i, j]);
                                    Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.LeftProperty));
                                    s.Children.Add(d);
                                }
                                else
                                {
                                    if (list[i + 1, j].Value == list[k, j].Value && list[i + 1, j].Element2 == null)
                                    {
                                        list[i + 1, j].Element2 = list[k, j];
                                        list[k, j] = null;
                                        DoubleAnimation d = new DoubleAnimation();
                                        d.To = (i + 1) * 50 + 2;
                                        d.Duration = TimeSpan.FromSeconds(0.3);
                                        Storyboard.SetTarget(d, list[i + 1, j].Element2);
                                        Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.LeftProperty));
                                        s.Children.Add(d);
                                        i++;
                                    }
                                    else
                                    {
                                        list[i, j] = list[k, j];
                                        list[k, j] = null;
                                        DoubleAnimation d = new DoubleAnimation();
                                        d.To = i * 50 + 2;
                                        d.Duration = TimeSpan.FromSeconds(0.3);
                                        Storyboard.SetTarget(d, list[i, j]);
                                        Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.LeftProperty));
                                        s.Children.Add(d);
                                    }
                                }
                            }
                            else
                            {
                                if (list[i, j].Value == list[k, j].Value)
                                {
                                    list[i, j].Element2 = list[k, j];
                                    list[k, j] = null;
                                    DoubleAnimation d = new DoubleAnimation();
                                    d.To = i * 50 + 2;
                                    d.Duration = TimeSpan.FromSeconds(0.3);
                                    Storyboard.SetTarget(d, list[i, j].Element2);
                                    Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.LeftProperty));
                                    s.Children.Add(d);
                                }
                            }
                        else
                            break;
                    }
            status = 1;
            s.Begin();
            RightMove++;
        }

        public void MoveDown()
        {
            for (int i = 0; i < 4; i++)
                if (!IsColumnEmpty(i))
                    for (int j = 3; j >= 0; j--)
                    {
                        int k = -1;
                        for (int l = j - 1; l >= 0; l--)
                            if (list[i, l] != null)
                            {
                                k = l;
                                break;
                            }
                        if (k >= 0 && k < j)
                            if (list[i, j] == null)
                            {
                                if (j == 3)
                                {
                                    list[i, j] = list[i, k];
                                    list[i, k] = null;
                                    DoubleAnimation d = new DoubleAnimation();
                                    d.To = j * 50 + 2;
                                    d.Duration = TimeSpan.FromSeconds(0.3);
                                    Storyboard.SetTarget(d, list[i, j]);
                                    Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.TopProperty));
                                    s.Children.Add(d);
                                }
                                else
                                {
                                    if (list[i, j + 1].Value == list[i, k].Value && list[i, j + 1].Element2 == null)
                                    {
                                        list[i, j + 1].Element2 = list[i, k];
                                        list[i, k] = null;
                                        DoubleAnimation d = new DoubleAnimation();
                                        d.To = (j + 1) * 50 + 2;
                                        d.Duration = TimeSpan.FromSeconds(0.3);
                                        Storyboard.SetTarget(d, list[i, j + 1].Element2);
                                        Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.TopProperty));
                                        s.Children.Add(d);
                                        j++;
                                    }
                                    else
                                    {
                                        list[i, j] = list[i, k];
                                        list[i, k] = null;
                                        DoubleAnimation d = new DoubleAnimation();
                                        d.To = j * 50 + 2;
                                        d.Duration = TimeSpan.FromSeconds(0.3);
                                        Storyboard.SetTarget(d, list[i, j]);
                                        Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.TopProperty));
                                        s.Children.Add(d);
                                    }
                                }
                            }
                            else
                            {
                                if (list[i, j].Value == list[i, k].Value)
                                {
                                    list[i, j].Element2 = list[i, k];
                                    list[i, k] = null;
                                    DoubleAnimation d = new DoubleAnimation();
                                    d.To = j * 50 + 2;
                                    d.Duration = TimeSpan.FromSeconds(0.3);
                                    Storyboard.SetTarget(d, list[i, j].Element2);
                                    Storyboard.SetTargetProperty(d, new PropertyPath(Canvas.TopProperty));
                                    s.Children.Add(d);
                                }
                            }
                        else
                            break;
                    }
            status = 1;
            s.Begin();
            DownMove++;
        }

        void AutoPlay()
        {
            if (CanMove())
            {
                IsAutoPlay = true;
                if (currentMove == 1)
                {
                    if (CanMoveLeftRight())
                    {
                        MoveLeft();
                        GenerateNewNumber();
                    }
                    else if (CanMoveUpDown())
                    {
                        if (new EvaluationTool(list).MoveDown() > new EvaluationTool(list).MoveUp())
                            MoveDown();
                        else
                            MoveUp();
                        GenerateNewNumber();
                    }
                }
                else if (currentMove == 2)
                {
                    if (CanMoveUpDown())
                    {
                        MoveUp();
                        GenerateNewNumber();
                    }
                    else if (CanMoveLeftRight())
                    {
                        if (new EvaluationTool(list).MoveRight() > new EvaluationTool(list).MoveLeft())
                            MoveRight();
                        else
                            MoveLeft();
                        GenerateNewNumber();
                    }
                }
                else if (currentMove == 3)
                {
                    if (CanMoveLeftRight())
                    {
                        MoveRight();
                        GenerateNewNumber();
                    }
                    else if (CanMoveUpDown())
                    {
                        if (new EvaluationTool(list).MoveDown() > new EvaluationTool(list).MoveUp())
                            MoveDown();
                        else
                            MoveUp();
                        GenerateNewNumber();
                    }
                }
                else if (currentMove == 4)
                {
                    if (CanMoveUpDown())
                    {
                        MoveDown();
                        GenerateNewNumber();
                    }
                    else if (CanMoveLeftRight())
                    {
                        if (new EvaluationTool(list).MoveRight() > new EvaluationTool(list).MoveLeft())
                            MoveRight();
                        else
                            MoveLeft();
                        GenerateNewNumber();
                    }
                }
                else
                {
                    if (CanMoveLeftRight())
                    {
                        if (new EvaluationTool(list).MoveRight() > new EvaluationTool(list).MoveLeft())
                            MoveRight();
                        else
                            MoveLeft();
                        GenerateNewNumber();
                    }
                    else if (CanMoveUpDown())
                    {
                        if (new EvaluationTool(list).MoveDown() > new EvaluationTool(list).MoveUp())
                            MoveDown();
                        else
                            MoveUp();
                        GenerateNewNumber();
                    }
                }
            }
            else
                IsAutoPlay = false;
        }

        void SetHighlight(int i)
        {
            if (i == 1)
            {
                pLeft.Fill = new SolidColorBrush(Colors.White);
                pUp.Fill = new SolidColorBrush(Colors.Transparent);
                pRight.Fill = new SolidColorBrush(Colors.Transparent);
                pDown.Fill = new SolidColorBrush(Colors.Transparent);
            }
            else if (i == 2)
            {
                pLeft.Fill = new SolidColorBrush(Colors.Transparent);
                pUp.Fill = new SolidColorBrush(Colors.White);
                pRight.Fill = new SolidColorBrush(Colors.Transparent);
                pDown.Fill = new SolidColorBrush(Colors.Transparent);
            }
            else if (i == 3)
            {
                pLeft.Fill = new SolidColorBrush(Colors.Transparent);
                pUp.Fill = new SolidColorBrush(Colors.Transparent);
                pRight.Fill = new SolidColorBrush(Colors.White);
                pDown.Fill = new SolidColorBrush(Colors.Transparent);
            }
            else if (i == 4)
            {
                pLeft.Fill = new SolidColorBrush(Colors.Transparent);
                pUp.Fill = new SolidColorBrush(Colors.Transparent);
                pRight.Fill = new SolidColorBrush(Colors.Transparent);
                pDown.Fill = new SolidColorBrush(Colors.White);
            }
            else
            {
                pLeft.Fill = new SolidColorBrush(Colors.Transparent);
                pUp.Fill = new SolidColorBrush(Colors.Transparent);
                pRight.Fill = new SolidColorBrush(Colors.Transparent);
                pDown.Fill = new SolidColorBrush(Colors.Transparent);
            }
        }

        void s_Completed(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] != null && list[i, j].Element2 != null)
                    {
                        list[i, j].Value *= 2;
                        board.Children.Remove(list[i, j].Element2);
                        list[i, j].Element2 = null;

                        Score += list[i, j].Value;
                    }
                }
            }
            s.Children.Clear();
            status = 0;
            Check();

            if (CanMove())
            {
                EvaluationTool et = new EvaluationTool(list);
                int i1;
                et.Evaluate(out i1);
                SetHighlight(i1);
                currentMove = i1;
            }
            else
                SetHighlight(0);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (status == 0 && IsAutoPlay)
            {
                AutoPlay();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckHighScore();
            NewGame();
        }

        public void Check()
        {
            bool flag = false;

            if (CanMove() && Win())
            {
                status = 0;
                lblInfo.Content = "CONGRATULATIONS: YOU WIN!";
                flag = true;
            }
            else if (!CanMove() && Win())
            {
                status = 1;
                lblInfo.Content = "GAME OVER. YOU WIN!";
                flag = true;
            }
            else if (!CanMove() && !Win())
            {
                status = 1;
                lblInfo.Content = "SORRY: YOU LOSE!";
                flag = true;
            }

            if (flag && lblInfo.Visibility == Visibility.Hidden)
            {
                DoubleAnimation d = new DoubleAnimation();
                d.From = 0;
                d.To = 1;
                d.Duration = TimeSpan.FromSeconds(0.3);
                Storyboard.SetTarget(d, lblInfo);
                Storyboard.SetTargetProperty(d, new PropertyPath(Label.OpacityProperty));
                b.Children.Add(d);
                b.Begin();
                lblInfo.Visibility = Visibility.Visible;
            }

            if (!CanMove())
            {
                timer.Stop();
                IsAutoPlay = false;
                CheckHighScore();
            }
        }

        void b_Completed(object sender, EventArgs e)
        {
            b.Children.Clear();
        }

        public void CheckHighScore()
        {
            RefreshHighScoreData();
            if (HighScore == null && Score > 0)
            {
                CreateHighScore();
            }
            if (HighScore != null && Score > 0)
            {
                if (Score > HighScore.Score)
                {
                    CreateHighScore();
                }
            }
        }

        private void CreateHighScore()
        {
            if (CanShowHighScoreWindow && (HighScoresWindow != null && !HighScoresWindow.IsVisible || HighScoresWindow == null))
            {
                CanShowHighScoreWindow = false;
                HighScoresWindow = new HighScores(new HighScoreData() { Score = Score, DateTime = DateTime.Now });
                HighScoresWindow.ShowDialog();
                if (!HighScoresWindow.IsCancelled)
                    RefreshHighScoreData();
            }
        }

        public void RefreshHighScoreData()
        {
            HighScore = DataIO.ReadHighScore();
            if (HighScore != null)
            {
                lblName.Text = "NAME: " + HighScore.Name;
                lblHighScore.Text = "HIGH SCORE: " + HighScore.Score.ToString();
                lblDate.Text = "DATE: " + HighScore.DateTime.ToShortDateString();
                lblTime.Text = "TIME: " + HighScore.DateTime.ToShortTimeString();
            }
            else
            {
                lblName.Text = "NAME: [UNKNOWN]";
                lblHighScore.Text = "HIGH SCORE: [UNKNOWN]";
                lblDate.Text = "DATE: [UNKNOWN]";
                lblTime.Text = "TIME: [UNKNOWN]";
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (status == 0 && !IsAutoPlay)
            {
                if (e.Key == Key.Left)
                {
                    if (CanMoveLeftRight())
                    {
                        MoveLeft();
                        GenerateNewNumber();
                    }
                }
                else if (e.Key == Key.Up)
                {
                    if (CanMoveUpDown())
                    {
                        MoveUp();
                        GenerateNewNumber();
                    }
                }
                else if (e.Key == Key.Right)
                {
                    if (CanMoveLeftRight())
                    {
                        MoveRight();
                        GenerateNewNumber();
                    }
                }
                else if (e.Key == Key.Down)
                {
                    if (CanMoveUpDown())
                    {
                        MoveDown();
                        GenerateNewNumber();
                    }
                }
            }

            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.N)
            {
                NewGame();
            }
            if (e.Key == Key.F1)
            {
                new AboutWindow().ShowDialog();
            }
        }

        private void NewGame_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NewGame();
        }

        private void AutoPlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsAutoPlay)
            {
                timer.Start();
                AutoPlay();
            }
            else
            {
                timer.Stop();
                IsAutoPlay = false;
            }
        }

        private void Left_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (status == 0 && CanMoveLeftRight() && !IsAutoPlay)
            {
                MoveLeft();
                GenerateNewNumber();
            }
        }

        private void Up_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (status == 0 && CanMoveUpDown() && !IsAutoPlay)
            {
                MoveUp();
                GenerateNewNumber();
            }
        }

        private void Right_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (status == 0 && CanMoveLeftRight() && !IsAutoPlay)
            {
                MoveRight();
                GenerateNewNumber();
            }
        }

        private void Down_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (status == 0 && CanMoveUpDown() && !IsAutoPlay)
            {
                MoveDown();
                GenerateNewNumber();
            }
        }
    }

    public class HighScoreData
    {
        public string Name { get; set; }
        public long Score { get; set; }
        public DateTime DateTime { get; set; }
    }

    public static class DataIO
    {
        static string DataDirectory = Environment.CurrentDirectory + "\\2048.data";

        public static HighScoreData ReadHighScore()
        {
            try
            {
                HighScoreData hsd = new HighScoreData();
                string data = EncryptOrDecrypt(ReadData());
                string[] s = data.Split(new char[] { ';' });
                hsd.Name = s[0];
                hsd.Score = long.Parse(s[1]);
                hsd.DateTime = DateTime.Parse(s[2]);
                return hsd;
            }
            catch
            {
                return null;
            }
        }

        public static void WriteHighScore(HighScoreData data)
        {
            try
            {
                WriteHighScore(data.Name, data.Score, data.DateTime);
            }
            catch
            {
            }
        }

        static void WriteHighScore(string name, long score, DateTime dateTime)
        {
            string s = name + ";" + score.ToString() + ";" + dateTime.ToString();
            WriteData(EncryptOrDecrypt(s));
        }

        static string EncryptOrDecrypt(string s)
        {
            byte[] b = Encoding.Unicode.GetBytes(s);
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = (byte)(b[i] ^ -6);
            }
            return Encoding.Unicode.GetString(b);
        }

        static string ReadData()
        {
            if (File.Exists(DataDirectory))
            {
                FileStream file = new FileStream(DataDirectory, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(file, Encoding.Unicode);
                string result = reader.ReadToEnd();
                reader.Close();
                file.Close();
                return result;
            }
            else
                return "";
        }

        static void WriteData(string s)
        {
            FileStream file = new FileStream(DataDirectory, FileMode.OpenOrCreate, FileAccess.Write);
            file.SetLength(0);
            StreamWriter writer = new StreamWriter(file, Encoding.Unicode);
            writer.Write(s);
            writer.Close();
            file.Close();
        }
    }

    public class EvaluationTool
    {
        public class Data
        {
            public int Value { get; set; }
            public bool CanChange { get; set; }
        }

        public Data[,] dataList;

        private Data[,] Copy(Data[,] list)
        {
            Data[,] result = new Data[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[i, j] = null;
                    if (list[i, j] != null)
                    {
                        result[i, j] = new Data();
                        result[i, j].Value = list[i, j].Value;
                        result[i, j].CanChange = true;
                    }
                }
            }
            return result;
        }

        private void SetCanChange(Data[,] list)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] != null)
                    {
                        list[i, j].CanChange = true;
                    }
                }
            }
        }

        public EvaluationTool(Element[,] ele)
        {
            dataList = new Data[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    dataList[i, j] = null;
                    if (ele[i, j] != null)
                    {
                        dataList[i, j] = new Data();
                        dataList[i, j].Value = ele[i, j].Value;
                        dataList[i, j].CanChange = true;
                    }
                }
            }
        }

        public EvaluationTool(Data[,] dat)
        {
            dataList = Copy(dat);
        }

        private bool IsRowEmpty(Data[,] list, int y)
        {
            for (int i = 0; i < 4; i++)
            {
                if (list[i, y] != null)
                    return false;
            }
            return true;
        }

        private bool IsColumnEmpty(Data[,] list, int x)
        {
            for (int j = 0; j < 4; j++)
            {
                if (list[x, j] != null)
                    return false;
            }
            return true;
        }

        public long Move_0()
        {
            return MoveLeft();
        }

        public long Move_1()
        {
            return MoveUp();
        }

        public long Move_2()
        {
            return MoveRight();
        }

        public long Move_3()
        {
            return MoveDown();
        }

        public long MoveLeft(Data[,] list)
        {
            long result = 0;
            for (int j = 0; j < 4; j++)
                if (!IsRowEmpty(list, j))
                    for (int i = 0; i < 4; i++)
                    {
                        int k = -1;
                        for (int l = i + 1; l < 4; l++)
                            if (list[l, j] != null)
                            {
                                k = l;
                                break;
                            }
                        if (k > i)
                            if (list[i, j] == null)
                                if (i == 0)
                                {
                                    list[i, j] = list[k, j];
                                    list[k, j] = null;
                                }
                                else
                                    if (list[i - 1, j].Value == list[k, j].Value && list[i - 1, j].CanChange)
                                    {
                                        list[i - 1, j].CanChange = false;
                                        list[i - 1, j].Value += list[k, j].Value;
                                        result += list[i - 1, j].Value;
                                        list[k, j] = null;
                                        i--;
                                    }
                                    else
                                    {
                                        list[i, j] = list[k, j];
                                        list[k, j] = null;
                                    }
                            else
                            {
                                if (list[i, j].Value == list[k, j].Value)
                                {
                                    list[i, j].CanChange = false;
                                    list[i, j].Value += list[k, j].Value;
                                    result += list[i, j].Value;
                                    list[k, j] = null;
                                }
                            }
                        else
                            break;
                    }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] != null)
                        list[i, j].CanChange = true;
                }
            }
            return result;
        }

        public long MoveUp(Data[,] list)
        {
            long result = 0;
            for (int i = 0; i < 4; i++)
                if (!IsColumnEmpty(list, i))
                    for (int j = 0; j < 4; j++)
                    {
                        int k = -1;
                        for (int l = j + 1; l < 4; l++)
                            if (list[i, l] != null)
                            {
                                k = l;
                                break;
                            }
                        if (k > j)
                            if (list[i, j] == null)
                            {
                                if (j == 0)
                                {
                                    list[i, j] = list[i, k];
                                    list[i, k] = null;
                                }
                                else
                                    if (list[i, j - 1].Value == list[i, k].Value && list[i, j - 1].CanChange)
                                    {
                                        list[i, j - 1].CanChange = false;
                                        list[i, j - 1].Value += list[i, k].Value;
                                        result += list[i, j - 1].Value;
                                        list[i, k] = null;
                                        j--;
                                    }
                                    else
                                    {
                                        list[i, j] = list[i, k];
                                        list[i, k] = null;
                                    }
                            }
                            else
                            {
                                if (list[i, j].Value == list[i, k].Value)
                                {
                                    list[i, j].CanChange = false;
                                    list[i, j].Value += list[i, k].Value;
                                    result += list[i, j].Value;
                                    list[i, k] = null;
                                }
                            }
                        else
                            break;
                    }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] != null)
                        list[i, j].CanChange = true;
                }
            }
            return result;
        }

        public long MoveRight(Data[,] list)
        {
            long result = 0;
            for (int j = 0; j < 4; j++)
                if (!IsRowEmpty(list, j))
                    for (int i = 3; i >= 0; i--)
                    {
                        int k = -1;
                        for (int l = i - 1; l >= 0; l--)
                            if (list[l, j] != null)
                            {
                                k = l;
                                break;
                            }
                        if (k >= 0 && k < i)
                            if (list[i, j] == null)
                            {
                                if (i == 3)
                                {
                                    list[i, j] = list[k, j];
                                    list[k, j] = null;
                                }
                                else
                                {
                                    if (list[i + 1, j].Value == list[k, j].Value && list[i + 1, j].CanChange)
                                    {
                                        list[i + 1, j].CanChange = false;
                                        list[i + 1, j].Value += list[k, j].Value;
                                        result += list[i + 1, j].Value;
                                        list[k, j] = null;
                                        i++;
                                    }
                                    else
                                    {
                                        list[i, j] = list[k, j];
                                        list[k, j] = null;
                                    }
                                }
                            }
                            else
                            {
                                if (list[i, j].Value == list[k, j].Value)
                                {
                                    list[i, j].CanChange = false;
                                    list[i, j].Value += list[k, j].Value;
                                    result += list[i, j].Value;
                                    list[k, j] = null;
                                }
                            }
                        else
                            break;
                    }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] != null)
                        list[i, j].CanChange = true;
                }
            }
            return result;
        }

        public long MoveDown(Data[,] list)
        {
            long result = 0;
            for (int i = 0; i < 4; i++)
                if (!IsColumnEmpty(list, i))
                    for (int j = 3; j >= 0; j--)
                    {
                        int k = -1;
                        for (int l = j - 1; l >= 0; l--)
                            if (list[i, l] != null)
                            {
                                k = l;
                                break;
                            }
                        if (k >= 0 && k < j)
                            if (list[i, j] == null)
                            {
                                if (j == 3)
                                {
                                    list[i, j] = list[i, k];
                                    list[i, k] = null;
                                }
                                else
                                {
                                    if (list[i, j + 1].Value == list[i, k].Value && list[i, j + 1].CanChange)
                                    {
                                        list[i, j + 1].CanChange = false;
                                        list[i, j + 1].Value += list[i, k].Value;
                                        result += list[i, j + 1].Value;
                                        list[i, k] = null;
                                        j++;
                                    }
                                    else
                                    {
                                        list[i, j] = list[i, k];
                                        list[i, k] = null;
                                    }
                                }
                            }
                            else
                            {
                                if (list[i, j].Value == list[i, k].Value)
                                {
                                    list[i, j].CanChange = false;
                                    list[i, j].Value += list[i, k].Value;
                                    result += list[i, j].Value;
                                    list[i, k] = null;
                                }
                            }
                        else
                            break;
                    }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] != null)
                        list[i, j].CanChange = true;
                }
            }
            return result;
        }

        public long MoveLeft()
        {
            return MoveLeft(dataList);
        }

        public long MoveUp()
        {
            return MoveUp(dataList);
        }

        public long MoveRight()
        {
            return MoveRight(dataList);
        }

        public long MoveDown()
        {
            return MoveDown(dataList);
        }

        private bool IsInCorner(int x, int y)
        {
            return (x == 0 || x == 3) && (y == 0 || y == 3);
        }

        private bool IsInCorner(Data[,] list, Data d)
        {
            int x = -1, y = -1;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] == d)
                    {
                        x = i;
                        y = j;
                        break;
                    }
                }
            }
            return IsInCorner(x, y);
        }

        private Data GetMax(Data[,] list)
        {
            Data max = null;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (max == null)
                    {
                        max = list[i, j];
                    }
                    else if (list[i, j] != null && list[i, j].Value > max.Value)
                    {
                        max = list[i, j];
                    }
                }
            }
            Data[] d = new Data[16];
            int idx = -1;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] != null && list[i, j].Value == max.Value)
                    {
                        idx++;
                        d[idx] = list[i, j];
                    }
                }
            }
            if (idx != 0)
                for (int i = 0; i <= idx; i++)
                {
                    if (IsInCorner(list, d[i]))
                    {
                        max = d[i];
                        break;
                    }
                }
            return max;
        }

        private Data[] GetSeconds(Data[,] list)
        {
            Data max = GetMax(list);
            Data second = null;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (second == null)
                    {
                        if (list[i, j] != null && list[i, j].Value < max.Value)
                            second = list[i, j];
                    }
                    else if (list[i, j] != null && list[i, j].Value > second.Value && list[i, j].Value < max.Value)
                    {
                        second = list[i, j];
                    }
                }
            }
            if (second != null)
            {
                Data[] seconds = new Data[16];
                int idx = -1;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (list[i, j] != null && list[i, j].Value == second.Value)
                        {
                            idx++;
                            seconds[idx] = list[i, j];
                        }
                    }
                }
                Data[] result = new Data[idx + 1];
                for (int i = 0; i <= idx; i++)
                {
                    result[i] = seconds[i];
                }
                return result;
            }
            else
                return null;
        }

        private bool IsNear(Data[,] list, Data d1, Data d2)
        {
            int x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] == d1)
                    {
                        x1 = i;
                        y1 = j;
                    }
                    if (list[i, j] == d2)
                    {
                        x2 = i;
                        y2 = j;
                    }
                }
            }
            return (x1 + 1 == x2 || x1 - 1 == x2) && y1 == y2 || x1 == x2 && (y1 + 1 == y2 || y1 - 1 == y2);
        }

        public bool IsMaxSecondNear()
        {
            if (IsMaxInCorner())
            {
                if (GetMax(dataList).Value > 8)
                {
                    Data max = GetMaxInCorner(dataList);
                    Data[] seconds = GetSeconds(dataList);
                    if (seconds == null)
                        return true;
                    else
                    {
                        for (int i = 0; i < seconds.Length; i++)
                        {
                            if (IsNear(dataList, max, seconds[i]))
                                return true;
                        }
                        return false;
                    }
                }
                else
                    return true;
            }
            else
                return false;
        }

        private bool IsMaxInCorner(Data[,] list)
        {
            if (list[0, 0] != null && list[0, 0].Value == GetMax(list).Value)
                return true;
            else if (list[3, 0] != null && list[3, 0].Value == GetMax(list).Value)
                return true;
            else if (list[0, 3] != null && list[0, 3].Value == GetMax(list).Value)
                return true;
            else if (list[3, 3] != null && list[3, 3].Value == GetMax(list).Value)
                return true;
            else
                return false;
        }

        private Data GetMaxInCorner(Data[,] list)
        {
            if (list[0, 0] != null && list[0, 0].Value == GetMax(list).Value)
                return list[0, 0];
            else if (list[3, 0] != null && list[3, 0].Value == GetMax(list).Value)
                return list[3, 0];
            else if (list[0, 3] != null && list[0, 3].Value == GetMax(list).Value)
                return list[0, 3];
            else if (list[3, 3] != null && list[3, 3].Value == GetMax(list).Value)
                return list[3, 3];
            else
                return null;
        }

        public bool IsMaxInCorner()
        {
            return IsMaxInCorner(dataList);
        }

        private int GetLength(Data[,] list)
        {
            int result = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] != null)
                    {
                        result++;
                    }
                }
            }
            return result;
        }

        private void NormalMove(out int move)
        {
            move = 0;
            //if (GetLength(dataList) < 4 || GetLength(dataList) > 14)
            //{
            long[, ,] result = new long[4, 4, 4];
            Type t = Type.GetType("WPF2048.EvaluationTool");
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        object obj = Activator.CreateInstance(t, dataList);
                        MethodInfo mi_0 = t.GetMethod("IsTheSame");
                        MethodInfo mi_1 = t.GetMethod("Move_" + i);
                        MethodInfo mi_2 = t.GetMethod("Move_" + j);
                        MethodInfo mi_3 = t.GetMethod("Move_" + k);
                        FieldInfo f = t.GetField("dataList");

                        Data[,] d = Copy((Data[,])f.GetValue(obj));
                        long tmp = (long)mi_1.Invoke(obj, null);
                        if (!(bool)mi_0.Invoke(obj, new object[] { d }))
                        {
                            result[i, j, k] += tmp;
                            d = Copy((Data[,])f.GetValue(obj));
                            tmp = (long)mi_2.Invoke(obj, null);
                            if (!(bool)mi_0.Invoke(obj, new object[] { d }))
                            {
                                result[i, j, k] += tmp;
                                d = Copy((Data[,])f.GetValue(obj));
                                tmp = (long)mi_3.Invoke(obj, null);
                                if (!(bool)mi_0.Invoke(obj, new object[] { d }))
                                {
                                    result[i, j, k] += tmp;
                                }
                            }
                        }
                    }
                }
            }
            int max1 = 0, max2 = 0, max3 = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        if (result[i, j, k] > result[max1, max2, max3])
                        {
                            max1 = i;
                            max2 = j;
                            max3 = k;
                        }
                    }
                }
            }
            if (result[max1, max2, max3] != 0 && max1 >= 0 && max1 < 4)
                move = ++max1;
            //}
            //else
            //{
            //long[, , , ,] result = new long[4, 4, 4, 4, 4];
            //Type t = Type.GetType("WPF2048.EvaluationTool");
            //for (int i = 0; i < 4; i++)
            //{
            //    for (int j = 0; j < 4; j++)
            //    {
            //        for (int k = 0; k < 4; k++)
            //        {
            //            for (int l = 0; l < 4; l++)
            //            {
            //                for (int m = 0; m < 4; m++)
            //                {
            //                    object obj = Activator.CreateInstance(t, dataList);
            //                    MethodInfo mi_0 = t.GetMethod("IsTheSame");
            //                    MethodInfo mi_1 = t.GetMethod("Move_" + i);
            //                    MethodInfo mi_2 = t.GetMethod("Move_" + j);
            //                    MethodInfo mi_3 = t.GetMethod("Move_" + k);
            //                    MethodInfo mi_4 = t.GetMethod("Move_" + l);
            //                    MethodInfo mi_5 = t.GetMethod("Move_" + m);
            //                    FieldInfo f = t.GetField("dataList");

            //                    Data[,] d = Copy((Data[,])f.GetValue(obj));
            //                    long tmp = (long)mi_1.Invoke(obj, null);
            //                    if (!(bool)mi_0.Invoke(obj, new object[] { d }))
            //                    {
            //                        result[i, j, k, l, m] += tmp;
            //                        d = Copy((Data[,])f.GetValue(obj));
            //                        tmp = (long)mi_2.Invoke(obj, null);
            //                        if (!(bool)mi_0.Invoke(obj, new object[] { d }))
            //                        {
            //                            result[i, j, k, l, m] += tmp;
            //                            d = Copy((Data[,])f.GetValue(obj));
            //                            tmp = (long)mi_3.Invoke(obj, null);
            //                            if (!(bool)mi_0.Invoke(obj, new object[] { d }))
            //                            {
            //                                result[i, j, k, l, m] += tmp;
            //                                d = Copy((Data[,])f.GetValue(obj));
            //                                tmp = (long)mi_4.Invoke(obj, null);
            //                                if (!(bool)mi_0.Invoke(obj, new object[] { d }))
            //                                {
            //                                    result[i, j, k, l, m] += tmp;
            //                                    d = Copy((Data[,])f.GetValue(obj));
            //                                    tmp = (long)mi_5.Invoke(obj, null);
            //                                    if (!(bool)mi_0.Invoke(obj, new object[] { d }))
            //                                    {
            //                                        result[i, j, k, l, m] += tmp;
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //int max1 = 0, max2 = 0, max3 = 0, max4 = 0, max5 = 0;
            //for (int i = 0; i < 4; i++)
            //{
            //    for (int j = 0; j < 4; j++)
            //    {
            //        for (int k = 0; k < 4; k++)
            //        {
            //            for (int l = 0; l < 4; l++)
            //            {
            //                for (int m = 0; m < 4; m++)
            //                {
            //                    if (result[i, j, k, l, m] > result[max1, max2, max3, max4, max5])
            //                    {
            //                        max1 = i;
            //                        max2 = j;
            //                        max3 = k;
            //                        max4 = l;
            //                        max5 = m;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //if (result[max1, max2, max3, max4, max5] != 0 && max1 >= 0 && max1 < 4)
            //    move = ++max1;
            //}
        }

        private void CornerMove(out int move)
        {
            move = 0;
            long[, ,] result = new long[4, 4, 4];
            long tmp = 0;
            Type t = Type.GetType("WPF2048.EvaluationTool");
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        object obj = Activator.CreateInstance(t, dataList);
                        //MethodInfo mi_ = t.GetMethod("IsMaxSecondNear");
                        MethodInfo mi_0 = t.GetMethod("IsMaxInCorner");
                        MethodInfo mi_1 = t.GetMethod("Move_" + i);
                        MethodInfo mi_2 = t.GetMethod("Move_" + j);
                        MethodInfo mi_3 = t.GetMethod("Move_" + k);
                        tmp = (long)mi_1.Invoke(obj, null);
                        if ((bool)mi_0.Invoke(obj, null))
                        {
                            result[i, j, k] += tmp;
                            tmp = (long)mi_2.Invoke(obj, null);
                            if ((bool)mi_0.Invoke(obj, null))
                            {
                                result[i, j, k] += tmp;
                                tmp = (long)mi_3.Invoke(obj, null);
                                if ((bool)mi_0.Invoke(obj, null))
                                {
                                    result[i, j, k] += tmp;
                                }
                            }
                        }
                    }
                }
            }
            int max1 = 0, max2 = 0, max3 = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        if (result[i, j, k] > result[max1, max2, max3])
                        {
                            max1 = i;
                            max2 = j;
                            max3 = k;
                        }
                    }
                }
            }
            if (result[max1, max2, max3] != 0 && max1 >= 0 && max1 < 4)
                move = ++max1;
        }

        private void GetToCorner(out int move)
        {
            move = 0;

            Data[,] list = Copy(dataList);
            MoveLeft(list);
            if (IsMaxInCorner(list) && !IsTheSame(list))
            {
                move = 1;
                return;
            }

            list = Copy(dataList);
            MoveUp(list);
            if (IsMaxInCorner(list) && !IsTheSame(list))
            {
                move = 2;
                return;
            }

            list = Copy(dataList);
            MoveRight(list);
            if (IsMaxInCorner(list) && !IsTheSame(list))
            {
                move = 3;
                return;
            }

            list = Copy(dataList);
            MoveDown(list);
            if (IsMaxInCorner(list) && !IsTheSame(list))
            {
                move = 4;
                return;
            }
        }

        private void CleanMost(out int move)
        {
            move = 0;
            int[] result = new int[4];
            Data[,] list = Copy(dataList);
            MoveLeft(list);
            result[0] = GetLength(list);

            list = Copy(dataList);
            MoveUp(list);
            result[1] = GetLength(list);

            list = Copy(dataList);
            MoveRight(list);
            result[2] = GetLength(list);

            list = Copy(dataList);
            MoveDown(list);
            result[3] = GetLength(list);

            int min = 0;
            for (int i = 1; i < 4; i++)
            {
                if (result[min] > result[i])
                    min = i;
            }

            if (GetLength(dataList) == result[min])
            {
                move = 0;
            }
            else
            {
                move = ++min;

                if (!(move >= 0 && move <= 4))
                {
                    move = 0;
                }
            }
        }

        private void SingleMove(out int move)
        {
            move = 0;
            long[] result = new long[4];
            Data[,] list = Copy(dataList);
            result[0] = MoveLeft(list);

            list = Copy(dataList);
            result[1] = MoveUp(list);

            list = Copy(dataList);
            result[2] = MoveRight(list);

            list = Copy(dataList);
            result[3] = MoveDown(list);

            int max = 0;
            for (int i = 1; i < 4; i++)
            {
                if (result[max] < result[i])
                    max = i;
            }
            if (result[max] == 0)
                move = 0;
            else
                move = ++max;
        }

        public bool IsTheSame(Data[,] list)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (list[i, j] != null && dataList[i, j] != null && list[i, j].Value != dataList[i, j].Value || list[i, j] != null && dataList[i, j] == null || list[i, j] == null && dataList[i, j] != null)
                        return false;
                }
            }
            return true;
        }

        public void Evaluate(out int move)
        {
            //move = 0;

            //int length = GetLength(dataList);

            //if (length > 11)
            //{
            //    CleanMost(out move);
            //    if (move == 0)
            //    {
            //        NormalMove(out move);
            //    }
            //}
            //else
            //{
            //    if (IsMaxInCorner(dataList))
            //    {
            //        CornerMove(out move);
            //    }
            //    else
            //    {
            //        GetToCorner(out move);
            //        if (move == 0)
            //        {
            //            NormalMove(out move);
            //        }
            //    }
            //}

            NormalMove(out move);

            //if (IsMaxInCorner())
            //{
            //    CornerMove(out move);
            //    if (move == 0)
            //    {
            //        Data[,] d = Copy(dataList);
            //        MoveLeft(d);
            //        if (IsMaxInCorner(d))
            //        {
            //            move = 1;
            //            return;
            //        }
            //        d = Copy(dataList);
            //        MoveUp(d);
            //        if (IsMaxInCorner(d))
            //        {
            //            move = 2;
            //            return;
            //        }
            //        d = Copy(dataList);
            //        MoveRight(d);
            //        if (IsMaxInCorner(d))
            //        {
            //            move = 3;
            //            return;
            //        }
            //        d = Copy(dataList);
            //        MoveDown(d);
            //        if (IsMaxInCorner(d))
            //        {
            //            move = 4;
            //            return;
            //        }
            //    }
            //}
            //else
            //{
            //    GetToCorner(out move);
            //}
            //if (move == 0)
            //    NormalMove(out move);
        }
    }
}
