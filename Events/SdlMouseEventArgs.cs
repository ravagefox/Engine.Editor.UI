// Source: SdlMouseEventArgs
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
    public class SdlMouseEventArgs : EventArgs
    {
        public int X { get; }
        public int Y { get; }
        public SDL.SDL_MouseButtonEvent ButtonEvent { get; }
        public SDL.SDL_MouseWheelEvent WheelEvent { get; }
        public SDL.SDL_MouseMotionEvent MotionEvent { get; }

        public bool LeftButton => ButtonEvent.button == SDL.SDL_BUTTON_LEFT;
        public bool RightButton => ButtonEvent.button == SDL.SDL_BUTTON_RIGHT;
        public bool MiddleButton => ButtonEvent.button == SDL.SDL_BUTTON_MIDDLE;

        public bool IsScrolling => WheelEvent.type == SDL.SDL_EventType.SDL_MOUSEWHEEL;


        public SdlMouseEventArgs(SDL.SDL_MouseButtonEvent btnEvent)
        {
            this.X = btnEvent.x;
            this.Y = btnEvent.y;
            this.ButtonEvent = btnEvent;
        }

        public SdlMouseEventArgs(SDL.SDL_MouseMotionEvent motionEvent)
        {
            this.X = motionEvent.x;
            this.Y = motionEvent.y;
            this.MotionEvent = motionEvent;
        }

        public SdlMouseEventArgs(SDL.SDL_MouseWheelEvent wheelEvent)
        {
            this.WheelEvent = wheelEvent;
        }
    }
}
