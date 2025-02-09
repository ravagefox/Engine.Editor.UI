// Source: Extensions
/* 
   ---------------------------------------------------------------
                        CREXIUM PTY LTD
   ---------------------------------------------------------------

     The software is provided 'AS IS', without warranty of any kind,
   express or implied, including but not limited to the warrenties
   of merchantability, fitness for a particular purpose and
   noninfringement. In no event shall the authors or copyright
   holders be liable for any claim, damages, or other liability,
   whether in an action of contract, tort, or otherwise, arising
   from, out of or in connection with the software or the use of
   other dealings in the software.
*/

using SharpDX.Mathematics.Interop;
using System.Drawing;

namespace Engine.Editor.UI.Extensions
{
    public static partial class Extensions
    {
        public static RawColor4 ToRawColor4(this Color color)
        {
            var r = color.R / 255.0f;
            var g = color.G / 255.0f;
            var b = color.B / 255.0f;
            var a = color.A / 255.0f;

            return new RawColor4(r, g, b, a);
        }

    }
}
