namespace Analytics
{
    public static class Printer
    {
        public static string TwoDimensial(double[,] T, bool isA=false)
        {
            string result = "";
            if (isA)
                result += "Матрица коэффициентов влияния:\n";
            for (int i = 0; i < T.GetLength(0); i++)
            {
                for (int j = 0; j < T.GetLength(1); j++)
                {
                    if (double.IsNaN(T[i, j]))
                        result += "?" + "\t";
                    else
                        result += T[i, j] + "\t";
                }
                result += "\n";
            }
            return result;
        }

        public static string OneDimensial(double[] T)
        {
            string result = "";
            for (int i = 0; i < T.Length; i++)
            {                
                    if (double.IsNaN(T[i]))
                        result += "?" + "\t";
                    else
                        result += T[i] + "\t";
            }
            result += "\n";
            return result;
        }
    }
}
