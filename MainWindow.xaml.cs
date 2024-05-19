using System.Windows;

namespace vichmat
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Result(object sender, RoutedEventArgs e)
        {
            MathParser parser = new MathParser(TextBox1.Text);
            Dictionary<string, double> variables = parser.ReadExpressionFromStr();
            foreach(var var_ in variables)
            {
                TextBox2.Text = $"key: {var_.Key}  value: {Convert.ToString(var_.Value)}";
            }
        }
    }
}