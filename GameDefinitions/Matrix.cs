using System;
using System.Text;
using System.Collections.Generic;

namespace MRL.SSL.GameDefinitions
{
    public class MathMatrix
    {
        /// <summary>
        /// Class attributes/members
        /// </summary>
        int m_iRows;
        int m_iCols;
        double[,] m_iElement;

        /// <summary>
        /// Constructors
        /// </summary>	
        public MathMatrix(MathMatrix t)
        {
            m_iRows = t.m_iRows;
            m_iCols = t.m_iCols;
            m_iElement = new double[m_iRows, m_iCols];
            for (int i = 0; i < t.m_iRows; i++)
                for (int j = 0; j < t.m_iCols; j++)
                    m_iElement[i, j] = t.m_iElement[i, j];
        }

        /// <summary>
        /// Constructors
        /// </summary>	
        public MathMatrix(int[,] elements)
        {
            m_iRows = elements.GetLength(0);
            m_iCols = elements.GetLength(1); ;
            m_iElement = new double[m_iRows, m_iCols];
            for (int i = 0; i < elements.GetLength(0); i++)
                for (int j = 0; j < elements.GetLength(1); j++)
                    m_iElement[i, j] = elements[i, j];
        }

        public MathMatrix(double[,] elements)
        {
            m_iRows = elements.GetLength(0);
            m_iCols = elements.GetLength(1); ;
            m_iElement = new double[m_iRows, m_iCols];
            for (int i = 0; i < elements.GetLength(0); i++)
                for (int j = 0; j < elements.GetLength(1); j++)
                    m_iElement[i, j] = elements[i, j];
        }

        public MathMatrix(int iRows, int iCols)
        {
            m_iRows = iRows;
            m_iCols = iCols;
            m_iElement = new double[iRows, iCols];
        }

        public MathMatrix(int iRows, int iCols, double value)
            : this(iRows, iCols)
        {
            for (int i = 0; i < iRows; i++)
                for (int j = 0; j < iCols; j++)
                    m_iElement[i, j] = value;
        }

        public void FillRandom(double min, double max)
        {
            Random r = new Random();
            for (int i = 0; i < m_iRows; i++)
                for (int j = 0; j < m_iCols; j++)
                    m_iElement[i, j] = r.NextDouble() * (max - min) + min;
        }

        /// <summary>
        /// Properites
        /// </summary>
        public int Rows
        {
            get { return m_iRows; }
        }

        public int Cols
        {
            get { return m_iCols; }
        }

        /// <summary>
        /// Indexer
        /// </summary>
        public double this[int iRow, int iCol]		// matrix's index starts at 0,0
        {
            get
            {
                if (iRow < 0 || iRow > m_iRows - 1 || iCol < 0 || iCol > m_iCols - 1)
                    throw new MatrixException("Invalid index specified");
                return m_iElement[iRow, iCol];
            }
            set
            {
                if (iRow < 0 || iRow > m_iRows - 1 || iCol < 0 || iCol > m_iCols - 1)
                    throw new MatrixException("Invalid index specified");
                m_iElement[iRow, iCol] = value;
            }
        }
        /// <summary>
        /// The function returns the current Matrix object as a string
        /// </summary>
        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < m_iRows; i++)
            {
                for (int j = 0; j < m_iCols; j++)
                    str += m_iElement[i, j].ToString() + "\t";
                str += "\n";
            }
            return str;
        }
        /// <summary>
        /// The function return the Minor of element[Row,Col] of a Matrix object 
        /// </summary>
        public MathMatrix Minor(int iRow, int iCol)
        {
            MathMatrix minor = new MathMatrix(m_iRows - 1, m_iCols - 1);
            int m = 0, n = 0;
            for (int i = 0; i < m_iRows; i++)
            {
                if (i == iRow)
                    continue;
                n = 0;
                for (int j = 0; j < m_iCols; j++)
                {
                    if (j == iCol)
                        continue;
                    minor.m_iElement[m, n] = m_iElement[i, j];
                    n++;
                }
                m++;
            }
            return minor;
        }
        /// <summary>
        /// The function returns the determinent of the current Matrix object as double
        /// </summary>
        public double Determinent
        {
            get
            {
                double det = 0;
                if (m_iRows != m_iCols)
                    throw new MatrixException("Determinent of a non-square matrix doesn't exist");
                if (m_iRows == 1)
                    return m_iElement[0, 0];
                for (int j = 0; j < m_iCols; j++)
                    det += (m_iElement[0, j] * Minor(0, j).Determinent * (1 - 2 * (j % 2)));
                return det;
            }
        }

        /// <summary>
        /// The function multiplies the given row of the current matrix object by a double 
        /// </summary>
        public void MultiplyRow(int iRow, double frac)
        {
            for (int j = 0; j < this.m_iCols; j++)
                m_iElement[iRow, j] *= frac;
        }

        /// <summary>
        /// The function multiplies the given row of the current matrix object by an integer
        /// </summary>
        public void MultiplyRow(int iRow, int iNo)
        {
            this.MultiplyRow(iRow, iNo);
        }

        /// <summary>
        /// The function adds two m_iRows for current matrix object
        /// It performs the following calculation:
        /// iTargetRow = iTargetRow + iMultiple*iSecondRow
        /// </summary>
        public void AddRow(int iTargetRow, int iSecondRow, double iMultiple)
        {
            for (int j = 0; j < m_iCols; j++)
                m_iElement[iTargetRow, j] += (m_iElement[iSecondRow, j] * iMultiple);
        }
        /// <summary>
        /// The function interchanges two m_iRows of the current matrix object
        /// </summary>
        public void InterchangeRow(int iRow1, int iRow2)
        {
            for (int j = 0; j < this.m_iCols; j++)
            {
                double temp = m_iElement[iRow1, j];
                m_iElement[iRow1, j] = m_iElement[iRow2, j];
                m_iElement[iRow2, j] = temp;
            }
        }
        public static MathMatrix Quaternion2Rotation(double x, double y, double z, double w)
        {
            MathMatrix res = new MathMatrix(3, 3);
            double x2 = x * x;
            double y2 = y * y;
            double z2 = z * z;
            double xy = x * y;
            double wz = w * z;
            double xw = x * w;
            double xz = x * z;
            double wy = w * y;
            double yz = y * z;

            res[0, 0] = 1 - 2 * (y2 + z2); res[0, 1] = 2 * (xy - wz); res[0, 2] = 2 * (xz + wy);
            res[1, 0] = 2 * (xy + wz); res[1, 1] =1 - 2 * (x2 + z2); res[1, 2] = 2 * (yz - xw);
            res[2, 0] = 2 * (xz - wy); res[2, 1] = 2 * (yz + xw); res[2, 2] = 1 - 2 * (x2 + y2);

            return res;
        }
        /// <summary>
        /// The function concatenates the two given matrices column-wise
        /// </summary>
        public static MathMatrix Concatenate(MathMatrix matrix1, MathMatrix matrix2)
        {
            if (matrix1.m_iRows != matrix2.m_iRows)
                throw new MatrixException("Concatenation not possible");
            MathMatrix matrix = new MathMatrix(matrix1.m_iRows, matrix1.m_iCols + matrix2.m_iCols);
            for (int i = 0; i < matrix.m_iRows; i++)
                for (int j = 0; j < matrix.m_iCols; j++)
                {
                    if (j < matrix1.m_iCols)
                        matrix[i, j] = matrix1[i, j];
                    else
                        matrix[i, j] = matrix2[i, j - matrix1.m_iCols];
                }
            return matrix;
        }
        /// <summary>
        /// The function returns the inverse of the current matrix
        /// </summary>
        public MathMatrix Inverse
        {
            get
            {
                
                double d = Determinent;
                if (d == 0)
                    throw new MatrixException("Inverse of a singular matrix is not possible");
                return (Adjoint / d);
            }
        }
        public MathMatrix PsuedoInverse
        {
            get
            {
                //double d = Det.det(Transpose * this);
                //if (d == 0)
                //    throw new MatrixException("Inverse of a singular matrix is not possible");
                return GameDefinitions.Inverse.invert(Transpose * this) * Transpose;
            }
        }

        public MathMatrix Vectorize
        {
            get
            {
                MathMatrix result = new MathMatrix(Rows * Cols, 1);
                for (int i = 0; i < result.Rows; i++)
                    result[i, 0] = this[i % Rows, (int)(i / Rows)];
                return result;
            }
        }


        /// <summary>
        /// The function returns the adjoint of the current matrix
        /// </summary>
        public MathMatrix Adjoint
        {
            get
            {
                if (m_iRows != m_iCols)
                    throw new MatrixException("Adjoint of a non-square matrix does not exists");
                MathMatrix AdjointMatrix = new MathMatrix(m_iRows, m_iCols);
                for (int i = 0; i < m_iRows; i++)
                    for (int j = 0; j < m_iCols; j++)
                        AdjointMatrix.m_iElement[i, j] = ((1 - 2 * ((i + j) % 2))) * Minor(i, j).Determinent;
                AdjointMatrix = AdjointMatrix.Transpose;
                return AdjointMatrix;
            }
        }

        /// <summary>
        /// The function returns the transpose of a given matrix
        /// </summary>
        public MathMatrix Transpose
        {
            get
            {
                MathMatrix TransposeMatrix = new MathMatrix(m_iCols, m_iRows);
                for (int i = 0; i < TransposeMatrix.m_iRows; i++)
                    for (int j = 0; j < TransposeMatrix.m_iCols; j++)
                        TransposeMatrix.m_iElement[i, j] = m_iElement[j, i];
                return TransposeMatrix;
            }
        }

        /// <summary>
        /// The function duplicates the current Matrix object
        /// </summary>
        public MathMatrix Clone()
        {
            MathMatrix matrix = new MathMatrix(m_iRows, m_iCols);
            for (int i = 0; i < m_iRows; i++)
                for (int j = 0; j < m_iCols; j++)
                    matrix[i, j] = m_iElement[i, j];
            return matrix;
        }

        /// <summary>
        /// The function returns a Scalar Matrix of dimension ( Row x Col ) and scalar K
        /// </summary>
        public static MathMatrix ScalarMatrix(int im_iRows, int im_iCols, int K)
        {
            double zero = 0;
            double scalar = 1;
            MathMatrix matrix = new MathMatrix(im_iRows, im_iCols);
            for (int i = 0; i < im_iRows; i++)
                for (int j = 0; j < im_iCols; j++)
                {
                    if (i == j)
                        matrix[i, j] = scalar;
                    else
                        matrix[i, j] = zero;
                }
            return matrix;
        }

        /// <summary>
        /// The function returns an identity matrix of dimensions ( Row x Col )
        /// </summary>
        public static MathMatrix IdentityMatrix(int im_iRows, int im_iCols)
        {
            return ScalarMatrix(im_iRows, im_iCols, 1);
        }

        /// <summary>
        /// The function returns a Unit Matrix of dimension ( Row x Col )
        /// </summary>
        public static MathMatrix UnitMatrix(int im_iRows, int im_iCols)
        {
            MathMatrix matrix = new MathMatrix(im_iRows, im_iCols, 0);
            for (int i = 0; i < im_iRows; i++)
                matrix.m_iElement[i, i] = 1;
            return matrix;
        }

        /// <summary>
        /// The function returns a Null Matrix of dimension ( Row x Col )
        /// </summary>
        public static MathMatrix NullMatrix(int im_iRows, int im_iCols)
        {
            return new MathMatrix(im_iRows, im_iCols, 0);
        }

        /// <summary>
        /// Operators for the Matrix object
        /// includes -(unary), and binary opertors such as +,-,*,/
        /// </summary>
        public static MathMatrix operator -(MathMatrix matrix)
        { return MathMatrix.Negate(matrix); }

        public static MathMatrix operator +(MathMatrix matrix1, MathMatrix matrix2)
        { return MathMatrix.Add(matrix1, matrix2); }

        public static MathMatrix operator -(MathMatrix matrix1, MathMatrix matrix2)
        { return MathMatrix.Add(matrix1, -matrix2); }

        public static MathMatrix operator *(MathMatrix matrix1, MathMatrix matrix2)
        { return MathMatrix.Multiply(matrix1, matrix2); }

        public static MathMatrix operator &(MathMatrix A, MathMatrix B)
        {
            MathMatrix C = new MathMatrix(A.Rows * B.Rows, A.Cols * B.Cols);
            int m = A.Rows
                ,p = B.Rows
                ,n = A.Cols
                ,q = B.Cols;
            for (int i = 0; i < C.Rows; i++)
                for (int j = 0; j < C.Cols; j++)
                    C[i, j] = A[(int)(i / p), (int)(j / q)] * B[i % p, j % q];
            return C;
        }

        public static MathMatrix operator *(MathMatrix matrix1, int iNo)
        { return MathMatrix.Multiply(matrix1, iNo); }

        public static MathMatrix operator *(MathMatrix matrix1, double frac)
        { return MathMatrix.Multiply(matrix1, frac); }

        public static MathMatrix operator *(int iNo, MathMatrix matrix1)
        { return MathMatrix.Multiply(matrix1, iNo); }

        public static MathMatrix operator *(double frac, MathMatrix matrix1)
        { return MathMatrix.Multiply(matrix1, frac); }

        public static MathMatrix operator /(MathMatrix matrix1, int iNo)
        { return MathMatrix.Multiply(matrix1, 1.0 / iNo); }

        public static MathMatrix operator /(MathMatrix matrix1, double frac)
        { return MathMatrix.Multiply(matrix1, 1.0 / frac); }


        /// <summary>
        /// Internal Fucntions for the above operators
        /// </summary>
        private static MathMatrix Negate(MathMatrix matrix)
        {
            return MathMatrix.Multiply(matrix, -1);
        }

        private static MathMatrix Add(MathMatrix matrix1, MathMatrix matrix2)
        {
            if (matrix1.m_iRows != matrix2.m_iRows || matrix1.m_iCols != matrix2.m_iCols)
                throw new MatrixException("Operation not possible");
            MathMatrix result = new MathMatrix(matrix1.m_iRows, matrix1.m_iCols);
            for (int i = 0; i < result.m_iRows; i++)
                for (int j = 0; j < result.m_iCols; j++)
                    result.m_iElement[i, j] = matrix1.m_iElement[i, j] + matrix2.m_iElement[i, j];
            return result;
        }

        private static MathMatrix Multiply(MathMatrix matrix1, MathMatrix matrix2)
        {
            if (matrix1.m_iCols != matrix2.m_iRows)
                throw new MatrixException("Operation not possible");
            //MathMatrix result = MathMatrix.NullMatrix(matrix1.m_iRows, matrix2.m_iCols);
            MathMatrix result = new MathMatrix(matrix1.m_iRows, matrix2.m_iCols);
            for (int i = 0; i < result.m_iRows; i++)
                for (int j = 0; j < result.m_iCols; j++)
                    for (int k = 0; k < matrix1.m_iCols; k++)
                        result.m_iElement[i, j] += matrix1.m_iElement[i, k] * matrix2.m_iElement[k, j];
            return result;
        }

        private static MathMatrix Multiply(MathMatrix matrix, int iNo)
        {
            MathMatrix result = new MathMatrix(matrix.m_iRows, matrix.m_iCols);
            for (int i = 0; i < matrix.m_iRows; i++)
                for (int j = 0; j < matrix.m_iCols; j++)
                    result.m_iElement[i, j] = matrix.m_iElement[i, j] * iNo;
            return result;
        }

        private static MathMatrix Multiply(MathMatrix matrix, double frac)
        {
            MathMatrix result = new MathMatrix(matrix.m_iRows, matrix.m_iCols);
            for (int i = 0; i < matrix.m_iRows; i++)
                for (int j = 0; j < matrix.m_iCols; j++)
                    result.m_iElement[i, j] = matrix.m_iElement[i, j] * frac;
            return result;
        }



    }

    /// <summary>
    /// Exception class for Matrix class, derived from Exception
    /// </summary>
    public class MatrixException : Exception
    {
        public MatrixException()
         : base()
        { }

        public MatrixException(string Message)
            : base(Message)
        { }

        public MatrixException(string Message, Exception InnerException)
            : base(Message, InnerException)
        { }
    }



    ///////////////////////////////////////////////////////////////////////////
//                                                                       //
// Program file name: Det.java                                           //
//                                                                       //
// © Tao Pang 2006                                                       //
//                                                                       //
// Last modified: January 18, 2006                                       //
//                                                                       //
// (1) This Java program is part of the book, "An Introduction to        //
//     Computational Physics, 2nd Edition," written by Tao Pang and      //
//     published by Cambridge University Press on January 19, 2006.      //
//                                                                       //
// (2) No warranties, express or implied, are made for this program.     //
//                                                                       //
///////////////////////////////////////////////////////////////////////////

// An example of evaluating the determinant of a matrix
// via the partial-pivoting Gaussian elimination.


public class  Det 
{
  

// Method to evaluate the determinant of a matrix.

  public static double det(MathMatrix b)
   {
       MathMatrix a = new MathMatrix(b);
       int n = a.Rows;
    int[] index = new int[n];


 // Transform the matrix into an upper triangle
    gaussian(a, index);

 // Take the product of the diagonal elements
    double d = 1;
    for (int i=0; i<n; ++i) d = d*a[index[i],i];

 // Find the sign of the determinant
    int sgn = 1;
    for (int i=0; i<n; ++i) {
      if (i != index[i]) {
        sgn = -sgn;
        int j = index[i];
        index[i] = index[j];
        index[j] = j;
      }
    }
    return sgn*d;
  }

// Method to carry out the partial-pivoting Gaussian
// elimination.  Here index[] stores pivoting order.

  public static void gaussian(MathMatrix a,int[] index) 
   {
    int n = a.Rows;
    double[] c = new double[n];

 // Initialize the index
    for (int i=0; i<n; ++i) index[i] = i;

 // Find the rescaling factors, one from each row
    for (int i=0; i<n; ++i) {
      double c1 = 0;
      for (int j=0; j<n; ++j) {
        double c0 = Math.Abs(a[i,j]);
        if (c0 > c1) c1 = c0;
      }
      c[i] = c1;
    }

 // Search the pivoting element from each column
    int k = 0;
    for (int j=0; j<n-1; ++j) {
      double pi1 = 0;
      for (int i=j; i<n; ++i) {
        double pi0 = Math.Abs(a[index[i],j]);
        pi0 /= c[index[i]];
        if (pi0 > pi1) {
          pi1 = pi0;
          k = i;
        }
      }

   // Interchange rows according to the pivoting order
      int itmp = index[j];
      index[j] = index[k];
      index[k] = itmp;
      for (int i=j+1; i<n; ++i) {
        double pj = a[index[i],j]/a[index[j],j];

     // Record pivoting ratios below the diagonal
        a[index[i], j] = pj;

     // Modify other elements accordingly
        for (int l = j + 1; l < n; ++l)
            a[index[i], l] -= pj * a[index[j], l];
      }
    }
  }
}



///////////////////////////////////////////////////////////////////////////
//                                                                       //
// Program file name: Inverse.java                                       //
//                                                                       //
// © Tao Pang 2006                                                       //
//                                                                       //
// Last modified: January 18, 2006                                       //
//                                                                       //
// (1) This Java program is part of the book, "An Introduction to        //
//     Computational Physics, 2nd Edition," written by Tao Pang and      //
//     published by Cambridge University Press on January 19, 2006.      //
//                                                                       //
// (2) No warranties, express or implied, are made for this program.     //
//                                                                       //
///////////////////////////////////////////////////////////////////////////

// An example of performing matrix inversion through the
// partial-pivoting Gaussian elimination.


public class Inverse {
 

  public static MathMatrix invert(MathMatrix a) {
    int n = a.Rows;
    MathMatrix x = new MathMatrix(n,n);
    MathMatrix b = new MathMatrix(n,n);
    int[] index = new int[n];
    for (int i=0; i<n; ++i) b[i,i] = 1;

 // Transform the matrix into an upper triangle
    gaussian(a, index);

 // Update the matrix b[i][j] with the ratios stored
    for (int i=0; i<n-1; ++i)
      for (int j=i+1; j<n; ++j)
        for (int k=0; k<n; ++k)
          b[index[j],k]
            -= a[index[j],i]*b[index[i],k];

 // Perform backward substitutions
    for (int i=0; i<n; ++i) {
      x[n-1,i] = b[index[n-1],i]/a[index[n-1],n-1];
      for (int j=n-2; j>=0; --j) {
        x[j,i] = b[index[j],i];
        for (int k=j+1; k<n; ++k) {
          x[j,i] -= a[index[j],k]*x[k,i];
        }
        x[j,i] /= a[index[j],j];
      }
    }
  return x;
  }

// Method to carry out the partial-pivoting Gaussian
// elimination.  Here index[] stores pivoting order.

  public static void gaussian(MathMatrix a, int[] index)
  {
      int n = a.Rows;
      double[] c = new double[n];

      // Initialize the index
      for (int i = 0; i < n; ++i) index[i] = i;

      // Find the rescaling factors, one from each row
      for (int i = 0; i < n; ++i)
      {
          double c1 = 0;
          for (int j = 0; j < n; ++j)
          {
              double c0 = Math.Abs(a[i, j]);
              if (c0 > c1) c1 = c0;
          }
          c[i] = c1;
      }

      // Search the pivoting element from each column
      int k = 0;
      for (int j = 0; j < n - 1; ++j)
      {
          double pi1 = 0;
          for (int i = j; i < n; ++i)
          {
              double pi0 = Math.Abs(a[index[i], j]);
              pi0 /= c[index[i]];
              if (pi0 > pi1)
              {
                  pi1 = pi0;
                  k = i;
              }
          }

          // Interchange rows according to the pivoting order
          int itmp = index[j];
          index[j] = index[k];
          index[k] = itmp;
          for (int i = j + 1; i < n; ++i)
          {
              double pj = a[index[i], j] / a[index[j], j];

              // Record pivoting ratios below the diagonal
              a[index[i], j] = pj;

              // Modify other elements accordingly
              for (int l = j + 1; l < n; ++l)
                  a[index[i], l] -= pj * a[index[j], l];
          }
      }
  }
}



}