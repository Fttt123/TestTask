using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Task2.Models;
using Task2.Value;

namespace Task2.Controllers
{
    public class CalculatorController : Controller
    {
        private ClassValue classValue;

        public IActionResult Index(string value = "")
        {
            classValue = new ClassValue();
            classValue.Value = value;
            return View();
        }

        [HttpPost]
        public IActionResult calculate(string value, string operation, int degreeValue)
        {
            try
            {
                //В operation передаётся строка определяющая какая операция будет выполнена. В degreeValue передаёться степень. В value строка введёная пользователем
                switch (operation)
                {
                    case "degree":
                        return RedirectToAction("Index",
                            new { value = Convert.ToString(Math.Pow(decision(value), degreeValue)) });
                    case "root":
                        return RedirectToAction("Index",
                            new { value = Convert.ToString(Math.Sqrt(decision(value))) });
                    case "decision":
                        return RedirectToAction("Index", new { value = decision(value) });
                    default:
                        return RedirectToAction("Error");
                }
            }
            catch (Exception ex)
            {
                var errorViewModel = new ErrorViewModel
                {
                    Error = ex.Message
                };
                return RedirectToAction("Error", errorViewModel);
            }
        }

        //Метод в которую передаётся строка с примером. Функция возврощает решённый пример.
        private double decision(string value)
        {
            try
            {
                value = value.Replace(" ", "");

                if (!IsValidInput(value))
                {
                    throw new Exception("Ошибка: Некорректный ввод");
                }

                //Замена тире на минус
                value = value.Replace("–", "-");

                //Так как dataTable.Compute не может решить степень Степени решаются отдельно
                value = ReplacePowerOperatorWithMathPow(value);

                //Решение примера
                var dataTable = new System.Data.DataTable();
                var result = dataTable.Compute(value, "");

                return Convert.ToDouble(result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private static string ReplacePowerOperatorWithMathPow(string expression)
        {
            while (expression.Contains("^"))
            {
                int powerIndex = expression.LastIndexOf("^");

                int startIndex = FindNumberStartIndex(expression, powerIndex - 1);
                int endIndex = FindNumberEndIndex(expression, powerIndex + 1);

                string baseNumberPart = expression.Substring(startIndex, powerIndex - startIndex).Trim();
                string exponentNumberPart = expression.Substring(powerIndex + 1, endIndex - powerIndex - 1).Trim();

                //Решение примера в степени
                var dataTable = new System.Data.DataTable();
                double baseNumber = Convert.ToDouble(dataTable.Compute(baseNumberPart, ""));
                double exponentNumber = Convert.ToDouble(dataTable.Compute(exponentNumberPart, ""));
                
                double result = Math.Pow(baseNumber, exponentNumber);

                expression = expression.Substring(0, startIndex) + result.ToString() + expression.Substring(endIndex);
            }

            return expression;
        }

        //нахождения индекса границ степени в строке
        private static int FindNumberStartIndex(string expression, int currentIndex)
        {
            while (currentIndex >= 0 && char.IsNumber(expression[currentIndex]))
            {
                currentIndex--;
            }

            return currentIndex + 1;
        }

        //нахождения индекса границ степени в строке
        private static int FindNumberEndIndex(string expression, int currentIndex)
        {
            while (currentIndex < expression.Length && expression[currentIndex] != '^')
            {
                currentIndex++;
            }

            return currentIndex;
        }

        //Метод проверябщая правильность ввода
        private bool IsValidInput(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsDigit(c) && c != '.' && c != '+' && c != '-' && c != '*' && c != '/' && c != '(' && c != ')' && c != '^' && c != '–')
                {
                    return false;
                }
            }

            return true;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string error)
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Error = error });
        }
    }
}
