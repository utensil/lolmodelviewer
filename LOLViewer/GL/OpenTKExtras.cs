
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
