// Decompiled with JetBrains decompiler
// Type: UnityEngine.QuaternionD
// Assembly: UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 252AE736-B8DE-4BA1-9858-0D124F41CDFC
// Assembly location: C:\Program Files\Unity5\Editor\Data\Managed\UnityEngine.dll

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;

namespace UnityEngine
{
	public struct QuaternionD
	{
		public const double kEpsilon = 1E-06;
		public double x;
		public double y;
		public double z;
		public double w;

		public double this[int index]
		{
			get
			{
				switch (index)
				{
				case 0:
					return this.x;
				case 1:
					return this.y;
				case 2:
					return this.z;
				case 3:
					return this.w;
				default:
					throw new IndexOutOfRangeException("Invalid QuaternionD index!");
				}
			}
			set
			{
				switch (index)
				{
				case 0:
					this.x = value;
					break;
				case 1:
					this.y = value;
					break;
				case 2:
					this.z = value;
					break;
				case 3:
					this.w = value;
					break;
				default:
					throw new IndexOutOfRangeException("Invalid QuaternionD index!");
				}
			}
		}

		public static QuaternionD identity
		{
			get
			{
				return new QuaternionD(0.0, 0.0, 0.0, 1.0);
			}
		}

		public Vector3d eulerAngles
		{
			get
			{
				return QuaternionD.Internal_ToEulerRad(this) * 57.29578;
			}
			set
			{
				this = QuaternionD.Internal_FromEulerRad(value * (Math.PI / 180.0));
			}
		}
		
		public QuaternionD(double x, double y, double z, double w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}


		public static QuaternionD operator *(QuaternionD lhs, QuaternionD rhs)
		{
			return new QuaternionD( ((double) lhs.w * (double) rhs.x + (double) lhs.x * (double) rhs.w + (double) lhs.y * (double) rhs.z - (double) lhs.z * (double) rhs.y), ((double) lhs.w * (double) rhs.y + (double) lhs.y * (double) rhs.w + (double) lhs.z * (double) rhs.x - (double) lhs.x * (double) rhs.z), ((double) lhs.w * (double) rhs.z + (double) lhs.z * (double) rhs.w + (double) lhs.x * (double) rhs.y - (double) lhs.y * (double) rhs.x), ((double) lhs.w * (double) rhs.w - (double) lhs.x * (double) rhs.x - (double) lhs.y * (double) rhs.y - (double) lhs.z * (double) rhs.z));
		}


		public static Vector3d operator *(QuaternionD rotation, Vector3d point)
		{
			double num1 = rotation.x * 2.0;
			double num2 = rotation.y * 2.0;
			double num3 = rotation.z * 2.0;
			double num4 = rotation.x * num1;
			double num5 = rotation.y * num2;
			double num6 = rotation.z * num3;
			double num7 = rotation.x * num2;
			double num8 = rotation.x * num3;
			double num9 = rotation.y * num3;
			double num10 = rotation.w * num1;
			double num11 = rotation.w * num2;
			double num12 = rotation.w * num3;
			Vector3d vector3;
			vector3.x = ((1.0 - ((double) num5 + (double) num6)) * (double) point.x + ((double) num7 - (double) num12) * (double) point.y + ((double) num8 + (double) num11) * (double) point.z);
			vector3.y = (((double) num7 + (double) num12) * (double) point.x + (1.0 - ((double) num4 + (double) num6)) * (double) point.y + ((double) num9 - (double) num10) * (double) point.z);
			vector3.z = (((double) num8 - (double) num11) * (double) point.x + ((double) num9 + (double) num10) * (double) point.y + (1.0 - ((double) num4 + (double) num5)) * (double) point.z);
			return vector3;
		}

		public static bool operator ==(QuaternionD lhs, QuaternionD rhs)
		{
			return (double) QuaternionD.Dot(lhs, rhs) > 0.999998986721039;
		}
		
		public static bool operator !=(QuaternionD lhs, QuaternionD rhs)
		{
			return (double) QuaternionD.Dot(lhs, rhs) <= 0.999998986721039;
		}
		
		public void Set(double new_x, double new_y, double new_z, double new_w)
		{
			this.x = new_x;
			this.y = new_y;
			this.z = new_z;
			this.w = new_w;
		}

		public static double Dot(QuaternionD a, QuaternionD b)
		{
			return ((double) a.x * (double) b.x + (double) a.y * (double) b.y + (double) a.z * (double) b.z + (double) a.w * (double) b.w);
		}

		public static QuaternionD AngleAxis(double angle, Vector3d axis)
		{
			return QuaternionD.INTERNAL_CALL_AngleAxis(angle, ref axis);
		}
		
		//[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern QuaternionD INTERNAL_CALL_AngleAxis(double angle, ref Vector3d axis);

		public void ToAngleAxis(out double angle, out Vector3d axis)
		{
			QuaternionD.Internal_ToAxisAngleRad(this, out axis, out angle);
			angle = angle * 57.29578;
		}

		public static QuaternionD FromToRotation(Vector3d fromDirection, Vector3d toDirection)
		{
			return QuaternionD.INTERNAL_CALL_FromToRotation(ref fromDirection, ref toDirection);
		}
		
		//[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern QuaternionD INTERNAL_CALL_FromToRotation(ref Vector3d fromDirection, ref Vector3d toDirection);
		
		public void SetFromToRotation(Vector3d fromDirection, Vector3d toDirection)
		{
			this = QuaternionD.FromToRotation(fromDirection, toDirection);
		}
		
		public static QuaternionD LookRotation(Vector3d forward, [DefaultValue("Vector3d.up")] Vector3d upwards)
		{
			return QuaternionD.INTERNAL_CALL_LookRotation(ref forward, ref upwards);
		}
		
		[ExcludeFromDocs]
		public static QuaternionD LookRotation(Vector3d forward)
		{
			Vector3d up = Vector3d.up;
			return QuaternionD.INTERNAL_CALL_LookRotation(ref forward, ref up);
		}
		
		//[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern QuaternionD INTERNAL_CALL_LookRotation(ref Vector3d forward, ref Vector3d upwards);
		
		[ExcludeFromDocs]
		public void SetLookRotation(Vector3d view)
		{
			Vector3d up = Vector3d.up;
			this.SetLookRotation(view, up);
		}
		
		public void SetLookRotation(Vector3d view, [DefaultValue("Vector3d.up")] Vector3d up)
		{
			this = QuaternionD.LookRotation(view, up);
		}
		
		public static QuaternionD Slerp(QuaternionD from, QuaternionD to, double t)
		{
			return QuaternionD.INTERNAL_CALL_Slerp(ref from, ref to, t);
		}
		
		//[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern QuaternionD INTERNAL_CALL_Slerp(ref QuaternionD from, ref QuaternionD to, double t);
		
		public static QuaternionD Lerp(QuaternionD from, QuaternionD to, double t)
		{
			return QuaternionD.INTERNAL_CALL_Lerp(ref from, ref to, t);
		}
		
		//[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern QuaternionD INTERNAL_CALL_Lerp(ref QuaternionD from, ref QuaternionD to, double t);
		
		public static QuaternionD RotateTowards(QuaternionD from, QuaternionD to, double maxDegreesDelta)
		{
			double num = QuaternionD.Angle(from, to);
			if ((double) num == 0.0)
				return to;
			double t = Mathd.Min(1.0, maxDegreesDelta / num);
			return QuaternionD.UnclampedSlerp(from, to, t);
		}
		
		private static QuaternionD UnclampedSlerp(QuaternionD from, QuaternionD to, double t)
		{
			return QuaternionD.INTERNAL_CALL_UnclampedSlerp(ref from, ref to, t);
		}
		
		//[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern QuaternionD INTERNAL_CALL_UnclampedSlerp(ref QuaternionD from, ref QuaternionD to, double t);
		
		public static QuaternionD Inverse(QuaternionD rotation)
		{
			return QuaternionD.INTERNAL_CALL_Inverse(ref rotation);
		}
		
		//[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern QuaternionD INTERNAL_CALL_Inverse(ref QuaternionD rotation);

		/*
		public override string ToString()
		{
			string fmt = "({0:F1}, {1:F1}, {2:F1}, {3:F1})";
			object[] objArray = new object[4];
			int index1 = 0;
			// ISSUE: variable of a boxed type
			__Boxed<double> local1 = (ValueType) this.x;
			objArray[index1] = (object) local1;
			int index2 = 1;
			// ISSUE: variable of a boxed type
			__Boxed<double> local2 = (ValueType) this.y;
			objArray[index2] = (object) local2;
			int index3 = 2;
			// ISSUE: variable of a boxed type
			__Boxed<double> local3 = (ValueType) this.z;
			objArray[index3] = (object) local3;
			int index4 = 3;
			// ISSUE: variable of a boxed type
			__Boxed<double> local4 = (ValueType) this.w;
			objArray[index4] = (object) local4;
			return UnityString.Format(fmt, objArray);
		}

		public string ToString(string format)
		{
			string fmt = "({0}, {1}, {2}, {3})";
			object[] objArray = new object[4];
			int index1 = 0;
			string str1 = this.x.ToString(format);
			objArray[index1] = (object) str1;
			int index2 = 1;
			string str2 = this.y.ToString(format);
			objArray[index2] = (object) str2;
			int index3 = 2;
			string str3 = this.z.ToString(format);
			objArray[index3] = (object) str3;
			int index4 = 3;
			string str4 = this.w.ToString(format);
			objArray[index4] = (object) str4;
			return UnityString.Format(fmt, objArray);
		}
		*/
		
		public static double Angle(QuaternionD a, QuaternionD b)
		{
			return ((double) Mathd.Acos(Mathd.Min(Mathd.Abs(QuaternionD.Dot(a, b)), 1.0)) * 2.0 * 57.2957801818848);
		}
		
		public static QuaternionD Euler(double x, double y, double z)
		{
			return QuaternionD.Internal_FromEulerRad(new Vector3d(x, y, z) * (Math.PI / 180.0));
		}
		
		public static QuaternionD Euler(Vector3d euler)
		{
			return QuaternionD.Internal_FromEulerRad(euler * (Math.PI / 180.0));
		}

		private static Vector3d Internal_ToEulerRad(QuaternionD rotation)
		{
			return QuaternionD.INTERNAL_CALL_Internal_ToEulerRad(ref rotation);
		}


		//[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Vector3d INTERNAL_CALL_Internal_ToEulerRad(ref QuaternionD rotation);

		private static QuaternionD Internal_FromEulerRad(Vector3d euler)
		{
			return QuaternionD.INTERNAL_CALL_Internal_FromEulerRad(ref euler);
		}
		
		//[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern QuaternionD INTERNAL_CALL_Internal_FromEulerRad(ref Vector3d euler);

		private static void Internal_ToAxisAngleRad(QuaternionD q, out Vector3d axis, out double angle)
		{
			QuaternionD.INTERNAL_CALL_Internal_ToAxisAngleRad(ref q, out axis, out angle);
		}
		
		//[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_CALL_Internal_ToAxisAngleRad(ref QuaternionD q, out Vector3d axis, out double angle);


		//[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern QuaternionD INTERNAL_CALL_AxisAngle(ref Vector3d axis, double angle);

		
		public override int GetHashCode()
		{
			return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2 ^ this.w.GetHashCode() >> 1;
		}
		
		public override bool Equals(object other)
		{
			if (!(other is QuaternionD))
				return false;
			QuaternionD quaternion = (QuaternionD) other;
			if (this.x.Equals(quaternion.x) && this.y.Equals(quaternion.y) && this.z.Equals(quaternion.z))
				return this.w.Equals(quaternion.w);
			return false;
		}

		public static implicit operator QuaternionD(Quaternion q)
		{
			return new QuaternionD(q.x, q.y, q.z, q.w);
		}

		public static implicit operator Quaternion(QuaternionD q)
		{
			return new Quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w);
		}

	}
}
