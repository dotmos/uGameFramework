using UnityEngine;
using System.Collections;
using System;

namespace Service.GlobalNetwork{
    public partial class Compression {
        static float Convertresolution(uint resolution){
            return Mathf.Max(1, resolution*10);
        }

        /// <summary>
        /// Gets the first two decimals of a float and stores them in a byte. Returns value between -99 and 99
        /// </summary>
        /// <returns>The float decimals to byte.</returns>
        /// <param name="input">Input.</param>
        public static sbyte PackFloatDecimalsToByte(float input){
            return (sbyte)Mathf.RoundToInt((input - (float)Math.Truncate(input)) * 100);
            //return (sbyte)Mathf.RoundToInt(input*Convertresolution(2));
        }

        /// <summary>
        /// Unpacks the first two decimals of a float from the given byte. Returns value between -0.99 and 0.99
        /// </summary>
        /// <returns>The float decimals from byte.</returns>
        /// <param name="input">Input.</param>
        public static float UnpackFloatDecimalsFromByte(sbyte input){
            return (float)input*0.01f;
        }

        /// <summary>
        /// Packs the float to short (-32.768 to 32.767). resolution defines the number of decimal places that should be kept from the float var.
        /// </summary>
        /// <returns>The float to short.</returns>
        /// <param name="input">Input.</param>
        /// <param name="resolution">resolution.</param>
        public static short PackFloatToShort(float input, uint resolution){
            return (short)Mathf.RoundToInt(input*Convertresolution(resolution));
        }

        /// <summary>
        /// Unpacks the float from short. resolution defines the number of decimal places that the float should have and muust be the same as was used while packing.
        /// </summary>
        /// <returns>The float from short.</returns>
        /// <param name="input">Input.</param>
        /// <param name="resolution">resolution.</param>
        public static float UnpackFloatFromShort(short input, uint resolution){
            return ((float)input/Convertresolution(resolution));
        }

        /// <summary>
        /// Packs the float to int (-2.147.483.648 to 2.147.483.647). resolution defines the number of decimal places that should be kept from the float var.
        /// </summary>
        /// <returns>The float to int.</returns>
        /// <param name="input">Input.</param>
        /// <param name="resolution">resolution.</param>
        public static int PackFloatToInt(float input, uint resolution){
            return Mathf.RoundToInt(input*Convertresolution(resolution));
        }

        /// <summary>
        /// Unpacks the float from int. resolution defines the number of decimals places that the float should have and muust be the same as was used while packing.
        /// </summary>
        /// <returns>The float fromint.</returns>
        /// <param name="input">Input.</param>
        /// <param name="resolution">resolution.</param>
        public static float UnpackFloatFromInt(int input, uint resolution){
            return (float)input/Convertresolution(resolution);
        }



        public struct ShortVector3{
            public short x;
            public short y;
            public short z;
        }
        /// <summary>
        /// Packs the Vector3 to short. resolution defines the number of decimal places that should be kept from the Vector3 x,y,z vars.
        /// </summary>
        /// <returns>The vector3 to short.</returns>
        /// <param name="input">Input.</param>
        /// <param name="resolution">resolution.</param>
        public static ShortVector3 PackVector3ToShort(Vector3 input, uint resolution){
            ShortVector3 _v;
            _v.x = PackFloatToShort(input.x, resolution);
            _v.y = PackFloatToShort(input.y, resolution);
            _v.z = PackFloatToShort(input.z, resolution);
            return _v;
        }
        /// <summary>
        /// Unpacks the Vector3 from short. resolution defines the number of decimals places that the vector should have and muust be the same as was used while packing.
        /// </summary>
        /// <returns>The vector3 from short.</returns>
        /// <param name="input">Input.</param>
        /// <param name="resolution">resolution.</param>
        public static Vector3 UnpackVector3FromShort(ShortVector3 input, uint resolution){
            return UnpackVector3FromShort(input.x, input.y, input.z, resolution);
        }
        public static Vector3 UnpackVector3FromShort(short x, short y, short z, uint resolution){
            Vector3 _v;
            _v.x = UnpackFloatFromShort(x, resolution);
            _v.y = UnpackFloatFromShort(y, resolution);
            _v.z = UnpackFloatFromShort(z, resolution);
            return _v;
        }


        public struct IntVector3{
            public int x;
            public int y;
            public int z;
        }
        /// <summary>
        /// Packs the Vector3 to int. resolution defines the number of decimal places that should be kept from the Vector3 x,y,z vars.
        /// </summary>
        /// <returns>The vector3 to int.</returns>
        /// <param name="input">Input.</param>
        /// <param name="resolution">Resolution.</param>
        public static IntVector3 PackVector3ToInt(Vector3 input, uint resolution){
            IntVector3 _v;
            _v.x = PackFloatToInt(input.x, resolution);
            _v.y = PackFloatToInt(input.y, resolution);
            _v.z = PackFloatToInt(input.z, resolution);
            return _v;
        }
        /// <summary>
        /// Unpacks the Vector3 from int. resolution defines the number of decimals places that the vector should have and muust be the same as was used while packing.
        /// </summary>
        /// <returns>The vector3 from int.</returns>
        /// <param name="input">Input.</param>
        /// <param name="resolution">Resolution.</param>
        public static Vector3 UnpackVector3FromInt(IntVector3 input, uint resolution){
            return UnpackVector3FromInt(input.x, input.y, input.z, resolution);
        }
        public static Vector3 UnpackVector3FromInt(int x, int y, int z, uint resolution){
            Vector3 _v;
            _v.x = UnpackFloatFromInt(x, resolution);
            _v.y = UnpackFloatFromInt(y, resolution);
            _v.z = UnpackFloatFromInt(z, resolution);
            return _v;
        }

        public struct ByteQuaternion{
            public sbyte a;
            public sbyte b;
            public sbyte c;
            public byte maxIndex;
        }
        /// <summary>
        /// Packs the quaternion.
        /// </summary>
        /// <returns>The quaternion.</returns>
        /// <param name="input">Input.</param>
        public static ByteQuaternion PackQuaternion(Quaternion input){
            //Apply smallest of three compression
            // http://gafferongames.com/networked-physics/snapshot-compression/
            // and http://www.gamedev.net/topic/461253-compressed-quaternions/

            ByteQuaternion _q;

            float sign = 1f;
            float maxValue = float.MinValue;
            _q.maxIndex = 0;

            //Find max value index of quaternion
            for(int i=0; i<4; ++i){
                float v = input[i];
                float abs = Mathf.Abs(v);
                if(abs > maxValue){
                    sign = v < 0 ? -1 : 1;
                    _q.maxIndex = (byte)i;
                    maxValue = abs;
                }
            }

            //Convert Quaternion to sbyte using quantizing
            if(_q.maxIndex == 0){
                _q.a = (sbyte)Mathf.RoundToInt(input.y * sbyte.MaxValue * sign);
                _q.b = (sbyte)Mathf.RoundToInt(input.z * sbyte.MaxValue * sign);
                _q.c = (sbyte)Mathf.RoundToInt(input.w * sbyte.MaxValue * sign);
            }
            else if(_q.maxIndex == 1){
                _q.a = (sbyte)Mathf.RoundToInt(input.x * sbyte.MaxValue * sign);
                _q.b = (sbyte)Mathf.RoundToInt(input.z * sbyte.MaxValue * sign);
                _q.c = (sbyte)Mathf.RoundToInt(input.w * sbyte.MaxValue * sign);
            }
            else if(_q.maxIndex == 2){
                _q.a = (sbyte)Mathf.RoundToInt(input.x * sbyte.MaxValue * sign);
                _q.b = (sbyte)Mathf.RoundToInt(input.y * sbyte.MaxValue * sign);
                _q.c = (sbyte)Mathf.RoundToInt(input.w * sbyte.MaxValue * sign);
            }
            else{
                _q.a = (sbyte)Mathf.RoundToInt(input.x * sbyte.MaxValue * sign);
                _q.b = (sbyte)Mathf.RoundToInt(input.y * sbyte.MaxValue * sign);
                _q.c = (sbyte)Mathf.RoundToInt(input.z * sbyte.MaxValue * sign);
            }

            //Convert Quaternion to sbyte using quantizing and square "compression" to linearize values. BROKEN CODE. NOT WORKING AT THE MOMENT :(
//            if(_q.maxIndex == 0){
//                _q.a = (sbyte)Mathf.RoundToInt(input.y * input.y * sbyte.MaxValue);
//                _q.b = (sbyte)Mathf.RoundToInt(input.z * input.z * sbyte.MaxValue);
//                _q.c = (sbyte)Mathf.RoundToInt(input.w * input.w * sbyte.MaxValue);
//                if(input.y * sign < 0) _q.a *= -1;
//                if(input.z * sign < 0) _q.b *= -1;
//                if(input.w * sign < 0) _q.c *= -1;
//            }
//            else if(_q.maxIndex == 1){
//                _q.a = (sbyte)Mathf.RoundToInt(input.x * input.x * sbyte.MaxValue);
//                _q.b = (sbyte)Mathf.RoundToInt(input.z * input.z * sbyte.MaxValue);
//                _q.c = (sbyte)Mathf.RoundToInt(input.w * input.w * sbyte.MaxValue);
//                if(input.x * sign < 0) _q.a *= -1;
//                if(input.z * sign < 0) _q.b *= -1;
//                if(input.w * sign < 0) _q.c *= -1;
//            }
//            else if(_q.maxIndex == 2){
//                _q.a = (sbyte)Mathf.RoundToInt(input.x * input.x * sbyte.MaxValue);
//                _q.b = (sbyte)Mathf.RoundToInt(input.y * input.y * sbyte.MaxValue);
//                _q.c = (sbyte)Mathf.RoundToInt(input.w * input.w * sbyte.MaxValue);
//                if(input.x * sign < 0) _q.a *= -1;
//                if(input.y * sign < 0) _q.b *= -1;
//                if(input.w * sign < 0) _q.c *= -1;
//            }
//            else{
//                _q.a = (sbyte)Mathf.RoundToInt(input.x * input.x * sbyte.MaxValue);
//                _q.b = (sbyte)Mathf.RoundToInt(input.y * input.y * sbyte.MaxValue);
//                _q.c = (sbyte)Mathf.RoundToInt(input.z * input.z * sbyte.MaxValue);
//                if(input.x * sign < 0) _q.a *= -1;
//                if(input.y * sign < 0) _q.b *= -1;
//                if(input.z * sign < 0) _q.c *= -1;
//            }

            return _q;
        }
        /// <summary>
        /// Unpacks the quaternion.
        /// </summary>
        /// <returns>The quaternion.</returns>
        /// <param name="input">Input.</param>
        public static Quaternion UnpackQuaternion(ByteQuaternion input){
            return UnpackQuaternion(input.a, input.b, input.c, input.maxIndex);
        }
        public static Quaternion UnpackQuaternion(sbyte x, sbyte y, sbyte z, byte maxIndex){

            //Convert back to float
            float a = (float)x / (float)sbyte.MaxValue;
            float b = (float)y / (float)sbyte.MaxValue;
            float c = (float)z / (float)sbyte.MaxValue;

            //Convert back to float. Remove square "compression" by getting the square root. BROKEN CODE. NOT WORKING AT THE MOMENT :(
            //to do: Add lookup table instead of using sqrt. There are only 256 possible values
//            float a;
//            if(x >= 0) a = Mathf.Sqrt((float)x / (float)sbyte.MaxValue);
//            else a = -Mathf.Sqrt((float)x / (float)sbyte.MaxValue);
//            float b;
//            if(y >= 0) b = Mathf.Sqrt((float)y / (float)sbyte.MaxValue);
//            else b = -Mathf.Sqrt((float)y / (float)sbyte.MaxValue);
//            float c;
//            if(z >= 0) c = Mathf.Sqrt((float)z / (float)sbyte.MaxValue);
//            else c = -Mathf.Sqrt((float)z / (float)sbyte.MaxValue);

            float d = Mathf.Sqrt(1.0f - (a*a + b*b + c*c));

            //Create quaternion
            if(maxIndex == 0){
                return new Quaternion(d, a, b, c);
            }
            else if(maxIndex == 1){
                return new Quaternion(a, d, b, c);
            }
            else if(maxIndex == 2){
                return new Quaternion(a, b, d, c);
            }
            else{
                return new Quaternion(a, b, c, d);
            }
        }
    }
}