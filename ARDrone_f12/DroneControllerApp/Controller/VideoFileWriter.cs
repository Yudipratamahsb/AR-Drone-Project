#region Copyright Notice

//Copyright © 2007-2011, PARROT SA, all rights reserved. 

//DISCLAIMER 
//The APIs is provided by PARROT and contributors "AS IS" and any express or implied warranties, including, but not limited to, the implied warranties of merchantability 
//and fitness for a particular purpose are disclaimed. In no event shall PARROT and contributors be liable for any direct, indirect, incidental, special, exemplary, or 
//consequential damages (including, but not limited to, procurement of substitute goods or services; loss of use, data, or profits; or business interruption) however 
//caused and on any theory of liability, whether in contract, strict liability, or tort (including negligence or otherwise) arising in any way out of the use of this 
//software, even if advised of the possibility of such damage. 

//Author            : Wilke Jansoone
//Email             : wilke.jansoone@digitude.net
//Publishing date   : 28/11/2010 

//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions
//are met:
//    - Redistributions of source code must retain the above copyright notice, this list of conditions, the disclaimer and the original author of the source code.
//    - Neither the name of the PixVillage Team, nor the names of its contributors may be used to endorse or promote products derived from this software without 
//      specific prior written permission.

#endregion

#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using WindowsMediaLib;
using WindowsMediaLib.Defs;

using Wilke.Interactive.Drone.Control.Enumerations;

#endregion

namespace Wilke.Interactive.Drone.Control
{
    internal class VideoFileWriter : IDisposable
    {
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, [MarshalAs(UnmanagedType.U4)] int Length);

        #region Private Fields

        private IWMInputMediaProps mediaProperties = null;
        private IWMWriter mediaWriter = null;
        private bool disposed = false;

        #endregion

        #region Private Properties

        private IWMWriter MediaWriter
        {
            get { return mediaWriter; }
            set { mediaWriter = value; }
        }
        private IWMInputMediaProps MediaProperties
        {
            get { return mediaProperties; }
            set { mediaProperties = value; }
        }
        private bool Initialized { get; set; }
        private bool MediaWriterConfigured { get; set; }        
        private int CurrentSampleIndex { get; set; }
        private long CurrentSampleTimeStamp { get; set; }
        private int VideoChannelIndex { get; set; }
        private Rectangle BitmapBounds { get; set; }

        #endregion

        #region internal Properties

        internal bool MediaWriterIsWriting { get; private set; }

        #endregion

        #region Construction

        internal VideoFileWriter(string destinationFileName, VideoQuality videoQuality)
        {
            Initialize(destinationFileName, videoQuality);
        }

        #endregion

        #region Private Methods

        private bool Initialize(string destinationFileName, VideoQuality videoQuality)
        {
            IWMProfileManager profileManagerTemp = null;
            IWMProfile profile = null;

            int inputCount = 0;
            Guid inputTypeId;
            bool success = false;

            try
            {
                #region Initialize Properties

                CurrentSampleIndex = 0;
                CurrentSampleTimeStamp = 0;
                VideoChannelIndex = -1;
                MediaWriterConfigured = false;
                MediaWriterIsWriting = false;

                #endregion

                #region Create ProfileManager

                WMUtils.WMCreateProfileManager(out profileManagerTemp);
                IWMProfileManager2 profileManager = (IWMProfileManager2)profileManagerTemp;

                #endregion

                #region Configure ProfileManager

                profileManager.SetSystemProfileVersion(WMVersion.V8_0);

                #endregion

                #region Load Profile
                
                switch (videoQuality)
                { 
                    case VideoQuality.Kbps128:
                        profileManager.LoadProfileByData(Wilke.Interactive.Drone.Control.Properties.Resources.VideoProfile128kbps, out profile);
                        break;
                    case VideoQuality.Kbps256:
                        profileManager.LoadProfileByData(Wilke.Interactive.Drone.Control.Properties.Resources.VideoProfile256kbps, out profile);
                        break;
                }                

                #endregion

                #region Create Writer

                WMUtils.WMCreateWriter(IntPtr.Zero, out mediaWriter);

                #endregion

                #region Configure Writer

                MediaWriter.SetProfile(profile);

                MediaWriter.GetInputCount(out inputCount);

                // Find the first video input on the writer
                for (int index = 0; index < inputCount; index++)
                {
                    // Get the properties of channel #index
                    MediaWriter.GetInputProps(index, out mediaProperties);

                    // Read the type of the channel
                    MediaProperties.GetType(out inputTypeId);

                    // If it is video, we are done
                    if (inputTypeId == MediaType.Video)
                    {
                        VideoChannelIndex = index;
                        break;
                    }
                }

                // Didn't find a video channel
                if (VideoChannelIndex == -1)
                {
                    throw new Exception("Profile does not accept video input");
                }

                MediaWriter.SetOutputFilename(destinationFileName);

                #endregion

                success = true;
            }
            catch
            {
                throw;
            }

            return success;
        }

        private void ConfigureMediaWriter(int width, int height, Guid mediaSubType, short bitCount)
        {
            AMMediaType mediaType = new AMMediaType();
            VideoInfoHeader videoInfo = new VideoInfoHeader();

            // Create the VideoInfoHeader using info from the bitmap
            videoInfo.BmiHeader.Size = Marshal.SizeOf(typeof(BitmapInfoHeader));
            videoInfo.BmiHeader.Width = width;
            videoInfo.BmiHeader.Height = height;
            videoInfo.BmiHeader.Planes = 1;

            // compression thru clrimportant don't seem to be used. Init them anyway
            videoInfo.BmiHeader.Compression = 0;
            videoInfo.BmiHeader.ImageSize = 0;
            videoInfo.BmiHeader.XPelsPerMeter = 0;
            videoInfo.BmiHeader.YPelsPerMeter = 0;
            videoInfo.BmiHeader.ClrUsed = 0;
            videoInfo.BmiHeader.ClrImportant = 0;
          
            mediaType.subType = mediaSubType;
            videoInfo.BmiHeader.BitCount = bitCount;            

            videoInfo.SrcRect = new Rectangle(0, 0, width, height);
            videoInfo.TargetRect = videoInfo.SrcRect;
            videoInfo.BmiHeader.ImageSize = width * height * (videoInfo.BmiHeader.BitCount / 8);
            videoInfo.BitRate = videoInfo.BmiHeader.ImageSize * Constants.VideoFrameRate;
            videoInfo.BitErrorRate = 0;
            videoInfo.AvgTimePerFrame = 10000 * 1000 / Constants.VideoFrameRate;

            mediaType.majorType = MediaType.Video;
            mediaType.fixedSizeSamples = true;
            mediaType.temporalCompression = false;
            mediaType.sampleSize = videoInfo.BmiHeader.ImageSize;
            mediaType.formatType = FormatType.VideoInfo;
            mediaType.unkPtr = IntPtr.Zero;
            mediaType.formatSize = Marshal.SizeOf(typeof(VideoInfoHeader));

            // Lock the videoInfo structure, and put the pointer
            // into the mediatype structure
            GCHandle handle = GCHandle.Alloc(videoInfo, GCHandleType.Pinned);

            try
            {
                // Set the inputprops using the structures
                mediaType.formatPtr = handle.AddrOfPinnedObject();
                MediaProperties.SetMediaType(mediaType);
            }
            finally
            {
                handle.Free();
                mediaType.formatPtr = IntPtr.Zero;
            }

            // Now take the inputprops, and set them on the file writer
            MediaWriter.SetInputProps(VideoChannelIndex, MediaProperties);
        }

        private void GetMediaType(System.Drawing.Imaging.PixelFormat pixelFormat, out Guid mediaSubType, out short bitCount)
        {
            switch (pixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    mediaSubType = MediaSubType.RGB32;
                    bitCount = 32;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    mediaSubType = MediaSubType.RGB32;
                    bitCount = 32;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    mediaSubType = MediaSubType.RGB24;
                    bitCount = 24;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                    mediaSubType = MediaSubType.RGB565;
                    bitCount = 16;
                    break;
                default:
                    throw new Exception("Unrecognized Pixelformat in bitmap");
            }
        }

        private void GetMediaType(System.Windows.Media.PixelFormat pixelFormat, out Guid mediaSubType, out short bitCount)
        {     
            //Very basic implementation, should look how to convert 
            //"System.Drawing.Imaging.PixelFormat to "System.Windows.Media.PixelFormat" 

            switch (pixelFormat.BitsPerPixel)
            { 
                case 32:
                    bitCount = 32;
                    mediaSubType = MediaSubType.RGB32;
                    break;
                case 24:
                    bitCount = 24;
                    mediaSubType = MediaSubType.RGB24;
                    break;
                case 16:
                    bitCount = 16;
                    mediaSubType = MediaSubType.RGB565;
                    break;
                default:
                    throw new Exception("Unrecognized Pixelformat in bitmap");
            }
        }

        private void CopyFrame(BitmapData bitmapData, IntPtr ip, int size)
        {
            // If the bitmap is rightside up
            if (bitmapData.Stride < 0)
            {
                CopyMemory(ip, bitmapData.Scan0, size);
            }
            else
            {
                // Copy it line by line from bottom to top
                IntPtr ip2 = (IntPtr)(ip.ToInt32() + size - bitmapData.Stride);
                for (int x = 0; x < bitmapData.Height; x++)
                {
                    CopyMemory(ip2, (IntPtr)(bitmapData.Scan0.ToInt32() + (bitmapData.Stride * x)), bitmapData.Stride);
                    ip2 = (IntPtr)(ip2.ToInt32() - bitmapData.Stride);
                }
            }
        }

        private void CopyFrame(IntPtr source, IntPtr destination, int size)
        {           
            CopyMemory(destination, source, size);           
        }

        #endregion

        #region internal Methods

        internal void AddFrame(Bitmap bitmap)
        {
            INSSBuffer sampleBuffer = null;
            BitmapData bitmapData = null;

            if (BitmapBounds.Width != bitmap.Width)
            {
                Guid mediaSubType;
                short bitCount;

                BitmapBounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                GetMediaType(bitmap.PixelFormat, out mediaSubType, out bitCount);
                ConfigureMediaWriter(BitmapBounds.Width, BitmapBounds.Height, mediaSubType, bitCount);
                MediaWriterConfigured = true;

                if (!MediaWriterIsWriting)
                {
                    MediaWriter.BeginWriting();
                    MediaWriterIsWriting = true;
                }
            }

            // Lock the bitmap, which gets us a pointer to the raw bitmap data
            bitmapData = bitmap.LockBits(BitmapBounds, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            try
            {
                // Compute size of bitmap in bytes.  Strides may be negative.
                int bitmapSize = Math.Abs(bitmapData.Stride * bitmapData.Height);

                IntPtr framePointer;

                // Get a sample interface
                MediaWriter.AllocateSample(bitmapSize, out sampleBuffer);

                // Get the buffer from the sample interface.  This is
                // where we copy the bitmap data to
                sampleBuffer.GetBuffer(out framePointer);

                // Copy the bitmap data into the sample buffer
                CopyFrame(bitmapData, framePointer, bitmapSize);

                // Write the sample to the output - Sometimes, for reasons I can't explain,
                // writing a sample fails.  However, writing the same sample again
                // works.  Go figure.
                int iRetry = 0;
                do
                {
                    try
                    {
                        MediaWriter.WriteSample(VideoChannelIndex, 10000 * CurrentSampleTimeStamp, SampleFlag.CleanPoint, sampleBuffer);
                        break;
                    }
                    catch (COMException e)
                    {
                        if ((iRetry++ < 3) && (e.ErrorCode != NSResults.E_INVALID_DATA))
                        {
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                } while (true);

                // update the time based on the framerate
                CurrentSampleTimeStamp = (++CurrentSampleIndex * 1000) / Constants.VideoFrameRate;
            }
            finally
            {
                // Release the locals
                if (sampleBuffer != null)
                {
                    Marshal.ReleaseComObject(sampleBuffer);
                    sampleBuffer = null;
                }

                bitmap.UnlockBits(bitmapData);
            }
        }

        internal void AddFrame(WriteableBitmap bitmap)
        {
            INSSBuffer sampleBuffer = null;

            if (BitmapBounds.Width != bitmap.Width)
            {
                Guid mediaSubType;
                short bitCount;

                BitmapBounds = new Rectangle(0, 0, (int)bitmap.Width, (int)bitmap.Height);
                GetMediaType(bitmap.Format, out mediaSubType, out bitCount);
                ConfigureMediaWriter(BitmapBounds.Width, BitmapBounds.Height, mediaSubType, bitCount);
                MediaWriterConfigured = true;

                if (!MediaWriterIsWriting)
                {
                    MediaWriter.BeginWriting();
                    MediaWriterIsWriting = true;
                }
            }

           try
            {
               
                // Compute size of bitmap in bytes.  Strides may be negative.
                int bitmapSize = Math.Abs(bitmap.BackBufferStride * (int)bitmap.Height);

                IntPtr framePointer;

                // Get a sample interface
                MediaWriter.AllocateSample(bitmapSize, out sampleBuffer);

                // Get the buffer from the sample interface.  This is
                // where we copy the bitmap data to
                sampleBuffer.GetBuffer(out framePointer);

                // Copy the bitmap data into the sample buffer
                CopyFrame(bitmap.BackBuffer, framePointer, bitmapSize);

                // Write the sample to the output - Sometimes, for reasons I can't explain,
                // writing a sample fails.  However, writing the same sample again
                // works.  Go figure.
                int iRetry = 0;
                do
                {
                    try
                    {
                        MediaWriter.WriteSample(VideoChannelIndex, 10000 * CurrentSampleTimeStamp, SampleFlag.CleanPoint, sampleBuffer);
                        break;
                    }
                    catch (COMException e)
                    {
                        if ((iRetry++ < 3) && (e.ErrorCode != NSResults.E_INVALID_DATA))
                        {
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                } while (true);

                // update the time based on the framerate
                CurrentSampleTimeStamp = (++CurrentSampleIndex * 1000) / Constants.VideoFrameRate;
            }
            finally
            {
                // Release the locals
                if (sampleBuffer != null)
                {
                    Marshal.ReleaseComObject(sampleBuffer);
                    sampleBuffer = null;
                }
            }
        }

        internal void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //No managed resources to expose.
                }

                #region Dispose MediaWriter

                if (MediaWriterIsWriting)   //We are currently writing
                {
                    if (MediaWriter != null)
                    {
                        // Close the file
                        try
                        {
                            MediaWriter.EndWriting();
                        }
                        catch { }
                    }

                    MediaWriterIsWriting = false;
                }

                if (MediaProperties != null)
                {
                    Marshal.ReleaseComObject(MediaProperties);
                    MediaProperties = null;
                }
                if (MediaWriter != null)
                {
                    Marshal.ReleaseComObject(MediaWriter);
                    MediaWriter = null;
                }

                #endregion

                disposed = true;
            }
        }

        #endregion

        ~VideoFileWriter()
        {
            Dispose(false);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
