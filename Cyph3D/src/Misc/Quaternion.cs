using System;
using GlmSharp;

namespace Cyph3D.Misc
{
	public readonly struct Quaternion
	{
		public double X { get; }
		public double Y { get; }
		public double Z { get; }
		public double W { get; }

		public Quaternion(double x, double y, double z, double w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public Quaternion(dvec4 vec) : this(vec.x, vec.y, vec.z, vec.w)
		{
			
		}

		// http://www.euclideanspace.com/maths/geometry/rotations/conversions/eulerToQuaternion/index.htm
		public Quaternion(double eulerX, double eulerY, double eulerZ)
		{
			// Assuming the angles are in radians.
			double c1 = Math.Cos(eulerY/2);
			double s1 = Math.Sin(eulerY/2);
			double c2 = Math.Cos(eulerZ/2);
			double s2 = Math.Sin(eulerZ/2);
			double c3 = Math.Cos(eulerX/2);
			double s3 = Math.Sin(eulerX/2);
			double c1c2 = c1*c2;
			double s1s2 = s1*s2;
			
			W = c1c2*c3 - s1s2*s3;
			X = c1c2*s3 + s1s2*c3;
			Y = s1*c2*c3 + c1*s2*s3;
			Z = c1*s2*c3 - s1*c2*s3;
		}

		public Quaternion(dvec3 euler) : this(euler.x, euler.y, euler.z)
		{
			
		}
		
		// http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/index.htm
		public vec3 EulerAngles
		{
			get
			{
				dvec3 euler = dvec3.Zero;
				
				double test = X*Y + Z*W;
				if (test > 0.499) { // singularity at north pole
					euler.x = 0;
					euler.y = 2 * Math.Atan2(X, W);
					euler.z = Math.PI/2;
				}
				else if (test < -0.499) { // singularity at south pole
					euler.x = 0;
					euler.y = -2 * Math.Atan2(X, W);
					euler.z = -Math.PI/2;
				}
				else
				{
					double sqx = X*X;
					double sqy = Y*Y;
					double sqz = Z*Z;
					euler.y = Math.Atan2(2 * Y * W - 2 * X * Z, 1 - 2 * sqy - 2 * sqz);
					euler.z = Math.Asin(2 * test);
					euler.x = Math.Atan2(2 * X * W - 2 * Y * Z, 1 - 2 * sqx - 2 * sqz);
				}

				return (vec3)euler;
			}
		}

		public Quaternion Normalized => this / this.Norm;
		
		public double Norm => glm.Sqrt(X*X + Y*Y + Z*Z + W*W);
		
		public Quaternion Conjugate => new Quaternion(-X, -Y, -Z, W);

		// public mat4 ToMat4
		// {
		// 	get
		// 	{
		// 		float x = (float) X;
		// 		float y = (float) Y;
		// 		float z = (float) Z;
		// 		float w = (float) W;
		// 		
		// 		return new mat4
		// 		(
		// 			new vec4(w, -x, -y, -z),
		// 			new vec4(x, w, -z, y),
		// 			new vec4(y, z, w, -x),
		// 			new vec4(z, -y, x, w)
		// 		);
		// 	}
		// }
		
		public mat4 ToMat4 => new mat4(ToMat3);
		
		public mat3 ToMat3
		{
			get
			{
				float x = (float) X;
				float y = (float) Y;
				float z = (float) Z;
				float w = (float) W;
				
				return new mat3(
					1 - 2*y*y - 2*z*z,     2*x*y - 2*z*w,     2*x*z + 2*y*w,
					    2*x*y + 2*z*w, 1 - 2*x*x - 2*z*z,     2*y*z - 2*x*w,
					    2*x*z - 2*y*w,     2*y*z + 2*x*w, 1 - 2*x*x - 2*y*y
				);
			}
		}

		public Quaternion Dot(Quaternion other)
		{
			return new Quaternion(
				X * other.X,
				Y * other.Y,
				Z * other.Z,
				W * other.W
			);
		}
		
		public Quaternion Rotate(float angle, vec3 axes)
		{
			return this * new Quaternion(
				new vec4(
					glm.Sin(angle / 2) * axes,
					glm.Cos(angle / 2)
				)
			);
		}
		
		public bool Equals(Quaternion other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			return obj is Quaternion other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y, Z, W);
		}

		public static Quaternion operator +(Quaternion l, Quaternion r)
		{
			return new Quaternion(
				l.X + r.X,
				l.Y + r.Y,
				l.Z + r.Z,
				l.W + r.W
				);
		}
		
		public static Quaternion operator *(Quaternion l, Quaternion r)
		{
			return new Quaternion(
				l.W*r.X + l.X*r.W + l.Y*r.Z - l.Z*r.Y,
				l.W*r.Y - l.X*r.Z + l.Y*r.W + l.Z*r.X,
				l.W*r.Z + l.X*r.Y - l.Y*r.X + l.Z*r.W, 
				l.W*r.W - l.X*r.X - l.Y*r.Y - l.Z*r.Z
			);
		}
		
		// ReSharper disable CompareOfFloatsByEqualityOperator
		public static bool operator ==(Quaternion l, Quaternion r)
		{
			return l.X == r.X &&
			       l.Y == r.Y &&
			       l.Z == r.Z &&
			       l.W == r.W;
		}

		public static bool operator !=(Quaternion l, Quaternion r)
		{
			return !(l == r);
		}

		public static Quaternion operator /(Quaternion q, double factor)
		{
			return new Quaternion(
				q.X / factor,
				q.Y / factor,
				q.Z / factor,
				q.W / factor
			);
		}
	}
}