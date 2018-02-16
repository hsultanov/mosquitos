//=============================================================================
// Copyright © 2017 FLIR Integrated Imaging Solutions, Inc. All Rights Reserved.
//
// This software is the confidential and proprietary information of FLIR
// Integrated Imaging Solutions, Inc. ("Confidential Information"). You
// shall not disclose such Confidential Information and shall use it only in
// accordance with the terms of the license agreement you entered into
// with FLIR Integrated Imaging Solutions, Inc. (FLIR).
//
// FLIR MAKES NO REPRESENTATIONS OR WARRANTIES ABOUT THE SUITABILITY OF THE
// SOFTWARE, EITHER EXPRESSED OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, OR NON-INFRINGEMENT. FLIR SHALL NOT BE LIABLE FOR ANY DAMAGES
// SUFFERED BY LICENSEE AS A RESULT OF USING, MODIFYING OR DISTRIBUTING
// THIS SOFTWARE OR ITS DERIVATIVES.
//=============================================================================
//=============================================================================
// $Id: Form1.cs 316528 2017-02-22 00:03:53Z alin $
//=============================================================================

using System;
using System.Windows;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;

using FlyCapture2Managed;
using FlyCapture2Managed.Gui;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

using System.IO;

using System.IO.Ports;
using System.Configuration;
using System.Collections.Specialized;

namespace GantryPointGrey
{
    public partial class Form1 : Form
    {
        private FlyCapture2Managed.Gui.CameraControlDialog m_camCtlDlg;
        private ManagedCameraBase m_camera = null;
        private ManagedImage m_rawImage;
        private ManagedImage m_processedImage;
        private bool m_grabImages;
        private AutoResetEvent m_grabThreadExited;
        private BackgroundWorker m_grabThread;

        public Form1()
        {
            InitializeComponent();

            m_rawImage = new ManagedImage();
            m_processedImage = new ManagedImage();
            m_camCtlDlg = new CameraControlDialog();

            m_grabThreadExited = new AutoResetEvent(false);
        }





        private Bitmap cropAtRect(Bitmap b, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            Graphics g = Graphics.FromImage(nb);
            g.DrawImage(b, -r.X, -r.Y);
            return nb;
        }
        Rectangle rect = new Rectangle(200, 200, 1200, 1200);

        Point aPoint = new Point(100, 100);











        //private void _updatePics(Bitmap bmp)
        //{

        //    pictureBox1.Image = bmp;
        //    pictureBox1.Refresh();
        //    pictureBox1.Update();
        //    this.Refresh();
        //}





    
                    public void _updateUI(string _wellID, Bitmap bmp)
        {
            if (this.pictureBox1.InvokeRequired)
            {
                try
                {
                    this.pictureBox1.Invoke(dlgUpdateUI, new object[] { _wellID, bmp });
                    Console.WriteLine("UI update OK ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("failed on ui update: ", ex.Message);
                }
            }
            else
            {
                Console.WriteLine(" . . . . . . updating ui . . . . . . . . . . ");

                Bitmap b = new Bitmap(m_processedImage.bitmap);
                Bitmap _bmp = cropAtRect(b, rect);

                pictureBox1.Image = _bmp; // 
               
                Graphics g = Graphics.FromImage(pictureBox1.Image);
                aPoint.X = pictureBox1.Image.Width / 2;
                aPoint.Y = pictureBox1.Image.Height / 2;
           
               


                this.label.Text = _wellID.ToString();
                //this.pictureBox1.Image = _bmp;
              


                PaintCross(g, aPoint);
                g.Dispose();

                pictureBox1.Update();
                this.Refresh();

                UpdateStatusBar();
              
                //this.Refresh();
            }

        }




        //private void _updateUI()
        //{
        //    UpdateStatusBar();
        //    lock (this)
        //    {


        //        Bitmap b = new Bitmap(m_processedImage.bitmap);
        //        Bitmap bmp = cropAtRect(b, rect);

        //        pictureBox1.Image = bmp; // 
        //                                 //pictureBox1.Invalidate();
        //        Graphics g = Graphics.FromImage(pictureBox1.Image);
        //        aPoint.X = pictureBox1.Image.Width / 2;
        //        aPoint.Y = pictureBox1.Image.Height / 2;

        //        PaintCross(g, aPoint);
        //        g.Dispose();

        //    }

        //    //dlgUpdatePics.Invoke();

        //    pictureBox1.Refresh();
        //    pictureBox1.Update();
        //    this.Refresh();
        //}
        private void UpdateUI(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine("Update UI........");
            _updateUI(wellId, bmpToShow);
           
        }


        Pen blackPen = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
        Pen greenPen = new Pen(Color.FromArgb(255, 0, 255, 0), 20);
        Pen greenPenThin = new Pen(Color.FromArgb(255, 0, 255, 0), 3);
        private void PaintCross(Graphics g, Point loc)

        {


            //Half length of the line.

            const int HALF_LEN = 200;

            //Draw horizontal line.
            Point p1 = new Point(loc.X - HALF_LEN, loc.Y);
            Point p2 = new Point(loc.X + HALF_LEN, loc.Y);

            g.DrawLine(greenPenThin, p1, p2);

            //Draw the vertical line.
            p1 = new Point(loc.X, loc.Y - HALF_LEN);
            p2 = new Point(loc.X, loc.Y + HALF_LEN);
            g.DrawLine(greenPenThin, p1, p2);


            p1 = new Point(0, loc.Y);
            p2 = new Point(100, loc.Y);
            g.DrawLine(greenPen, p1, p2);


            p1 = new Point(loc.X * 2, loc.Y);
            p2 = new Point(loc.X * 2 - 100, loc.Y);
            g.DrawLine(greenPen, p1, p2);



            p1 = new Point(loc.X, 0);
            p2 = new Point(loc.X, 100);
            g.DrawLine(greenPen, p1, p2);


            p1 = new Point(loc.X, loc.Y * 2 - 100);
            p2 = new Point(loc.X, loc.Y * 2);
            g.DrawLine(greenPen, p1, p2);

        }
        private void SaveImage(Bitmap bmp, string folderName, string wellId)
        {

            string dirName = _imageFolder + "/" + folderName;
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);



            string fileName = string.Format("{0}/{1}.jpg", dirName, wellId);
            Console.WriteLine(string.Format("{0} : saving under file name {1}", DateTime.Now, fileName));
            bmp.Save(fileName, ImageFormat.Jpeg);
        }

        private void UpdateStatusBar()
        {
            String statusString;

            statusString = String.Format(
                "Image size: {0} x {1}",
                m_rawImage.cols,
                m_rawImage.rows);

            toolStripStatusLabelImageSize.Text = statusString;

            try
            {
                statusString = String.Format(
                "Requested frame rate: {0}Hz",
                m_camera.GetProperty(PropertyType.FrameRate).absValue);
                
            }
            catch (FC2Exception ex)
            {
                statusString = "Requested frame rate: 0.00Hz";
            }

            toolStripStatusLabelFrameRate.Text = statusString;

            TimeStamp timestamp;

            lock (this)
            {
                timestamp = m_rawImage.timeStamp;
            }

            statusString = String.Format(
                "Timestamp: {0:000}.{1:0000}.{2:0000}",
                timestamp.cycleSeconds,
                timestamp.cycleCount,
                timestamp.cycleOffset);

            toolStripStatusLabelTimestamp.Text = statusString;
            statusStrip1.Refresh();
        }


        NameValueCollection settings;
        string _deltaX_string;
        string _deltaY_string;
        string _imageFolder = "./";
        string _arduino_com_port = "COM1";

        decimal deltaX;
        decimal deltaY;

        private void Form1_Load(object sender, EventArgs e)
        {
            dlgUpdateUI = new deldefUpdateUI(_updateUI);


            ManagedBusManager busMgr = new ManagedBusManager();
            ManagedPGRGuid guid = busMgr.GetCameraFromIndex(0);
            m_camera = new ManagedGigECamera();
            m_camera.Connect(guid);

            m_camCtlDlg.Connect(m_camera);

            CameraInfo camInfo = m_camera.GetCameraInfo();
            UpdateFormCaption(camInfo);

            // Set embedded timestamp to on
            EmbeddedImageInfo embeddedInfo = m_camera.GetEmbeddedImageInfo();
            embeddedInfo.timestamp.onOff = true;
            m_camera.SetEmbeddedImageInfo(embeddedInfo);

            m_camera.StartCapture();

            m_grabImages = true;

            // StartGrabLoop();
           


            // Grab Continuous
            btnGrabContinuous.Text = "PAUSED " + Environment.NewLine + "click to start";
            btnGrabContinuous.ForeColor = Color.DimGray;
            btnGrabContinuous.BackColor = Color.DarkGray;





            settings = ConfigurationManager.GetSection("customAppSettingsGroup/customAppSettings") as NameValueCollection;
            try
            {
                _deltaX_string = settings["DELTA_X"];
                deltaX = Convert.ToDecimal(_deltaX_string);

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Delta X is missing  :{0}", ex.Message));
                return;
            }


            try
            {
                _deltaY_string = settings["DELTA_Y"];
                deltaY = Convert.ToDecimal(_deltaY_string);

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Delta Y is missing  :{0}", ex.Message));
                return;
            }

            try
            {
                _imageFolder = settings["IMAGE_FOLDER"];


            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("iMAGE FOLDER is missing  :{0}", ex.Message));
                return;
            }



            // ARDUINO_COM_PORT
            try
            {
                _arduino_com_port = settings["ARDUINO_COM_PORT"];


            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("ARDUINO COM PORT SETTING is missing  :{0}", ex.Message));
                return;
            }

            // initalize the UI
            _createPositions();
            //_loadPositions();
            _connectToArduino();
            //StartGrabOne("test", "_");
            _zeroOutPosition();


            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            // Set the MaximizeBox to false to remove the maximize box.
            this.MaximizeBox = false;
            // Set the MinimizeBox to false to remove the minimize box.
            this.MinimizeBox = false;

            GrabOneBlocking("test", "-");
            try
            {
                _updateUI("0", bmpToShow);
            }
            catch
            {
                Console.WriteLine(string.Format("ui update filed "));
            }




            Show();
        }

        private void UpdateFormCaption(CameraInfo camInfo)
        {
            String captionString = String.Format(
                "Gantry Point Grey - {0} {1} ({2})",
                camInfo.vendorName,
                camInfo.modelName,
                camInfo.serialNumber);

            try
            {
                this.toolStripStatusTCP.Text = camInfo.ipAddress.ToString();
            }
            catch
            {
                this.toolStripStatusTCP.Text = "Unknown";
            }

            this.Text = captionString;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                toolStripButtonStop_Click(sender, e);
                m_camera.Disconnect();
            }
            catch (FC2Exception ex)
            {
                // Nothing to do here
            }
            catch (NullReferenceException ex)
            {
                // Nothing to do here
            }
        }

        private void StartGrabLoop()
        {
            m_grabThread = new BackgroundWorker();
            m_grabThread.ProgressChanged += new ProgressChangedEventHandler(UpdateUI);
            m_grabThread.DoWork += new DoWorkEventHandler(GrabLoop);
            m_grabThread.WorkerReportsProgress = true;
            m_grabThread.RunWorkerAsync();
        }

        DateTime lastReportTime = DateTime.Now;
        System.TimeSpan timeDiff;
        private void GrabLoop(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (m_grabImages)
            {
                try
                {
                    m_camera.RetrieveBuffer(m_rawImage);
                }
                catch (FC2Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                    continue;
                }

                lock (this)
                {

                    m_rawImage.Convert(FlyCapture2Managed.PixelFormat.PixelFormatBgr, m_processedImage);
                }

                timeDiff = DateTime.Now.Subtract(lastReportTime);
                if (timeDiff.Seconds > .0012)
                {
                    lastReportTime = DateTime.Now;
                    worker.ReportProgress(0);
                }
            }

            m_grabThreadExited.Set();
        }





        private void GrabSingle(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            object[] parameters = e.Argument as object[];
            string folderName = parameters[0].ToString();
            string fileName = parameters[1].ToString();

            try
            {
                m_camera.RetrieveBuffer(m_rawImage);
            }
            catch (FC2Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);

            }

            lock (this)
            {
                m_rawImage.Convert(FlyCapture2Managed.PixelFormat.PixelFormatBgr, m_processedImage);

            }


            SaveImage(m_processedImage.bitmap, folderName, fileName);
            worker.ReportProgress(0);


            m_grabThreadExited.Set();
        }

        public delegate void deldefGrab(string folderName, string fileName);
        public deldefGrab dlgGrabOneBlocking;// = new deldefGrab(GrabOneBlocking);


        Bitmap bmpToShow = null;
        public void GrabOneBlocking(string folderName, string fileName)
        {


            try
            {
                m_camera.RetrieveBuffer(m_rawImage);
            }
            catch (FC2Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);

            }

            lock (this)
            {
                m_rawImage.Convert(FlyCapture2Managed.PixelFormat.PixelFormatBgr, m_processedImage);

            }

            //UpdateStatusBar();            

            bmpToShow = cropAtRect(m_processedImage.bitmap, rect);

            // pictureBox1.Image = bmp; // 
            //pictureBox1.Invalidate();

            SaveImage(bmpToShow, folderName, fileName);

            //dlgUpdatePics.Invoke();
            //pictureBox1.Refresh();
            //pictureBox1.Update();
            //this.Refresh();

        }

        private void StartGrabOne(string folderName, string fileName)
        {
            object paramObj1 = folderName;
            object paramObj2 = fileName;
            object[] parameters = new object[] { paramObj1, paramObj2 };

            m_grabThread = new BackgroundWorker();
            m_grabThread.ProgressChanged += new ProgressChangedEventHandler(UpdateUI);
            m_grabThread.DoWork += new DoWorkEventHandler(GrabSingle);
            m_grabThread.WorkerReportsProgress = true;
            m_grabThread.RunWorkerAsync(parameters);
            Thread.Sleep(1000);
        }






        private void toolStripButtonStart_Click(object sender, EventArgs e)
        {
            m_camera.StartCapture();

            m_grabImages = true;

            StartGrabLoop();

      
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            m_grabImages = false;

            try
            {
                m_camera.StopCapture();
            }
            catch (FC2Exception ex)
            {
                Debug.WriteLine("Failed to stop camera: " + ex.Message);
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("Camera is null");
            }

        }

        private void toolCamSettings(object sender, EventArgs e)
        {
            if (m_camCtlDlg.IsVisible())
            {
                m_camCtlDlg.Hide();
                //toolStripButtonCameraControl.Checked = false;
            }
            else
            {
                m_camCtlDlg.Show();
               // toolStripButtonCameraControl.Checked = true;
            }
        }


        private void _resetConnection()
        {
            if (m_grabImages == true)
            {
               
                m_camCtlDlg.Hide();
                m_camCtlDlg.Disconnect();
                m_camera.Disconnect();
            }
        }
        private void OnNewCameraClick(object sender, EventArgs e)
        {
            if (m_grabImages == true)
            {
                toolStripButtonStop_Click(sender, e);
                m_camCtlDlg.Hide();
                m_camCtlDlg.Disconnect();
                m_camera.Disconnect();
            }

            Form1_Load(sender, e);
        }

        private void btnGrabImage_Click(object sender, EventArgs e)
        {
            if (m_grabImages == false)
            {
                m_camera.StartCapture();
                m_grabImages = true;
            }

            StartGrabOne("test", "_");

            // ADD SERVER - CLIENT VIA TCP TO GRAB IMAGES

        }

        //AsyncServer triggerServer;
       

        SerialPort arduinoBoard = new SerialPort();

        private void _connectToArduino()
        {
            if (!arduinoBoard.IsOpen)
            {
                arduinoBoard.PortName = _arduino_com_port;
                arduinoBoard.BaudRate = 9600;
                arduinoBoard.Open();
            }
            else
            {
                arduinoBoard.Close();
            }


            if (arduinoBoard.IsOpen)
            {
                btnConnectToArduino.Text = "CONNECTED" + Environment.NewLine + "click to disconnect";
                btnConnectToArduino.ForeColor = Color.White;
                btnConnectToArduino.BackColor = Color.Green;
            }
            else
            {
                btnConnectToArduino.Text = "DISCONNECTED" + Environment.NewLine + "click to connect";
                btnConnectToArduino.ForeColor = Color.DimGray;
                btnConnectToArduino.BackColor = Color.DarkGray;

            }

        }
        private void btnConnectToArduino_Click(object sender, EventArgs e)
        {


            _connectToArduino();

        }
        string command = "";


        private Object thisLock = new Object();
        private void stopJogging(object sender, EventArgs e)
        {

            lock (thisLock)
            {
                int direction = Convert.ToInt32(1);
                command = String.Format("[jstp]");
                _sendCommandToArduino(command);

            }
        }






        bool zeroSet = false;

        private void _zeroOutPosition()
        {
            this.txtX.Text = "0";
            this.txtY.Text = "0";

            curX = "0";
            curY = "0";

            command = String.Format("[zero]");
            _sendCommandToArduino(command);
            zeroSet = true;
        }
        private void btnSetZero_Click(object sender, EventArgs e)
        {
            _zeroOutPosition();

        }


        private void _moveToNewX()
        {
            int distance = _convertMMtoPulseCounts(int.Parse(curX));
            command = String.Format("[movx{0}]", -1 * distance);
            _sendCommandToArduino(command);
        }

        private void _moveToNewY()
        {
            int distance = _convertMMtoPulseCounts(int.Parse(curY));
            command = String.Format("[movy{0}]", distance);
            _sendCommandToArduino(command);
        }
        private void btnMoveToPos_Click(object sender, EventArgs e)
        {

            _moveToNewX();
            System.Threading.Thread.Sleep(2000);

            _moveToNewY();
            System.Threading.Thread.Sleep(2000);
        }

        private void _sendCommandToArduino(string command)
        {

            Console.WriteLine(string.Format("sending to ..{0}", command));
            arduinoBoard.WriteLine(command);

        }

        private string _receiveDataFromArduino()
        {
            return arduinoBoard.ReadLine();
        }

        private int _convertMMtoPulseCounts(int mm)
        {
            return Convert.ToInt32(mm * 161.5);
        }



        public List<Coordinate> coordinates = new List<Coordinate>();




        private void _createPositions2()
        {
            for (int r = 0; r < 1; r++)
                for (int c = 0; c < 4; c++)
                {
                    int wellID = (c + 1) + (5 - r) * 8;
                    decimal x = c * deltaX;
                    decimal y = r * deltaY;
                    Coordinate aCoor = new Coordinate(wellID.ToString(),
                        x.ToString(), y.ToString());
                    coordinates.Add(aCoor);
                }



            // _addColumnsToGridView(this.dgvCoordinates);
            BindingSource bs = new BindingSource();
            bs.DataSource = coordinates;
            //dgvCoordinates.DataSource = bs;
        }

        private void _createPositions()
        {
            for (int r = 0; r < 6; r++)
                for (int c = 0; c < 8; c++)
                {
                    int wellID = (c + 1) + (5 - r) * 8;
                    decimal x = c * deltaX;
                    decimal y = r * deltaY;
                    Coordinate aCoor = new Coordinate(wellID.ToString(),
                        x.ToString(), y.ToString());
                    coordinates.Add(aCoor);
                }



           // _addColumnsToGridView(this.dgvCoordinates);
            BindingSource bs = new BindingSource();
            bs.DataSource = coordinates;
            //dgvCoordinates.DataSource = bs;
        }







        BindingSource bs = new BindingSource();




        private void _loadPositions()
        {
            string[] lines = System.IO.File.ReadAllLines(@"./positions.txt");
            foreach (string line in lines)
            {

                string[] values = line.Replace("\t", " ").Split(',');
                Console.WriteLine(line);
                Coordinate aCoor = new Coordinate(values[0], values[1], values[2]);
                coordinates.Add(aCoor);
            }

            //_addColumnsToGridView(this.dgvCoordinates);
            BindingSource bs = new BindingSource();
            bs.DataSource = coordinates;
            //dgvCoordinates.DataSource = bs;
        }



        private void _addColumnsToGridView(DataGridView dgv)
        {

            dgv.AutoGenerateColumns = false;
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                dgv.Columns[i].Visible = false;
            }
            var colID = new DataGridViewTextBoxColumn();
            var colX = new DataGridViewTextBoxColumn();
            var colY = new DataGridViewTextBoxColumn();


            colID.HeaderText = "WellID";
            colID.DataPropertyName = "WellId";
            colID.Width = 60;
            colID.DefaultCellStyle.Format = "N0";


            colX.HeaderText = "X";
            colX.DataPropertyName = "X";
            colX.Width = 60;

            colY.HeaderText = "Y";
            colY.DataPropertyName = "Y";
            colY.Width = 60;





            dgv.Columns.Clear();
            dgv.Columns.AddRange(new DataGridViewColumn[] { colID, colX, colY });



        }


        private void button6_Click(object sender, EventArgs e)
        {
            _runAcq();
        }

        string prevY = "";
        string wellId = "test";
        string curX = "0";
        string curY = "0";

        int curWellIndex = 0;
        private void _runAcq(int startWellIndex = 0, string folderName = "./")
        {

            // string folderName = DateTime.Now.ToString("yyMMddHHmm");

            int n = coordinates.Count;
            bool newRow = true;

            for (int i = startWellIndex; i < n && curAcqState == acqState.Run; i++)
            {

                //if (i - 1 >= 0)
                //    if (dgvCoordinates.Rows[i - 1].Selected)
                //        dgvCoordinates.Rows[i - 1].Selected = false;


                //dgvCoordinates.Rows[i].Selected = true;



                wellId = coordinates[i].WellID;
                curX = coordinates[i].X;
                curY = coordinates[i].Y;


                //wellId = dgvCoordinates.Rows[i].Cells[0].Value.ToString();
                //this.txtX.Text = dgvCoordinates.Rows[i].Cells[1].Value.ToString();
                //this.txtY.Text = dgvCoordinates.Rows[i].Cells[2].Value.ToString();
                if (prevY != curY)
                {
                    newRow = true;
                    prevY = curY;
                }
                else
                {
                    newRow = false;
                }


                if (i != 0)
                {
                    _moveToNewX();
                    System.Threading.Thread.Sleep(2000);
                    if (i == 0)
                        System.Threading.Thread.Sleep(2000);

                    if (newRow)
                    {
                        _moveToNewY();
                        System.Threading.Thread.Sleep(2000);
                    }

                    if (i == 0)
                        System.Threading.Thread.Sleep(2000);

                    if (newRow)
                        System.Threading.Thread.Sleep(4000);
                }
                //StartGrabOne(folderName,  wellId.ToString());
                //dlgGrabOneBlocking.Invoke(folderName, wellId.ToString());
                GrabOneBlocking(folderName, wellId.ToString());
                try
                {
                    _updateUI(wellId, bmpToShow);
                } catch
                {
                    Console.WriteLine(string.Format("ui update filed "));
                }
                //dgvCoordinates.FirstDisplayedScrollingRowIndex = i;
                //dgvCoordinates.Invoke(new Action(() => dgvCoordinates.FirstDisplayedScrollingRowIndex =i));
                try
                {
                    //Console.WriteLine(string.Format("well = {0}", wellId));
                    _updateIndicator(int.Parse(wellId), 1);
                } catch
                {

                }

                curWellIndex = i;
                //this.Refresh();
            }


            if (curWellIndex == n - 1)
                curAcqState = acqState.Completed;

            string currentStateAtTheEnd = curAcqState.ToString();

            Console.WriteLine(" _runAcq stopped with {0}", currentStateAtTheEnd);
            if (curAcqState == acqState.Stopped || curAcqState == acqState.Completed)
            {
                MoveGantryToZero();
                btnGo.BackgroundImage = Properties.Resources.btnPlay2;

            }
        }

        private void MoveGantryToZero()
        {
            curX = "0";
            curY = "0";
            _moveToNewX();
            System.Threading.Thread.Sleep(2000);
            _moveToNewY();
            System.Threading.Thread.Sleep(2000);
        }


        private void _resetAllIndicators()
        {
            int n =  coordinates.Count;
            for (int i = 0; i< n; i++ )
            {
               string _curId = coordinates[i].WellID;

                try
                {
                    int curId_int = int.Parse(_curId);
                    _updateIndicator(curId_int, 0);

                } catch
                {

                }
            }

        }

        private void _updateIndicator(int i, int status)
        {
            System.Windows.Forms.Panel aPanel = null;

            switch (i)
            {
                case 1:
                    aPanel = pan01;
                    break;
                case 2:
                    aPanel = pan02;
                    break;
                case 3:
                    aPanel = pan03;
                    break;
                case 4:
                    aPanel = pan04;
                    break;
                case 5:
                    aPanel = pan05;
                    break;
                case 6:
                    aPanel = pan06;
                    break;
                case 7:
                    aPanel = pan07;
                    break;
                case 8:
                    aPanel = pan08;
                    break;
                case 9:
                    aPanel = pan09;
                    break;
                case 10:
                    aPanel = pan10;
                    break;
                case 11:
                    aPanel = pan11;
                    break;
                case 12:
                    aPanel = pan12;
                    break;
                case 13:
                    aPanel = pan13;
                    break;
                case 14:
                    aPanel = pan14;
                    break;
                case 15:
                    aPanel = pan15;
                    break;
                case 16:
                    aPanel = pan16;
                    break;
                case 17:
                    aPanel = pan17;
                    break;
                case 18:
                    aPanel = pan18;
                    break;
                case 19:
                    aPanel = pan19;
                    break;
                case 20:
                    aPanel = pan20;
                    break;
                case 21:
                    aPanel = pan21;
                    break;
                case 22:
                    aPanel = pan22;
                    break;
                case 23:
                    aPanel = pan23;
                    break;
                case 24:
                    aPanel = pan24;
                    break;
                case 25:
                    aPanel = pan25;
                    break;
                case 26:
                    aPanel = pan26;
                    break;
                case 27:
                    aPanel = pan27;
                    break;
                case 28:
                    aPanel = pan28;
                    break;
                case 29:
                    aPanel = pan29;
                    break;
                case 30:
                    aPanel = pan30;
                    break;
                case 31:
                    aPanel = pan31;
                    break;
                case 32:
                    aPanel = pan32;
                    break;
                case 33:
                    aPanel = pan33;
                    break;
                case 34:
                    aPanel = pan34;
                    break;
                case 35:
                    aPanel = pan35;
                    break;
                case 36:
                    aPanel = pan36;
                    break;
                case 37:
                    aPanel = pan37;
                    break;
                case 38:
                    aPanel = pan38;
                    break;
                case 39:
                    aPanel = pan39;
                    break;
                case 40:
                    aPanel = pan40;
                    break;
                case 41:
                    aPanel = pan41;
                    break;
                case 42:
                    aPanel = pan42;
                    break;
                case 43:
                    aPanel = pan43;
                    break;
                case 44:
                    aPanel = pan44;
                    break;
                case 45:
                    aPanel = pan45;
                    break;
                case 46:
                    aPanel = pan46;
                    break;
                case 47:
                    aPanel = pan47;
                    break;
                case 48:
                    aPanel = pan48;
                    break;
            };

            if (aPanel == null)
            {
                return;
            }

            if (status == 0)
                aPanel.BackgroundImage = Properties.Resources.donutGrey;

            if (status == 1)
                aPanel.BackgroundImage = Properties.Resources.donutGreen;

            if (status == 2)
                aPanel.BackgroundImage = Properties.Resources.donutRed;

            aPanel.Refresh();



            return;
        }





        private void btnJogYPlus_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine(" start jogging Y+");
            lastStartJoggingTimeStamp = DateTime.Now;
            int direction = Convert.ToInt32("1");
            command = String.Format("[jogy{0}]", direction);


            var t = Task.Run(() => _sendCommandToArduino(command));
            t.Wait();
            //_sendCommandToArduino(command);
        }




        private void btnJogYNeg_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine(" - - -  start jogging Y-");
            lastStartJoggingTimeStamp = DateTime.Now;
            int direction = -1; Convert.ToInt32("-1");
            command = String.Format("[jogy{0}]", direction);

            var t = Task.Run(() => _sendCommandToArduino(command));
            t.Wait();


        }






        private void btnGrabContinuous_Click(object sender, EventArgs e)
        {
            //  _turnCameraOnOff();
            Console.WriteLine("XOXOXOX ");
        }

        
        private void _turnCameraOnOff(bool cameraOn) {
            if (cameraOn)
            {
                //if (m_grabImages == false)
                //{
                //    m_camera.StartCapture();
                //    m_grabImages = true;
                //}

                // grabInLoopFlag = true;
                try
                {
                    m_camera.StartCapture();
                }
                catch
                {
                    // do nothing if is started let it be.
                }


                m_grabImages = true;
                StartGrabLoop();

                //btnGrabImage.Enabled = false;


            }
            else
            {
                // grabInLoopFlag = false;
                m_grabImages = false;
                try
                {
                    //m_camera.StopCapture();


                    try
                    {
                        m_grabThread.CancelAsync();
                    }
                    catch
                    {
                        Console.WriteLine("filed to stopped the grab cont thread");
                    }
                }
                catch (FC2Exception ex)
                {
                    Debug.WriteLine("Failed to stop camera: " + ex.Message);
                }
                catch (NullReferenceException)
                {
                    Debug.WriteLine("Camera is null");
                }
                // btnGrabImage.Enabled = true;

            }





        }










        private enum AppOpsMode { Manual, Auto };

        AppOpsMode curAppOpsMode = AppOpsMode.Auto;

        private void rbtnAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnAuto.Checked)
            {
                panelManual.Visible = false;
                panelAuto.Visible = true;
                btnPause.BackgroundImage = Properties.Resources.btnPause2;
                btnPause.Enabled = true;
                btnImageGrab.Visible = false;
                curAppOpsMode = AppOpsMode.Auto;
            } else
            {
                panelManual.Visible = true;
                panelAuto.Visible = false;
                btnPause.Enabled = false;
                btnPause.BackgroundImage = Properties.Resources.btnPauseDisabled;
                btnImageGrab.Visible = true;
                curAppOpsMode = AppOpsMode.Manual;
            }
        }

        private void btnJogYPlus_Click_1(object sender, EventArgs e)
        {
            Console.WriteLine(" stop jogging");
            command = String.Format("[jstp]");
            _sendCommandToArduino(command);
        }


        private void stopJogging(object sender, MouseEventArgs e)
        {



            timeDiff = DateTime.Now.Subtract(lastStartJoggingTimeStamp);
            if (timeDiff.Seconds < .5)
            {
                lastStartJoggingTimeStamp = DateTime.Now;
                Thread.Sleep(1000);
            }


            Console.WriteLine(" stop jogging ");
            command = String.Format("[jstp]");
            _sendCommandToArduino(command);
            Thread.Sleep(1000);
            _sendCommandToArduino(command);

        }






    private DateTime lastStartJoggingTimeStamp = DateTime.Now;
        
        private void btnJogXNeg_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine(" start jogging X-");

            int direction = Convert.ToInt32("1");
            command = String.Format("[jogx{0}]", direction);
            lastStartJoggingTimeStamp = DateTime.Now;
            var t = Task.Run(() => _sendCommandToArduino(command));
            t.Wait();
        }

        private void btnJogXPos_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine(" start jogging X+");
            lastStartJoggingTimeStamp = DateTime.Now;
            int direction = Convert.ToInt32("-1");
            command = String.Format("[jogx{0}]", direction);
            var t = Task.Run(() => _sendCommandToArduino(command));
            t.Wait();
        }

        private void panel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Button Clicked ");
            var cts = new CancellationTokenSource();

            //var t = Task.Run(() => _longBlockingTask(5));


            Console.WriteLine("Task Completed ");
            dlgUpdateUI = new deldefUpdateUI(_updateUI);


        }


        int j = 0;
        //Bitmap b01 = new Bitmap(@"c:\temp\z\01.bmp");
        //Bitmap b02 = new Bitmap(@"c:\temp\z\02.bmp");
        //private void _longBlockingTask(int i)
        //{
        //    Bitmap bmp = b01;
        //    for (j = i; j < 15; j++)
        //    {
        //        if (j % 2 == 0)
        //        {
        //            bmp = new Bitmap(b01);
        //            Console.WriteLine("bmp = b01");
        //        }
        //        else
        //        {
        //            bmp = new Bitmap(b02);
        //            Console.WriteLine("bmp = b02");
        //        }

        //        _updateUI(j.ToString(), bmp);

        //        Thread.Sleep(1000);
        //    }
        //}

        public delegate void deldefUpdateUI(string _wellID, Bitmap bmp);
        public deldefUpdateUI dlgUpdateUI;

       
           
        



        private enum acqState
        {
            Stopped,
            Paused,
            Run,
            Completed


        }


        private void _createPositions3()
        {
            for (int r = 0; r < 6; r++)
                for (int c = 0; c < 8; c++)
                {
                    int wellID = (c + 1) + (5 - r) * 8;
                    decimal x = c * deltaX;
                    decimal y = r * deltaY;
                    Coordinate aCoor = new Coordinate(wellID.ToString(),
                        x.ToString(), y.ToString());
                    coordinates.Add(aCoor);
                }



            // _addColumnsToGridView(this.dgvCoordinates);
            BindingSource bs = new BindingSource();
            bs.DataSource = coordinates;
            //dgvCoordinates.DataSource = bs;
        }

        private acqState curAcqState = acqState.Stopped;
        string folderName = "";
        private void btnGo_Click(object sender, EventArgs e)
        {
            if (curAcqState == acqState.Stopped || curAcqState == acqState.Completed || curAcqState == acqState.Paused)
            {
                if (curAcqState == acqState.Stopped  || curAcqState == acqState.Completed )
                {
                    curWellIndex = 0;
                    folderName = "";
                }

                curAcqState = acqState.Run;
                
                btnGo.BackgroundImage = Properties.Resources.btnPlayOn;
                btnPause.BackgroundImage = Properties.Resources.btnPause2;
               // btnImageGrab.BackgroundImage = Properties.Resources.btnCam;

                btnImageGrab.BackgroundImage = Properties.Resources.btnCamDisabled;
                btnImageGrab.Enabled = false;
                _resetAllIndicators();
                this.Refresh();



            }



            if (curAppOpsMode == AppOpsMode.Auto)
            {
                //var t = Task.Run(() => _runAcq(curWellIndex));
                //t.Wait();

              
                if (folderName == "")
                    folderName = DateTime.Now.ToString("yyMMddHHmm");

                //_runAcq(curWellIndex, folderName);
                Console.WriteLine("- ------------- Starting  Acq in Async ----------------");
                var t = Task.Run(() => _runAcq(curWellIndex, folderName));
                Console.WriteLine("- ------------- Acq in Async Initiated ----------------");
                //t.Wait();
                
               
            }
            else
            {
                
                _turnCameraOnOff(true);
            }


        }







      











        private void btnPause_Click(object sender, EventArgs e)
        {
            if (curAcqState == acqState.Run )
            {
                curAcqState = acqState.Paused;
                btnGo.BackgroundImage = Properties.Resources.btnPlay2;
                btnPause.BackgroundImage = Properties.Resources.btnPauseOn;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (curAcqState == acqState.Run || curAcqState == acqState.Paused)
            {
                curAcqState = acqState.Stopped;

                btnGo.BackgroundImage = Properties.Resources.btnPlay2;
                btnPause.BackgroundImage = Properties.Resources.btnPause2;
                btnImageGrab.BackgroundImage = Properties.Resources.btnCam;
                btnImageGrab.Enabled = true;
                this.Refresh();

            }

            if (curAppOpsMode == AppOpsMode.Manual )
            {
                _turnCameraOnOff(false);
           
            }

        }

        private void btnStop_MouseDown(object sender, MouseEventArgs e)
        {
            btnStop.BackgroundImage = Properties.Resources.btnStopOn;
        }

        private void btnStop_MouseUp(object sender, MouseEventArgs e)
        {
            btnStop.BackgroundImage = Properties.Resources.btnStop;
        }

        private void btnImageGrab_Click(object sender, EventArgs e)
        {

            string folderName = "manual";
            string fileName = "one_shot";

            try
            {
                m_camera.RetrieveBuffer(m_rawImage);
            }
            catch (FC2Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);

            }

            lock (this)
            {
                m_rawImage.Convert(FlyCapture2Managed.PixelFormat.PixelFormatBgr, m_processedImage);

            }

            bmpToShow = cropAtRect(m_processedImage.bitmap, rect);
            _updateUI("00", bmpToShow);
            SaveImage(m_processedImage.bitmap, folderName, fileName);



            //if (m_grabImages == false)
            //{
            //    m_camera.StartCapture();
            //    m_grabImages = true;
            //}

            //StartGrabOne("manual", "one_shot");

            //if (m_grabImages == true)
            //{
            //    m_camera.StopCapture();
            //    m_grabImages = false;
            //}
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }



    public class Coordinate
    {
        public string WellID { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public Coordinate(string _wellId, string _x, string _y)
        {
            X = _x.ToString();
            Y = _y.ToString();
            string  _temp = ("00" + _wellId.Trim()).ToString();
            WellID = _temp.Substring(_temp.Length - 2);
        }
    }
}

//=============================================================================
// $Log: not supported by cvs2svn $
// Revision 1.3  2011/02/02 17:52:47  soowei
// [1] Handle grab errors in the grab loop
//
// Revision 1.2  2011/02/02 01:20:16  soowei
// [1] Add some more information to GUI
//
// Revision 1.1  2011/02/01 23:10:35  soowei
// [1] Adding FlyCapture2SimpleGUI_CSharp example
//
//=============================================================================
