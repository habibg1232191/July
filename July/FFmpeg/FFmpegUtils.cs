using System;
using System.Collections.Generic;
using SysBitmap = System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using FFmpeg.AutoGen;
using SkiaSharp;

namespace July.FFmpeg;

public static unsafe class FFmpegUtils
{
    public static string UrlVideo = @"http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4";
    
    static FFmpegUtils()
    {
        Initialize();
        SetupLogging();
    }
    
    public static SKBitmap Get()
    {
        AVFormatContext* pFormatContext = ffmpeg.avformat_alloc_context();
        ffmpeg.avformat_open_input(&pFormatContext, UrlVideo, null, null);
        
        ffmpeg.avformat_close_input(&pFormatContext);
        ffmpeg.avformat_free_context(pFormatContext);
        return new SKBitmap();
    }
    
    public static unsafe Bitmap DecodeAllFramesToImages(AVHWDeviceType HWDevice)
    {
        SKBitmap resultBitmap;
        // decode all frames from url, please not it might local resorce, e.g. string url = "../../sample_mpeg4.mp4";
        var url = "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4"; // be advised this file holds 1440 frames
        using var vsd = new VideoStreamDecoder(url, HWDevice);

        Console.WriteLine($"codec name: {vsd.CodecName}");

        var info = vsd.GetContextInfo();
        info.ToList().ForEach(x => Console.WriteLine($"{x.Key} = {x.Value}"));

        var sourceSize = vsd.FrameSize;
        var sourcePixelFormat = HWDevice == AVHWDeviceType.AV_HWDEVICE_TYPE_NONE
            ? vsd.PixelFormat
            : GetHWPixelFormat(HWDevice);
        var destinationSize = sourceSize;
        var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_RGB24;
        using var vfc =
            new VideoFrameConverter(sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat);

        var frameNumber = 0;

        while (vsd.TryDecodeNextFrame(out var frame))
        {
                var convertedFrame = vfc.Convert(frame);
                Console.WriteLine($"Converted Frame: ${convertedFrame}");
                return convertedFrame.ToAvaloniaBitmap();
                // return SKBitmap.Decode(SKData.Create((IntPtr) convertedFrame.data[0], convertedFrame.pkt_size));
                frameNumber++;
        }

        return null;
    }

    public static Bitmap ToAvaloniaBitmap(this AVFrame frame)
    {
#pragma warning disable CA1416
        using var bitmap = new SysBitmap.Bitmap(frame.width, 
            frame.height,
            frame.linesize[0], 
            PixelFormat.Format24bppRgb, 
            (IntPtr) frame.data[0]);
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        stream.Seek(0, SeekOrigin.Begin);
        
        return new Bitmap(stream);
    }
    
    public static AVPixelFormat GetHWPixelFormat(AVHWDeviceType hWDevice)
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
    
    public static void Initialize()
    {
        var current = Environment.CurrentDirectory;
        var probe = Path.Combine("FFmpeg", "bin", Environment.Is64BitProcess ? "x64" : "x86");
        while (current != null)
        {
            var ffmpegBinaryPath = Path.Combine(current, probe);
            if (Directory.Exists(ffmpegBinaryPath))
            {
                ffmpeg.RootPath = ffmpegBinaryPath;
                return;
            }

            current = Directory.GetParent(current)?.FullName;
        }
    }
    
    public static void SetupLogging()
    {
        ffmpeg.av_log_set_level(ffmpeg.AV_LOG_VERBOSE);

        // do not convert to local function
        av_log_set_callback_callback logCallback = (p0, level, format, vl) =>
        {
            if (level > ffmpeg.av_log_get_level()) return;

            var lineSize = 1024;
            var lineBuffer = stackalloc byte[lineSize];
            var printPrefix = 1;
            ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
            var line = Marshal.PtrToStringAnsi((IntPtr) lineBuffer);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(line);
            Console.ResetColor();
        };

        ffmpeg.av_log_set_callback(logCallback);
    }
    
    public static void ConfigureHWDecoder(out AVHWDeviceType HWtype)
    {
        HWtype = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
        var availableHwDecoders = new Dictionary<int, AVHWDeviceType>();

        Console.WriteLine("Select hardware decoder:");
        var type = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
        var number = 0;

        while ((type = ffmpeg.av_hwdevice_iterate_types(type)) != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
        {
            Console.WriteLine($"{++number}. {type}");
            availableHwDecoders.Add(number, type);
        }

        if (availableHwDecoders.Count == 0)
        {
            Console.WriteLine("Your system have no hardware decoders.");
            HWtype = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
            return;
        }

        var decoderNumber = availableHwDecoders
            .SingleOrDefault(t => t.Value == AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2).Key;
        if (decoderNumber == 0)
            decoderNumber = availableHwDecoders.First().Key;
        Console.WriteLine($"Selected [{decoderNumber}]");
        int.TryParse(Console.ReadLine(), out var inputDecoderNumber);
        availableHwDecoders.TryGetValue(inputDecoderNumber == 0 ? decoderNumber : inputDecoderNumber,
            out HWtype);
    }
}