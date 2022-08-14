using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using FFmpeg.AutoGen;
using July.FFmpeg;

namespace July.Views.Controls;

public partial class MediaView : TemplatedControl
{
    private VideoFrameConverter _frameConverter;
    private VideoStreamDecoder _streamDecoder;
    private Image _renderImage;
    
    public static readonly StyledProperty<string> SourceProperty =
        AvaloniaProperty.Register<MediaView, string>(nameof(Source));

    public string Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    
    public MediaView()
    {
        
    }

    private async void VideoStream()
    {
        while (_streamDecoder.TryDecodeNextFrame(out var frame))
        {
            var convertedFrame = _frameConverter.Convert(frame);
            unsafe
            {
                var bt = new Bitmap(
                    PixelFormat.Bgra8888,
                    AlphaFormat.Opaque,
                    (IntPtr) frame.data[0],
                    new PixelSize(frame.width, frame.height),
                    Vector.One, frame.linesize[0]);
            }
            // var avaloniaBitmap = convertedFrame.ToAvaloniaBitmap();
            // if (_renderImage != null) _renderImage.Source = avaloniaBitmap;
            await Task.Delay(TimeSpan.FromMilliseconds(1));
            // avaloniaBitmap.Dispose();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _renderImage = e.NameScope.Find<Image>("RenderImage").ThrowExceptionIfNotFound();
        FFmpegUtils.ConfigureHWDecoder(out var deviceType);
        _streamDecoder = new VideoStreamDecoder(Source, deviceType);
        
        var sourceSize = _streamDecoder.FrameSize;
        var sourcePixelFormat = deviceType == AVHWDeviceType.AV_HWDEVICE_TYPE_NONE
            ? _streamDecoder.PixelFormat
            : GetHWPixelFormat(deviceType);
        var destinationSize = sourceSize;
        var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_BGR8;
        var info = _streamDecoder.GetContextInfo();
        info.ToList().ForEach(x => Console.WriteLine($"{x.Key} = {x.Value}"));
        
        _frameConverter = 
            new VideoFrameConverter(sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat);
        Dispatcher.UIThread.Post(VideoStream);
    }
    
    private static AVPixelFormat GetHWPixelFormat(AVHWDeviceType hWDevice)
    {
        return hWDevice switch
        {
            AVHWDeviceType.AV_HWDEVICE_TYPE_NONE => AVPixelFormat.AV_PIX_FMT_NONE,
            AVHWDeviceType.AV_HWDEVICE_TYPE_VDPAU => AVPixelFormat.AV_PIX_FMT_VDPAU,
            AVHWDeviceType.AV_HWDEVICE_TYPE_CUDA => AVPixelFormat.AV_PIX_FMT_CUDA,
            AVHWDeviceType.AV_HWDEVICE_TYPE_VAAPI => AVPixelFormat.AV_PIX_FMT_VAAPI,
            AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2 => AVPixelFormat.AV_PIX_FMT_NV12,
            AVHWDeviceType.AV_HWDEVICE_TYPE_QSV => AVPixelFormat.AV_PIX_FMT_QSV,
            AVHWDeviceType.AV_HWDEVICE_TYPE_VIDEOTOOLBOX => AVPixelFormat.AV_PIX_FMT_VIDEOTOOLBOX,
            AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA => AVPixelFormat.AV_PIX_FMT_NV12,
            AVHWDeviceType.AV_HWDEVICE_TYPE_DRM => AVPixelFormat.AV_PIX_FMT_DRM_PRIME,
            AVHWDeviceType.AV_HWDEVICE_TYPE_OPENCL => AVPixelFormat.AV_PIX_FMT_OPENCL,
            AVHWDeviceType.AV_HWDEVICE_TYPE_MEDIACODEC => AVPixelFormat.AV_PIX_FMT_MEDIACODEC,
            _ => AVPixelFormat.AV_PIX_FMT_NONE
        };
    }
}

public static class Utils
{
    public static T ThrowExceptionIfNotFound<T>(this T? control) where T : IControl
    {
        if (control == null)
            throw new NullReferenceException();
        return control;
    }
}