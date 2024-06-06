/*using System;
using System.Text;
using System.Threading.Tasks;
using OxyPlot.Series;
using System.Collections.Generic;*/
using System.Linq;
using System.Dynamic;
using System.Numerics;

namespace vichmat
{
    internal class MathParser
    {
        public string extr; //Строка в которой хранится функция
        private string[] list; //Список строк, содержащий переменные
        //Список типов токенов
        public enum TokenType
        {
            //Переменная, константа, корень, (, ), функция, операция
            var, num, root, op_br, cl_br, func, op
        };
        //Создаём структуру токена
        public struct Token(string str, TokenType typ)
        {
            public string name = str; //Название токена
            public TokenType type = typ; //Тип токена
        }
        //возможное кол-во корней. Определяется по наибольшей степени
        public int num_roots = 0;
        List<string> roots = [];
        public double eps;
        public double num;
        //Массивы токенов
        public List<Token> texpr;
        public List<Token> pexpr;
        //Множество разделителей
        List<char> DelimSet;
        //Словарь приоритетов операций. Ключ - название операции
        public Dictionary<string, int> prior = new Dictionary<string, int>();
        //Словарь переменных. Ключ - имя переменной
        public Dictionary<string, double> variables;
        //Указатель на функцию(для операций)
        public delegate double func_type(Stack<double> s);
        //Инициализация массива операций
        public Dictionary<string, func_type> ops = new Dictionary<string, func_type>();
        public double result;
        public MathParser()
        {
            DelimSet = [' ', '(', ')', '+', '-', '*', '/', '^']; //Заполняем множество разделителей
            prior = CreatePrior(); //Заполняем словарь приоритетов операций
            ops = CreateOps();
        }

        //Проверяем является ли символ разделителем
        private bool IsDelimiter(char sym) 
        {
            return DelimSet.Find(x => x == sym) > 0;
        }
        //Функция для заполнения словаря приоритетов операций
        private Dictionary<string, int> CreatePrior()
        {
            prior["+"] = 10;
            prior["-"] = 10;
            prior["*"] = 20;
            prior["/"] = 20;
            prior["^"] = 30;
            prior["opposite"] = 10;
            prior["factorial"] = 30;
            prior["%"] = 20;
            prior["!"] = 40;
            return prior;
        }
        private Dictionary<string, func_type> CreateOps()
        {
            ops["+"] = op_plus;
            ops["-"] = op_minus;
            ops["*"] = op_mul;
            ops["/"] = op_dev;
            ops["^"] = op_deg;
            ops["opposite"] = op_opposite;
            ops["factorial"] = op_factorial;
            ops["%"] = op_odiv;
            ops["sin"] = op_sin;
            ops["cos"] = op_cos;
            ops["tan"] = op_tan;
            ops["acos"] = op_acos;
            ops["asin"] = op_asin;
            ops["atan"] = op_atan;
            return ops;
        }
        double fact(double n) 
        {
            if(n == 0)
                return 1;
            return n * fact(n - 1);
        }
        double op_plus(Stack<double> s) 
        {
            double a, b;
            a = s.Pop();
            b = s.Pop();
            return a + b;
        }
        double op_minus(Stack<double> s) 
        {
            double a, b;
            a = s.Pop();
            b = s.Pop();
            return b - a;
        }
        
        double op_mul(Stack<double> s) 
        {
            double a, b;
            a = s.Pop();
            b = s.Pop();
            return b * a;
        }
        double op_dev(Stack<double> s) 
        {
            double a, b;
            a = s.Pop();
            b = s.Pop();
            return b / a;
        }
        double op_deg(Stack<double> s) 
        {
            double a, b;
            a = s.Pop();
            b = s.Pop();
            return Math.Pow(b, a);
        }
        double op_opposite(Stack<double> s) 
        {
            double a;
            a = s.Pop();
            return -a;
        }
        double op_factorial(Stack<double> s) 
        {
            double a;
            a = s.Pop();
            return fact(a);
        }
        double op_odiv(Stack<double> s) 
        {
            double a, b;
            a = s.Pop();
            b = s.Pop();
            return b % a;
        }
        double op_sin(Stack<double> s) 
        {
            double a;
            a = s.Pop();
            return Math.Sin(a);
        }
        double op_cos(Stack<double> s) 
        {
            double a;
            a = s.Pop();
            return Math.Cos(a);
        }
        double op_tan(Stack<double> s) 
        {
            double a;
            a = s.Pop();
            return Math.Tan(a);
        }
        double op_asin(Stack<double> s) 
        {
            double a;
            a = s.Pop();
            return Math.Asin(a);
        }
        double op_acos(Stack<double> s) 
        {
            double a;
            a = s.Pop();
            return Math.Acos(a);
        }
        double op_atan(Stack<double> s) 
        {
            double a;
            a = s.Pop();
            return Math.Atan(a);
        }
        //Функция поиска и запоминания переменных
        private Dictionary<string, double> ReadExpressionFromStr()
        {
            variables = new Dictionary<string, double>();
            string temp;
            int pos;
            while(list.Length > 0)
            {
                temp = list[0];
                list = list.Where(val => val != temp).ToArray();
                temp.Replace(" ", "");
                pos = temp.IndexOf("=");
                if(pos > 0)
                {
                    string name = temp[..pos].Replace(" ", "");
                    double value = Convert.ToDouble(temp[(pos + 1)..]);
                    variables[name] = value;
                }
            }
            return variables;
        }
        //Функция разбиения выражения на токены
        private List<Token> CreateTokensFromExpression()
        {
            bool exp = false;
            texpr = [];
            string ex = extr + " ";
            string name;
            int i = 0;
            //Получаем имя токена
            while(i < ex.Length - 1)
            {
                name = "";
                //Если текущий символ разделитель
                if (IsDelimiter(ex[i])) 
                {
                    if (ex[i] == ' ') 
                    { //Пробел просто перепрыгиваем
                        i++;
                        continue;
                    }
                    if(ex[i] == '^')
                    {
                        exp = true;
                    }
                    name += ex[i]; //Любой другой добавляем в имя токена
                    i++;
                }
                else
                {
                    while(!IsDelimiter(ex[i]))
                    /*Если не разделитель непример, переменная или имя массива,
                    Считываем его польностью */
                    {
                        if(exp)
                        {
                            if(char.IsDigit(ex[i]))
                            {
                                if(double.Parse(ex[i].ToString()) > num_roots)
                                {
                                    num_roots = int.Parse(ex[i].ToString());
                                }
                                exp = false;
                            }
                        }
                        name += ex[i];
                        i ++;
                    }
                }
                texpr.Add(new Token(name, TokenType.var));
            }
            //Раздаем получившимся токенам типы
            for (int j = 0; j < texpr.Count; j++) 
            {
                if (texpr[j].name[0] == '(') 
                {
                    Token token_ = texpr[j];
                    token_.type = TokenType.op_br; 
                    texpr[j] = token_;
                    continue;
                }
                if (texpr[j].name[0] == ')') 
                {
                    Token token_ = texpr[j];
                    token_.type = TokenType.cl_br; 
                    texpr[j] = token_;
                    continue;
                }
                if (char.IsDigit(texpr[j].name[0])) 
                {
                    Token token_ = texpr[j];
                    token_.type = TokenType.num; 
                    texpr[j] = token_;
                    continue;
                }
                if(char.IsLetter(texpr[j].name[0]))
                {
                    if(j < texpr.Count - 1 && texpr[j + 1].name[0] == '(')
                    {
                        Token token_ = texpr[j];
                        token_.type = TokenType.func; 
                        texpr[j] = token_;
                    }
                    else
                    {
                        if(!variables.ContainsKey(texpr[j].name))
                        {
                            Token token_ = texpr[j];
                            token_.type = TokenType.root; 
                            texpr[j] = token_;
                            if(roots.IndexOf(texpr[j].name) == -1)
                            {
                                roots.Add(texpr[j].name);
                            }
                        }
                    }
                    continue;   
                }
                Token token = texpr[j];
                token.type = TokenType.op; 
                texpr[j] = token;
            }
            //Проверяем минус и !, что это префиксные операции
            for (int j = 0; j < texpr.Count; j++) 
            {
                if (texpr[j].name == "-" && (j == 0 || texpr[j - 1].type == TokenType.op_br))
                {
                    Token token = texpr[j];
                    token.name = "opposite"; 
                    texpr[j] = token;
                }
                if (texpr[j].name == "!" && (j == texpr.Count - 1 || texpr[j + 1].type == TokenType.cl_br || texpr[j + 1].type == TokenType.op))
                {
                    Token token = texpr[j];
                    token.name = "factorial"; 
                    texpr[j] = token;
                }
            }       
            return texpr;
        }
        //Переводим выражение в постфиксную запись
        private List<Token> CreatePostfixFromTokens()
        {
            pexpr = [];
            Stack<Token> TStack = [];
            //Ловим токены и работаем по алгоритму
            for(int i = 0; i < texpr.Count; i ++)
            {
                switch (texpr[i].type) 
                {
                    case TokenType.root:
                    case TokenType.var:
                    case TokenType.num:
                    {
                        pexpr.Add(texpr[i]);
                        break;
                    }
                    case TokenType.op_br:
                    {
                        TStack.Push(texpr[i]);
                        break;
                    }
                    case TokenType.cl_br:
                    {
                        while(TStack.Peek().type != TokenType.op_br)
                        {
                            pexpr.Add(TStack.Pop());
                        }
                        TStack.Pop();
                        break;
                    }
                    case TokenType.op:
                    {
                        if(TStack.Count > 0)
                        {
                            while(TStack.Count > 0 && ((TStack.Peek().type == TokenType.op && 
                                prior[texpr[i].name] <= prior[TStack.Peek().name]) ||
                                TStack.Peek().type == TokenType.func))
                                {
                                    pexpr.Add(TStack.Pop());
                                }
                        }
                        TStack.Push(texpr[i]);
                        break;
                    }
                    case TokenType.func:
                    {
                        while (TStack.Count > 0 && TStack.Peek().type == TokenType.func)
                        {
                            pexpr.Add(TStack.Pop());
                        }
                        TStack.Push(texpr[i]);
                        break;
                    }
                }
            }
            while(TStack.Count > 0)
            {
                pexpr.Add(TStack.Pop());
            }
            return pexpr;
        }
        //Разбираем выражение
        public bool preprocessing(string str)
        {
            roots.Clear();
            //Разделяем входную строку на отдельные строки
            list = str.Split(new char[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
            extr = list[0]; //Сохраняем строку с функцией в extr
            list = list.Where(val => val != extr).ToArray(); //Удаляем строку с функцией из списка
            variables = ReadExpressionFromStr();//Ищем переменные и сохраняем их
            texpr = CreateTokensFromExpression();//Разбиваем строку на токены
            pexpr = CreatePostfixFromTokens();//Записываем выражение в постфиксном виде
            num = variables.ContainsKey("Предел_a") ? variables["Предел_a"] : 1000;
            eps = variables.ContainsKey("Точность") ? variables["Точность"] : 0.00001;
            bool equation = roots.Count > 0 ? true : false;
            return equation;
        }
        //Считаем результат выражения
        public double ResultExpr(List<double> values = null)
        {
            roots.Distinct().ToList();
            if(roots.Count > 0)
            {
                for(int i = 0; i < roots.Count; i ++)
                {
                    variables[roots[i]] = values[i];
                }
            }
            Stack<double> s = [];
            for(int i = 0; i < pexpr.Count; i ++)
            {
                switch(pexpr[i].type)
                {
                    case TokenType.num:
                    {
                        s.Push(double.Parse(pexpr[i].name));
                        break;
                    }
                    case TokenType.root:
                    case TokenType.var:
                    {
                        foreach(KeyValuePair<string, double> entry in variables)
                        {
                            if(entry.Key == pexpr[i].name)
                            {
                                s.Push((double)entry.Value);
                                break;
                            }
                        }
                        break;
                    }
                    case TokenType.func:
                    case TokenType.op:
                    {
                        foreach(KeyValuePair<string, func_type> entry in ops)
                        {
                            if(entry.Key == pexpr[i].name)
                            {
                                s.Push(entry.Value(s));
                            }
                        }
                        break;
                    }
                }
            }
            return s.Peek();
        }
    }
}