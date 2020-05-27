using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace NotasProject.Services.Helpers
{
    public static class ArrayHelper
    {
        public static T[] Concat<T>(params T[][] arrays)
        {
            var result = new T[arrays.Sum(a => a.Length)];
            int offset = 0;
            for (int x = 0; x < arrays.Length; x++)
            {
                arrays[x].CopyTo(result, offset);
                offset += arrays[x].Length;
            }
            return result;
        }
        public static byte[] ConvertTimeStampToRandomizedNonce(this string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                hexString = hexString.Substring(0, hexString.Length - 1);
            }
            string byteValue;
            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return data;
        }
    }
}