using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MRL.SSL.GameDefinitions
{
    public class FMatrix
    {
        public int rows;
        public int cols;
        public double[,] mat;

        public FMatrix L;
        public FMatrix U;
        private int[] pi;
        private double detOfP = 1;

        public FMatrix(int iRows, int iCols)         // FMatrix Class constructor
        {
            rows = iRows;
            cols = iCols;
            mat = new double[rows, cols];
        }
        public FMatrix(double[,] elements)
        {
            rows = elements.GetLength(0);
            cols = elements.GetLength(1); ;
            mat = new double[rows, cols];
            for (int i = 0; i < elements.GetLength(0); i++)
                for (int j = 0; j < elements.GetLength(1); j++)
                    mat[i, j] = elements[i, j];
        }
        public FMatrix(FMatrix t)
        {
            rows = t.rows;
            cols = t.cols;
            mat = new double[rows, cols];
            for (int i = 0; i < t.rows; i++)
                for (int j = 0; j < t.cols; j++)
                    mat[i, j] = t.mat[i, j];
        }
        public Boolean IsSquare()
        {
            return (rows == cols);
        }

        public double this[int iRow, int iCol]      // Access this FMatrix as a 2D array
        {
            get { return mat[iRow, iCol]; }
            set { mat[iRow, iCol] = value; }
        }

        public FMatrix GetCol(int k)
        {
            FMatrix m = new FMatrix(rows, 1);
            for (int i = 0; i < rows; i++) m[i, 0] = mat[i, k];
            return m;
        }

        public void SetCol(FMatrix v, int k)
        {
            for (int i = 0; i < rows; i++) mat[i, k] = v[i, 0];
        }

        public void MakeLU()                        // Function for LU decomposition
        {
            if (!IsSquare()) throw new MException("The Matrix is not square!");
            L = IdentityMatrix(rows, cols);
            U = Duplicate();

            pi = new int[rows];
            for (int i = 0; i < rows; i++) pi[i] = i;

            double p = 0;
            double pom2;
            int k0 = 0;
            int pom1 = 0;

            for (int k = 0; k < cols - 1; k++)
            {
                p = 0;
                for (int i = k; i < rows; i++)      // find the row with the biggest pivot
                {
                    if (Math.Abs(U[i, k]) > p)
                    {
                        p = Math.Abs(U[i, k]);
                        k0 = i;
                    }
                }
                if (p == 0) // samé nuly ve sloupci
                    throw new MException("The Matrix is singular!");

                pom1 = pi[k]; pi[k] = pi[k0]; pi[k0] = pom1;    // switch two rows in permutation FMatrix

                for (int i = 0; i < k; i++)
                {
                    pom2 = L[k, i]; L[k, i] = L[k0, i]; L[k0, i] = pom2;
                }

                if (k != k0) detOfP *= -1;

                for (int i = 0; i < cols; i++)                  // Switch rows in U
                {
                    pom2 = U[k, i]; U[k, i] = U[k0, i]; U[k0, i] = pom2;
                }

                for (int i = k + 1; i < rows; i++)
                {
                    L[i, k] = U[i, k] / U[k, k];
                    for (int j = k; j < cols; j++)
                        U[i, j] = U[i, j] - L[i, k] * U[k, j];
                }
            }
        }


        public FMatrix SolveWith(FMatrix v)                        // Function solves Ax = v in confirmity with solution vector "v"
        {
            if (rows != cols) throw new MException("The Matrix is not square!");
            if (rows != v.rows) throw new MException("Wrong number of results in solution vector!");
            if (L == null) MakeLU();

            FMatrix b = new FMatrix(rows, cols);
            for (int i = 0; i < rows; i++) b[pi[i], 0] = v[i, 0];   // switch two items in "v" due to permutation FMatrix

            FMatrix z = SubsForth(L, b);
            FMatrix x = SubsBack(U, z);

            return x;
        }

        public FMatrix Invert()                                   // Function returns the inverted FMatrix
        {
            if (L == null) MakeLU();

            FMatrix inv = new FMatrix(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                FMatrix Ei = FMatrix.ZeroMatrix(rows, 1);
                Ei[pi[pi[i]], 0] = 1;
                FMatrix col = SolveWith(Ei);
                inv.SetCol(col, i);
            }
            return inv;
        }


        public double Det()                         // Function for determinant
        {
            if (L == null) MakeLU();
            double det = detOfP;
            for (int i = 0; i < rows; i++) det *= U[i, i];
            return det;
        }

        public FMatrix GetP()                        // Function returns permutation FMatrix "P" due to permutation vector "pi"
        {
            if (L == null) MakeLU();

            FMatrix Matrix = ZeroMatrix(rows, cols);
            for (int i = 0; i < rows; i++) Matrix[i, pi[i]] = 1;
            return Matrix;
        }

        public FMatrix Duplicate()                   // Function returns the copy of this FMatrix
        {
            FMatrix Matrix = new FMatrix(rows, cols);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    Matrix[i, j] = mat[i, j];
            return Matrix;
        }

        public static FMatrix SubsForth(FMatrix A, FMatrix b)          // Function solves Ax = b for A as a lower triangular FMatrix
        {
            if (A.L == null) A.MakeLU();
            int n = A.rows;
            FMatrix x = new FMatrix(n, 1);

            for (int i = 0; i < n; i++)
            {
                x[i, 0] = b[i, 0];
                for (int j = 0; j < i; j++) x[i, 0] -= A[i, j] * x[j, 0];
                x[i, 0] = x[i, 0] / A[i, i];
            }
            return x;
        }

        public static FMatrix SubsBack(FMatrix A, FMatrix b)           // Function solves Ax = b for A as an upper triangular FMatrix
        {
            if (A.L == null) A.MakeLU();
            int n = A.rows;
            FMatrix x = new FMatrix(n, 1);

            for (int i = n - 1; i > -1; i--)
            {
                x[i, 0] = b[i, 0];
                for (int j = n - 1; j > i; j--) x[i, 0] -= A[i, j] * x[j, 0];
                x[i, 0] = x[i, 0] / A[i, i];
            }
            return x;
        }

        public static FMatrix ZeroMatrix(int iRows, int iCols)       // Function generates the zero FMatrix
        {
            FMatrix Matrix = new FMatrix(iRows, iCols);
            for (int i = 0; i < iRows; i++)
                for (int j = 0; j < iCols; j++)
                    Matrix[i, j] = 0;
            return Matrix;
        }

        public static FMatrix IdentityMatrix(int iRows, int iCols)   // Function generates the identity FMatrix
        {
            FMatrix Matrix = ZeroMatrix(iRows, iCols);
            for (int i = 0; i < Math.Min(iRows, iCols); i++)
                Matrix[i, i] = 1;
            return Matrix;
        }

        public static FMatrix RandomFMatrix(int iRows, int iCols, int dispersion)       // Function generates the zero FMatrix
        {
            Random random = new Random();
            FMatrix Matrix = new FMatrix(iRows, iCols);
            for (int i = 0; i < iRows; i++)
                for (int j = 0; j < iCols; j++)
                    Matrix[i, j] = random.Next(-dispersion, dispersion);
            return Matrix;
        }

        public static FMatrix Parse(string s)                        // Function parses the FMatrix from string
        {
            string[] rows = Regex.Split(s, "\r\n");
            string[] nums = rows[0].Split(' ');
            FMatrix Matrix = new FMatrix(rows.Length, nums.Length);
            try
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    nums = rows[i].Split(' ');
                    for (int j = 0; j < nums.Length; j++) Matrix[i, j] = double.Parse(nums[j]);
                }
            }
            catch (FormatException exc) { throw new MException("Wrong input format!"); }
            return Matrix;
        }

        public override string ToString()                           // Function returns FMatrix as a string
        {
            string s = "";
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++) s += String.Format("{0,5:0.00}", mat[i, j]) + " ";
                s += "\r\n";
            }
            return s;
        }

        public static FMatrix Transpose(FMatrix m)              // FMatrix transpose
        {
            FMatrix t = new FMatrix(m.cols, m.rows);
            for (int i = 0; i < m.rows; i++)
                for (int j = 0; j < m.cols; j++)
                    t[j, i] = m[i, j];
            return t;
        }

        public static FMatrix Power(FMatrix m, int pow)           // Power FMatrix to exponent
        {
            if (pow == 0) return IdentityMatrix(m.rows, m.cols);
            if (pow == 1) return m.Duplicate();
            if (pow == -1) return m.Invert();

            FMatrix x;
            if (pow < 0) { x = m.Invert(); pow *= -1; }
            else x = m.Duplicate();

            FMatrix ret = IdentityMatrix(m.rows, m.cols);
            while (pow != 0)
            {
                if ((pow & 1) == 1) ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        private static void SafeAplusBintoC(FMatrix A, int xa, int ya, FMatrix B, int xb, int yb, FMatrix C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                    if (xb + j < B.cols && yb + i < B.rows) C[i, j] += B[yb + i, xb + j];
                }
        }

        private static void SafeAminusBintoC(FMatrix A, int xa, int ya, FMatrix B, int xb, int yb, FMatrix C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                    if (xb + j < B.cols && yb + i < B.rows) C[i, j] -= B[yb + i, xb + j];
                }
        }

        private static void SafeACopytoC(FMatrix A, int xa, int ya, FMatrix C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                }
        }

        private static void AplusBintoC(FMatrix A, int xa, int ya, FMatrix B, int xb, int yb, FMatrix C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j] + B[yb + i, xb + j];
        }

        private static void AminusBintoC(FMatrix A, int xa, int ya, FMatrix B, int xb, int yb, FMatrix C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j] - B[yb + i, xb + j];
        }

        private static void ACopytoC(FMatrix A, int xa, int ya, FMatrix C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j];
        }

        private static FMatrix StrassenMultiply(FMatrix A, FMatrix B)                // Smart FMatrix multiplication
        {
            if (A.cols != B.rows) throw new MException("Wrong dimension of FMatrix!");

            FMatrix R;

            int msize = Math.Max(Math.Max(A.rows, A.cols), Math.Max(B.rows, B.cols));

            if (msize < 32)
            {
                R = ZeroMatrix(A.rows, B.cols);
                for (int i = 0; i < R.rows; i++)
                    for (int j = 0; j < R.cols; j++)
                        for (int k = 0; k < A.cols; k++)
                            R[i, j] += A[i, k] * B[k, j];
                return R;
            }

            int size = 1; int n = 0;
            while (msize > size) { size *= 2; n++; };
            int h = size / 2;


            FMatrix[,] mField = new FMatrix[n, 9];

            /*
             *  8x8, 8x8, 8x8, ...
             *  4x4, 4x4, 4x4, ...
             *  2x2, 2x2, 2x2, ...
             *  . . .
             */

            int z;
            for (int i = 0; i < n - 4; i++)          // rows
            {
                z = (int)Math.Pow(2, n - i - 1);
                for (int j = 0; j < 9; j++) mField[i, j] = new FMatrix(z, z);
            }

            SafeAplusBintoC(A, 0, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 1], 1, mField); // (A11 + A22) * (B11 + B22);

            SafeAplusBintoC(A, 0, h, A, h, h, mField[0, 0], h);
            SafeACopytoC(B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 2], 1, mField); // (A21 + A22) * B11;

            SafeACopytoC(A, 0, 0, mField[0, 0], h);
            SafeAminusBintoC(B, h, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 3], 1, mField); //A11 * (B12 - B22);

            SafeACopytoC(A, h, h, mField[0, 0], h);
            SafeAminusBintoC(B, 0, h, B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 4], 1, mField); //A22 * (B21 - B11);

            SafeAplusBintoC(A, 0, 0, A, h, 0, mField[0, 0], h);
            SafeACopytoC(B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 5], 1, mField); //(A11 + A12) * B22;

            SafeAminusBintoC(A, 0, h, A, 0, 0, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 6], 1, mField); //(A21 - A11) * (B11 + B12);

            SafeAminusBintoC(A, h, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, h, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 7], 1, mField); // (A12 - A22) * (B21 + B22);

            R = new FMatrix(A.rows, B.cols);                  // result

            /// C11
            for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
                for (int j = 0; j < Math.Min(h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 1][i, j] + mField[0, 1 + 4][i, j] - mField[0, 1 + 5][i, j] + mField[0, 1 + 7][i, j];

            /// C12
            for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
                for (int j = h; j < Math.Min(2 * h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 3][i, j - h] + mField[0, 1 + 5][i, j - h];

            /// C21
            for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
                for (int j = 0; j < Math.Min(h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 2][i - h, j] + mField[0, 1 + 4][i - h, j];

            /// C22
            for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
                for (int j = h; j < Math.Min(2 * h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 1][i - h, j - h] - mField[0, 1 + 2][i - h, j - h] + mField[0, 1 + 3][i - h, j - h] + mField[0, 1 + 6][i - h, j - h];

            return R;
        }

        // function for square FMatrix 2^N x 2^N

        private static void StrassenMultiplyRun(FMatrix A, FMatrix B, FMatrix C, int l, FMatrix[,] f)    // A * B into C, level of recursion, Matrix field
        {
            int size = A.rows;
            int h = size / 2;

            if (size < 32)
            {
                for (int i = 0; i < C.rows; i++)
                    for (int j = 0; j < C.cols; j++)
                    {
                        C[i, j] = 0;
                        for (int k = 0; k < A.cols; k++) C[i, j] += A[i, k] * B[k, j];
                    }
                return;
            }

            AplusBintoC(A, 0, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 1], l + 1, f); // (A11 + A22) * (B11 + B22);

            AplusBintoC(A, 0, h, A, h, h, f[l, 0], h);
            ACopytoC(B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 2], l + 1, f); // (A21 + A22) * B11;

            ACopytoC(A, 0, 0, f[l, 0], h);
            AminusBintoC(B, h, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 3], l + 1, f); //A11 * (B12 - B22);

            ACopytoC(A, h, h, f[l, 0], h);
            AminusBintoC(B, 0, h, B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 4], l + 1, f); //A22 * (B21 - B11);

            AplusBintoC(A, 0, 0, A, h, 0, f[l, 0], h);
            ACopytoC(B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 5], l + 1, f); //(A11 + A12) * B22;

            AminusBintoC(A, 0, h, A, 0, 0, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 6], l + 1, f); //(A21 - A11) * (B11 + B12);

            AminusBintoC(A, h, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, h, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 7], l + 1, f); // (A12 - A22) * (B21 + B22);

            /// C11
            for (int i = 0; i < h; i++)          // rows
                for (int j = 0; j < h; j++)     // cols
                    C[i, j] = f[l, 1 + 1][i, j] + f[l, 1 + 4][i, j] - f[l, 1 + 5][i, j] + f[l, 1 + 7][i, j];

            /// C12
            for (int i = 0; i < h; i++)          // rows
                for (int j = h; j < size; j++)     // cols
                    C[i, j] = f[l, 1 + 3][i, j - h] + f[l, 1 + 5][i, j - h];

            /// C21
            for (int i = h; i < size; i++)          // rows
                for (int j = 0; j < h; j++)     // cols
                    C[i, j] = f[l, 1 + 2][i - h, j] + f[l, 1 + 4][i - h, j];

            /// C22
            for (int i = h; i < size; i++)          // rows
                for (int j = h; j < size; j++)     // cols
                    C[i, j] = f[l, 1 + 1][i - h, j - h] - f[l, 1 + 2][i - h, j - h] + f[l, 1 + 3][i - h, j - h] + f[l, 1 + 6][i - h, j - h];
        }

        public static FMatrix StupidMultiply(FMatrix m1, FMatrix m2)                  // Stupid Matrix multiplication
        {
            if (m1.cols != m2.rows) throw new MException("Wrong dimensions of FMatrix!");

            FMatrix result = ZeroMatrix(m1.rows, m2.cols);
            for (int i = 0; i < result.rows; i++)
                for (int j = 0; j < result.cols; j++)
                    for (int k = 0; k < m1.cols; k++)
                        result[i, j] += m1[i, k] * m2[k, j];
            return result;
        }
        private static FMatrix Multiply(double n, FMatrix m)                          // Multiplication by constant n
        {
            FMatrix r = new FMatrix(m.rows, m.cols);
            for (int i = 0; i < m.rows; i++)
                for (int j = 0; j < m.cols; j++)
                    r[i, j] = m[i, j] * n;
            return r;
        }
        private static FMatrix Add(FMatrix m1, FMatrix m2)         // Sčítání matic
        {
            if (m1.rows != m2.rows || m1.cols != m2.cols) throw new MException("Matrices must have the same dimensions!");
            FMatrix r = new FMatrix(m1.rows, m1.cols);
            for (int i = 0; i < r.rows; i++)
                for (int j = 0; j < r.cols; j++)
                    r[i, j] = m1[i, j] + m2[i, j];
            return r;
        }

        //   O P E R A T O R S

        public static FMatrix operator -(FMatrix m)
        { return FMatrix.Multiply(-1, m); }

        public static FMatrix operator +(FMatrix m1, FMatrix m2)
        { return FMatrix.Add(m1, m2); }

        public static FMatrix operator -(FMatrix m1, FMatrix m2)
        { return FMatrix.Add(m1, -m2); }

        public static FMatrix operator *(FMatrix m1, FMatrix m2)
        { return FMatrix.StrassenMultiply(m1, m2); }

        public static FMatrix operator *(double n, FMatrix m)
        { return FMatrix.Multiply(n, m); }
    }

    //  The class for exceptions

    public class MException : Exception
    {
        public MException(string Message)
            : base(Message)
        { }
    }


}
