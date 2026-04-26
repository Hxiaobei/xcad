using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace XCad.Structures {
    /// <summary>
    /// Structure representing a 3D Vector/Point (Double Precision)
    /// </summary>
    [DebuggerDisplay("{" + nameof(X) + "}; {" + nameof(Y) + "}; {" + nameof(Z) + "}")]
    public struct Vec3d : IEquatable<Vec3d>, IFormattable {
        public double X, Y, Z;

        public static readonly Vec3d Zero = new Vec3d(0.0, 0.0, 0.0);
        public static readonly Vec3d UnitX = new Vec3d(1.0, 0.0, 0.0);
        public static readonly Vec3d UnitY = new Vec3d(0.0, 1.0, 0.0);
        public static readonly Vec3d UnitZ = new Vec3d(0.0, 0.0, 1.0);
        public static readonly Vec3d NaN = new Vec3d(double.NaN, double.NaN, double.NaN);

        #region 构造函数与索引器

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d(double value) : this(value, value, value) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d(double x, double y, double z) { X = x; Y = y; Z = z; }

        public Vec3d(double[] array) {
            if(array == null) throw new ArgumentNullException(nameof(array));
            if(array.Length != 3) throw new ArgumentOutOfRangeException(nameof(array), "数组维度必须为3。");
            X = array[0]; Y = array[1]; Z = array[2];
        }

        public double this[int index] {
            get {
                switch(index) {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    default: throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
            set {
                switch(index) {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        #endregion

        #region Core Math Operations
        public bool IsZero() => Numeric.IsZero(X) && Numeric.IsZero(Y) && Numeric.IsZero(Z);
        public bool IsNaN() => double.IsNaN(X) && double.IsNaN(Y) && double.IsNaN(Z);
        public double LengthSquared() => X * X + Y * Y + Z * Z;
        public double Modulus() => Math.Sqrt(LengthSquared());

        /// <summary>
        /// Returns a normalized version of this vector (Length = 1)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d Normalize() {
            var len = Modulus();
            if(Numeric.IsZero(len)) return Zero; // 防止除以零
            return new Vec3d(X / len, Y / len, Z / len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NormalizeInPlace() {
            var len = Modulus();
            if(Numeric.IsZero(len)) return;
            X /= len; Y /= len; Z /= len;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Dot(Vec3d v) => X * v.X + Y * v.Y + Z * v.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d Cross(Vec3d v) => new Vec3d(
            Y * v.Z - Z * v.Y,
            Z * v.X - X * v.Z,
            X * v.Y - Y * v.X
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Distance(Vec3d pt) => (this - pt).Modulus();

        public static Vec3d MidPoint(Vec3d p1, Vec3d p2) => new Vec3d((p1.X + p2.X) * 0.5, (p1.Y + p2.Y) * 0.5, (p1.Z + p2.Z) * 0.5);

        /// <summary>
        /// Creates perpendicular vector
        /// </summary>
        /// <param name="dir">Vector to base on</param>
        /// <param name="tol">Tolerance</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d CreateAnyPerpendicular(double tol = Numeric.AngularTolerance) {
            Vec3d refDir;
            var zVec = new Vec3d(0, 0, 1);

            if(IsParallel(zVec, tol)) {
                refDir = new Vec3d(1, 0, 0);
            } else {
                refDir = Cross(zVec);
            }

            return refDir;
        }

        /// <summary>
        /// Finds the angle between vectors
        /// </summary>
        /// <param name="thisVec">First vector</param>
        /// <param name="otherVec">Other vector</param>
        /// <param name="tol">Tolerance</param>
        /// <returns>Angle in radians</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetAngle(Vec3d otherVec, double tol = Numeric.AngularTolerance) {
            double m1 = Modulus();
            double m2 = otherVec.Modulus();

            // 1. 处理零向量情况，防止除以零
            if(m1 < tol || m2 < tol) return 0;

            double denominator = m1 * m2;
            double dot = Dot(otherVec);

            // 2. 计算余弦值
            double cosine = dot / denominator;

            // 3. 严格限幅在 [-1, 1] 之间
            if(cosine > 1.0) cosine = 1.0;
            else if(cosine < -1.0) cosine = -1.0;

            return Math.Acos(cosine);
        }

        /// <summary>
        /// Finds the angle between this vector and plane
        /// </summary>
        /// <param name="vec">Vector</param>
        /// <param name="plane">Plane</param>
        /// <returns>Angle</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetAngle(Plane plane) => Math.PI / 2 - GetAngle(plane.Normal);

        /// <summary>
        /// Gets the full angle (360 degress) between 2 vectors on the plane
        /// </summary>
        /// <param name="thisVec">First vector</param>
        /// <param name="otherVec">Second vector</param>
        /// <param name="plane">Plane to get angle at</param>
        /// <param name="tol">Vector tolerance</param>
        /// <returns>Angle in radians</returns>
        /// <exception cref="Exception">Vector must not be perpendicular to the plane</exception>
        /// <remarks>Vectors will be projected onto the plane</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetAngleOnPlane(Vec3d otherVec, Plane plane, double tol = Numeric.Tolerance) {
            // 1. 获取平面的逆变换矩阵（将全局空间转为平面局部空间）
            var invTransform = plane.GetTransformation();
            invTransform.InvertInPlace();

            // 2. 将两个向量转换到平面坐标系（转换后 Z 分量理论上应接近 0）
            var v1Local = this.MulVector(invTransform);
            var v2Local = otherVec.MulVector(invTransform);

            // 3. 计算在平面上的投影模长（仅看 X 和 Y）
            double m1 = Math.Sqrt(v1Local.X * v1Local.X + v1Local.Y * v1Local.Y);
            double m2 = Math.Sqrt(v2Local.X * v2Local.X + v2Local.Y * v2Local.Y);

            if(m1 < tol || m2 < tol) {
                throw new Exception("Vector projection onto plane is too short (perpendicular).");
            }

            // 4. 使用行列式(Det)和点积(Dot)计算带符号夹角
            // 在 2D 中：dot = x1*x2 + y1*y2, det = x1*y2 - y1*x2
            var dot = v1Local.X * v2Local.X + v1Local.Y * v2Local.Y;
            var det = v1Local.X * v2Local.Y - v1Local.Y * v2Local.X;

            // Atan2(y, x) 能够自动处理四个象限，返回 [-π, π]
            return Math.Atan2(det, dot);
        }

        /// <summary>
        /// Checks if 2 vectors are parallel
        /// </summary>
        /// <param name="firstVec">First vector</param>
        /// <param name="secondVec">Second vector</param>
        /// <param name="tol">Angle tolerance</param>
        /// <returns>True if vectors are parallel, False if not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsVertical(Vec3d v2, double tol = Numeric.AngularTolerance)
            => Math.Abs(Dot(v2)) < (Modulus() * v2.Modulus() * tol);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsParallel(Vec3d secondVec, double tol = Numeric.AngularTolerance) {

            double m1 = Modulus();
            double m2 = secondVec.Modulus();

            if(m1 < 1e-12 || m2 < 1e-12) return false;

            double dot = Dot(secondVec);
            double cosAbs = Math.Abs(dot / (m1 * m2));

            return (1.0 - cosAbs) < tol;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScaleInPlace(double scalar) {
            X *= scalar;
            Y *= scalar;
            Z *= scalar;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d Scale(double scalar) => this * scalar;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveInPlace(Vec3d dir, double dist) {
            var normalizedDir = dir.Normalize();
            X += normalizedDir.X * dist;
            Y += normalizedDir.Y * dist;
            Z += normalizedDir.Z * dist;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d Move(Vec3d dir, double dist) {
            dir.NormalizeInPlace();
            return this + dir * dist;
        }

        public double[] ToArray() => new[] { X, Y, Z };
        #endregion

        #region Matrix Transformations (明确区分点和向量)

        /// <summary>
        /// Transforms this Vec3d as a POINT (w=1). 
        /// This WILL apply the translation (movement) part of the matrix.
        /// </summary>
        public Vec3d MulPoint(in Transform matrix) => matrix.MulPoint(this);

        /// <summary>
        /// Transforms this Vec3d as a VECTOR (w=0).
        /// This will IGNORE the translation (movement) part of the matrix, affecting only rotation and scale.
        /// </summary>
        public Vec3d MulVector(in Transform matrix) => matrix.MulVector(this);

        #endregion

        #region Advanced Geometry (Planes & Rotations)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d ProjectOntoPlane(Vec3d normal) {
            var n = normal.Normalize();
            return this - n * Dot(n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d Project(Plane plane, Vec3d? projectDir = null) {
            var n = plane.Normal.Normalize();
            var d = (projectDir ?? n).Normalize();
            var dot = d.Dot(n);

            if(Math.Abs(dot) < 1e-10) return this;

            var t = n.Dot(plane.Point - this) / dot;
            return this + d * t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3d RotateAroundAxis(Vec3d axis, double angleRad) {
            if(axis.IsZero()) return this; // 新增：零轴直接返回原向量
            var k = axis.Normalize();
            var cos = Math.Cos(angleRad);
            var sin = Math.Sin(angleRad);

            // Rodrigues' rotation formula
            return this * cos + k.Cross(this) * sin + k * (k.Dot(this) * (1 - cos));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CalculateAngleOnPlane(Vec3d other, Vec3d normal, double tol = 1e-12) {
            // 1. 获取单位法线
            var n = normal.Normalize();

            // 2. 投影到平面
            var a = this.ProjectOntoPlane(n);
            var b = other.ProjectOntoPlane(n);

            // 3. 安全检查：如果投影向量太短，说明无法在该平面计算夹角
            double ma = a.Modulus();
            double mb = b.Modulus();
            if(ma < tol || mb < tol) return 0;

            // 4. 单位化
            a = a.Scale(1.0 / ma);
            b = b.Scale(1.0 / mb);

            // 5. 计算夹角大小
            var dot = a.Dot(b);
            if(dot > 1.0) dot = 1.0;
            else if(dot < -1.0) dot = -1.0;

            var angle = Math.Acos(dot);

            // 6. 判定方向：利用叉积与法线的点积
            // 如果 (a x b) · n < 0，说明是顺时针，角度变负
            var cross = a.Cross(b);
            if(n.Dot(cross) < 0) {
                angle = -angle;
            }

            return angle;
        }

        #endregion

        #region 运算符重载
        public static bool operator ==(Vec3d u, Vec3d v) => u.Equals(v);
        public static bool operator !=(Vec3d u, Vec3d v) => !u.Equals(v);
        public static Vec3d operator +(Vec3d u, Vec3d v) => new Vec3d(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        public static Vec3d operator -(Vec3d u, Vec3d v) => new Vec3d(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        public static Vec3d operator -(Vec3d u) => new Vec3d(-u.X, -u.Y, -u.Z);
        public static Vec3d operator *(Vec3d u, double a) => new Vec3d(u.X * a, u.Y * a, u.Z * a);
        public static Vec3d operator *(double a, Vec3d u) => new Vec3d(u.X * a, u.Y * a, u.Z * a);
        public static Vec3d operator *(Vec3d u, Vec3d v) => new Vec3d(u.X * v.X, u.Y * v.Y, u.Z * v.Z);
        public static Vec3d operator /(Vec3d u, double a) {
            double inv = 1.0 / a;
            return new Vec3d(u.X * inv, u.Y * inv, u.Z * inv);
        }
        #endregion

        #region 重写
        public override bool Equals(object obj) => obj is Vec3d other && Equals(other);
        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Z.GetHashCode();
                return hash;
            }
        }
        public override string ToString() => ToString(null, CultureInfo.CurrentCulture);
        public string ToString(IFormatProvider provider) => ToString(null, provider);
        public string ToString(string format, IFormatProvider provider) {
            var culture = (provider as CultureInfo) ?? CultureInfo.CurrentCulture;
            string sep = culture.TextInfo.ListSeparator;
            return string.Format("{0}{3} {1}{3} {2}", X.ToString(format, provider), Y.ToString(format, provider), Z.ToString(format, provider), sep);
        }
        #endregion
        public bool Equals(Vec3d other) => Equals(other, Numeric.Epsilon);
        public bool Equals(Vec3d other, double threshold) => Numeric.IsEqual(X, other.X, threshold) &&
            Numeric.IsEqual(Y, other.Y, threshold) && Numeric.IsEqual(Z, other.Z, threshold);
    }
}