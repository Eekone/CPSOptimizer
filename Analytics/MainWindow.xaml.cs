using Microsoft.SolverFoundation.Services;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Analytics
{
    public partial class MainWindow : System.Windows.Window
    {
        string filePath = "";

        ObservableCollection<KeyValuePair<string, double>> currUChart = new ObservableCollection<KeyValuePair<string, double>>();
        ObservableCollection<KeyValuePair<string, double>> maxUChart = new ObservableCollection<KeyValuePair<string, double>>();
        ObservableCollection<KeyValuePair<string, double>> minUChart = new ObservableCollection<KeyValuePair<string, double>>();

        double minU=0.9;
        double maxU=1.5;

        public MainWindow()
        {
            InitializeComponent();
            InitialiseColumnChart();
        }
          
        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName.ToString();
                filePathTB.Text = filePath;
            }       
                            
           string extension = Path.GetExtension(filePath);
           if (extension == ".xlsx")
            {
                try { solve(); }
                catch (IOException ex)
                {
                    MessageBoxResult opened = MessageBox.Show(ex.Message, ex.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                }
               catch (System.ArgumentException xe)
                {
                    MessageBoxResult noWay = MessageBox.Show("Недопустимый путь", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }                            
            }
            else {MessageBoxResult wrongType = MessageBox.Show("Откройте файл .xlsx", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);}
        }   

        
        public void solve ()
        {
            minU = Convert.ToDouble(minUTB.Text);
            maxU = Convert.ToDouble(maxUTB.Text);

            double[,] A;
            double[] U;
            
            FileInfo newFile = new FileInfo(filePath);
            using (ExcelPackage xlPackage = new ExcelPackage(newFile))
            {
                SolverContext context = SolverContext.GetContext();
                context.ClearModel();
                Model model = context.CreateModel();                
                textBlock.Text = "";

                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets[1];

                A = Parcer.A(worksheet);
                textBlock.Text += Printer.TwoDimensial(A, true);
                U = Parcer.Ust(worksheet);                             
                textBlock.Text += "\nСтационарные потенциалы:\n" + Printer.OneDimensial(U);
                

                Decision[] decs = new Decision[A.GetLength(1)];
                for (int i = 0; i < decs.Length; i++)
                    decs[i] = new Decision(Domain.RealNonnegative, "I" + i.ToString());
                model.AddDecisions(decs);

                textBlock.Text += "\nСистема уравнений:\n";
                for (int i = 0; i < A.GetLength(0); i++)
                {
                    string expression = "";
                    string max = (minU+ "<=").Replace(',', '.');
                    string min = ("<="+maxU).Replace(',', '.');

                    for (int h = 0; h < decs.Length; h++)                    
                        expression += A[i, h] + "*I" + h + "+";
                    expression += U[i];

                    model.AddConstraint("U" + i, max + expression.Replace(',', '.') + min);
                    textBlock.Text += "КИП" + i + ": U" + i + "=" + expression + "\n";
                   
                }               
                model.AddGoal("minI", GoalKind.Minimize, Model.Sum(decs));
              
                Solution solution = context.Solve(new Directive());  
                textBlock.Text += "\nРезультат расчетов:\n";
                for (int h = 0; h < decs.Length; h++)
                    textBlock.Text += decs[h].Name+"="+decs[h].ToDouble().ToString()+"\n";            

                textBlock.Text += "\nПри этом достигаются следующие потенциалы:\n";            
                for (int i = 0; i < A.GetLength(0); i++)
                {
                    double Uf = 0;
                    for (int h = 0; h < decs.Length; h++)
                    {
                        Uf += A[i, h]*decs[h].GetDouble();
                    }
                    Uf += U[i];
                   
                    currUChart.Add(new KeyValuePair<string, double>("КИП"+i, Uf));
                    maxUChart.Add(new KeyValuePair<string, double>("КИП" +  i, 1.5));
                    minUChart.Add(new KeyValuePair<string, double>("КИП" + i, 0.9));                    
                    textBlock.Text += "U" +i+" = "+ Uf + "\n";
                }

                textBlock.Text += "\nВремя вычисления: " + solution.GetReport().SolveTime.ToString()+"мс\n";
                if (currUChart.Count > 10)                
                    chart.Width = 80 * currUChart.Count;               
                else chart.Width = double.NaN; 
            }
        }

        private void InitialiseColumnChart()
        {
            var dataSourceList = new List<ObservableCollection<KeyValuePair<string, double>>>();
            dataSourceList.Add(currUChart);
            dataSourceList.Add(maxUChart);
            dataSourceList.Add(minUChart);
            chart.DataContext = dataSourceList;
        }
        private void textBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            filePath = filePathTB.Text;
        }

        private void GOSTValuesCB_Checked(object sender, RoutedEventArgs e)
        {
            minUTB.IsEnabled = false;
            minUTB.Text = "0,9";
            maxUTB.IsEnabled = false;
            maxUTB.Text = "1,5";
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            minUTB.IsEnabled = true;
            maxUTB.IsEnabled = true;
        }            

        private void GOSTValuesCB_Unchecked(object sender, RoutedEventArgs e)
        {
            minUTB.IsEnabled = true;         
            maxUTB.IsEnabled = true;

        }
        private void float_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            bool approvedDecimalPoint = false;

            if (e.Text == ",")
            {
                if (!((TextBox)sender).Text.Contains(","))
                    approvedDecimalPoint = true;
            }

            if (!(char.IsDigit(e.Text, e.Text.Length - 1) || approvedDecimalPoint))
                e.Handled = true;
        }      
    }
}
