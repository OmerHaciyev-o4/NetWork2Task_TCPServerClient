using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetWork2Task_TCPServerClient.MVVM.ViewModel
{
    public class ViewModel : BaseViewModel
    {
        public ObservableCollection<ListBoxItem> Items { get; set; }

        public ViewModel()
        {
            Items = new ObservableCollection<ListBoxItem>();
            
            Thread thread = new Thread(() =>
            {
                Receive();
            });
            thread.ApartmentState = ApartmentState.STA;
            thread.Start();
        }

        private void Receive()
        {
            var ipAddress = IPAddress.Parse("192.168.1.100");
            var port = 11804;

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                var endPoint = new IPEndPoint(ipAddress, port);
                socket.Bind(endPoint);

                socket.Listen(10);


                while (true)
                {
                    var client = socket.Accept();

                    Task.Run(() =>
                    {
                        var bytes = new byte[1024];
                        int length = 0;

                        length = client.Receive(bytes);
                        var tempData = new byte[length];
                        Array.Copy(bytes, 0, tempData, 0, length);
                        bytes = new byte[length];
                        Array.Copy(tempData, 0, bytes, 0, length);


                        var img = ByteArrayToImage(bytes);

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            if (img.Source != null)
                            {
                                img.Width = 200;
                                img.Height = 200;

                                StackPanel panel = new StackPanel() {Orientation = Orientation.Horizontal};
                                Label label = new Label()
                                {
                                    Content = client.RemoteEndPoint,
                                    FontSize = 20
                                };

                                panel.Children.Add(img);
                                panel.Children.Add(label);

                                ListBoxItem item = new ListBoxItem();
                                item.Content = panel;

                                Items.Add(item);
                            }
                        });
                    });
                }
            } 
        }

        public byte[] AddByteArrayToArray(byte[] data, byte[] info)
        {
            int length = 0;

            if (data != null)
                length = data.Length;

            byte[] dataInfo = new byte[length + info.Length];
            if (length > 0)
                Array.Copy(data, 0, dataInfo, 0, length);

            Array.Copy(info, 0, dataInfo, length, info.Length);

            return dataInfo;
        }

        public Image ByteArrayToImage(byte[] buffer)
        {
            Image returnImage = null;
            string path = Encoding.UTF8.GetString(buffer);

            App.Current.Dispatcher.Invoke(() =>
            {
                returnImage = new Image();
                returnImage.Source = new BitmapImage(new Uri(path));
            });

            return returnImage;
        }
    }
}