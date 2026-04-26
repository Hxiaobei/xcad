using System;
using System.Runtime.CompilerServices;

namespace XCad.Structures {
    public struct Transform : IEquatable<Transform> {
        public Matrix3 Rotation;
        public Vec3d Trans;

        public static readonly Transform Identity = new Transform(Matrix3.Identity, Vec3d.Zero);

        #region 构造函数
        public Transform(double[] data) {
            if(data == null || data.Length < 16)
                throw new ArgumentException("变换矩阵数组长度必须为16", nameof(data));

            double scale = data[12];
            Rotation = new Matrix3(
                data[0] * scale, data[3] * scale, data[6] * scale,
                data[1] * scale, data[4] * scale, data[7] * scale,
                data[2] * scale, data[5] * scale, data[8] * scale
            );
            Trans = new Vec3d(data[9], data[10], data[11]);
        }

        public Transform(in Matrix3 matrix, Vec3d transVec) {
            Rotation = matrix;
            Trans = transVec;
        }

        public Transform(Vec3d xVec, Vec3d yVec, Vec3d zVec, Vec3d transVec) {
            Vec3d nx = xVec.Normalize();
            Vec3d ny = yVec.Normalize();
            Vec3d nz = zVec.Normalize();
            Rotation = new Matrix3(nx.X, ny.X, nz.X, nx.Y, ny.Y, nz.Y, nx.Z, ny.Z, nz.Z);
            Trans = transVec;
        }
        #endregion

        #region 核心变换（纯函数 + InPlace 原地方法）
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d MulPoint(Vec3d pt) => Rotation * pt + Trans;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d MulVector(Vec3d vec) => Rotation * vec;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform Scale(double factor) => new Transform(Rotation * factor, Trans * factor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScaleInPlace(double factor) {
            Rotation.ScaleInPlace(factor);
            Trans.ScaleInPlace(factor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform MoveTransform(Vec3d translation) => new Transform(Rotation, Trans + translation);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveInPlace(Vec3d translation) {
            Trans += translation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform MoveLocally(Vec3d localOffset) => new Transform(Rotation, Trans + Rotation * localOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveLocallyInPlace(Vec3d localOffset) {
            Trans += Rotation * localOffset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform Inverse() {
            Matrix3 invRot = Rotation.Inverse();
            return new Transform(invRot, invRot * (-Trans));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InvertInPlace() {
            Matrix3 invRot = Rotation.Inverse();
            Trans = invRot * (-Trans);
            Rotation = invRot;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform Multiply(in Transform other) => this * other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MultiplyInPlace(in Transform other) {
            Trans = Rotation * other.Trans + Trans;
            Rotation *= other.Rotation;
        }
        #endregion

        #region 静态工厂方法
        public static Transform Translation(Vec3d translation) => new Transform(Matrix3.Identity, translation);
        public static Transform RotationX(double angle) => new Transform(Matrix3.RotationX(angle), Vec3d.Zero);
        public static Transform RotationY(double angle) => new Transform(Matrix3.RotationY(angle), Vec3d.Zero);
        public static Transform RotationZ(double angle) => new Transform(Matrix3.RotationZ(angle), Vec3d.Zero);

        public static Transform FromCoordinateSystems(Vec3d sourceX, Vec3d sourceY, Vec3d sourceOrigin, Vec3d targetX, Vec3d targetY, Vec3d targetOrigin) {
            Func<Vec3d, Vec3d, Matrix3> CreateRot = (x, y) => {
                Vec3d z = x.Cross(y);
                return new Matrix3(x.X, y.X, z.X, x.Y, y.Y, z.Y, x.Z, y.Z, z.Z);
            };

            Matrix3 src = CreateRot(sourceX, sourceY);
            Matrix3 tgt = CreateRot(targetX, targetY);
            Matrix3 rot = tgt * src.Transpose();
            return new Transform(rot, targetOrigin - rot * sourceOrigin);
        }

        public static Transform FromLocalToWorld(Vec3d xAxis, Vec3d yAxis, Vec3d origin) {
            Vec3d z = xAxis.Cross(yAxis);
            return new Transform(new Matrix3(xAxis.X, yAxis.X, z.X, xAxis.Y, yAxis.Y, z.Y, xAxis.Z, yAxis.Z, z.Z), origin);
        }
        #endregion

        #region 相等比较（C#7.3 兼容：传统哈希）
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(in Transform a, in Transform b, double threshold) {
            return a.Rotation.Equals(b.Rotation, threshold) && a.Trans.Equals(b.Trans, threshold);
        }

        public bool Equals(Transform other) => Equals(this, other, Numeric.Tolerance);

        public override bool Equals(object obj) => obj is Transform t && Equals(t);

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 31 + Rotation.GetHashCode();
            hash = hash * 31 + Trans.GetHashCode();
            return hash;
        }

        public static bool operator ==(in Transform a, in Transform b) => a.Equals(b);
        public static bool operator !=(in Transform a, in Transform b) => !a.Equals(b);
        #endregion

        #region 组合运算符
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Transform operator *(in Transform a, in Transform b) {
            return new Transform(a.Rotation * b.Rotation, a.Rotation * b.Trans + a.Trans);
        }
        #endregion

        public override string ToString() => $"{Rotation}\n[{Trans}]";
    }
}