using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.System;

namespace MinRepo_HandleLeak
{
	public sealed partial class MainWindow : Window
	{
		private WriteableBitmap image1Bitmap = new(200, 200);
		private WriteableBitmap image2Bitmap = new(200, 200);
		private readonly DispatcherQueue uiDispatcherQueue;
		private byte[] sourcePixels1;
		private byte[] sourcePixels2;

		public MainWindow()
		{
			InitializeComponent();
			uiDispatcherQueue = DispatcherQueue.GetForCurrentThread();
			Init();
		}

		private async void Init()
		{
			StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///winui-logo.png"));
			using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
			{
				BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
				BitmapTransform transform = new BitmapTransform()
				{
					ScaledWidth = Convert.ToUInt32(200),
					ScaledHeight = Convert.ToUInt32(200)
				};
				PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
					BitmapPixelFormat.Bgra8,
					BitmapAlphaMode.Straight,
					transform,
					ExifOrientationMode.IgnoreExifOrientation,
					ColorManagementMode.DoNotColorManage
				);

				sourcePixels1 = pixelData.DetachPixelData();

				pixelData = await decoder.GetPixelDataAsync(
					BitmapPixelFormat.Bgra8,
					BitmapAlphaMode.Straight,
					transform,
					ExifOrientationMode.IgnoreExifOrientation,
					ColorManagementMode.DoNotColorManage
				);

				sourcePixels2 = pixelData.DetachPixelData();

				var timer1 = uiDispatcherQueue.CreateTimer();
				timer1.Interval = TimeSpan.FromMilliseconds(1);
				timer1.Tick += async (_, _) => await UpdateImage1();

				var timer2 = uiDispatcherQueue.CreateTimer();
				timer2.Interval = TimeSpan.FromMilliseconds(1);
				timer2.Tick += async (_, _) => await UpdateImage2();

				timer1.Start();
				timer2.Start();
			}
		}

		private async Task UpdateImage1()
		{
			using (Stream stream = image1Bitmap.PixelBuffer.AsStream())
			{
				await stream.WriteAsync(sourcePixels1, 0, sourcePixels1.Length);
			}
			image1Bitmap.Invalidate();
		}

		private async Task UpdateImage2()
		{
			using (Stream stream = image2Bitmap.PixelBuffer.AsStream())
			{
				await stream.WriteAsync(sourcePixels2, 0, sourcePixels2.Length);
			}
			image2Bitmap.Invalidate();
		}
	}
}