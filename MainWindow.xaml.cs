using System.Windows;
using Syncfusion.UI.Xaml.Charts;
using System.Windows.Media;

namespace vichmat
{

    public partial class MainWindow : Window
    {
        MathParser parser;
        ViewModel data = new ViewModel();
        public MainWindow()
        {
            InitializeComponent();
            parser = new MathParser();
            this.DataContext = data;
        }
        //Функция отделения корней
        private List<List<double>> range()
        {
            List<List<double>> rlist = new List<List<double>>();
            int num = (int)parser.num;
            double a = -num;
            double b = num;
            double h = 0.1;
            double y0, y1;
            List<double> val1 = new List<double>() {a};
            y0 = parser.ResultExpr(val1);
            a += h;
            do
            {
                List<double> val2 = new List<double>() {a};
                y1 = parser.ResultExpr(val2);
                if (y0 * y1 < 0)
                {
                    List<double> coord = new List<double>() {a - h, a};
                    rlist.Add(coord);
                }
                y0 = y1;
                a += h;
            } while(a <= b);
            rlist = rlist.Distinct().ToList();
            rlist = cut(rlist);
            return rlist;
        }
        //Функция удаления пересекающихся интервалов
        List<List<double>> cut(List<List<double>> rlist)
        {
            List<List<double>> indexes = new List<List<double>>();
            //Сортируем интервалы по нижней границе
            rlist = rlist.OrderBy(a => a[0]).ToList();
            //Отслеживаем нижнюю границу
            double leftBorder = double.NegativeInfinity;
            //Просматриваем отсортированный список
            for(int i = 0; i < rlist.Count; i ++)
            {
                double left = rlist[i][0];
                double right = rlist[i][1];
                if(leftBorder <= left)
                {
                    //Если текущий интервал не пересекается
                    leftBorder = right;
                }
                else
                {
                    //При пересечении решаем какой интервал будет удалён
                    if(right < leftBorder)
                    {
                        int j = i - 1;
                        while(rlist[j][1] < leftBorder) j --;
                        rlist[j][1] = double.NegativeInfinity;
                        indexes.Add(rlist[j]);
                        leftBorder = rlist[i][1];
                    }
                    else
                    {
                        rlist[i][1] = double.NegativeInfinity;
                        indexes.Add(rlist[i]);
                    }
                }
            }
            for(int i = 0; i < indexes.Count; i ++)
            {
                rlist.Remove(indexes[i]);
            }
            return rlist;
        }
        //Функция вычисления производной методом левых разностей
        double ldiff(double x, double h)
        {
            List<double> values1 = new List<double>() {x};
            List<double> values2 = new List<double>() {x - h};
            double yh = (parser.ResultExpr(values1) - parser.ResultExpr(values2)) / h;
            return yh;
        }
        //Функция вычисления производной методом правых разностей
        double rdiff(double x, double h)
        {
            List<double> values1 = new List<double>() {x + h};
            List<double> values2 = new List<double>() {x};
            double yh = (parser.ResultExpr(values1) - parser.ResultExpr(values2)) / h;
            return yh;
        }
        //Функция для вычисления производной второго порядка(не понадобилась, но оставил)
        double diff2(double x, double h)
        {
            List<double> values1 = new List<double>() {x + h};
            List<double> values2 = new List<double>() {x};
            List<double> values3 = new List<double>() {x - h};
            double yhh = (parser.ResultExpr(values1) - 2 * parser.ResultExpr(values2) + parser.ResultExpr(values3)) / Math.Pow(h, 2);
            return yhh;
        }
        //Функция поиска корней методом итераций
        Dictionary<List<double>, List<double>> iteration(List<List<double>> rlist)
        {
            Dictionary<List<double>, List<double>> results = [];
            for(int i = 0; i < rlist.Count; i ++)
            {
                List<double> roots = [];
                List<double> doubles = rlist[i];
                double eps = parser.eps;
                double a, b, h;
                if (doubles[0] < doubles[1])
                {
                    a = doubles[0];
                    b = doubles[1];
                }
                else
                {
                    a = doubles[1];
                    b = doubles[0];
                }
                int iter = 0;
                double d;
                double x0 = a;
                double x1;
                h = Math.Abs(b - a) / 100;
                double l = 1 / rdiff(a, h);
                do
                {
                    List<double> values = new List<double>() {x0};
                    x1 = x0 - l * parser.ResultExpr(values);
                    d = Math.Abs(x1 - x0);
                    iter ++;
                    x0 = x1;
                } while(d > eps);
                roots.Add(x1);
                results[rlist[i]] = roots;
            }
            return results;
        }
        private void Result(object sender, RoutedEventArgs e)
        {
            TextBox2.Clear();
            string extr = TextBox1.Text;
            bool equation = parser.preprocessing(extr);
            if (equation)
            {
                List<List<double>> rlist = range();
                Dictionary<List<double>, List<double>> results = iteration(rlist);
                TextBox2.Text += "Список корней\n";
                for(int i = 0; i < rlist.Count; i ++)
                {
                    TextBox2.Text += "На интервале от " + Math.Round(rlist[i][0], 3).ToString() + " до " + 
                        Math.Round(rlist[i][1], 3).ToString() + ":\n";
                    List<double> roots = results[rlist[i]];
                    for(int j = 0; j < roots.Count; j ++)
                    {
                        TextBox2.Text += " " + (j + 1).ToString() + ") X" + " = " + Math.Round(roots[j], 5).ToString() + "\n";
                    }
                }
                if (rlist.Count > 0)
                {
                    //Строим график
                    schart.Series.Clear();
                    //Массив координат
                    List<Coords> coords = new List<Coords>();
                    for (double x = -rlist.Last()[1]; x <= rlist.Last()[1]; x += (2 * rlist.Last()[1]) / 100)
                    {
                        Coords coord = new Coords();
                        coord.X = x;
                        List<double> list = new List<double>() {x};
                        coord.Y = parser.ResultExpr(list);
                        coords.Add(coord);
                    }
                    NumericalAxis xAxis = new NumericalAxis()
                    {
                        Header = "X",
                        Minimum = -rlist.Last()[1] * 2,
                        Maximum = rlist.Last()[1] * 2
                    };
                    List<Coords> coords3 = coords.OrderBy(a => a.Y).ToList();
                    NumericalAxis yAxis = new NumericalAxis()
                    {
                        Header = "Y",
                    };
                    schart.PrimaryAxis = xAxis;
                    schart.SecondaryAxis = yAxis;
                    //Добавляем данные
                    data.Data = coords;
                    LineSeries series = new LineSeries()
                    {
                        XBindingPath = "X",
                        YBindingPath = "Y",
                        ItemsSource = data.Data
                    };
                    schart.Series.Add(series);
                }
            }
            else
            {
                double result = parser.ResultExpr();
                TextBox2.Text += "Результат - " + result.ToString();
            }
        }
    }
}