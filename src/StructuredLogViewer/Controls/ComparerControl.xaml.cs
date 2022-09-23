using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Build.Logging.StructuredLogger;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace StructuredLogViewer.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ComparerControl : UserControl
    {
        public ComparerControl()
        {
            InitializeComponent();
        }

        public ComparerControl(string leftView, string rightView)
        {
            InitializeComponent();
            var cvm = new CompareControlViewModel(leftView, rightView);
            cvm.scrollTo += ScrollToNewCurrIndex;
            DataContext = cvm;
            searchLogControl.WatermarkContent = new TextBlock { Text = "Work in progress" };
            //ScrollToNewCurrIndex();
        }

        private void ScrollToNewCurrIndex(int index)
        {
            leftPaneView.ScrollIntoView(leftPaneView.Items[index]);
            rightPaneView.ScrollIntoView(rightPaneView.Items[index]);
        }
    }

    public class CompareControlViewModel : ObservableObject
    {
        private string leftView;
        private string rightView;
        private List<(int LineA, int LineB, int CountA, int CountB)> editScript;
        private List<int> diffIndexes;
        private int currIndex = -1;

        public Action<int> scrollTo
        {
            get; set;
        }

        public string LeftView
        {
            get => leftView;
            set
            {
                leftView = value;
                RaisePropertyChanged(nameof(LeftView));
            }
        }

        public string RightView
        {
            get => rightView;
            set
            {
                rightView = value;
                RaisePropertyChanged(nameof(RightView));
            }
        }

        public int CurrentIndex
        {
            get { return currIndex; }
            set
            {
                currIndex = value;
                RaisePropertyChanged(nameof(CurrentIndex));
            }
        }

        private ObservableCollection<LineRecord> leftPaneViewRecords = new();
        public ObservableCollection<LineRecord> LeftPaneViewRecords
        {
            get => leftPaneViewRecords;
            set
            {
                leftPaneViewRecords = value;
                RaisePropertyChanged(nameof(LeftPaneViewRecords));
            }
        }

        private ObservableCollection<LineRecord> rightPaneViewRecords = new();
        public ObservableCollection<LineRecord> RightPaneViewRecords
        {
            get => rightPaneViewRecords;
            set
            {
                rightPaneViewRecords = value;
                RaisePropertyChanged(nameof(RightPaneViewRecords));
            }
        }

        private ICommand nextDifferenceCommand;
        public ICommand NextDifferenceCommand => nextDifferenceCommand ??= new Command(NextDifference);

        private void NextDifference()
        {
            if(CurrentIndex < diffIndexes.Count)
            {
                CurrentIndex++;
            }
            scrollTo?.Invoke(diffIndexes[CurrentIndex]);
        }

        private ICommand prevDifferenceCommand;
        public ICommand PrevDifferenceCommand => prevDifferenceCommand ??= new Command(PrevDifference);

        private void PrevDifference()
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
            }
            scrollTo?.Invoke(diffIndexes[CurrentIndex]);
        }

        private ICommand dummyCommand;
        public ICommand DummyCommand => dummyCommand ??= new Command(DummyCommandHandle);

        private void DummyCommandHandle()
        {
            scrollTo?.Invoke(100);
            //LeftPaneViewRecords.Last().Background = BackgroundHighlightColor.Red;
            //LeftPaneViewRecords.Insert(LeftPaneViewRecords.Count - 2, LineRecord.EmptyLine);

            //RightPaneViewRecords.First().Background = BackgroundHighlightColor.Green;
            //RightPaneViewRecords.Skip(1).First().Background = BackgroundHighlightColor.Green;
            //RightPaneViewRecords.Skip(3).First().Background = BackgroundHighlightColor.Green;
        }

        public CompareControlViewModel(string leftView, string rightView)
        {
            LeftView = leftView;
            RightView = rightView;

            string[] lines1 = File.ReadAllLines(leftView);
            string[] lines2 = File.ReadAllLines(rightView);

            editScript = BinLogDiffer.GetEditScript(lines1, lines2).ToList();

            GetRecords(lines1).ForEach(LeftPaneViewRecords.Add);
            GetRecords(lines2).ForEach(RightPaneViewRecords.Add);

            ShowDiff();
        }

        private void ShowDiff()
        {
            diffIndexes = new List<int>();

            int leftIndex = 0, rightIndex = 0;
            int leftOffset = 0, rightOffset = 0;
            foreach ((int LineA, int LineB, int CountA, int CountB) diffTuple in editScript)
            {
                leftIndex = diffTuple.LineA + leftOffset;
                rightIndex = diffTuple.LineB + rightOffset;
                diffIndexes.Add(leftIndex);

                if (diffTuple.CountA == 0)
                {
                    for (int k = 0; k < diffTuple.CountB; k++)
                    {
                        LeftPaneViewRecords.Insert(leftIndex + k, LineRecord.EmptyLine);
                        RightPaneViewRecords[rightIndex + k].Background = BackgroundHighlightColor.Green;
                        leftOffset += 1;
                    }
                }
                else if (diffTuple.CountB == 0)
                {
                    for (int k = 0; k < diffTuple.CountB; k++)
                    {
                        rightPaneViewRecords.Insert(rightIndex + k, LineRecord.EmptyLine);
                        LeftPaneViewRecords[leftIndex + k].Background = BackgroundHighlightColor.Red;
                        rightOffset += 1;
                    }
                }
                else
                {
                    for (int k = 0; k < diffTuple.CountB; k++)
                    {
                        rightPaneViewRecords[rightIndex + k].Background = BackgroundHighlightColor.Green;
                    }
                    for (int k = 0; k < diffTuple.CountA; k++)
                    {
                        leftPaneViewRecords[leftIndex + k].Background = BackgroundHighlightColor.Red;
                    }
                }
            }
        }

        private static List<LineRecord> GetRecords(string fileName)
        {
            return System.IO.File.ReadLines(fileName)
               .Select((rec, index) => new LineRecord((index + 1).ToString(), rec, BackgroundHighlightColor.None))
               .ToList();
        }

        private static List<LineRecord> GetRecords(string[] lines)
        {
            List<LineRecord> lr = new List<LineRecord>();
            for (int i = 0; i < lines.Length; i++)
            {
                lr.Add(new LineRecord((i + 1).ToString(), lines[i], BackgroundHighlightColor.None));
            }
            return lr;
        }
    }
    public class LineRecord : ObservableObject
    {
        public static readonly LineRecord EmptyLine = new LineRecord("", "", BackgroundHighlightColor.None);
        private BackgroundHighlightColor background;

        public string LineNumber { get; }
        public string Text { get; }
        public BackgroundHighlightColor Background
        {
            get => background;
            set
            {
                background = value;
                RaisePropertyChanged(nameof(Background));
            }
        }

        public LineRecord(string lineNumber, string text, BackgroundHighlightColor background)
        {
            LineNumber = lineNumber;
            Text = text;
            Background = background;
        }
    }

    public enum BackgroundHighlightColor { Green, Red, None }

    public class BackgroundColorConverter : IValueConverter
    {
        private readonly SolidColorBrush None = new SolidColorBrush(Colors.Transparent);
        private readonly SolidColorBrush Green = new SolidColorBrush(Colors.LightGreen);
        private readonly SolidColorBrush Red = new SolidColorBrush(Colors.LightSalmon);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not BackgroundHighlightColor scb) return None;

            return scb switch
            {
                BackgroundHighlightColor.Green => Green,
                BackgroundHighlightColor.Red => Red,
                _ => None
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
