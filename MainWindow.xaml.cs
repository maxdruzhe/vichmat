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
            List<MathParser.Token> tokens = parser.tokens1;
            foreach(var token in tokens)
            {
                TextBox2.Text += token.name + " - " + token.type.ToString() + "\n";
            }
        }
    }
}