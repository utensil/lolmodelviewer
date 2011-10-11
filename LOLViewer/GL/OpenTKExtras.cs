
#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2006-2008 the OpenTK Team.
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTKExtras
{
        
    //
    // Helper Quaternion Function
    // I couldn't find these functions in the OpenTK release.
    // I got them from their website.
    //

    public class Quaternion
    {
        public static OpenTK.Matrix4 CreateRotationMatrix(ref OpenTK.Quaternion q)
        {
            OpenTK.Matrix4 result = OpenTK.Matrix4.Identity;

            float X = q.X;
            float Y = q.Y;
            float Z = q.Z;
            float W = q.W;

            float xx = X * X;
            float xy = X * Y;
            float xz = X * Z;
            float xw = X * W;
            float yy = Y * Y;
            float yz = Y * Z;
            float yw = Y * W;
            float zz = Z * Z;
            float zw = Z * W;

            result.M11 = 1 - 2 * (yy + zz);
            result.M21 = 2 * (xy - zw);
            result.M31 = 2 * (xz + yw);
            result.M12 = 2 * (xy + zw);
            result.M22 = 1 - 2 * (xx + zz);
            result.M32 = 2 * (yz - xw);
            result.M13 = 2 * (xz - yw);
            result.M23 = 2 * (yz + xw);
            result.M33 = 1 - 2 * (xx + yy);
            return result;
        }

    }

    public class Matrix4
    {
        public static OpenTK.Quaternion CreateQuatFromMatrix(OpenTK.Matrix4 m)
        {
            float trace = 1 + m.M11 + m.M22 + m.M33;
            float S = 0;
            float X = 0;
            float Y = 0;
            float Z = 0;
            float W = 0;

            if (trace > 0.0000001)
            {
                S = (float)Math.Sqrt(trace) * 2;
                X = (m.M23 - m.M32) / S;
                Y = (m.M31 - m.M13) / S;
                Z = (m.M12 - m.M21) / S;
                W = 0.25f * S;
            }
            else
            {
                if (m.M11 > m.M22 && m.M11 > m.M33)
                {
                    // Column 0: 
                    S = (float)Math.Sqrt(1.0 + m.M11 - m.M22 - m.M33) * 2;
                    X = 0.25f * S;
                    Y = (m.M12 + m.M21) / S;
                    Z = (m.M31 + m.M13) / S;
                    W = (m.M23 - m.M32) / S;
                }
                else if (m.M22 > m.M33)
                {
                    // Column 1: 
                    S = (float)Math.Sqrt(1.0 + m.M22 - m.M11 - m.M33) * 2;
                    X = (m.M12 + m.M21) / S;
                    Y = 0.25f * S;
                    Z = (m.M23 + m.M32) / S;
                    W = (m.M31 - m.M13) / S;
                }
                else
                {
                    // Column 2:
                    S = (float)Math.Sqrt(1.0 + m.M33 - m.M11 - m.M22) * 2;
                    X = (m.M31 + m.M13) / S;
                    Y = (m.M23 + m.M32) / S;
                    Z = 0.25f * S;
                    W = (m.M12 - m.M21) / S;
                }
            }

            return new OpenTK.Quaternion(X, Y, Z, W);
        }
    }
}
