﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace AdvancedCoding2
{
    public class Client : IClientModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int port, csvRowsNum;
        private volatile int playSpeed, lineNum;
        private string server;
        private volatile string filePath;

       public int TransSpeed { 
            get
            {
                return playSpeed;
            }
            set
            {
                if(TransSpeed != value)
                {
                    playSpeed = value;
                    NotifyPropertyChanged("playSpeed");
                }
            }
        }

        public int simLen
        {
            get
            {
                return csvRowsNum;
            }
            set
            {
                if (simLen != value)
                {
                    csvRowsNum = value;
                    NotifyPropertyChanged("simLen");
                }
            }
        }
      
        public int lineNumber
        {
            get
            {
                return lineNum;
            }
            set
            {
                if (lineNumber != value)
                {
                    lineNum = value;
                    NotifyPropertyChanged("lineNumber");
                }
            }
        }

        public string path
        {
            get
            {
                return filePath;
            }
            set
            {
                if (path != value)
                    filePath = value;
            }
        }

        
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public Client(String server, int port)
        {
            this.port = port;
            this.server = server;
        }

        public void connect()
        {
            try
            {

                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                TcpClient client = new TcpClient(server, port);
                // Get a client stream for reading and writing.
                var stream = client.GetStream();

                // reading csv file into string array of lines
                String[] csvLine = File.ReadAllLines(path);
                // getting number of rows
                simLen = csvLine.Length;

               
                //setting up playing speed to 100 mill-sec
                playSpeed = 100;

                // sending one line at a time to server
                while (simLen > lineNumber)
                {
                    //get a line from array
                    csvLine[lineNumber] += "\n";
                    //Encode to bytes
                    Byte[] lineBytes = System.Text.Encoding.ASCII.GetBytes(csvLine[lineNumber]);
                    // Send the message to the connected TcpServer
                    stream.Write(lineBytes, 0, lineBytes.Length);
                    //inc index to next line
                    lineNumber++;
                    //sleep for playspeed mil-sec for sending ten times in a second
                    Thread.Sleep(playSpeed);
                }

                // Close - stream and socket
                stream.Close();
                client.Close();
            }
            //catch problem with port or server ip
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            //catch problem with socket
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                MessageBox.Show("Please make sure to open FlightGear simulator with following additional setting and than try again:\n" +
                    "--generic=socket,in,10,127.0.0.1,5400,tcp,playback_small\n--fdm = null" , "Couldn't Reach Server", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

    }
}

