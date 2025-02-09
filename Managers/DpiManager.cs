// Source: DpiManager
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

using SDL2;
using System;

namespace Engine.Editor.UI.Managers
{
    public static class DPIManager
    {
        private const float REFERENCE_DPI = 96.0f; // Standard DPI baseline (Windows default)

        public static float GetDPIScaleFactor(IntPtr window)
        {
            int displayIndex = SDL.SDL_GetWindowDisplayIndex(window);
            float ddpi, hdpi, vdpi;

            if (SDL.SDL_GetDisplayDPI(displayIndex, out ddpi, out hdpi, out vdpi) == 0)
            {
                return hdpi / REFERENCE_DPI; // Scale factor relative to reference DPI
            }

            return 1.0f; // Default to 100% scaling if DPI info is unavailable
        }
    }
}
