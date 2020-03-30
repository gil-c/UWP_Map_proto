using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWP_Map_proto
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();

            CustomMapTileDataSource customDataSource = new CustomMapTileDataSource();
            
            // Attach a handler for the BitmapRequested event.
            customDataSource.BitmapRequested += customDataSource_BitmapRequestedAsync;
            MapTileSource customTileSource = new MapTileSource(customDataSource);
            myMap.TileSources.Add(customTileSource);

            // Customize component
            customTileSource.Layer = MapTileLayer.BackgroundReplacement;
            customTileSource.IsFadingEnabled = false;
            myMap.Style = MapStyle.None;
            myMap.MapProjection = MapProjection.WebMercator;

        }


        // Handle the BitmapRequested event.
        private async void customDataSource_BitmapRequestedAsync(
            CustomMapTileDataSource sender,
            MapTileBitmapRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();
            args.Request.PixelData = await CreateBitmapAsStreamAsync(args.ZoomLevel, args.X, args.Y);
            deferral.Complete();
        }


        // Create the custom tiles.
        private async Task<RandomAccessStreamReference> CreateBitmapAsStreamAsync(int ZoomLevel, int X, int Y)
        {
            Debug.WriteLine($"ZoomLevel: {ZoomLevel}, X: {X}, Y: {Y}");

            int pixelHeight = 256;
            int pixelWidth = 256;
            int bpp = 4;

            byte[] bytes = new byte[pixelHeight * pixelWidth * bpp];

            if ((X + Y) % 2 == 0)
            {
                for (int y = 0; y < pixelHeight; y++)
                {
                    for (int x = 0; x < pixelWidth; x++)
                    {
                        int pixelIndex = y * pixelWidth + x;
                        int byteIndex = pixelIndex * bpp;

                        // Set the current pixel bytes.
                        bytes[byteIndex] = 0xff;        // Red
                        bytes[byteIndex + 1] = 0x00;    // Green
                        bytes[byteIndex + 2] = 0x00;    // Blue
                        bytes[byteIndex + 3] = 0x80;    // Alpha (0xff = fully opaque)
                    }
                }
            }
            else
            {
                for (int y = 0; y < pixelHeight; y++)
                {
                    for (int x = 0; x < pixelWidth; x++)
                    {
                        int pixelIndex = y * pixelWidth + x;
                        int byteIndex = pixelIndex * bpp;

                        // Set the current pixel bytes.
                        bytes[byteIndex] = 0x00;        // Red
                        bytes[byteIndex + 1] = 0xff;    // Green
                        bytes[byteIndex + 2] = 0x00;    // Blue
                        bytes[byteIndex + 3] = 0x80;    // Alpha (0xff = fully opaque)
                    }
                }
            }

            

            // Create RandomAccessStream from byte array.
            InMemoryRandomAccessStream randomAccessStream =
                new InMemoryRandomAccessStream();
            IOutputStream outputStream = randomAccessStream.GetOutputStreamAt(0);
            DataWriter writer = new DataWriter(outputStream);
            writer.WriteBytes(bytes);
            await writer.StoreAsync();
            await writer.FlushAsync();
            return RandomAccessStreamReference.CreateFromStream(randomAccessStream);
        }
    }
}
