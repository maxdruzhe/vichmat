/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot.Series;*/

namespace vichmat
{
    internal class MathParser
    {
        private string extr;
        private string[] list;
        public Dictionary<string, double> variables = new Dictionary<string, double>();
        public MathParser(string str)
        {
            list = str.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            extr = list[0];
            list = list.Where(val => val != extr).ToArray();
        }
        public Dictionary<string, double> ReadExpressionFromStr()
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
    }
}