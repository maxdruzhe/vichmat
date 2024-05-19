using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace vichmat;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MathParser parser;

    public MainWindow()
    {
        InitializeComponent();
    }
    private void Result(object sender, RoutedEventArgs e)
    {
        parser = new MathParser(TextBox1.Text);
        TextBox2.Text = parser.ReadExpressionFromStr();
    }
}