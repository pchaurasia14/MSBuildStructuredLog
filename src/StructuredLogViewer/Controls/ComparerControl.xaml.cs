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
using static System.Net.Mime.MediaTypeNames;

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
            DataContext = new CompareControlViewModel(leftView, rightView);
            searchLogControl.WatermarkContent = new TextBlock { Text = "Work in progress" };

        }
    }

    public class CompareControlViewModel : ObservableObject
    {
        private string leftView;
        private string rightView;

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
            
        }

        private ICommand prevDifferenceCommand;
        public ICommand PrevDifferenceCommand => prevDifferenceCommand ??= new Command(PrevDifference);

        private void PrevDifference()
        {

        }

        private ICommand dummyCommand;
        public ICommand DummyCommand => dummyCommand??= new Command(DummyCommandHandle);

        private void DummyCommandHandle()
        {
            LeftPaneViewRecords.Last().Background = BackgroundHighlightColor.Red;
            LeftPaneViewRecords.Insert(LeftPaneViewRecords.Count - 2, LineRecord.EmptyLine);

            RightPaneViewRecords.First().Background = BackgroundHighlightColor.Green;
            RightPaneViewRecords.Skip(1).First().Background = BackgroundHighlightColor.Green;
            RightPaneViewRecords.Skip(3).First().Background = BackgroundHighlightColor.Green;
        }

        public CompareControlViewModel(string leftView, string rightView)
        {
            LeftView = leftView;
            RightView = rightView;
            IEnumerable<(ResultType ResultType, string AItem, string BItem)> results = BinLogDiffer.ComputeDiff(leftView, rightView);
            int li = 0, ri = 0;
            foreach((ResultType rt, string a, string b) result in results)
            {
                switch (result.rt)
                {
                    case ResultType.Both:
                        LeftPaneViewRecords.Add(new LineRecord((li + 1).ToString(), result.a, BackgroundHighlightColor.None));
                        RightPaneViewRecords.Add(new LineRecord((ri + 1).ToString(), result.b, BackgroundHighlightColor.None));
                        li += 1;
                        ri += 1;
                        break;
                    case ResultType.A:
                        LeftPaneViewRecords.Add(new LineRecord((li + 1).ToString(), result.a, BackgroundHighlightColor.Red));
                        li += 1;
                        break;
                    case ResultType.B:
                        RightPaneViewRecords.Add(new LineRecord((ri + 1).ToString(), result.b, BackgroundHighlightColor.Green));
                        ri += 1;
                        break;
                }
            }
            //GetRecords(LeftView).ForEach(LeftPaneViewRecords.Add);
            //GetRecords(RightView).ForEach(RightPaneViewRecords.Add);
        }

        private static List<LineRecord> GetRecords(string fileName)
        {
            return System.IO.File.ReadLines(fileName)
               .Select((rec, index) => new LineRecord((index + 1).ToString(), rec, BackgroundHighlightColor.None))
               .ToList();
        }
    }

    public class LineRecord : ObservableObject
    {
        public static readonly LineRecord EmptyLine = new LineRecord("","", BackgroundHighlightColor.None);
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
