using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SP579Assemblerv2 {
    public static class TriRichTextBoxExtension {
        [DllImport( "user32" )]
        private static extern bool GetScrollInfo ( IntPtr hwnd, int nBar, ref SCROLLINFO scrollInfo );

        [DllImport( "user32.dll" )]
        public static extern int SetScrollInfo ( IntPtr hwnd, int nBar, SCROLLINFO scrollInfo, bool reDraw );

        public struct SCROLLINFO {
            public int cbSize;
            public int fMask;
            public int min;
            public int max;
            public int nPage;
            public int nPos;
            public int nTrackPos;
        }

        private const int SB_HORZ = 0x0;
        private const int SB_VERT = 0x1;

        private const int SIF_RANGE = 0x1;
        private const int SIF_PAGE = 0x2;
        private const int SIF_POS = 0x4;
        private const int SIF_TRACKPOS = 0x10;

        public static SCROLLINFO getScrollInfo ( this RichTextBox rtb ) {
            SCROLLINFO scrollInfo = new SCROLLINFO();
            scrollInfo.cbSize = Marshal.SizeOf( scrollInfo );
            scrollInfo.fMask = SIF_RANGE | SIF_POS | SIF_PAGE | SIF_TRACKPOS;
            GetScrollInfo( rtb.Handle, 1, ref scrollInfo ); //nBar = 1 -> VScrollbar

            return scrollInfo;
        }

        public static void setScrollInfo ( this RichTextBox rtb, SCROLLINFO scInfo ) {
            SetScrollInfo( rtb.Handle, 1, scInfo, true );

        }

    }
}
