
using System;
using System.Runtime.CompilerServices;

namespace XCad.Structures {
    public static class Numeric {

        public const double DegToRad = Math.PI / 180.0;
        public const double RadToDeg = 180.0 / Math.PI;

        public const double Tolerance = 1E-12;
        public const double LinearTolerance = 1E-6;
        public const double AngularTolerance = 1E-9;

        public const double HalfPI = 0.5 * Math.PI;
        public const double PI = Math.PI;
        public const double TwoPI = 2 * Math.PI;

        private static double _epsilon = Tolerance;
        public static double Epsilon {
            get => _epsilon;
            set {
                if(value <= 0.0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Epsilon must be positive.");
                _epsilon = value;
            }
        }

        #region 基础判断 (内联优化)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(double number, double threshold = Tolerance) => Math.Abs(number) <= threshold;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOne(double number, double threshold = Tolerance) => IsZero(number - 1, threshold);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEqual(double a, double b, double threshold = Tolerance) => IsZero(a - b, threshold);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(double number, double threshold = Tolerance) => IsZero(number, threshold) ? 0 : Math.Sign(number);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(double a, double b, double threshold = Tolerance) {
            if(IsEqual(a, b, threshold)) return 0;
            return a > b ? 1 : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetween(double value, double min, double max, double threshold = Tolerance)
            => value > min - threshold && value < max + threshold;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double NormalizeAngle(double angle) {
            angle %= TwoPI;
            if(angle < 0) angle += TwoPI;
            return angle;
        }

        #endregion
    }
}
