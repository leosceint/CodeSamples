//Sample: displaying a page from PDF document in WPF window using UWP API (Windows 10 only)
//Copyright (c) 2019, MSDN.WhiteKnight

//Required references:
// C:\Program Files (x86)\Windows Kits\10\UnionMetadata\[...]\Windows.winmd
// C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5\System.Runtime.WindowsRuntime.dll

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();            
        }        

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(@"C:\files\document.pdf");
            PdfDocument pdf = await PdfDocument.LoadFromFileAsync(file);
            PdfPage page = pdf.GetPage(0);
            BitmapImage image = new BitmapImage();

            using (var stream = new InMemoryRandomAccessStream())
            {
                await page.RenderToStreamAsync(stream);

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream.AsStream();
                image.EndInit();
            }
            img.Source = image;      
        }
    }    
}
