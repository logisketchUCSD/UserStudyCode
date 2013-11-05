/* May 23 2007
 * This application is intended to implement a small InkPad
 * as an addin in PowerPoint. It might even have some functionality.
 */

namespace PowerpointMinipad
{
	using System;
	using Extensibility;
	using System.Runtime.InteropServices;
    using System.Reflection;
    using Microsoft.Office.Core;
    using System.Windows.Forms;



	
	/// <summary>
	///   The object for implementing an Add-in.
	/// </summary>
	/// <seealso class='IDTExtensibility2' />
	[GuidAttribute("851BD2FC-9DF0-4B68-A03C-79642503FEAA"), ProgId("PowerpointMinipad.Connect")]
	public class Connect : Object, Extensibility.IDTExtensibility2
	{
		/// <summary>
		///		Implements the constructor for the Add-in object.
		///		Place your initialization code within this method.
		/// </summary>
		public Connect()
		{
        }
       

        #region Members global to Connect class

        private CommandBarButton ActivationButton; //button to invoke the minipad
        private MiniPadForm minipadform;
        private object applicationObject; // the instance of PowerPoint we are running, I think
        private object addInInstance;

        private PPTcontrol pptController;
    
        #endregion

        /// <summary>
		///      Implements the OnConnection method of the IDTExtensibility2 interface.
		///      Receives notification that the Add-in is being loaded.
		/// </summary>
		/// <param term='application'>
		///      Root object of the host application.
		/// </param>
		/// <param term='connectMode'>
		///      Describes how the Add-in is being loaded.
		/// </param>
		/// <param term='addInInst'>
		///      Object representing this Add-in.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, Extensibility.ext_ConnectMode connectMode, object addInInst, ref System.Array custom)
		{
			applicationObject = application;
			addInInstance = addInInst;

            
            if (connectMode != Extensibility.ext_ConnectMode.ext_cm_Startup)
            {
                OnStartupComplete(ref custom);
            }
            
		} // end onConnection method

		/// <summary>
		///     Implements the OnDisconnection method of the IDTExtensibility2 interface.
		///     Receives notification that the Add-in is being unloaded.
		/// </summary>
		/// <param term='disconnectMode'>
		///      Describes how the Add-in is being unloaded.
		/// </param>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(Extensibility.ext_DisconnectMode disconnectMode, ref System.Array custom)
		{
            if (disconnectMode != Extensibility.ext_DisconnectMode.ext_dm_HostShutdown)
            {
                OnBeginShutdown(ref custom);
            }
            applicationObject = null;
		} // end OnDisconnection method

		/// <summary>
		///      Implements the OnAddInsUpdate method of the IDTExtensibility2 interface.
		///      Receives notification that the collection of Add-ins has changed.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnAddInsUpdate(ref System.Array custom)
		{
		} // end OnAddinsUpdate method

		/// <summary>
		///      Implements the OnStartupComplete method of the IDTExtensibility2 interface.
		///      Receives notification that the host application has completed loading.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref System.Array custom)
		{
            System.Windows.Forms.MessageBox.Show("Welcome to PowerPoint");

            pptController = new PPTcontrol(applicationObject);
 
            CommandBars oCommandBars;
            CommandBar oStandardBar;

            oCommandBars = (CommandBars)applicationObject.GetType().InvokeMember("CommandBars",BindingFlags.GetProperty, 
                                                                                 null, applicationObject, null);

            oStandardBar = oCommandBars["Standard"];

            // tutorial says this block is for "in case the button was not deleted, use the exiting one"
            try
            {
                ActivationButton = (CommandBarButton)oStandardBar.Controls["PowerPoint MiniPad"];
                // This exception seems to get thrown an awful lot
                // Seems like bad practice, but I'm still not entirely sure what it's being used for 
            }
            catch (Exception)
            {
               // System.Windows.Forms.MessageBox.Show("An exception was thrown making the button.");
     
                object omissing = System.Reflection.Missing.Value;
                ActivationButton = (CommandBarButton)oStandardBar.Controls.Add(1, omissing, omissing, omissing, omissing);

                ActivationButton.Caption = "MiniPad";
                ActivationButton.Style = MsoButtonStyle.msoButtonIcon;
                ActivationButton.FaceId = 59;
            } // end stupid exception
            /* aside: catalogue of FaceID's:
             * 59 = smiley face! use this one please :-)
             * 58 = x_2
             * 57 = x^2
             * 52 = pig
             *  53 - 56 = assorted distance apart horizontal lines?
             */

            ActivationButton.Visible = true;
            ActivationButton.Click += new _CommandBarButtonEvents_ClickEventHandler(ActivationButton_Click);
            oStandardBar = null;
            oCommandBars = null;
		        
        
        } // end OnStartupComplete method

        void ActivationButton_Click(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            //System.Windows.Forms.MessageBox.Show("You Clicked the Button :-)");     
            ShowPad();
        } // end activationbutton_click method

		/// <summary>
		///      Implements the OnBeginShutdown method of the IDTExtensibility2 interface.
		///      Receives notification that the host application is being unloaded.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref System.Array custom)
		{
            object omissing = System.Reflection.Missing.Value;
            ActivationButton.Delete(omissing);
            ActivationButton = null;

		}


        private void ShowPad()
        {
            minipadform = new MiniPadForm(pptController);
            minipadform.Show();
        }// end ShowPad method






    

	}//end class Connect
}// end namespace PowerPoint MiniPad





#region Read me for Add-in installation and setup information.
// When run, the Add-in wizard prepared the registry for the Add-in.
// At a later time, if the Add-in becomes unavailable for reasons such as:
//   1) You moved this project to a computer other than which is was originally created on.
//   2) You chose 'Yes' when presented with a message asking if you wish to remove the Add-in.
//   3) Registry corruption.
// you will need to re-register the Add-in by building the PowerpointMinipadSetup project, 
// right click the project in the Solution Explorer, then choose install.
#endregion