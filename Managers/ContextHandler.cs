// Source: ContextHandler
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

using Engine.Core;
using SDL2;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Editor.UI.Managers
{
    public sealed class WindowContextManager : Singleton<WindowContextManager>, IMsgTranslator
    {

        public event Action<SDL.SDL_Event> WndProc;


        private IntPtr _defaultHandle;
        private Dictionary<IntPtr, WindowContext> _contexts;
        private Dictionary<IntPtr, WindowRenderTarget> _renderContexts;
        private WindowRenderTarget _renderTarget;
        private Factory _d2dfactory;
        private WindowContext _activeWindow;


        protected override void Initialize()
        {
            this._contexts = new Dictionary<IntPtr, WindowContext>();
            this._renderContexts = new Dictionary<IntPtr, WindowRenderTarget>();
        }


        internal void SetActiveWindow(WindowContext context)
        {
            var noError = true;

            try
            {
                if (this._contexts.ContainsKey(context.Handle))
                {
                    SDL.SDL_ShowWindow(context.Handle);
                }
            }
            catch
            {
                noError = false;
            }
            finally
            {
                if (noError) { this._activeWindow = context; }
            }
        }

        public WindowContext GetActiveWindow()
        {
            return this._activeWindow;
        }

        public void Run()
        {
            while (this._contexts.Count > 0)
            {
                if (SDL.SDL_PollEvent(out var evt) != 0)
                {
                    this.Translate(evt);
                    this.DispatchMsg(evt);
                }

                this.Paint();
            }

            if (this._defaultHandle != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(this._defaultHandle);

            }

            this._renderTarget?.Dispose();
            this._renderTarget = null;

            this._d2dfactory?.Dispose();
            this._d2dfactory = null;
            SDL.SDL_Quit();
        }

        private void DispatchMsg(SDL.SDL_Event evt)
        {
            this._contexts.Values
                .ToList()
                .ForEach(x => x.Translate(evt));
        }

        public bool CreateDefaultContext()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) < 0)
            {
                return false;
            }

            var flags = SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;

            this._defaultHandle = SDL.SDL_CreateWindow(
                string.Empty, 0, 0, 100, 100, flags);
            if (this._defaultHandle != IntPtr.Zero)
            {
                this._renderTarget = this.CreateGraphics(this._defaultHandle);
                this._d2dfactory = new Factory(FactoryType.MultiThreaded);

                return true;
            }

            return false;
        }

        public WindowContext CreateNew(string title, int width, int height)
        {
            var windowContext = new WindowContext
            {
                Visible = true,
                BackgroundColor = new SharpDX.Mathematics.Interop.RawColor4(1, 1, 1, 1),
            };

            var flags =
                SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
                | SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;

            var sdlWindow = SDL.SDL_CreateWindow(
                title, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
                width, height,
                flags);

            if (sdlWindow != IntPtr.Zero)
            {
                windowContext.Handle = sdlWindow;
                windowContext.Width = width;
                windowContext.Height = height;

                this._contexts[sdlWindow] = windowContext;
            }

            return windowContext;
        }

        public void Translate(SDL.SDL_Event e)
        {
            if (e.type == SDL.SDL_EventType.SDL_QUIT)
            {
                foreach (var ctx in this._contexts.Values) { ctx?.Dispose(); }
                this._contexts.Clear();

                return;
            }
        }

        public void Paint()
        {
            foreach (var ctx in this._contexts.Values)
            {
                if (ctx.Visible)
                {
                    var graphics = this.CreateGraphics(ctx.Handle);

                    graphics.BeginDraw();
                    ctx.Paint();
                    graphics.EndDraw();
                }
            }
        }


        internal WindowRenderTarget CreateGraphics(IntPtr handle)
        {
            if (this._renderContexts.TryGetValue(handle, out var renderContext))
            {
                return renderContext;
            }

            if (!this._contexts.TryGetValue(handle, out var context))
            {
                return null;
            }

            var wmInfo = new SDL.SDL_SysWMinfo();
            SDL.SDL_VERSION(out wmInfo.version);
            _ = SDL.SDL_GetWindowWMInfo(handle, ref wmInfo);

            var hwnd = wmInfo.info.win.window;
            var renderProps = new RenderTargetProperties(
                new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied))
            {
                Usage = RenderTargetUsage.None,
                Type = RenderTargetType.Hardware,
            };

            var hwndProps = new HwndRenderTargetProperties
            {
                Hwnd = hwnd,
                PixelSize = new SharpDX.Size2((int)context.Width, (int)context.Height),
                PresentOptions = PresentOptions.RetainContents,
            };

            var renderTarget = new WindowRenderTarget(this._d2dfactory, renderProps, hwndProps);
            this._renderContexts[handle] = renderTarget;

            return renderTarget;
        }

        internal void Destroy(ref IntPtr handle)
        {
            if (this._contexts.TryGetValue(handle, out _))
            {
                if (this._renderContexts.TryGetValue(handle, out var renderer))
                {
                    if (this._renderContexts.Remove(handle))
                    {
                        renderer?.Dispose();
                    }
                }

                if (this._contexts.Remove(handle))
                {
                    SDL.SDL_DestroyWindow(handle);

                    if (handle != IntPtr.Zero)
                    {
                        handle = IntPtr.Zero;
                    }
                }
            }
        }

        internal bool IsWindow(WindowContext windowContext)
        {
            return this._contexts.ContainsKey(windowContext.Handle);
        }

    }
}
