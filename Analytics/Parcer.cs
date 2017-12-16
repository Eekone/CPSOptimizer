using MathNet.Numerics;
using OfficeOpenXml;
using System;
using System.Collections.Generic;

namespace Analytics
{
    public static class Parcer
    {
        public static int kipCount(ExcelWorksheet worksheet)
        {
            if (worksheet.Cells[2, 1].Text.Equals(""))
                return -1;
            else
            {
                int kipcount = 0; 
                for (int i = 2; i <= worksheet.Dimension.End.Row; i++)                               
                    if (i == worksheet.Dimension.End.Row || !worksheet.Cells[i, 1].Text.Equals(worksheet.Cells[i + 1, 1].Text))
                        kipcount++;                
                return kipcount;
            }
        }

        public static int skzCount(ExcelWorksheet worksheet)
        {
            if (worksheet.Dimension.Columns<5|| (worksheet.Dimension.Columns-3)%2!=0)
                return -1;
            else          
                return (worksheet.Dimension.Columns - 3) / 2;            
        }

        public static double[,] A(ExcelWorksheet worksheet)
        {
            double[,] A = new double[kipCount(worksheet), skzCount(worksheet)];           

            for (int col = 4; col < worksheet.Dimension.Columns; col += 2)
            {
                int kipNumber = 0;
                bool error = false;
                double[] Us = Ust(worksheet);  
                List<double> I = new List<double>();
                List<double> U = new List<double>();
                
                for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                {
                    try
                    {
                        I.Add(Convert.ToDouble(worksheet.Cells[row, col].Text));
                        U.Add(Convert.ToDouble(worksheet.Cells[row, col + 1].Text));
                    } catch (FormatException) { error = true; }
                        
                 
                    string kip = worksheet.Cells[row, 1].Text;
                    if (row== worksheet.Dimension.Rows||!worksheet.Cells[row, 1].Text.Equals(worksheet.Cells[row+1, 1].Text))
                    {
                        if (error)
                            A[kipNumber, (col / 2) - 2] = double.NaN;
                        else
                        {
                            I.Add(0);
                            U.Add(Us[kipNumber]);
                            A[kipNumber, (col / 2) - 2] = Fit.Line(I.ToArray(), U.ToArray()).Item2;
                        }
                            
                        error = false;                    
                        kipNumber++;
                        I.Clear(); U.Clear();
                    } 
                }               
             }            
            return A;
        }

        public static double[] Ust (ExcelWorksheet worksheet)
        {
            double[] result=new double[kipCount(worksheet)];
            int count = 0;
            for (int row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                if (row == worksheet.Dimension.Rows || !worksheet.Cells[row, 1].Text.Equals(worksheet.Cells[row + 1, 1].Text))
                {
                    try {result[count] = Convert.ToDouble(worksheet.Cells[row, 3].Text);}
                    catch (FormatException) {result[count] = double.NaN;}
                    count++;
                }
            }
            return result;
        }
    }
}
