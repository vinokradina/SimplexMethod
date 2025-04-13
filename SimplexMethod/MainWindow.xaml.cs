using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace SimplexMethod
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] constraintSigns;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateForm_Click(object sender, RoutedEventArgs e)
        {
            ConstraintsPanel.Children.Clear();

            if (!int.TryParse(VariablesBox.Text, out int vars) || !int.TryParse(ConstraintsBox.Text, out int cons))
            {
                MessageBox.Show("Введите корректное количество переменных и ограничений.");
                return;
            }

            for (int i = 0; i < cons; i++)
            {
                StackPanel row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

                TextBox coefBox = new TextBox
                {
                    Width = 300,
                    Margin = new Thickness(0, 0, 10, 0),
                    ToolTip = "Коэффициенты через пробел"
                };

                ComboBox signBox = new ComboBox
                {
                    Width = 60,
                    Margin = new Thickness(0, 0, 10, 0),
                    ItemsSource = new[] { "<=", ">=", "=" },
                    SelectedIndex = 0
                };

                TextBox rhsBox = new TextBox
                {
                    Width = 60,
                    ToolTip = "Правая часть"
                };

                row.Children.Add(coefBox);
                row.Children.Add(signBox);
                row.Children.Add(rhsBox);

                ConstraintsPanel.Children.Add(row);
            }
        }

        private void Solve_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int numVariables = int.Parse(VariablesBox.Text);
                int numConstraints = ConstraintsPanel.Children.Count;

                double[] objectiveCoefficients = ObjectiveBox.Text.Split(' ').Select(double.Parse).ToArray();
                double[,] constraintsCoefficients = new double[numConstraints, numVariables];
                double[] rhs = new double[numConstraints];
                constraintSigns = new string[numConstraints];

                for (int i = 0; i < numConstraints; i++)
                {
                    var row = ConstraintsPanel.Children[i] as StackPanel;
                    var coefBox = row.Children[0] as TextBox;
                    var signBox = row.Children[1] as ComboBox;
                    var rhsBox = row.Children[2] as TextBox;

                    var coefs = coefBox.Text.Split(' ').Select(double.Parse).ToArray();
                    for (int j = 0; j < numVariables; j++)
                        constraintsCoefficients[i, j] = coefs[j];

                    constraintSigns[i] = signBox.SelectedItem.ToString();
                    rhs[i] = double.Parse(rhsBox.Text);
                }

                string result = SimplexSolver.Solve(objectiveCoefficients, constraintsCoefficients, rhs, constraintSigns);
                ResultBlock.Text = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ResultBlock.Text = "";
            VariablesBox.Text = "";
            ConstraintsBox.Text = "";
            ObjectiveBox.Text = "";
            ConstraintsPanel.Children.Clear();
        }

        private void SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
                FileName = "результат_симплекс_метод.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, ResultBlock.Text);
                    MessageBox.Show("Результат успешно сохранён.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
