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
        private string extr; //Строка в которой хранится функция
        private string[] list; //Список строк, содержащий переменные
        //Список типов токенов
        public enum TokenType
        {
            //Переменная, константа, (, ), функция, операция
            var, num, op_br, cl_br, func, op
        };
        //Создаём структуру токена
        public struct Token(string str, TokenType typ)
        {
            public string name = str; //Название токена
            public TokenType type = typ; //Тип токена
        }
        //Массив токенов
        public List<Token> tokens1 = [];
        public List<Token> tokens2 = [];
        //Множество разделителей
        List<char> DelimSet;
        //Инициализирует множество разделителей
        //Словарь переменных. Ключ - имя переменной
        public Dictionary<string, double> variables = new Dictionary<string, double>();
        public MathParser(string str)
        {
            //Разделяем входную строку на отдельные строки
            list = str.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            extr = list[0]; //Сохраняем строку с функцией в extr
            list = list.Where(val => val != extr).ToArray(); //Удаляем строку с функцией из списка
            DelimSet = [' ', '(', ')', '+', '-', '*', '/', '^']; //Заполняем множество разделителей
            variables = ReadExpressionFromStr();
            tokens1 = CreateTokensFromExpression();
        }
        //Проверяем является ли символ разделителем
        private bool IsDelimiter(char sym) 
        {
            return DelimSet.Find(x => x == sym) > 0;
        }
        private Dictionary<string, double> ReadExpressionFromStr()
        {
            string temp;
            int pos;
            while(list.Length > 0)
            {
                temp = list[0];
                list = list.Where(val => val != temp).ToArray();
                pos = temp.IndexOf("=");
                if(pos > 0)
                {
                    string name = temp[..pos];
                    double value = Convert.ToDouble(temp[(pos + 1)..]);
                    variables[name] = value;
                }
            }
            return variables;
        }
        //Функция разбиения выражения на токены
        private List<Token> CreateTokensFromExpression()
        {
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
                    if (ex[i] == ' ') { //Пробел просто перепрыгиваем
                        i++;
                        continue;
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
                        name += ex[i];
                        i ++;
                    }
                }
                tokens1.Add(new Token(name, TokenType.var));
            }
            //Раздаем получившимся токенам типы
            for (int j = 0; j < tokens1.Count; j++) 
            {
                if (tokens1[j].name[0] == '(') 
                {
                    Token token_ = tokens1[j];
                    token_.type = TokenType.op_br; 
                    tokens1[j] = token_;
                    continue;
                }
                if (tokens1[j].name[0] == ')') 
                {
                    Token token_ = tokens1[j];
                    token_.type = TokenType.cl_br; 
                    tokens1[j] = token_;
                    continue;
                }
                if (char.IsDigit(tokens1[j].name[0])) 
                {
                    Token token_ = tokens1[j];
                    token_.type = TokenType.num; 
                    tokens1[j] = token_;
                    continue;
                }
                if(char.IsLetter(tokens1[j].name[0]))
                {
                    if(j < tokens1.Count - 1 && tokens1[j + 1].name[0] == '(')
                    {
                        Token token_ = tokens1[j];
                        token_.type = TokenType.func; 
                        tokens1[j] = token_;
                    }
                    continue;   
                }
                Token token = tokens1[j];
                token.type = TokenType.op; 
                tokens1[j] = token;
            }
            //Проверяем минус и !, что это префиксные операции
            for (int j = 0; j < tokens1.Count; j++) {
                if (tokens1[j].name == "-" && (j == 0 || tokens1[j - 1].type == TokenType.op_br))
                {
                    Token token = tokens1[j];
                    token.name = "opposite"; 
                    tokens1[j] = token;
                }
                if (tokens1[j].name == "!" && (j == tokens1.Count - 1 || tokens1[j + 1].type == TokenType.cl_br || tokens1[j + 1].type == TokenType.op))
                {
                    Token token = tokens1[j];
                    token.name = "factorial"; 
                    tokens1[j] = token;
                }
            }       
            return tokens1;
        }
    }
}