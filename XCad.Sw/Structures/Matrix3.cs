using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace XCad.Structures {
    public struct Matrix3 : IEquatable<Matrix3> {
        #region 字段
        public double M11, M12, M13;
        public double M21, M22, M23;
        public double M31, M32, M33;
        #endregion

        #region 静态常量
        public static readonly Matrix3 Identity = new Matrix3(1, 0, 0, 0, 1, 0, 0, 0, 1);
        public static readonly Matrix3 Zero = new Matrix3(0, 0, 0, 0, 0, 0, 0, 0, 0);
        #endregion

        #region 旋转矩阵（修复缺失方法）
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 RotationX(double angle) {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            return new Matrix3(
                1, 0, 0,
                0, cos, -sin,
                0, sin, cos
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 RotationY(double angle) {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            return new Matrix3(
                cos, 0, sin,
                0, 1, 0,
               -sin, 0, cos
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 RotationZ(double angle) {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            return new Matrix3(
                cos, -sin, 0,
                sin, cos, 0,
                0, 0, 1
            );
        }
        #endregion

        #region 构造函数
        public Matrix3(double m11, double m12, double m13,
                      double m21, double m22, double m23,
                      double m31, double m32, double m33) {
            M11 = m11; M12 = m12; M13 = m13;
            M21 = m21; M22 = m22; M23 = m23;
            M31 = m31; M32 = m32; M33 = m33;
        }
        #endregion

        #region 核心数学（纯函数 + InPlace 原地方法）
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3 Scale(double sc) => this * sc;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScaleInPlace(double sc) {
            M11 *= sc; M12 *= sc; M13 *= sc;
            M21 *= sc; M22 *= sc; M23 *= sc;
            M31 *= sc; M32 *= sc; M33 *= sc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3 Transpose() {
            return new Matrix3(
                M11, M21, M31,
                M12, M22, M32,
                M13, M23, M33
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TransposeInPlace() {
            double temp;

            temp = M12;
            M12 = M21;
            M21 = temp;

            temp = M13;
            M13 = M31;
            M31 = temp;

            temp = M23;
            M23 = M32;
            M32 = temp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3 Inverse() {
            double det = Determinant();
            if(Numeric.IsZero(det))
                throw new ArithmeticException("矩阵不可逆，行列式为0");

            double invDet = 1.0 / det;
            return new Matrix3(
                (M22 * M33 - M23 * M32) * invDet,
                (M13 * M32 - M12 * M33) * invDet,
                (M12 * M23 - M13 * M22) * invDet,
                (M23 * M31 - M21 * M33) * invDet,
                (M11 * M33 - M13 * M31) * invDet,
                (M13 * M21 - M11 * M23) * invDet,
                (M21 * M32 - M22 * M31) * invDet,
                (M12 * M31 - M11 * M32) * invDet,
                (M11 * M22 - M12 * M21) * invDet
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InvertInPlace() {
            double det = Determinant();
            if(Numeric.IsZero(det))
                throw new ArithmeticException("矩阵不可逆，行列式为0");
            double invDet = 1.0 / det;

            var m11 = (M22 * M33 - M23 * M32) * invDet;
            var m12 = (M13 * M32 - M12 * M33) * invDet;
            var m13 = (M12 * M23 - M13 * M22) * invDet;
            var m21 = (M23 * M31 - M21 * M33) * invDet;
            var m22 = (M11 * M33 - M13 * M31) * invDet;
            var m23 = (M13 * M21 - M11 * M23) * invDet;
            var m31 = (M21 * M32 - M22 * M31) * invDet;
            var m32 = (M12 * M31 - M11 * M32) * invDet;
            var m33 = (M11 * M22 - M12 * M21) * invDet;

            M11 = m11;
            M12 = m12;
            M13 = m13;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M31 = m31;
            M32 = m32;
            M33 = m33;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3 Multiply(in Matrix3 other) => this * other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MultiplyInPlace(in Matrix3 other) {
            this *= other;
        }
        #endregion

        #region 辅助方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetScale() => Math.Sqrt(M11 * M11 + M21 * M21 + M31 * M31);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Determinant() {
            return M11 * (M22 * M33 - M23 * M32)
                 - M12 * (M21 * M33 - M23 * M31)
                 + M13 * (M21 * M32 - M22 * M31);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Matrix3 other, double threshold) {
            return Numeric.IsEqual(M11, other.M11, threshold) &&
                   Numeric.IsEqual(M12, other.M12, threshold) &&
                   Numeric.IsEqual(M13, other.M13, threshold) &&
                   Numeric.IsEqual(M21, other.M21, threshold) &&
                   Numeric.IsEqual(M22, other.M22, threshold) &&
                   Numeric.IsEqual(M23, other.M23, threshold) &&
                   Numeric.IsEqual(M31, other.M31, threshold) &&
                   Numeric.IsEqual(M32, other.M32, threshold) &&
                   Numeric.IsEqual(M33, other.M33, threshold);
        }
        #endregion

        #region 运算符
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 operator *(in Matrix3 a, double b) {
            return new Matrix3(
                a.M11 * b, a.M12 * b, a.M13 * b,
                a.M21 * b, a.M22 * b, a.M23 * b,
                a.M31 * b, a.M32 * b, a.M33 * b
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 operator *(in Matrix3 a, in Matrix3 b) {
            return new Matrix3(
                a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
                a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
                a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,

                a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
                a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
                a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,

                a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
                a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
                a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3d operator *(in Matrix3 m, in Vec3d v) {
            return new Vec3d(
                m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z,
                m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z,
                m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z
            );
        }
        #endregion

        #region 相等比较
        public bool Equals(Matrix3 other) => Equals(other, Numeric.Tolerance);
        public override bool Equals(object obj) => obj is Matrix3 m && Equals(m);

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 31 + M11.GetHashCode();
            hash = hash * 31 + M12.GetHashCode();
            hash = hash * 31 + M13.GetHashCode();
            hash = hash * 31 + M21.GetHashCode();
            hash = hash * 31 + M22.GetHashCode();
            hash = hash * 31 + M23.GetHashCode();
            hash = hash * 31 + M31.GetHashCode();
            hash = hash * 31 + M32.GetHashCode();
            hash = hash * 31 + M33.GetHashCode();
            return hash;
        }

        public static bool operator ==(in Matrix3 a, in Matrix3 b) => a.Equals(b);
        public static bool operator !=(in Matrix3 a, in Matrix3 b) => !a.Equals(b);
        #endregion
    }
}