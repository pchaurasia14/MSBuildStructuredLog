using System;
using System.Collections.Generic;
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

namespace StructuredLogViewer.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ComparerControl : UserControl
    {
        private string leftView;
        private string rightView;

        public string LeftView
        {
            get => leftView;
            set
            {
                leftView = value;
                lftText.Text = leftView;
            }
        }

        public string RightView
        {
            get => rightView;
            set
            {
                rightView = value;
                rightText.Text = rightView;
            }
        }
        public ComparerControl()
        {
            InitializeComponent();

            //var items = new List<LineRecord>();

            //items.AddRange(Enumerable.Range(1, 500).Select(i => new LineRecord(i.ToString(), i.ToString(), "None")));

            //leftPaneView.ItemsSource = items;
        }
    }

    public class LineRecord
    {
        public string LineNumber { get; }
        public string Text { get; }
        public string Background { get; }

        public LineRecord(string lineNumber, string text, string background)
        {
            LineNumber = lineNumber;
            Text = text;
            Background = background;
        }
    }
}
