using System;
using Microsoft.Maui.Controls;

namespace HesapMakinesi
{
    public partial class StandartHesapMakinesi : ContentPage
    {
        private string currentInput = "0";
        private double firstOperand = 0;
        private string currentOperator = "";
        private bool isNewInput = true;
        private double memoryValue = 0; // Bellek değeri

        public StandartHesapMakinesi()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            resultText.Text = currentInput;
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

            if (!string.IsNullOrEmpty(currentOperator) && !isNewInput)
            {
                // Perform previous operation if one exists
                OnEqualsClicked(sender, e);
            }

            if (double.TryParse(currentInput, out double result))
            {
                firstOperand = result;
            }

            currentOperator = button.Text;
            isNewInput = true;
        }

        private void OnEqualsClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentOperator) || isNewInput)
                return;

            double secondOperand;
            if (!double.TryParse(currentInput, out secondOperand))
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
                    case "×":
                        result = firstOperand * secondOperand;
                        break;
                    case "÷": // 0'a bölme hatası
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
                        result = firstOperand * (secondOperand / 100);
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

        private void OnClearClicked(object sender, EventArgs e)
        {
            currentInput = "0";
            firstOperand = 0;
            currentOperator = "";
            isNewInput = true;
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

        private void OnSpecialFunctionClicked(object sender, EventArgs e)
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
                    case "¹/ₓ":
                        if (value == 0) // 0'a bölme hatası
                        {
                            DisplayAlert("Hata", "Sıfıra bölme hatası", "Tamam");
                            error = true;
                        }
                        else
                        {
                            result = 1 / value;
                        }
                        break;
                    case "x²":
                        result = Math.Pow(value, 2);
                        break;
                    case "√x":
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

        private void OnMemoryClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string memoryOperation = button.Text;

            try
            {
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
                        if (double.TryParse(currentInput, out double addValue))
                        {
                            memoryValue += addValue;
                        }
                        isNewInput = true;
                        break;
                    case "M-": // Memory Subtract
                        if (double.TryParse(currentInput, out double subtractValue))
                        {
                            memoryValue -= subtractValue;
                        }
                        isNewInput = true;
                        break;
                }

                UpdateDisplay();
            }
            catch (Exception ex)
            {
                DisplayAlert("Hata", "Bellek işlemi hatası: " + ex.Message, "Tamam");
            }
        }
    }
}