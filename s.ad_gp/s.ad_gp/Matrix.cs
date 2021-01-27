using System;

namespace s.ad_gp
{
    class Matrix
    {
        /********** 矩阵的属性 **********/
        private string _name;
        private int _row;  //行
        private int _column;  //列
        private double[,] _values;

        public string Name { get => _name; set => _name = value; }
        public int Row { get => _row; set => _row = value; }
        public int Column { get => _column; set => _column = value; }
        public double[,] Values { get => _values; set => _values = value; }

        /********** 构造函数 **********/
        public Matrix(string name)
        {
            Name = name;
        }

        public Matrix(int size)
        {
            Row = size;
            Column = size;
            Values = new double[Row, Column];
        }

        public Matrix(int row, int column)
        {
            Row = row;
            Column = column;
            Values = new double[Row, Column];
        }  //主要用于临时矩阵

        public Matrix(string name, int row, int column)
        {
            Name = name;
            Row = row;
            Column = column;
            Values = new double[Row, Column];
        }

        public Matrix(string name, int row, int column, double[,] values)
        {
            Name = name;
            Row = row;
            Column = column;
            Values = values;
        }

        public Matrix(Matrix matrix)
        {
            Row = matrix.Row;
            Column = matrix.Column;
            Values = new double[Row, Column];
            Values = matrix.Values;
        }

        ~Matrix()
        {

        }

        /********** 重载矩阵运算符 ***********/
        //矩阵加法
        public static Matrix operator +(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrixEnd = new Matrix(matrix1.Row, matrix1.Column);

            for (int i = 0; i < matrixEnd.Row; i++)
            {
                for (int j = 0; j < matrixEnd.Column; j++)
                {
                    matrixEnd.Values[i, j] = matrix1.Values[i, j] + matrix2.Values[i, j];
                }
            }

            return matrixEnd;
        }

        //矩阵减法
        public static Matrix operator -(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrixEnd = new Matrix(matrix1.Row, matrix1.Column);

            for (int i = 0; i < matrixEnd.Row; i++)
            {
                for (int j = 0; j < matrixEnd.Column; j++)
                {
                    matrixEnd.Values[i, j] = matrix1.Values[i, j] - matrix2.Values[i, j];
                }
            }

            return matrixEnd;
        }

        //数乘
        public static Matrix operator *(Matrix matrix, double p)
        {
            Matrix matrixEnd = new Matrix(matrix);

            for (int i = 0; i < matrix.Row; i++)
            {
                for (int j = 0; j < matrix.Column; j++)
                {
                    matrixEnd.Values[i, j] *= p;
                }
            }

            return matrixEnd;
        }

        //矩阵相乘
        public static Matrix operator *(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrixEnd = new Matrix(matrix1.Row, matrix2.Column);

            double temp = 0;
            for (int i = 0; i < matrixEnd.Row; i++)
            {
                for (int j = 0; j < matrixEnd.Column; j++)
                {
                    temp = 0;
                    for (int k = 0; k < matrix2.Row; k++)
                    {
                        temp += matrix1.Values[i, k] * matrix2.Values[k, j];
                    }
                    matrixEnd.Values[i, j] = temp;
                }
            }

            return matrixEnd;
        }

        /********** 常用函数 **********/
        //递归计算行列式的值
        public static double GetDeterminant(Matrix matrix)
        {
            double[][] matrixZ = new double[matrix.Row][];

            for (int i = 0; i < matrix.Row; i++)
            {
                matrixZ[i] = new double[matrix.Column];

                for (int j = 0; j < matrix.Column; j++)
                {
                    matrixZ[i][j] = matrix.Values[i, j];
                }
            }

            double HLS = Determinant(matrixZ);

            return HLS;
        }

        private static double Determinant(double[][] matrix)
        {
            //二阶及以下行列式直接计算
            if (matrix.Length == 0) return 0;
            else if (matrix.Length == 1) return matrix[0][0];
            else if (matrix.Length == 2)
            {
                return matrix[0][0] * matrix[1][1] - matrix[0][1] * matrix[1][0];
            }

            //对第一行使用“加边法”递归计算行列式的值
            double dSum = 0, dSign = 1;
            for (int i = 0; i < matrix.Length; i++)
            {
                double[][] matrixTemp = new double[matrix.Length - 1][];
                for (int count = 0; count < matrix.Length - 1; count++)
                {
                    matrixTemp[count] = new double[matrix.Length - 1];
                }

                for (int j = 0; j < matrixTemp.Length; j++)
                {
                    for (int k = 0; k < matrixTemp.Length; k++)
                    {
                        matrixTemp[j][k] = matrix[j + 1][k >= i ? k + 1 : k];
                    }
                }

                dSum += (matrix[0][i] * dSign * Determinant(matrixTemp));
                dSign = dSign * -1;
            }

            return dSum;
        }

        //生成单位矩阵
        public static Matrix GetIdentityMatrix(int size)
        {
            Matrix matrix = new Matrix(size);

            for (int i = 0; i < matrix.Row; i++)
            {
                for (int j = 0; j < matrix.Column; j++)
                {
                    matrix.Values[i, j] = ((i == j) ? 1 : 0);  //主对角线元素为1
                }
            }

            return matrix;
        }

        //生成权阵(p为权值)
        public static Matrix GetPMatrix(int size, double[] p)
        {
            Matrix matrix = new Matrix(size);
            int k = 0;

            for (int i = 0; i < matrix.Row; i++)
            {
                for (int j = 0; j < matrix.Column; j++)
                {
                    if (i == j)
                    {
                        matrix.Values[i, j] = p[k];
                        k++;
                    }
                }
            }

            return matrix;
        }

        //矩阵转置
        public Matrix Transpose()
        {
            Matrix matrixCopy = new Matrix(this);
            Matrix matrixEnd = new Matrix(Column, Row);

            for (int i = 0; i < matrixCopy.Row; i++)
            {
                for (int j = 0; j < matrixCopy.Column; j++)
                {
                    matrixEnd.Values[j, i] = matrixCopy.Values[i, j];
                }
            }
            return matrixEnd;
        }

        //正定矩阵求逆
        public Matrix Inverse()
        {
            int i = 0;
            int row = this.Row;
            Matrix MatrixZwei = new Matrix(row, row * 2);
            Matrix iMatrixInv = new Matrix(row, row);

            for (i = 0; i < row; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    MatrixZwei.Values[i, j] = this.Values[i, j];
                }
            }

            for (i = 0; i < row; i++)
            {
                for (int j = row; j < row * 2; j++)
                {
                    MatrixZwei.Values[i, j] = 0;
                    if (i + row == j)
                    {
                        MatrixZwei.Values[i, j] = 1;
                    }
                }
            }

            for (i = 0; i < row; i++)
            {
                if (MatrixZwei.Values[i, i] != 0)
                {
                    double intTemp = MatrixZwei.Values[i, i];
                    for (int j = 0; j < row * 2; j++)
                    {
                        MatrixZwei.Values[i, j] = MatrixZwei.Values[i, j] / intTemp;
                    }
                }

                for (int j = 0; j < row; j++)
                {
                    if (j == i)
                        continue;
                    double intTemp = MatrixZwei.Values[j, i];
                    for (int k = 0; k < row * 2; k++)
                    {
                        MatrixZwei.Values[j, k] = MatrixZwei.Values[j, k] - MatrixZwei.Values[i, k] * intTemp;
                    }
                }
            }

            for (i = 0; i < row; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    iMatrixInv.Values[i, j] = MatrixZwei.Values[i, j + row];
                }
            }

            return iMatrixInv;
        }

        //矩阵输出
        public static string OutputMatrix(Matrix matrix)
        {
            string s = "";

            for (int i = 0; i < matrix.Row; i++)
            {
                for (int j = 0; j < matrix.Column; j++)
                {
                    s += Math.Round(matrix.Values[i, j], 6).ToString() + " ";
                }
                s += "\r\n";
            }

            return s;
        }
    }
}