
using System;

namespace XCad.Structures {
    /// <summary>
    /// Represents the plane
    /// </summary>
    public class Plane {
        /// <summary>
        /// Root point of this plane
        /// </summary>
        public Vec3d Point { get; set; }

        /// <summary>
        /// Normal of this plane
        /// </summary>
        public Vec3d Normal { get; set; }

        /// <summary>
        /// Direction of this plane (X axis)
        /// </summary>
        public Vec3d Direction { get; set; }

        /// <summary>
        /// Reference vector of this plane (Y axis)
        /// </summary>
        public Vec3d Reference => Normal.Cross(Direction);

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="point">Origin point</param>
        /// <param name="normal">Plane normal</param>
        /// <param name="direction">Plane reference direction</param>
        public Plane(Vec3d point, Vec3d normal, Vec3d direction) {
            Point = point;
            Normal = normal;
            Direction = direction;
        }

        /// <summary>
        /// Creates a plane by point and normal
        /// </summary>
        /// <param name="point">Origin point</param>
        /// <param name="normal">Plane normal</param>
        public Plane(Vec3d point, Vec3d normal) {
            Point = point;
            Normal = normal.Normalize();

            // Find a direction (X axis) that is not parallel to the normal
            Vec3d dir = Vec3d.UnitX;
            if(Math.Abs(Normal.Dot(dir)) > 0.99) {
                dir = Vec3d.UnitY;
            }

            Direction = dir.ProjectOntoPlane(Normal).Normalize();
        }

        /// <summary>
        /// Finds the distance between plane and the point
        /// </summary>
        /// <param name="plane">Plane</param>
        /// <param name="point">Point coordinate</param>
        /// <returns>Shortest distance</returns>
        public double GetDistance(Vec3d point) => Math.Abs(Normal.Normalize().Dot(point) - Normal.Normalize().Dot(Point));

        /// <summary>
        /// Gets the transformation of this plane relative to the global XYZ
        /// </summary>
        /// <param name="plane">Plane</param>
        /// <returns>Transformation matrix</returns>
        public Transform GetTransformation() => new Transform(Direction, Reference, Normal, Point);
    }
}
