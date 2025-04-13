using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplexMethod
{
    public static class SimplexSolver
    {
        public static string Solve(double[] objectiveCoefficients, double[,] constraintsCoefficients, double[] rhs, string[] constraintSigns)
        {
            int numVariables = objectiveCoefficients.Length;
            int numConstraints = rhs.Length;

            // Подсчитываем количество дополнительных переменных (для >= и = добавляются искусственные переменные)
            int slackCount = 0;
            int artificialCount = 0;

            foreach (string sign in constraintSigns)
            {
                if (sign == "<=")
                    slackCount++;
                else if (sign == ">=")
                {
                    slackCount++;
                    artificialCount++;
                }
                else if (sign == "=")
                    artificialCount++;
            }

            int totalColumns = numVariables + slackCount + artificialCount + 1; // +1 для столбца RHS
            int totalRows = numConstraints + 1;

            double[,] tableau = new double[totalRows, totalColumns];
            int slackIndex = numVariables;
            int artificialIndex = numVariables + slackCount;

            // Построение таблицы
            int artificialRow = 0;
            for (int i = 0; i < numConstraints; i++)
            {
                // Коэффициенты переменных
                for (int j = 0; j < numVariables; j++)
                    tableau[i, j] = constraintsCoefficients[i, j];

                if (constraintSigns[i] == "<=")
                    tableau[i, slackIndex++] = 1;
                else if (constraintSigns[i] == ">=")
                {
                    tableau[i, slackIndex++] = -1;
                    tableau[i, artificialIndex] = 1;
                    artificialRow |= (1 << i); // Пометка искусственной переменной
                    artificialIndex++;
                }
                else if (constraintSigns[i] == "=")
                {
                    tableau[i, artificialIndex] = 1;
                    artificialRow |= (1 << i); // Пометка искусственной переменной
                    artificialIndex++;
                }

                tableau[i, totalColumns - 1] = rhs[i]; // RHS
            }

            // Целевая функция
            for (int j = 0; j < numVariables; j++)
                tableau[totalRows - 1, j] = -objectiveCoefficients[j];

            // Искусственные переменные не участвуют в ЦФ (если используем метод двух фаз — здесь первая фаза должна учитывать это)

            // Симплекс-алгоритм
            while (true)
            {
                int pivotCol = -1;
                double min = 0;

                for (int j = 0; j < totalColumns - 1; j++)
                {
                    if (tableau[totalRows - 1, j] < min)
                    {
                        min = tableau[totalRows - 1, j];
                        pivotCol = j;
                    }
                }

                if (pivotCol == -1)
                    break; // Оптимум найден

                int pivotRow = -1;
                double minRatio = double.MaxValue;

                for (int i = 0; i < numConstraints; i++)
                {
                    if (tableau[i, pivotCol] > 0)
                    {
                        double ratio = tableau[i, totalColumns - 1] / tableau[i, pivotCol];
                        if (ratio < minRatio)
                        {
                            minRatio = ratio;
                            pivotRow = i;
                        }
                    }
                }

                if (pivotRow == -1)
                    return "Решение не ограничено.";

                // Нормализация ведущей строки
                double pivotValue = tableau[pivotRow, pivotCol];
                for (int j = 0; j < totalColumns; j++)
                    tableau[pivotRow, j] /= pivotValue;

                // Приведение остальных строк
                for (int i = 0; i < totalRows; i++)
                {
                    if (i != pivotRow)
                    {
                        double factor = tableau[i, pivotCol];
                        for (int j = 0; j < totalColumns; j++)
                            tableau[i, j] -= factor * tableau[pivotRow, j];
                    }
                }
            }

            // Формирование результата
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Оптимальное решение:");

            for (int j = 0; j < numVariables; j++)
            {
                bool isBasic = false;
                double value = 0;

                for (int i = 0; i < numConstraints; i++)
                {
                    if (tableau[i, j] == 1)
                    {
                        bool otherOnes = false;
                        for (int k = 0; k < numConstraints; k++)
                        {
                            if (k != i && tableau[k, j] != 0)
                            {
                                otherOnes = true;
                                break;
                            }
                        }

                        if (!otherOnes)
                        {
                            isBasic = true;
                            value = tableau[i, totalColumns - 1];
                            break;
                        }
                    }
                }

                sb.AppendLine($"x{j + 1} = {Math.Round(isBasic ? value : 0, 4)}");
            }

            sb.AppendLine($"Максимум F = {Math.Round(tableau[totalRows - 1, totalColumns - 1], 4)}");
            return sb.ToString();
        }
    }
}
