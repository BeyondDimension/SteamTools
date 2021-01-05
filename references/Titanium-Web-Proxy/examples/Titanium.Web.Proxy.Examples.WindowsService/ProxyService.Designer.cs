namespace WindowsServiceExample
{
    partial class ProxyService
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ProxyServiceEventLog = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.ProxyServiceEventLog)).BeginInit();
            // 
            // ProxyServiceEventLog
            // 
            this.ProxyServiceEventLog.Log = "Application";
            this.ProxyServiceEventLog.Source = "ProxyService";
            // 
            // ProxyService
            // 
            this.ServiceName = "ProxyService";
            ((System.ComponentModel.ISupportInitialize)(this.ProxyServiceEventLog)).EndInit();

        }

        #endregion
        private System.Diagnostics.EventLog ProxyServiceEventLog;
    }
}
