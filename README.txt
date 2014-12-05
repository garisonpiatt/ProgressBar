Progress Bar Dialog Box

 Author:     Garison E Piatt
 Contact:    web@garisonwebdesign.com
 Created:    11/25/14
 Version:    1.0.0

This is a self-contained progress bar dialog box for WPF applications.  It does not contain any code specific
to the background process it represents.  Only the code which actually displays and updates the progress bar
is here.  For this reason, it can be used for multiple purposes in the same application without conflict.  
Hooks are provided to access application methods in response to progress bar events.

Currently, the progress bar DOES NOT support cancelation.  It is assumed that the purpose of this window is to
display the current progress of an operation which cannot be interrupted.  This feature is left to the wiles 
of the next programmer, if such a thing is needed.


Implement as follows:
First, add ProgressDialog.xaml and ProgressDialog.xaml.cs to your Visual Studio WPF project.

Next, edit the file which contains your long-running process.  At the top of that file, insert this:
	using progressBar;

Place this inside the class definition:
	private static ProgressDialog pb = null;	   // Progress bar window


In the constructor or initialize section of this file place this code:
	//Include and initalize the Progress Bar Dialog box.  The ShowDialog call starts the bar automatically.
	pb = new ProgressDialog();	   	                // Create the progress bar dialog box
	pb.SetProgressOptions(false, true);	                // Set cancellation, report-progress states (opt)
	pb.AddDoWorkHandler(bwDoWork);	   	                // Add the application Do-Work handler (required)
	pb.AddProgressChangedHandler(bwProgressChanged);        // Add the Progress-Changed handler (if needed)
	pb.AddProgressCompletedHandler(bwProgressConpleted);    // Add the Progress-Completed handler (if needed)
	pb.ShowDialog();	   	   	                // Open the dialog box and start the progress bar

Create a Do-Work function with the following code.  Change the flow and messages as required.
	void bwDoWork(object sender, DoWorkEventArgs e) {
	   // Tell the world what we're doing.
	   pb.ChangeWindowTitle("Background Process Title");
		
	   // Initialize the progress counter and percentage values.
	   int passPercentage = 100 / tableList.Count;
	   int passProgress = 0;

	   // This is the loop that does the work.
	   foreach (string item in itemList) {
	       // Send the item name to the progress bar, so we know what's going on.
	       pb.ChangeStatusMessage(item);

		[ Do each part of the long-running process here ]

	       // Update the progress bar
	       passProgress += passPercentage;
	       pb.UpdateProgress(passProgress);
	   } // end foreach (string tbl in tableList)
	   pb.ChangeStatusMessage("Process Complete");
	   pb.UpdateProgress(100);
	}
