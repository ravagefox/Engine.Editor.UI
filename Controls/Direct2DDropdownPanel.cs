// Source: Direct2DDropdownPanel
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

using System;

namespace Engine.Editor.UI.Controls
{
    public class Direct2DDropdownPanel : WindowContext
    {

        public bool IsExpandable { get; set; }
        public float MaxHeight { get; set; }


        protected override void OnPaintBackground(object sender, EventArgs e)
        {
            this.VerticalScroll.Visible = true;

            base.OnPaintBackground(sender, e);
        }

    }
}
