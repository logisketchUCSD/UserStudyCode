/// Connect.cs
/// Connect is generated by the Add-in wizard. It has access to
/// an object representing the PowerPoint application which is 
/// running the add-in.
namespace Basic
{
	using System;
	using Extensibility;
	using System.Runtime.InteropServices;
    using System.Reflection;
    using Microsoft.Office.Core;
    using System.Windows.Forms;




	#region Read me for Add-in installation and setup information.
	// When run, the Add-in wizard prepared the registry for the Add-in.
	// At a later time, if the Add-in becomes unavailable for reasons such as:
	//   1) You moved this project to a computer other than which is was originally created on.
	//   2) You chose 'Yes' when presented with a message asking if you wish to remove the Add-in.
	//   3) Registry corruption.
	// you will need to re-register the Add-in by building the BasicSetup project, 
	// right click the project in the Solution Explorer, then choose install.
	#endregion
	
	/// <summary>
	///   The object for implementing an Add-in.
	/// </summary>
	/// <seealso class='IDTExtensibility2' />
	[GuidAttribute("82B4F5E9-F77D-49FC-B137-3DAEBB5D5749"), ProgId("Basic.Connect")]
	public class Connect : Object, Extensibility.IDTExtensibility2
	{
		/// <summary>
		///		Implements the constructor for the Add-in object.
		///</summary>
		public Connect()
		{
		}

        #region Members global to Connect class


        private CommandBarButton ActivationButton; //activates the AddIn
        private BasicForm basicform; // The form containing the overlay
        private ButtonForm buttonform; // The form containing the controls
        private CornerForm cornerform; // test -- for turning on / off overlay
        private object applicationObject; // the instance of Powerpoint we are running
        private object addInInstance; // This object represents the add-in itself. Currently unused.
        
        private PPTcontrol pptController; // The class which controls PowerPoint

        /// <summary>
        /// The initialized state of the ButtonForm. msocTrue = uninitialized, msoTrue = initialized but hidden, msoFalse = visible.
        /// </summary>
        internal MsoTriState isButtonHidden;
        /// <summary>
        /// The initialized state of the BasicForm. msocTrue = uninitialized, msoTrue = initialized but hidden, msoFalse = visible.
        /// </summary>
        internal MsoTriState isBasicHidden; 

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
                OnStartupComplete(ref custom);  // when the add-in has loaded, call this in case the addin was loaded after ppt finished
                                                // starting up
            }
		}

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
                OnBeginShutdown(ref custom); // call this method if the addin is disconnecting but ppt is not shutting down
            }
            applicationObject = null;
		}

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
		}

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

            pptController = new PPTcontrol(applicationObject);

            CommandBars aCommandBars;
            CommandBar aStandardBar;

            // Get powerpoint's CommandBars object
            aCommandBars = (CommandBars)applicationObject.GetType().InvokeMember("CommandBars", BindingFlags.GetProperty,
                                                                                 null, applicationObject, null);
            // And find the CommandBar we want to put the button on
            // This MUST be a valid PowerPoint toolbar name such as Formatting, Standard, etc.
            aStandardBar = aCommandBars["Formatting"];

            
            // If the button we are going to create for use is already there, use the existing one
            try
            {
                ActivationButton = (CommandBarButton)aStandardBar.Controls["PowerPoint basicPad"];
            }
            catch (Exception)
            {
                // If the button wasn't already there (most of the time) , create it
                object missing = System.Reflection.Missing.Value;
                ActivationButton = (CommandBarButton)aStandardBar.Controls.Add(1, missing, missing, missing, missing);
                ActivationButton.Caption = "BasicPad";
                ActivationButton.Style = MsoButtonStyle.msoButtonIcon;
                ActivationButton.FaceId = 52;
            }// catch

            ActivationButton.Visible = true;

            // add the click event-handler to it
            ActivationButton.Click += new _CommandBarButtonEvents_ClickEventHandler(ActivationButton_Click);
            aStandardBar = null;
            aCommandBars = null;

            // set the pad toggle-var to ctrue = unititalized
            isButtonHidden = MsoTriState.msoCTrue;
            isBasicHidden = MsoTriState.msoCTrue;

		} // end of OnStartupComplete method

        void ActivationButton_Click(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            ShowPad();
        }

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
            // remove the button or else there will be two when you start powerpoint again :-O
            object missing = System.Reflection.Missing.Value;
            ActivationButton.Delete(missing);
            ActivationButton = null;

		}

        /// <summary>
        /// Initalizes and shows all the parts of our overlay
        /// </summary>
        internal void ShowPad()
        {
            // If the forms are uninitalized, set them up
            if(isBasicHidden.Equals(MsoTriState.msoCTrue))
            {
                // this does not actually show the window, it just initializes it. Misleading! TODO FIXME fix name
                InitBasic();
                basicform.Disposed += new EventHandler(basicform_Disposed);
            }
            if (isButtonHidden.Equals(MsoTriState.msoCTrue))
            {
                // this does not actually show the window, it just initializes it. Misleading! TODO FIXME fix name
                InitButton();
                buttonform.Disposed += new EventHandler(buttonform_Disposed);
            }

            // Actually show the forms

            if (basicform.Visible == false || buttonform.Visible == false)
            {
                try
                {
                    buttonform.Show();
                    basicform.Show();
                    cornerform.Show();
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString());
                }
            }
            else
            {
                buttonform.Hide();
                basicform.Hide();
                cornerform.Hide();
            }

            // initialize all the members to let the forms talk to one another
            pptController.myButton = buttonform;
            pptController.myForm = basicform;
            basicform.buttonForm = buttonform;
            buttonform.myBasicForm = basicform;
            cornerform.buttonform = buttonform;

            basicform.Activate();
        }

        /// <summary>
        /// Initializes the Button Form
        /// </summary>
        private void InitButton()
        {
            //initialize everything
            buttonform = new ButtonForm(pptController, basicform);
            // set it to "initialized but hidden"
            isButtonHidden = MsoTriState.msoTrue;
            buttonform.ShowInTaskbar = false;
        }

        /// <summary>
        /// Initializes the Basic Form
        /// </summary>
        private void InitBasic()
        {
            //initialize everything
            basicform = new BasicForm(pptController);
            // set it to "initialized but hidden"
            isBasicHidden = MsoTriState.msoTrue;
            basicform.ShowInTaskbar = false;

            //TEST FIXME TODO -- also initialize cornerform
            // fix : this should be in a separate method
            cornerform = new CornerForm(basicform);
        }

        /// <summary>
        /// When the button form is manually closed, or disposed, toggles the isButtonHidden
        /// variable to reflect that
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonform_Disposed(object sender, EventArgs e)
        {
            isButtonHidden = MsoTriState.msoCTrue;
        }

        /// <summary>
        /// When the basic form is manually closed, or disposed, toggles the isBasicHidden
        /// variable to reflect that
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void basicform_Disposed(object sender, EventArgs e)
        {
            isBasicHidden = MsoTriState.msoCTrue;
        }
	}
}