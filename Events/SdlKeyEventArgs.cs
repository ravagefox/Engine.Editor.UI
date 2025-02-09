// Source: SdlKeyEventArgs
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

namespace Engine.Editor.UI.Events
{
    public class SdlKeyEventArgs : EventArgs
    {
        public SDL.SDL_KeyboardEvent KeyEvent { get; }
        public SDL.SDL_Keymod Modifiers => KeyEvent.keysym.mod;
        public SDL.SDL_Scancode Scancode => KeyEvent.keysym.scancode;
        public SDL.SDL_Keycode Keycode => KeyEvent.keysym.sym;
        public bool IsKeyDown => KeyEvent.state == SDL.SDL_PRESSED;
        public bool IsKeyUp => KeyEvent.state == SDL.SDL_RELEASED;

        public SdlKeyEventArgs(SDL.SDL_KeyboardEvent keyEvent)
        {
            this.KeyEvent = keyEvent;
        }
    }
}
