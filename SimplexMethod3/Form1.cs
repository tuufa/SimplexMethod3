using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SimpleksMetod2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int constraintsCount = 0;
        int variablesCount = 0;
        int n = 0;
        int m = 0;
        double[,] table; //симплекс таблица


        List<int> basis; //список базисных переменных
        private void okBtn_Click(object sender, EventArgs e)
        {
            try
            {
                constraintsCount = Convert.ToInt32(nOfContraintsTextBox.Text);
                variablesCount = Convert.ToInt32(nOfVariablesTextBox.Text);
                fillConstraintsGrid();
                fillFunctionGrid();
                XnFunctionGrid();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        void fillConstraintsGrid()
        {
            constraintsGridView.Rows.Clear();
            constraintsGridView.ColumnCount = variablesCount + 2;
            constraintsGridView.RowHeadersVisible = false;
            for (int i = 0; i < variablesCount + 2; i++)
            {
                constraintsGridView.Columns[i].Width = 50;
                constraintsGridView.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (i < variablesCount)
                {
                    constraintsGridView.Columns[i].Name = "x" + (i + 1).ToString();
                }
                else if (i == variablesCount)
                {
                    constraintsGridView.Columns[i].Name = "Знак";
                    constraintsGridView.Columns[i+1].Name = "Рез.";
                }

            }

            for (int i = 0; i < constraintsCount; i++)
            {
                string[] row = new string[variablesCount];
                constraintsGridView.Rows.Add(row);
                constraintsGridView.Rows[i].Height = 20;
            }
        }
        void fillFunctionGrid()
        {
            functionGridView.Rows.Clear();
            functionGridView.ColumnCount = variablesCount;
            functionGridView.RowHeadersVisible = false;
            for (int i = 0; i < variablesCount; i++)
            {
                functionGridView.Columns[i].Width = 50;
                functionGridView.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (i < variablesCount)
                {
                    functionGridView.Columns[i].Name = "x" + (i + 1).ToString();
                }

            }
            functionGridView.Rows.Add();

        }
        void XnFunctionGrid()
        {
            XNGridView.Rows.Clear();
            XNGridView.ColumnCount = variablesCount;
            XNGridView.RowHeadersVisible = false;
            for (int i = 0; i < variablesCount; i++)
            {
                XNGridView.Columns[i].Width = 50;
                XNGridView.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (i < variablesCount)
                {
                    XNGridView.Columns[i].Name = "x" + (i + 1).ToString();
                }

            }
            XNGridView.Rows.Add();

        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            nOfContraintsTextBox.Clear();
            nOfVariablesTextBox.Clear();
            resultsGridView.Columns.Clear();
            functionGridView.Columns.Clear();
            constraintsGridView.Columns.Clear();
            extrComboBox.SelectedIndex = -1;
        }

        private void goBtn_Click(object sender, EventArgs e)
        {

            // Проверка выбора варианта (Max/Min)
            if (extrComboBox.SelectedItem == null || (extrComboBox.SelectedItem.ToString() != "Max" && extrComboBox.SelectedItem.ToString() != "Min"))
            {
                MessageBox.Show("Выберите корректный вариант экстремума (Max или Min).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка наличия корректных знаков в предпоследнем столбце constraintsGridView
            for (int i = 0; i < constraintsCount; i++)
            {
                string constraintSign = constraintsGridView.Rows[i].Cells[variablesCount].Value?.ToString();
                if (constraintSign != "<=" && constraintSign != ">=" && constraintSign != "=")
                {
                    MessageBox.Show($"Некорректный знак ограничения в строке {i + 1}. Допустимые значения: '<=', '>=', '='.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Проверка наличия чисел в последнем столбце constraintsGridView (свободные члены)
            for (int i = 0; i < constraintsCount; i++)
            {
                if (!double.TryParse(constraintsGridView.Rows[i].Cells[variablesCount + 1].Value?.ToString(), out double _))
                {
                    MessageBox.Show($"Некорректное значение свободного члена в строке {i + 1}. Убедитесь, что это число.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Проверка наличия чисел в столбцах ограничений constraintsGridView
            for (int i = 0; i < constraintsCount; i++)
            {
                for (int j = 0; j < variablesCount; j++)
                {
                    if (!double.TryParse(constraintsGridView.Rows[i].Cells[j].Value?.ToString(), out double _))
                    {
                        MessageBox.Show($"Некорректное значение в строке {i + 1}, столбце {j + 1}. Убедитесь, что это число.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            // Проверка наличия чисел в столбцах functionGridView (целевой функции)
            for (int j = 0; j < variablesCount; j++)
            {
                if (!double.TryParse(functionGridView.Rows[0].Cells[j].Value?.ToString(), out double _))
                {
                    MessageBox.Show($"Некорректное значение в целевой функции, столбец {j + 1}. Убедитесь, что это число.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            /*
            // Проверка наличия корректных знаков в XNGridView (если есть этот элемент)
            for (int i = 0; i < XNGridView.RowCount; i++)
            {
                string constraintSign = XNGridView.Rows[i].Cells[0].Value?.ToString();
                if (constraintSign != "<=" && constraintSign != ">=" && constraintSign != "=")
                {
                    MessageBox.Show($"Некорректный знак в XNGridView в строке {i + 1}. Допустимые значения: '<=', '>=', '='.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }*/



            Simplex();

            // Инициализация таблицы для результатов
            resultsGridView.ColumnCount = variablesCount + 1; // Количество столбцов (переменные + свободные члены)
            resultsGridView.Columns[0].Name = "Переменная";
            for (int i = 0; i < variablesCount; i++)
            {
                resultsGridView.Columns[i + 1].Name = $"X{i + 1}";
            }

            double[] result = new double[variablesCount];
            Calculate(result);
        }

        public void Simplex()
        {
            m = constraintsCount + 1; // Количество строк (ограничения + целевая функция)
            n = variablesCount + constraintsCount + 1; // Количество столбцов (переменные + базисные переменные + свободные члены)
            table = new double[m, n];

            basis = new List<int>(); // Инициализация списка базисных переменных

            int slackVarIndex = variablesCount + 1;

            for (int i = 0; i < constraintsCount; i++)
            {
                // Заполнение коэффициентов переменных
                for (int j = 0; j < variablesCount; j++)
                {
                    table[i, j + 1] = Convert.ToDouble(constraintsGridView.Rows[i].Cells[j].Value);
                }

                // Заполнение свободного члена
                table[i, 0] = Convert.ToDouble(constraintsGridView.Rows[i].Cells[variablesCount + 1].Value);

                // Обработка знаков ограничений
                string constraintSign = constraintsGridView.Rows[i].Cells[variablesCount].Value.ToString();
                if (constraintSign == "<=")
                {
                    table[i, slackVarIndex++] = 1; // Добавление фиктивной переменной
                }
                else if (constraintSign == ">=")
                {
                    table[i, slackVarIndex++] = -1; // Добавление фиктивной переменной с отрицательным коэффициентом
                    for (int j = 0; j <= variablesCount; j++) // Инвертирование строки ограничения
                    {
                        table[i, j] = -table[i, j];
                    }
                }
                else if (constraintSign == "=")
                {
                    // Для знака "=" фиктивная переменная не добавляется
                }

                // Добавление базисной переменной в список для знаков "<=" и ">="
                if (constraintSign != "=")
                {
                    basis.Add(slackVarIndex - 1);
                }
            }

            // Заполнение строки целевой функции
            string isMaximization = extrComboBox.SelectedItem.ToString();
            for (int j = 0; j < variablesCount; j++)
            {
                if (isMaximization == "Max")
                {
                    table[m - 1, j + 1] = -Convert.ToDouble(functionGridView.Rows[0].Cells[j].Value);
                }
                else // Минимизация
                {
                    table[m - 1, j + 1] = Convert.ToDouble(functionGridView.Rows[0].Cells[j].Value);
                }
            }
        }



        public double[,] Calculate(double[] result)
        {
            int mainCol, mainRow;
            while (!IsItEnd())
            {
                mainCol = findMainCol();
                mainRow = findMainRow(mainCol);

                if (mainRow == -1)
                {
                    MessageBox.Show("Решение неограниченно или система несовместна.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                // убеждаемся, что `basis` содержит достаточно элементов
                if (mainRow >= basis.Count)
                {
                    basis.Add(mainCol);  // Добавляем элемент, если его нет в `basis`
                }
                else
                {
                    basis[mainRow] = mainCol; // Иначе заменяем элемент
                }

                double[,] new_table = new double[m, n];

                // Пересчет симплекс-таблицы
                for (int j = 0; j < n; j++)
                    new_table[mainRow, j] = table[mainRow, j] / table[mainRow, mainCol];

                for (int i = 0; i < m; i++)
                {
                    if (i == mainRow) continue;
                    for (int j = 0; j < n; j++)
                        new_table[i, j] = table[i, j] - table[i, mainCol] * new_table[mainRow, j];
                }

                table = new_table;

                // Отображение текущей симплекс-таблицы после каждой итерации
                AddTableToResultsGridView();
            }

            // Заполнение результатов
            for (int i = 0; i < result.Length; i++)
            {
                int k = basis.IndexOf(i + 1);
                result[i] = (k != -1) ? table[k, 0] : 0;
            }

            return table;
        }

        private void AddTableToResultsGridView()
        {
            // убеждаемся, что количество столбцов соответствует количеству элементов в симплекс-таблице
            if (resultsGridView.ColumnCount < n)
            {
                resultsGridView.ColumnCount = n;
            }

            // Добавляем разделяющую строку с количеством пустых ячеек, равным числу столбцов
            DataGridViewRow separatorRow = new DataGridViewRow();
            separatorRow.CreateCells(resultsGridView);
            for (int i = 0; i < resultsGridView.ColumnCount; i++)
            {
                separatorRow.Cells[i].Value = "---------";
            }
            resultsGridView.Rows.Add(separatorRow);

            // Добавляем текущую симплекс-таблицу в resultsGridView
            for (int i = 0; i < m; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(resultsGridView);

                for (int j = 0; j < n; j++)
                {
                    row.Cells[j].Value = table[i, j];
                }

                resultsGridView.Rows.Add(row);
            }

            // Добавляем пустую строку после каждой таблицы для наглядности
            resultsGridView.Rows.Add(new DataGridViewRow());
        }

        private bool IsItEnd()
        {
            for (int j = 1; j < n; j++)
            {
                if (table[m - 1, j] < 0)
                    return false;
            }
            return true;
        }

        private int findMainCol()
        {
            int mainCol = 1;
            for (int j = 2; j < n; j++)
            {
                if (table[m - 1, j] < table[m - 1, mainCol]) 
                    mainCol = j;
            }
            return mainCol;
        }

        private int findMainRow(int mainCol)
        {
            int mainRow = -1;
            for (int i = 0; i < m - 1; i++)
            {
                if (table[i, mainCol] > 0)
                {
                    if (mainRow == -1 || (table[i, 0] / table[i, mainCol] < table[mainRow, 0] / table[mainRow, mainCol]))
                    {
                        mainRow = i;
                    }
                }
            }
            return mainRow;
        }

        private void constraintsGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}




