using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using TCPClient.MVVM.Commands;

namespace TCPClient.MVVM.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        #region Private Property

        private string _path;
        #endregion

        #region Full Property

        private Image img;
        public Image Img
        {
            get { return img; }
            set { img = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand SendCommand { get; set; }
        public ICommand OpenCommand { get; set; }

        #endregion


        public MainViewModel()
        {
            Img = new Image();

            SendCommand = new RelayCommand((e) =>
            {
                Thread thread = new Thread(() =>
                {
                    Send();
                });
                thread.Start();

            }, (pred) =>
            {
                if (Img.Source != null)
                    return true;

                return false;
            });

            OpenCommand = new RelayCommand((e) =>
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "PNG (*.png)|*.png|JPEG (*.jpeg)|*.jpeg|JPG (*.jpg)|*.jpg";
                open.FilterIndex = 1;
                open.Multiselect = false;

                if (Convert.ToBoolean(open.ShowDialog()) == true)
                {
                    Img.Source = new BitmapImage(new Uri(open.FileName));
                    _path = open.FileName;
                }
            });
        }

        private async void Send()
        {
            var ipAddress = IPAddress.Parse("192.168.1.100");
            var port = 11804;

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                var endPoint = new IPEndPoint(ipAddress, port);

                try
                {
                    socket.Connect(endPoint);

                    if (socket.Connected)
                    {
                        var bytes = Encoding.UTF8.GetBytes(_path);

                        socket.Send(bytes);

                        //List<byte[]> infos = GetSplitByteArray(bytes);


                        //foreach (var info in infos)
                        //{
                        //    socket.Send(info);
                        //}

                        MessageBox.Show("Image send successfully.", "Information", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Please 'End Point' check.", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Server not working. \n {e.Message}", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }
        }
        
        private List<byte[]> GetSplitByteArray(byte[] infoByte)
        {
            List<byte[]> tempInfos = new List<byte[]>();

            var count = infoByte.Length / 1024;

            for (int i = 0; i < count + 1; i++)
            {
                int lenght = 0;
                if (infoByte.Length < 1024)
                    lenght = infoByte.Length;
                else lenght = 1024;

                var bytes = new byte[lenght];
                
                Array.Copy(infoByte, 0, bytes, 0, lenght);

                infoByte = Remove(infoByte);

                tempInfos.Add(bytes);
            }

            return tempInfos;
        }

        private byte[] Remove(byte[] infoByte)
        {
            if (infoByte.Length > 1024)
            {
                var tempData = new byte[infoByte.Length - 1024];

                Array.Copy(infoByte, 1024, tempData, 0, infoByte.Length - 1024);

                return tempData;
            }

            return null;
        }

        public byte[] ImageToByteArray(string path)
        {
            return new byte[33];


            //using (MemoryStream ms = new MemoryStream())
            //{
            //    ImageSource source = null;
            //    App.Current.Dispatcher.Invoke(new Action(() =>
            //    {
            //        source = Img.Source;
            //    }));

            //    var bmp = source as BitmapImage;
            //    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            //    encoder.Frames.Add(BitmapFrame.Create(bmp));
            //    encoder.Save(ms);
            //    return ms.ToArray();
            //}
        }
    }
}