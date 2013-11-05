
#include <windows.h>
#include "msgpack.h"
#include "wintab.h"
#define PACKETDATA      (PK_X | PK_Y | PK_BUTTONS | PK_NORMAL_PRESSURE | \
  					     PK_ORIENTATION | PK_CURSOR)
#define PACKETMODE      0
#include "pktdef.h"
#include <math.h>
#include <string.h>
#include "tablet.h"

/* converts FIX32 to double */
#define FIX_DOUBLE(x)   ((double)(INT(x))+((double)FRAC(x)/65536))
#define pi 3.14159265359


HCTX            hTab = NULL;         /* Handle for Tablet Context */
POINT           ptNew;               /* XY value storage */
UINT            prsNew;              /* Pressure value storage */
UINT            curNew;              /* Cursor number storage */
ORIENTATION     ortNew;              /* Tilt value storage */
BOOL            tilt_support = TRUE; /* Is tilt supported */


/* ------------------------------------------------------------------------- */
BOOL TiltCheck(void)
{
	struct          tagAXIS TpOri[3]; /* The capabilities of tilt */
	double          tpvar;            /* A temp for converting fix to double */


	/* check if WinTab available. */
	if (!WTInfo(0, 0, NULL)) {
		MessageBox(NULL, "WinTab Services Not Available.", "WinTab", 
			   MB_OK | MB_ICONHAND);
		return FALSE;
	}

	/* check if WACOM available. */
    WTInfo(WTI_DEVICES, DVC_NAME, WName);
    if (strncmp(WName,"WACOM",5)) {
		MessageBox(NULL, "Wacom Tablet Not Installed.", "Tilt Test", 
		    	   MB_OK | MB_ICONHAND);

    }

	/* get info about tilt */
	tilt_support = WTInfo(WTI_DEVICES,DVC_ORIENTATION,&TpOri);
	if (tilt_support) {
		/* does the tablet support azimuth and altitude */
		if (!(TpOri[0].axResolution && TpOri[1].axResolution)) {
			tilt_support = FALSE;
		}
	}
	
	return (TRUE);

}

/* ------------------------------------------------------------------------- */
HCTX static NEAR TabletInit(HWND hWnd)
{
	LOGCONTEXT      lcMine;           /* The context of the tablet */
	AXIS            TabletX, TabletY; /* The maximum tablet size */

	/* get default region */
	WTInfo(WTI_DEFCONTEXT, 0, &lcMine);

	/* modify the digitizing region */
	wsprintf(lcMine.lcName, "TiltTest Digitizing");
	lcMine.lcOptions |= CXO_MESSAGES;
	lcMine.lcPktData = PACKETDATA;
	lcMine.lcPktMode = PACKETMODE;
	lcMine.lcMoveMask = PACKETDATA;
	lcMine.lcBtnUpMask = lcMine.lcBtnDnMask;

    /* Set the entire tablet as active */
	WTInfo(WTI_DEVICES,DVC_X,&TabletX);
	WTInfo(WTI_DEVICES,DVC_Y,&TabletY);
	lcMine.lcInOrgX = 0;
	lcMine.lcInOrgY = 0;
	lcMine.lcInExtX = TabletX.axMax;
	lcMine.lcInExtY = TabletY.axMax;

    /* output the data in screen coords */
	lcMine.lcOutOrgX = lcMine.lcOutOrgY = 0;
	lcMine.lcOutExtX = GetSystemMetrics(SM_CXSCREEN);
    /* move origin to upper left */
	lcMine.lcOutExtY = -GetSystemMetrics(SM_CYSCREEN);

	/* open the region */
	return WTOpen(hWnd, &lcMine, TRUE);

}

/* ------------------------------------------------------------------------- */
LRESULT FAR PASCAL MainWndProc(hWnd, message, wParam, lParam)
HWND            hWnd;
unsigned        message;
WPARAM          wParam;
LPARAM          lParam;
{
	FARPROC         lpProcAbout;     /* pointer to the about function */
	HDC             hDC;             /* handle for Device Context */
	PAINTSTRUCT     psPaint;         /* the paint structure */
	BOOL            fHandled = TRUE; /* whether the message was handled or not */
	LRESULT         lResult = 0L;    /* the result of the message */

	switch (message) {

		case WM_CREATE: /* The window was created so open a context */
			hTab = TabletInit(hWnd);
			if (!hTab) {
				MessageBox(NULL, " Could Not Open Tablet Context.", "WinTab", 
					   MB_OK | MB_ICONHAND);
				SendMessage(hWnd, WM_DESTROY, 0, 0L);
			}
			break;

		case WM_ACTIVATE: /* The window is activated or deactivated */
			if (GET_WM_ACTIVATE_STATE(wParam, lParam))
				InvalidateRect(hWnd, NULL, TRUE);
			/* if switching in the middle, disable the region */
			if (hTab) {
				WTEnable(hTab, GET_WM_ACTIVATE_STATE(wParam, lParam));
				if (hTab && GET_WM_ACTIVATE_STATE(wParam, lParam))
					WTOverlap(hTab, TRUE);
			}
			break;

		case WM_DESTROY: /* The window was destroyed */
			if (hTab)
				WTClose(hTab);
			PostQuitMessage(0);
			break;

		case WM_PAINT: { /* Paint the window */
			double  pen_tilt, pen_dir;

			if (tilt_support) {                             
				pen_tilt = ((double)ortNew.orAltitude) / 10.0;
				pen_dir = 90.0 - (double) ortNew.orAzimuth / 10.0; 
				if(pen_dir < 0.0) pen_dir += 360.0;



				/* draw CROSS based on tablet position */ 
				MoveTo(hDC,ptNew.x - 20,ptNew.y     );
				LineTo(hDC,ptNew.x + 20,ptNew.y     );
				MoveTo(hDC,ptNew.x     ,ptNew.y - 20);
				LineTo(hDC,ptNew.x     ,ptNew.y + 20);
				EndPaint(hWnd, &psPaint);
			}
			break;
		}

		case WT_PACKET: /* A packet is waiting from WINTAB */
			PACKET	pkt;             /* the current packet */
			if (WTPacket((HCTX)lParam, wParam, &pkt)) {
			
				/* save new co-ordinates */
				ptNew.x = (UINT)pkt.pkX;
				ptNew.y = (UINT)pkt.pkY;
				curNew = pkt.pkCursor;
				prsNew = pkt.pkNormalPressure;
				ortNew = pkt.pkOrientation;
				
			}
			break;
			
		default:
			fHandled = FALSE;
			break;
	}
	if (fHandled)
		return (lResult);
	else
		return (DefWindowProc(hWnd, message, wParam, lParam));
}

