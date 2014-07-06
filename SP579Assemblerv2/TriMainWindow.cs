using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SP579Assemblerv2 {
    public partial class TriMainWindow : Form {
        TriTableOfContens toc;
        TriKeywords keyword;
        TriEditor editor;
        TriError error;
        Thread t;
        BackgroundWorker bw;
        TriObjectFile objFile;
        Point origin;

        private string path;
        string fileName;
        bool isFileOpened = false;
        bool isFileOpened2 = false;
        private  bool changed;

        private delegate void updateUIDelegate ();

        public TriMainWindow () {
            keyword = new TriKeywords();
            error = new TriError( this );
            toc = new TriTableOfContens();
            editor = new TriEditor( keyword, this, error, toc );
            objFile = new TriObjectFile( this, editor, toc );
            fileName = string.Empty;
            bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            //TriError.form = this;

            InitializeComponent();
            rtbEditor.AcceptsTab = true;
            rtbLineNumber.ScrollBars = RichTextBoxScrollBars.None;
            origin = rtbLineNumber.Location;
            
        }

        private void rtbEditor_TextChanged ( object sender, EventArgs e ) {
            if ( ( sender as RichTextBox ).Text == string.Empty ) {
                changed = false;

            } else {
                if ( isFileOpened2 ) {
                    changed = true;
                }

            }

        }

        public void updateUI () {

        }

        private void exitToolStripMenuItem_Click ( object sender, EventArgs e ) {
            Environment.Exit( Environment.ExitCode );
        }

        private void Form1_Load ( object sender, EventArgs e ) {
            label4.Text = " Symbol                      Address                   Type";

        }

        public void updateAST ( Dictionary<string, TriAddressSymbolTableValues> AddressSymbolTable ) {



        }

        private void clearToolStripMenuItem_Click ( object sender, EventArgs e ) {
        }

        private void selectAllToolStripMenuItem_Click ( object sender, EventArgs e ) {
            rtbEditor.SelectAll();
        }

        private void copyToolStripMenuItem_Click ( object sender, EventArgs e ) {
            rtbEditor.Copy();
        }

        private void cutToolStripMenuItem_Click ( object sender, EventArgs e ) {
            rtbEditor.Cut();
        }

        private void pasteToolStripMenuItem_Click ( object sender, EventArgs e ) {
            rtbEditor.Paste();
        }

        private void openToolStripMenuItem_Click ( object sender, EventArgs e ) {
            openFile();
        }

        private void saveAsToolStripMenuItem_Click ( object sender, EventArgs e ) {
            SaveFileAs();
        }

        private void saveToolStripMenuItem_Click ( object sender, EventArgs e ) {
            SaveFile( false );
        }

        private void closeToolStripMenuItem_Click ( object sender, EventArgs e ) {
            DialogResult result = MessageBox.Show( "Do you want to save your work?", "Confirm File Close", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning );

            if ( result == DialogResult.Yes ) {
                SaveFile( false );
                CloseFile();

            } else if ( result == DialogResult.No ) {
                CloseFile();
            }

            rtbErrorList.Clear();
            rtbAddressSymbolTable.Clear();

        }

        private void SaveFileAs () {
            SaveFileDialog saveFile = new SaveFileDialog();

            saveFile.Title = "Save As";
            saveFile.Filter = "Text File|*.txt";
            DialogResult result = saveFile.ShowDialog();

            if ( result == DialogResult.OK ) {
                fileName = saveFile.FileName;

                StreamWriter stream = new StreamWriter( fileName );

                stream.Write( rtbEditor.Text );

                stream.Close();
            }
        }

        private void SaveFile ( bool isWindowsShuttingDown ) {
            if ( fileName == "" && !isWindowsShuttingDown && changed ) {
                SaveFileAs();
            } else {
                if ( changed ) {

                    StreamWriter stream = new StreamWriter( fileName );

                    stream.Write( rtbEditor.Text );

                    stream.Close();

                }
            }
        }

        private void CloseFile () {
            rtbEditor.Clear();
            rtbAddressSymbolTable.Clear();
            rtbErrorList.Clear();
            fileName = "";

            //How can I clear up the AST object and all the data structures?
            //is not neccecarly
        }

        private void openFile () {

            OpenFileDialog ofd = new OpenFileDialog();
            DialogResult dr;

            ofd.Filter = "Text File|*.txt|Any File|*.*";
            ofd.Multiselect = false;
            ofd.DereferenceLinks = true;
            ofd.Title = "Open Assembly File";
            if ( path == string.Empty ) {
                ofd.InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
            } else {
                ofd.InitialDirectory = path;
            }

            dr = ofd.ShowDialog();
            StreamReader sr;


            if ( dr == DialogResult.OK ) {
                fileName = ofd.FileName;
                path = fileName.Substring( 0, fileName.LastIndexOf( '\\' ) );
                sr = new StreamReader( fileName );
                rtbEditor.Text = sr.ReadToEnd();
                sr.Close();


                CheckSyntax();
                isFileOpened = true;

            }
        }

        private void aboutToolStripMenuItem_Click ( object sender, EventArgs e ) {
            new Tri_About().Show();
        }

        protected override void OnFormClosing ( FormClosingEventArgs e ) {
            base.OnFormClosing( e );

            if ( e.CloseReason == CloseReason.WindowsShutDown ) {
                if ( fileName != string.Empty ) {
                    SaveFile( true );
                }

                return;
            }

            SaveFile( false );

        }

        public void addToErrorInterface () {

            if ( rtbErrorList.InvokeRequired ) {
                rtbErrorList.Invoke( new updateUIDelegate( addToErrorInterface ) );
            } else {
                int pos = 0;
                int len = 0;

                for ( int i = 0; i < error.Error.Count; i++ ) {
                    if ( error.Error[i].data == string.Empty ) {
                        rtbErrorList.Text += "Line " + ( error.Error[i].lineNumber + 1 ) + ": "
                        + " " + error.ErrorDescription[error.Error[i].error] + Environment.NewLine;

                    } else {

                        rtbErrorList.Text += ( error.Error[i].lineNumber == -1? "" : "Line " + ( error.Error[i].lineNumber + 1 ).ToString() + ": " + " " )
                            + error.ErrorDescription[error.Error[i].error] + " ( " + error.Error[i].data + " )" + Environment.NewLine;
                    }

                    if ( error.Error[i].lineNumber != -1 ) {
                        pos = rtbEditor.GetFirstCharIndexFromLine( error.Error[i].lineNumber );
                        len = rtbEditor.Lines[error.Error[i].lineNumber].Length;

                        rtbEditor.Select( pos, len );
                        rtbEditor.SelectionColor = Color.Black;
                        rtbEditor.SelectionBackColor = Color.PaleVioletRed;
                        rtbEditor.DeselectAll();
                    }

                }

                label5.Text = "Number of Errors: " + error.Error.Count.ToString();


            }

        }

        public void addToAddressSymbolTableInterface () {

            if ( rtbAddressSymbolTable.InvokeRequired ) {
                rtbAddressSymbolTable.Invoke( new updateUIDelegate( addToAddressSymbolTableInterface ) );
            } else {
                foreach ( var item in toc.AddressSymbolTable ) {
                    rtbAddressSymbolTable.Text += "  " + item.Key + "\t\t   " + item.Value.Value.ToString( "X4" ) + "\t\t   "
                        + item.Value.LabelType.ToString() + Environment.NewLine;

                }
            }


        }

        public void addToLocationCounter () {
            if ( txtLocationCounter.InvokeRequired ) {
                txtLocationCounter.Invoke( new updateUIDelegate( addToLocationCounter ) );
            } else {
                if ( editor.isThereAnError ) {
                    txtLocationCounter.Text = "Error";
                } else {
                    txtLocationCounter.Text = editor.locationCounter.ToString( "X4" );
                }
            }
        }

        public void addToProgramLength () {
            if ( txtProgramLength.InvokeRequired ) {
                txtProgramLength.Invoke( new updateUIDelegate( addToProgramLength ) );
            } else {
                try {
                    if ( editor.isThereAnError ) {
                        txtProgramLength.Text = "Error";
                    } else {
                        txtProgramLength.Text = ( editor.locationCounter - toc.LinesOfCodes.First().LocationCounter ).ToString();
                        //txtProgramLength.Text = ( toc.LinesOfCodes.Last().locationCounter - toc.LinesOfCodes.Last().LabelValue ).ToString();
                    }

                } catch {

                }
            }
        }

        private void errorCheckToolStripMenuItem_Click ( object sender, EventArgs e ) {
            CheckSyntax();
        }

        private void rtbErrorList_KeyPress ( object sender, KeyPressEventArgs e ) {
            e.Handled = true;
        }

        private void CheckSyntax () {
            rtbErrorList.Clear();
            rtbAddressSymbolTable.Clear();
            if ( bw.IsBusy != true ) {
                editor.Content = rtbEditor.Lines;
                bw.RunWorkerAsync();
            }




            //editor.Content = rtbEditor.Lines;
            ////editor.rtbText = rtbEditor.Text;
            //
            //if ( t != null && t.IsAlive ) {
            //    t.Abort();
            //}
            //
            //t = new Thread( new ThreadStart( editor.Scan ) );
            //
            //t.IsBackground = true;
            //t.Start();
        }

        void bw_DoWork ( object sender, DoWorkEventArgs e ) {
            editor.Scan();
        }

        void bw_RunWorkerCompleted ( object sender, RunWorkerCompletedEventArgs e ) {
            if ( rtbEditor.Text == string.Empty )
                return;

            int pos = 0;
            int len = 0;
            for ( int i = 0; i < toc.LinesOfCodes.Count; i++ ) {
                if ( toc.LinesOfCodes[i].Label != string.Empty ) {
                    pos = rtbEditor.Lines[toc.LinesOfCodes[i].lineNumber].IndexOf( toc.LinesOfCodes[i].Label );
                    len = toc.LinesOfCodes[i].Label.Length;

                    pos += rtbEditor.GetFirstCharIndexFromLine( toc.LinesOfCodes[i].lineNumber );

                    rtbEditor.Select( pos, len );
                    rtbEditor.SelectionColor = Color.DarkMagenta;
                    rtbEditor.DeselectAll();
                }

                if ( toc.LinesOfCodes[i].OpCode == e_Keywords.NOP ) {
                    pos = rtbEditor.Lines[toc.LinesOfCodes[i].lineNumber].IndexOf( toc.LinesOfCodes[i].Directive.ToString() );
                    if ( pos == -1 ) {
                        string[] t = rtbEditor.Lines[toc.LinesOfCodes[i].lineNumber]
                            .Split( new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries );
                        bool b = false;

                        if ( t.Length == 2 ) {
                            foreach ( e_Directives d in Enum.GetValues( typeof( e_Directives ) ) ) {
                                if ( t[0].ToUpper() == d.ToString() ) {
                                    b = true;
                                    break;
                                }
                            }

                            if ( b ) {
                                pos = rtbEditor.Lines[toc.LinesOfCodes[i].lineNumber].IndexOf( t[0] );
                            }

                        } else if ( t.Length >= 3 ) {
                            foreach ( e_Directives d in Enum.GetValues( typeof( e_Directives ) ) ) {
                                if ( t[1].ToUpper() == d.ToString() ) {
                                    b = true;
                                    break;
                                }
                            }

                            if ( b ) {
                                pos = rtbEditor.Lines[toc.LinesOfCodes[i].lineNumber].IndexOf( t[1] );
                            }
                        }

                    }

                    len = toc.LinesOfCodes[i].Directive.ToString().Length;

                    pos += rtbEditor.GetFirstCharIndexFromLine( toc.LinesOfCodes[i].lineNumber );

                    rtbEditor.Select( pos, len );
                    rtbEditor.SelectionColor = Color.DarkCyan;
                    rtbEditor.DeselectAll();

                } else {
                    pos = rtbEditor.Lines[toc.LinesOfCodes[i].lineNumber].IndexOf( toc.LinesOfCodes[i].OpCode.ToString() );
                    if ( pos == -1 ) {
                        string[] t = rtbEditor.Lines[toc.LinesOfCodes[i].lineNumber]
                            .Split( new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries );
                        bool b = false;

                        if ( t.Length == 2 ) {
                            foreach ( e_Keywords d in Enum.GetValues( typeof( e_Keywords ) ) ) {
                                if ( t[0].ToUpper() == d.ToString() ) {
                                    b = true;
                                    break;
                                }
                            }

                            if ( b ) {
                                pos = rtbEditor.Lines[toc.LinesOfCodes[i].lineNumber].IndexOf( t[0] );
                            }

                        } else if ( t.Length >= 3 ) {
                            foreach ( e_Directives d in Enum.GetValues( typeof( e_Directives ) ) ) {
                                if ( t[1].ToUpper() == d.ToString() ) {
                                    b = true;
                                    break;
                                }
                            }

                            if ( b ) {
                                pos = rtbEditor.Lines[toc.LinesOfCodes[i].lineNumber].IndexOf( t[1] );
                            }
                        }

                    }


                    len = toc.LinesOfCodes[i].OpCode.ToString().Length;

                    pos += rtbEditor.GetFirstCharIndexFromLine( toc.LinesOfCodes[i].lineNumber );

                    rtbEditor.Select( pos, len );
                    rtbEditor.SelectionColor = Color.Blue;
                    rtbEditor.DeselectAll();
                }




            }

            if ( ( toc.LinesOfCodes.Last().lineNumber + 1 ) < rtbEditor.Lines.Length ) {
                pos = rtbEditor.GetFirstCharIndexFromLine( toc.LinesOfCodes.Last().lineNumber + 1 );
                rtbEditor.Select( pos, rtbEditor.Text.Length - pos );
                rtbEditor.SelectionColor = Color.Green;
                rtbEditor.DeselectAll();
            }

            if ( error.Error.Count != 0 ) {
                assembleToolStripMenuItem.Enabled = false;
            } else {
                assembleToolStripMenuItem.Enabled = true;

            }

            if ( isFileOpened ) {
                isFileOpened2 = true;
            }


        }

        private void assembleToolStripMenuItem_Click ( object sender, EventArgs e ) {
            string OC = objFile.objectCode();
            MessageBox.Show( OC );
            return;
            SaveFileDialog saveFile = new SaveFileDialog();
            if ( fileName == string.Empty ) {

                if ( saveFile.ShowDialog() == DialogResult.OK ) {
                    fileName = saveFile.FileName;
                } else {
                    return;
                }
            }

            string path = fileName.Substring( 0, fileName.LastIndexOf( '\\' ) );
            string file = fileName.Substring( fileName.LastIndexOf( '\\' ) + 1 );
            string[] file2 = file.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries );

            StreamWriter stream = new StreamWriter( path + file2[0] + "Object." + file2[1] );
            stream.Write( OC );
            stream.Close();
        }

        private void rtbEditor_VScroll ( object sender, EventArgs e ) {
            rtbLineNumber.Location = origin;

            TriRichTextBoxExtension.SCROLLINFO scInfo = TriRichTextBoxExtension.getScrollInfo( rtbEditor );

            int d = scInfo.nPos / 16;
            int r = scInfo.nPos % 16;

            rtbLineNumber.Clear();
            for ( int i = 1; i <= 14; i++ ) {
                rtbLineNumber.Text += ( i + d ).ToString() + Environment.NewLine;

            }

            rtbLineNumber.Location = new Point( rtbLineNumber.Location.X, rtbLineNumber.Location.Y - r );
            
        }

    }

}
