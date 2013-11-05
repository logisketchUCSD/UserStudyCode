using System;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.Ink;

namespace Labeler
{
	/// <summary>
	/// Summary description for InkPanel.
	/// </summary>
	public class InkPanel : Panel
	{
		private Microsoft.Ink.InkOverlay oInk;
	
		public InkPanel()
		{
			this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			this.UpdateStyles();

			oInk = new InkOverlay( this.Handle );

			// Setting this property to false means we have to do
			// all redrawing of the ink ourselves in OnPaint
			oInk.AutoRedraw = false;
			oInk.Enabled = true;
		}


		/// <summary>
		/// Overrides <see cref="Control"/>.Dispose.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			// InkOverlay is managed, so only dispose if necessary
			if ( disposing )
				oInk.Dispose();
		}


		/// <summary>
		/// Overrides <see cref="Control"/>.OnPaint.
		/// </summary>
		protected override void OnPaint( PaintEventArgs e )
		{      
			base.OnPaint( e );
        
			
			// Doing all our redrawing here enables us to take
			// full advantage of the automatic double buffering
			oInk.Renderer.Draw( e.Graphics, oInk.Ink.Strokes );
		}
	}
}
