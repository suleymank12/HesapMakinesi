using System;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;

namespace HesapMakinesi
{
    public partial class BilimselHesapMakinesi : ContentPage
    {
        private string currentInput = "0"; // mevcut giriş değeri
        private double firstOperand = 0;   // ilk operand değeri
        private string currentOperator = ""; // aktif operatör
        private bool isNewInput = true;    
        private bool isSecondFunction = false;
        private bool isScientificNotation = false;
        private bool isDegreeMode = true;  // true for DEG, false for RAD

        // bellek değeri
        private double memoryValue = 0;

        // açık parantez sayısı
        private int openParenthesisCount = 0;

        public BilimselHesapMakinesi()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            // Format the display based on whether scientific notation is enabled
            if (isScientificNotation)
            {
                // Try to parse as double and format in scientific notation
                if (double.TryParse(currentInput, out double value))
                {
                    resultText.Text = value.ToString("E6");
                }
                else
                {
                    resultText.Text = currentInput;
                }
            }
            else
            {
                resultText.Text = currentInput;
            }
        }

        private void OnNumberClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string digit = button.Text;

            if (isNewInput)
            {
                currentInput = digit;
                isNewInput = false;
            }
            else
            {
                if (currentInput == "0" && digit != ",")
                    currentInput = digit;
                else
                    currentInput += digit;
            }

            UpdateDisplay();
        }

        private void OnOperatorClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string operatorText = button.Text;

            // Map the display operator to the functional operator
            string functionalOperator = operatorText;
            if (operatorText == "×") functionalOperator = "*";
            if (operatorText == "÷") functionalOperator = "/";
            if (operatorText == "mod") functionalOperator = "%";

            if (!string.IsNullOrEmpty(currentOperator) && !isNewInput)
            {
                // Perform previous operation if one exists
                OnEqualsClicked(sender, e);
            }

            if (double.TryParse(currentInput, out double result))
            {
                firstOperand = result;
            }

            currentOperator = functionalOperator;
            isNewInput = true;
        }

        private void OnEqualsClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentOperator) || isNewInput)
                return;

            if (!double.TryParse(currentInput, out double secondOperand))
                return;

            double result = 0;
            bool error = false;

            try
            {
                switch (currentOperator)
                {
                    case "+":
                        result = firstOperand + secondOperand;
                        break;
                    case "-":
                        result = firstOperand - secondOperand;
                        break;
                    case "*":
                        result = firstOperand * secondOperand;
                        break;
                    case "/": // 0'a bölme hatası kontrol edilir
                        if (secondOperand == 0)
                        {
                            DisplayAlert("Hata", "Sıfıra bölme hatası", "Tamam");
                            error = true;
                        }
                        else
                        {
                            result = firstOperand / secondOperand;
                        }
                        break;
                    case "%":
                        result = firstOperand % secondOperand;
                        break;
                    case "xʸ":
                        result = Math.Pow(firstOperand, secondOperand);
                        break;
                }

                if (!error)
                {
                    currentInput = result.ToString();
                    UpdateDisplay();
                }
                else
                {
                    OnClearClicked(sender, e);
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Hata", "Hesaplama hatası: " + ex.Message, "Tamam");
                OnClearClicked(sender, e);
            }

            isNewInput = true;
            currentOperator = "";
        }

        private void OnScientificFunctionClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string function = button.Text;

            if (!double.TryParse(currentInput, out double value))
                return;

            double result = 0;
            bool error = false;

            try
            {
                switch (function)
                {
                    case "sin":
                        if (isDegreeMode)
                            result = Math.Sin(value * Math.PI / 180);
                        else
                            result = Math.Sin(value);
                        break;
                    case "cos":
                        if (isDegreeMode)
                            result = Math.Cos(value * Math.PI / 180);
                        else
                            result = Math.Cos(value);
                        break;
                    case "tan":  // tan için özel durum
                        if (isDegreeMode)
                        {
                            if (Math.Abs(value % 90) == 0 && Math.Abs(value % 180) != 0)
                            {
                                DisplayAlert("Hata", "Tanımlanamayan değer", "Tamam");
                                error = true;
                            }
                            else
                            {
                                result = Math.Tan(value * Math.PI / 180);
                            }
                        }
                        else
                        {
                            if (Math.Abs(value % (Math.PI / 2)) < 1e-10 && Math.Abs(value % Math.PI) > 1e-10)
                            {
                                DisplayAlert("Hata", "Tanımlanamayan değer", "Tamam");
                                error = true;
                            }
                            else
                            {
                                result = Math.Tan(value);
                            }
                        }
                        break;
                    case "log":
                        if (value <= 0) // log pozitif sayılarda olur
                        {
                            DisplayAlert("Hata", "Geçersiz logaritma girişi", "Tamam");
                            error = true;
                        }
                        else
                        {
                            result = Math.Log10(value);
                        }
                        break;
                    case "ln":
                        if (value <= 0)
                        {
                            DisplayAlert("Hata", "Geçersiz logaritma girişi", "Tamam");
                            error = true;
                        }
                        else
                        {
                            result = Math.Log(value);
                        }
                        break;
                    case "x²":
                        result = Math.Pow(value, 2);
                        break;
                    case "x³":
                        result = Math.Pow(value, 3);
                        break;
                    case "√":
                        if (value < 0)
                        {
                            DisplayAlert("Hata", "Negatif sayının karekökü alınamaz", "Tamam");
                            error = true;
                        }
                        else
                        {
                            result = Math.Sqrt(value);
                        }
                        break;
                    case "¹/ₓ":  // 1/x sıfır için tanımsızdır.
                        if (value == 0)
                        {
                            DisplayAlert("Hata", "Sıfıra bölme hatası", "Tamam");
                            error = true;
                        }
                        else
                        {
                            result = 1 / value;
                        }
                        break;
                    case "|x|":
                        result = Math.Abs(value);
                        break;
                    case "10ˣ":
                        result = Math.Pow(10, value);
                        break;
                    case "exp":
                        result = Math.Exp(value);
                        break;
                    case "n!":
                        if (value < 0 || value != Math.Floor(value))
                        {
                            DisplayAlert("Hata", "Faktöriyel yalnızca pozitif tam sayılar için tanımlıdır", "Tamam");
                            error = true;
                        }
                        else if (value > 170)
                        {
                            DisplayAlert("Hata", "Çok büyük sayı", "Tamam");
                            error = true;
                        }
                        else
                        {
                            result = Factorial((int)value);
                        }
                        break;
                }

                if (!error)
                {
                    currentInput = result.ToString();
                    UpdateDisplay();
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Hata", "Hesaplama hatası: " + ex.Message, "Tamam");
            }

            isNewInput = true;
        }

        private double Factorial(int n)
        {
            if (n == 0 || n == 1)
                return 1;

            double result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        private void OnConstantClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string constant = button.Text;

            switch (constant)
            {
                case "π":
                    currentInput = Math.PI.ToString();
                    break;
                case "e":
                    currentInput = Math.E.ToString();
                    break;
            }

            UpdateDisplay();
            isNewInput = true;
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            currentInput = "0";
            firstOperand = 0;
            currentOperator = "";
            isNewInput = true;
            openParenthesisCount = 0;
            UpdateDisplay();
        }

        private void OnClearEntryClicked(object sender, EventArgs e)
        {
            currentInput = "0";
            isNewInput = true;
            UpdateDisplay();
        }

        private void OnBackspaceClicked(object sender, EventArgs e)
        {
            if (currentInput.Length > 1)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
            }
            else
            {
                currentInput = "0";
                isNewInput = true;
            }
            UpdateDisplay();
        }

        private void OnDecimalPointClicked(object sender, EventArgs e)
        {
            if (isNewInput)
            {
                currentInput = "0,";
                isNewInput = false;
            }
            else if (!currentInput.Contains(","))
            {
                currentInput += ",";
            }
            UpdateDisplay();
        }

        private void OnNegateClicked(object sender, EventArgs e)
        {
            if (currentInput != "0")
            {
                if (currentInput.StartsWith("-"))
                    currentInput = currentInput.Substring(1);
                else
                    currentInput = "-" + currentInput;

                UpdateDisplay();
            }
        }

        private void OnParenthesisClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;

            if (button.Text == "(")
            {
                openParenthesisCount++;
                DisplayAlert("Bilgi", "Parantez işlemi: " + openParenthesisCount + " açık parantez", "Tamam");
            }
            else if (button.Text == ")")
            {
                if (openParenthesisCount > 0)
                {
                    openParenthesisCount--;
                    DisplayAlert("Bilgi", "Parantez kapatıldı: " + openParenthesisCount + " açık parantez kaldı", "Tamam");
                }
                else
                {
                    DisplayAlert("Hata", "Kapatılacak açık parantez yok", "Tamam");
                }
            }
        }

      

        private void OnSecondFunctionClicked(object sender, EventArgs e)
        {
            isSecondFunction = !isSecondFunction;
            ((Button)sender).BackgroundColor = isSecondFunction ?
                Color.FromArgb("#33b0ff") : Color.FromArgb("#2a2a2a");
            DisplayAlert("Bilgi", isSecondFunction ?
                "İkincil fonksiyonlar aktif" : "Temel fonksiyonlar aktif", "Tamam");
        }

        private void OnAngleModeClicked(object sender, EventArgs e)
        {
            isDegreeMode = !isDegreeMode;
            ((Button)sender).Text = isDegreeMode ? "DEG" : "RAD";
        }

        private void OnScientificNotationClicked(object sender, EventArgs e)
        {
            isScientificNotation = !isScientificNotation;
            UpdateDisplay();
        }

        private void OnMemoryClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string memoryOperation = button.Text;

            if (!double.TryParse(currentInput, out double currentValue))
                return;

            switch (memoryOperation)
            {
                case "MC": // Memory Clear
                    memoryValue = 0;
                    break;
                case "MR": // Memory Recall
                    currentInput = memoryValue.ToString();
                    isNewInput = true;
                    break;
                case "M+": // Memory Add
                    memoryValue += currentValue;
                    isNewInput = true;
                    break;
                case "M-": // Memory Subtract
                    memoryValue -= currentValue;
                    isNewInput = true;
                    break;
                case "MS": // Memory Store
                    memoryValue = currentValue;
                    isNewInput = true;
                    break;
                case "M∨": // Memory List
                    DisplayAlert("Bellek Değeri", "Mevcut bellek değeri: " + memoryValue, "Tamam");
                    break;
            }

            UpdateDisplay();
        }

        private void OnFunctionCategoryClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;

            if (button.Text == "Trigonometri")
            {
                DisplayAlert("Trigonometri",
                    "Trigonometri fonksiyonları: sin, cos, tan, arcsin, arccos, arctan", "Tamam");
                
            }
            else if (button.Text == "İşlev")
            {
                DisplayAlert("İşlev",
                    "Matematiksel işlevler: abs, floor, ceil, round, vs.", "Tamam");
                // Here you would expand/show a panel of mathematical functions
            }
        }
    }
}