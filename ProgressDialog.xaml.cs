/** Progress Dialog
**
**  Author:     Garison E Piatt
**  Contact:    web@garisonwebdesign.com
**  Created:    11/25/14
**  Version:    1.0.0
**
** This module presents a self-contained progress bar dialog box for WPF applications.  As such, it does not
** contain any code specific to the background process it represents.  Only the code which actually displays
** and updates the progress bar is here.  For this reason, it can be used for multiple purposes in the same
** application without conflict.  Hooks are provided to access application methods in response to progress
** bar events.
**
** Included in this process is the Background Worker, which asynchronously calls the application work method.
** As this method is not an integral part of the progress bar, it is not explained here, except to say that
** it must call the UpdateProgress function with an integer value representing the current status at each
** stage of its process.  Additionally, the application may provide handlers for Progress-Changed and
** Progress-Completed events.  It is the bailiwick of the calling application to determine what these do.
**
** Currently, the progress bar DOES NOT support cancelation.  It is assumed that the purpose of this window
** is to display the current progress of an operation which cannot be interrupted.  Such a features is left
** to the wiles of the next programmer, if such a thing is needed.
**
**
** Include and initalize the Progress Bar Dialog box.  The ShowDialog call starts the bar automatically.
**  pb = new ProgressDialog();                              // Create the progress bar dialog box
**  pb.SetProgressOptions(false, true);                     // Set cancellation, report-progress states (opt)
**  pb.AddDoWorkHandler(bwDoWork);                          // Add the application Do-Work handler (required)
**  pb.AddProgressChangedHandler(bwProgressChanged);        // Add the Progress-Changed handler (if needed)
**  pb.AddProgressCompletedHandler(bwProgressConpleted);    // Add the Progress-Completed handler (if needed)
**  pb.ShowDialog();                                        // Open the dialog box and start the progress bar
**
** Use these functions to update the header, status, and progress bar during the long-running background
** process.
**  pb.ChangeWindowTitle("Header/Title Text");              // Process name, current pass, etc.
**  pb.ChangeStatusMessage("Process Status Message");       // Current file, stage, countdown, etc
**  pb.UpdateProgress(progress);                            // Progress percentage (integer)
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace progressBar {
    /** Progress Dialog Class
    ** This class provides all of the functionality to implement, display, and update the progress bar.
    */
    public partial class ProgressDialog : Window {
        // We need to maintain access to these values for most functions here.
        private static BackgroundWorker bw = null;         // Background task handler
        private static string progressHeaderText = "Pending ...";
        private static string progressMessageText = "Starting ...";
        private static int progressPercentage = 0;

        /** ProgresssDialog constructor
        ** This constructor method creates the progress bar window and initializes the Background Worker.
        ** The default event handlers are also attached to the backgorund worker.  Additional handlers may
        ** be added by the calling application later.
        */
        public ProgressDialog() {
            // Initizlize the background worker here.  Also, add the default handlers for the Progress-Changed
            // and Progress-Completed events.  The backgound worker is started elsewhere, after the window
            // is created.
            bw = new BackgroundWorker();
            bw.ProgressChanged += new ProgressChangedEventHandler(bwProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwRunWorkerCompleted);

            // Set the default background worker options.  By default, cancellation is inhibited, while
            // report-pregress is required.  This should never change.
            SetProgressOptions(false, true);         // set cancellation, report-progress states

            // Create and display the window.
            InitializeComponent();
        }


        /** ****************************************************************************************** **/
        /*                                     CONFIGURATION METHODS                                    */
        /** ****************************************************************************************** **/
        // These methods are called as needed from the main application or sub-process.  All of these
        // methods, exxcept for AddDoWorkHandler, are optional.

        /** SetProgressOptions
        ** This method sets the cancellation and reports-progress states of the backgorund worker.  By
        ** default, these are set to false/off and true/on, respectively.  There should be no need, under
        ** the current design, to change these.  It is not known what will happen if they are.
        **
        ** Inputs:
        **      can         - supports-cancellation state (true or false)
        **      rpt         - report-progress state (true or false)
        */
        public void SetProgressOptions(bool can, bool rpt) {
            bw.WorkerSupportsCancellation = can;
            bw.WorkerReportsProgress = rpt;
        }

        /** AddDoWorkHandler
        ** Set the Do-Work handler of the background worker with a Do-Work event handler that is a part
        ** of the caller's application.  The application method is defined as follows:
        **      void bwDoWork(object sender, DoWorkEventArgs e) {
        **          [ long-running process ]
        **      }
        ** Note that multiple event handlers can be added.
        **
        ** This is the only required method of this group.  However, there is currently no error-checking 
        ** for event-handler-not-defined.  This may be added later, or be left as an exercise for the user.
        */
        public void AddDoWorkHandler(Action<object,DoWorkEventArgs> fn) {
            bw.DoWork += new DoWorkEventHandler(fn);
        }


        /** AddProgressChangedHandler
        ** Add an external progress-changed handler to the current event handler list.  The external 
        ** handler is defined as follows:
        **      void bwProgressChanged(object sender, ProgressChangedEventArgs e) {
        **          [ long-running process ]
        **      }
        ** Note that multiple event handlers can be added.
        **
        ** This method is only called (by the main application) if there is a specialized method for
        ** reacting to a progress-changed event.  In normal operations, this is not usually required.
        ** Therefore, the specifics of the external method are left to the programmer.
        */
        public void AddProgressChangedHandler(Action<object,ProgressChangedEventArgs> fn) {
            bw.ProgressChanged += new ProgressChangedEventHandler(fn);
        }

        /** AddProgressCompletedHandler
        ** Add an external progress-completed handler to the current event handler list.  The external 
        ** handler is defined as follows:
        **      void bwProgressConpleted(object sender, RunWorkerCompletedEventArgs e) {
        **          [ long-running process ]
        **      }
        ** Note that multiple event handlers can be added.
        **
        ** This method is only called (by the main application) if there is a specialized method for
        ** reacting to a progress-changed event.  In normal operations, this is not usually required.
        ** Therefore, the specifics of the external method are left to the programmer.
        */
        public void AddProgressCompletedHandler(Action<object, RunWorkerCompletedEventArgs> fn) {
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(fn);
        }



        /** ****************************************************************************************** **/
        /*                                   CALLER INTERFACE METHODS                                   */
        /** ****************************************************************************************** **/
        // These methods are called at various times by the main application or sub-process to start,
        // update, and describe the progress bar.

        /** Start
        ** Begin the background worker process.  This essentially sets up an asynchronous call to the
        ** application's Do-Work method.
        **
        ** Note that the backgorund process is started automatically when the progress-bar window is
        ** displayed, so this function is not necessary in most cases.
        */
        public void Start() {
            bw.RunWorkerAsync();
        }

        /** UpdateProgress
        ** This method begins an update to the progress bar.  Note that the progress bar is not 
        ** updated directly by this function.  Instead, it tells the backgorund worker to make the
        ** change, and the background worker does so when it can.
        **
        ** The value provided is an integer representing the percentage of completeness, from zero
        ** to one hundred.  This value is saved to use when updating the progress message.
        */
        public void UpdateProgress(int pct) {
            progressPercentage = pct;
            bw.ReportProgress(pct);
        }

        /** ChangeStatusMessage
        ** This method changes the status message (the line just above the progress bar) text as
        ** needed.  However, changes to the window text may not show up until the progress bar is
        ** updated, so we reload the current progress status to make that happen.
        */
        public void ChangeStatusMessage(string msg) {
            progressMessageText = msg;
            bw.ReportProgress(progressPercentage);
        }

        /** ChangeWindowTitle
        ** If the background process occurs in two or more separate stages, it may be desireable
        ** to inform the user of which stage is currently in prorgess.  This allow the application
        ** to modify the Header line, which is bound to the window title.  As with the status message,
        ** changes can only be made when the progress bar changes.
        */
        public void ChangeWindowTitle(string ttl) {
            progressHeaderText = ttl;
            bw.ReportProgress(progressPercentage);
        }


        /** ****************************************************************************************** **/
        /*                                    EVENT-HANDLER METHODS                                     */
        /** ****************************************************************************************** **/
        // This section contains the default event-handling methods.  There is no need for the user to 
        // interface with these in any way; they are set during the initialization phase, and called
        // automatically as needed.

        /** initBackgroundWorker
        ** This function starts the background process (the application's Do-Work function) by starting
        ** the Background Worker Run Worker Async process.  This is a Black Box, and we don't care how
        ** it works, just that it does.
        **
        ** This function is called automatically after the window is created (on the ContentRendered event).
        */
        private void initBackgroundWorker(object sender, EventArgs e) {
            bw.RunWorkerAsync();
        }

        /** bwProgressChanged
        ** This is the default function called automatically when the progress bar changes (the user's
        ** application can add additional handlers, but cannot override this one).  When invoked, it 
        ** assumes that everything has changed, and reloads the values for all display elements.
        */
        private void bwProgressChanged(object sender, ProgressChangedEventArgs e) {
            pbHeader.Text = progressHeaderText;
            pbMessage.Text = progressMessageText;
            pbStatus.Value = e.ProgressPercentage;
        }

        /** bwRunWorkerCompleted
        ** This function is called when the background process is complete (the process is assumed to
        ** be complete when the application's Do-Work function returns).  All this function does is
        ** close the window.  We assume the Backgorund Worker is dismissed at that point.
        */
        private void bwRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            Close();
        }
    } // end class ProgressDialog
} // end namespace progressBar

/*End-User License
**
** Permission is granted to use this package in any endeavor, personal or commercial, PROVIDED
** ALL ATTRIBUTIONS REMAIN.  Full credit for developing and maintaining this code must be given
** to me, and my name and contact information must remain in all relevant files.
**
** Payment for this pakage is not required, but donations to makahou@garisonpiatt.com will be
** gratefully accepted.
**
** Please notify me of any modifications or upgrades made to this package; suitable changes will be
** incorporated into the next update, with proper attributions applied (you get credit for your work).
**
** Garison Piatt
** web@garisonwebdesign.com
*/