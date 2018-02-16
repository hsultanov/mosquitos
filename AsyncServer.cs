using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace GantryPointGrey
{
    public class AsyncServer
    {

        public delegate void ConnectionChangedEventHandler();
        public delegate void TriggerEventHandler();

        public ConnectionChangedEventHandler connectionStatus;
        public TriggerEventHandler triggerRequest;

        public Socket serverSocket;
        private Socket clientSocket; // We will only accept one socket.
        private byte[] buffer;

        



        public AsyncServer()
        {

        }

        public void StartServer()
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, 3333));
                serverSocket.Listen(10);
                serverSocket.BeginAccept(AcceptCallback, null);
                connectionStatus();
            }
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }
        }
        private void AcceptCallback(IAsyncResult AR)
        {
            try
            {
                clientSocket = serverSocket.EndAccept(AR);
                buffer = new byte[clientSocket.ReceiveBufferSize];

                // Send a message to the newly connected client.
                var sendData = Encoding.ASCII.GetBytes("Hello");
                clientSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, SendCallback, null);
                // Listen for client data.
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
                // Continue listening for clients.
                serverSocket.BeginAccept(AcceptCallback, null);

                
            }
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }
        }

        private void SendCallback(IAsyncResult AR)
        {
            try
            {
                clientSocket.EndSend(AR);
            }
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                // Socket exception will raise here when client closes, as this sample does not
                // demonstrate graceful disconnects for the sake of simplicity.
                int received = clientSocket.EndReceive(AR);

                if (received == 0)
                {
                    return;
                }

                // The received data is deserialized in the PersonPackage ctor.
                VisionInstruction instruction = new VisionInstruction(buffer);

                Console.WriteLine(string.Format("command: {0} ", instruction.Cmd));
                Console.WriteLine(string.Format("data   : {0}", instruction.Data));

                if (instruction.Cmd == "TRGR")
                {
                    VisionInstruction instructionRes =
                               new VisionInstruction("ACKN", "");
                    byte[] buffer = instructionRes.ToByteArray();
                    //byte[] buffer = Encoding.ASCII.GetBytes(textBoxEmployee.Text);
                    clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, null);


                    // SEND REQUEST TO TRIGGER CAMERA
                    triggerRequest();
                    System.Threading.Thread.Sleep(1000);


                    VisionInstruction instructionExecRes =
                              new VisionInstruction("RSLT", "SOME DAATA");
                    byte[] bufferExecRes = instructionExecRes.ToByteArray();

                    clientSocket.BeginSend(bufferExecRes, 0, bufferExecRes.Length, SocketFlags.None, SendCallback, null);

                }

                // SubmitPersonToDataGrid(person);

                //string result = System.Text.Encoding.UTF8.GetString(buffer);
                //Console.WriteLine(string.Format(" got from client: {result}",result));
                //Console.WriteLine(result);



                // Start receiving data again.
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            // Avoid Pokemon exception handling in cases like these.
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }
        }

        /// <summary>
        /// Provides a thread safe way to add a row to the data grid.
        /// </summary>
        //private void SubmitPersonToDataGrid(VisionInstruction vi)
        //{
        //    Invoke((Action)delegate
        //    {
        //        // dataGridView.Rows.Add(vi.Name, person.Age, vi.IsMale);
        //    });
        //}

        private static void ShowErrorDialog(string message)
        {
            //MessageBox.Show(message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Console.WriteLine(string.Format("Error: {0} !!!", message));
        }


    }



}
