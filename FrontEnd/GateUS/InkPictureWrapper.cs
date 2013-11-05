using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPanelLib
{
    /// <summary>
    /// This class may fix a scrolling-related InkPicture bug; more testing is necessary.
    /// </summary>
    public class InkPictureWrapper : Microsoft.Ink.InkPicture
    {
        /// <summary>
        /// Fixes scrolling bug.
        /// <see cref="http://groups.google.com/group/microsoft.public.windows.tabletpc.developer/msg/a53f0b0027563241?hl=en&lr=&ie=UTF-8"/>
        /// </summary>
        /// <param name="m">message</param>
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case 0x0007:    // WM_SETFOCUS
                    m.Result = IntPtr.Zero;    // Specify that the message is handled
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }

        }
        
    }
}
