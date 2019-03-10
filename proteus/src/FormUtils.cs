using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    public class FormUtils
    {
        public static void ForceWindowFocus(System.Windows.Forms.Form objForm, int intTimeoutMilliseconds = 1000)
        {
            int tA, tB;
            tA = System.Environment.TickCount;
            tB = System.Environment.TickCount;

            while (((tB - tA) < intTimeoutMilliseconds) && !objForm.Focused) {
                tB = System.Environment.TickCount;

                objForm.TopMost = true;
                objForm.TopMost = false;
                objForm.Activate();
                System.Windows.Forms.Application.DoEvents();
            }

        }
    }
}
