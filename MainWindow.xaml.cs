using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace vichmat
{

    public partial class MainWindow : Window
    {
        MathParser parser;
        public MainWindow()
        {
            InitializeComponent();
            parser = new MathParser();
        }
        public PlotModel GraphModel { get; set; }
        //Функция отделения корней
        private List<List<double>> range()
        {
            List<List<double>> rlist = new List<List<double>>();
            Random rng = new Random();
            int num_roots = parser.num_roots;
            double num = parser.num;
            while(num_roots > 0)
            {
                double a = 0 + (rng.NextDouble() * (num - 0));
                double h = 0.1;
                double b = a + h;
                int iteration = 0;
                while(iteration <= 10000)
                {
                    List<double> values1 = new List<double>() {a};
                    List<double> values2 = new List<double>() {b};
                    double Fa = parser.ResultExpr(values1);
                    double Fb = parser.ResultExpr(values2);
                    if (Fa * Fb < 0)
                    {
                        List<double> coords = [];
                        coords.Add(a);
                        coords.Add(b);
                        rlist.Add(coords);
                        break;
                    }
                    else
                    {
                        if (Fa > 0 && Fb > 0)
                        {
                            if(Fa > Fb)
                            {
                                a = b;
                                b += h;
                            }
                            else
                            {
                                b = a;
                                a -= h;
                            }
                        }
                        else
                        {
                            if(Fa > Fb)
                            {
                                b = a;
                                a -= h;
                            }
                            else
                            {
                                a = b;
                                b += h;
                            }
                        }
                    }
                    iteration ++;
                }
                num_roots --;
                num = a;
            }
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
                double l = 1 / rdiff(a, 1);
                h = Math.Abs(b - a) / 100;
                double x0 = a;
                double x1;
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
                    GraphModel = CreatePlotModel(rlist);
                    this.DataContext = this;
                }
            }
            else
            {
                double result = parser.ResultExpr();
                TextBox2.Text += "Результат - " + result.ToString();
            }
        }
        private PlotModel CreatePlotModel(List<List<double>> rlist)
        {
            Func<double, double> resExtr = (x) => parser.ResultExpr(new List<double>() {x});
            var GraphModel = new PlotModel{ Title = "График функции " + parser.extr };
            var funcSeries = new FunctionSeries(resExtr, -rlist.Last()[1] * 10, rlist.Last()[1] * 10, 0.1);
            var yAxis = new LinearAxis
            { 
                Position = AxisPosition.Left,
                Title = "Y Axis" 
            };
            var xAxis = new LinearAxis
            { 
                Position = AxisPosition.Bottom,
                Title = "X Axis"  
            };
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            GraphModel.Series.Add(funcSeries);
            GraphModel.Axes.Add(yAxis);
            GraphModel.Axes.Add(xAxis);
            return GraphModel;
        }
    }
}