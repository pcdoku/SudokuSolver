namespace Kermalis.SudokuSolver.UI
{
    internal sealed partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer _components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (_components != null))
            {
                _components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this._splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._sudokuBoard = new Kermalis.SudokuSolver.UI.SudokuBoard();
            this._logList = new System.Windows.Forms.ListBox();
            this._solveButton = new System.Windows.Forms.Button();
            this._menuStrip1 = new System.Windows.Forms.MenuStrip();
            this._fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._importFPuzzlesStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this._statusStrip1 = new System.Windows.Forms.StatusStrip();
            this._puzzleLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer1)).BeginInit();
            this._splitContainer1.Panel1.SuspendLayout();
            this._splitContainer1.Panel2.SuspendLayout();
            this._splitContainer1.SuspendLayout();
            this._menuStrip1.SuspendLayout();
            this._statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _splitContainer1
            // 
            this._splitContainer1.IsSplitterFixed = true;
            this._splitContainer1.Location = new System.Drawing.Point(41, 61);
            this._splitContainer1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._splitContainer1.Name = "_splitContainer1";
            // 
            // _splitContainer1.Panel1
            // 
            this._splitContainer1.Panel1.Controls.Add(this._sudokuBoard);
            // 
            // _splitContainer1.Panel2
            // 
            this._splitContainer1.Panel2.Controls.Add(this._logList);
            this._splitContainer1.Size = new System.Drawing.Size(1098, 542);
            this._splitContainer1.SplitterDistance = 548;
            this._splitContainer1.SplitterWidth = 1;
            this._splitContainer1.TabIndex = 2;
            // 
            // _sudokuBoard
            // 
            this._sudokuBoard.Cursor = System.Windows.Forms.Cursors.Hand;
            this._sudokuBoard.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sudokuBoard.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this._sudokuBoard.Location = new System.Drawing.Point(0, 0);
            this._sudokuBoard.Margin = new System.Windows.Forms.Padding(0);
            this._sudokuBoard.Name = "_sudokuBoard";
            this._sudokuBoard.Size = new System.Drawing.Size(548, 542);
            this._sudokuBoard.TabIndex = 0;
            // 
            // _logList
            // 
            this._logList.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._logList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this._logList.FormattingEnabled = true;
            this._logList.HorizontalScrollbar = true;
            this._logList.Location = new System.Drawing.Point(0, 18);
            this._logList.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._logList.Name = "_logList";
            this._logList.Size = new System.Drawing.Size(549, 524);
            this._logList.TabIndex = 0;
            // 
            // _solveButton
            // 
            this._solveButton.Enabled = false;
            this._solveButton.Location = new System.Drawing.Point(523, 31);
            this._solveButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._solveButton.Name = "_solveButton";
            this._solveButton.Size = new System.Drawing.Size(88, 26);
            this._solveButton.TabIndex = 1;
            this._solveButton.Text = "Solve";
            this._solveButton.UseVisualStyleBackColor = true;
            this._solveButton.Click += new System.EventHandler(this.SolvePuzzle);
            // 
            // _menuStrip1
            // 
            this._menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this._menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileToolStripMenuItem});
            this._menuStrip1.Location = new System.Drawing.Point(0, 0);
            this._menuStrip1.Name = "_menuStrip1";
            this._menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this._menuStrip1.Size = new System.Drawing.Size(1180, 24);
            this._menuStrip1.TabIndex = 3;
            this._menuStrip1.Text = "menuStrip1";
            // 
            // _fileToolStripMenuItem
            // 
            this._fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._newToolStripMenuItem,
            this._openToolStripMenuItem,
            this._importFPuzzlesStripMenuItem1,
            this._saveAsToolStripMenuItem,
            this._exitToolStripMenuItem});
            this._fileToolStripMenuItem.Name = "_fileToolStripMenuItem";
            this._fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this._fileToolStripMenuItem.Text = "File";
            this._fileToolStripMenuItem.Click += new System.EventHandler(this._fileToolStripMenuItem_Click);
            // 
            // _newToolStripMenuItem
            // 
            this._newToolStripMenuItem.Name = "_newToolStripMenuItem";
            this._newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this._newToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._newToolStripMenuItem.Text = "New";
            this._newToolStripMenuItem.Click += new System.EventHandler(this.NewPuzzle);
            // 
            // _openToolStripMenuItem
            // 
            this._openToolStripMenuItem.Name = "_openToolStripMenuItem";
            this._openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this._openToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._openToolStripMenuItem.Text = "Open";
            this._openToolStripMenuItem.Click += new System.EventHandler(this.OpenPuzzle);
            // 
            // _saveAsToolStripMenuItem
            // 
            this._saveAsToolStripMenuItem.Enabled = false;
            this._saveAsToolStripMenuItem.Name = "_saveAsToolStripMenuItem";
            this._saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this._saveAsToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._saveAsToolStripMenuItem.Text = "Save As";
            this._saveAsToolStripMenuItem.Click += new System.EventHandler(this.SavePuzzle);
            // 
            // _exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            this._exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._exitToolStripMenuItem.Text = "Exit";
            this._exitToolStripMenuItem.Click += new System.EventHandler(this.Exit);
            // 
            // _importFPuzzlesStripMenuItem1
            // 
            this._importFPuzzlesStripMenuItem1.Name = "_importFPuzzlesStripMenuItem1";
            this._importFPuzzlesStripMenuItem1.Size = new System.Drawing.Size(193, 22);
            this._importFPuzzlesStripMenuItem1.Text = "Import f-puzzles URL...";
            this._importFPuzzlesStripMenuItem1.Click += new System.EventHandler(this._importFPuzzlesStripMenuItem1_Click);
            // 
            // _statusStrip1
            // 
            this._statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this._statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._puzzleLabel,
            this._statusLabel});
            this._statusStrip1.Location = new System.Drawing.Point(0, 615);
            this._statusStrip1.Name = "_statusStrip1";
            this._statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this._statusStrip1.Size = new System.Drawing.Size(1180, 22);
            this._statusStrip1.TabIndex = 0;
            this._statusStrip1.Text = "statusStrip1";
            // 
            // _puzzleLabel
            // 
            this._puzzleLabel.Name = "_puzzleLabel";
            this._puzzleLabel.Size = new System.Drawing.Size(68, 17);
            this._puzzleLabel.Text = "puzzleLabel";
            // 
            // _statusLabel
            // 
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new System.Drawing.Size(66, 17);
            this._statusLabel.Text = "statusLabel";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1180, 637);
            this.Controls.Add(this._statusStrip1);
            this.Controls.Add(this._solveButton);
            this.Controls.Add(this._splitContainer1);
            this.Controls.Add(this._menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this._menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "MainWindow";
            this.Text = "Sudoku Solver";
            this._splitContainer1.Panel1.ResumeLayout(false);
            this._splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer1)).EndInit();
            this._splitContainer1.ResumeLayout(false);
            this._menuStrip1.ResumeLayout(false);
            this._menuStrip1.PerformLayout();
            this._statusStrip1.ResumeLayout(false);
            this._statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer _splitContainer1;
        private SudokuSolver.UI.SudokuBoard _sudokuBoard;
        private System.Windows.Forms.Button _solveButton;
        private System.Windows.Forms.MenuStrip _menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem _fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _openToolStripMenuItem;
        private System.Windows.Forms.StatusStrip _statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel _statusLabel;
        private System.Windows.Forms.ListBox _logList;
        private System.Windows.Forms.ToolStripStatusLabel _puzzleLabel;
        private System.Windows.Forms.ToolStripMenuItem _newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _importFPuzzlesStripMenuItem1;
    }
}

