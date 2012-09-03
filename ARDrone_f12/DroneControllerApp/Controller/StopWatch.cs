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

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DroneController
{
    internal class StopWatch
    {
        //[DllImport("kernel32.dll")]
        internal static bool QueryPerformanceFrequency(out long value)
        {
            value = 0;
            return false;
        }

        //[DllImport("kernel32.dll")]
        internal static bool QueryPerformanceCounter(out long value)
        {
            value = 0;
            return false;
        }

        // Fields
        private long elapsed;
        internal static readonly long Frequency;
        internal static readonly bool IsHighResolution;
        private bool isRunning;
        private long startTimeStamp;
        private static readonly double tickFrequency;
        private const long TicksPerMillisecond = 0x2710L;
        private const long TicksPerSecond = 0x989680L;

        // Methods
        static StopWatch()
        {
            if (!QueryPerformanceFrequency(out Frequency))
            {
                IsHighResolution = false;
                Frequency = 0x989680L;
                tickFrequency = 1.0;
            }
            else
            {
                IsHighResolution = true;
                tickFrequency = 10000000.0;
                tickFrequency /= (double)Frequency;
            }
        }

        internal StopWatch()
        {
            this.Reset();
        }

        private long GetElapsedDateTimeTicks()
        {
            long rawElapsedTicks = this.GetRawElapsedTicks();
            if (IsHighResolution)
            {
                double num2 = rawElapsedTicks;
                num2 *= tickFrequency;
                return (long)num2;
            }
            return rawElapsedTicks;
        }

        private long GetRawElapsedTicks()
        {
            long elapsed = this.elapsed;
            if (this.isRunning)
            {
                long num3 = GetTimestamp() - this.startTimeStamp;
                elapsed += num3;
            }
            return elapsed;
        }

        internal static long GetTimestamp()
        {
            if (IsHighResolution)
            {
                long num = 0L;
                QueryPerformanceCounter(out num);
                return num;
            }
            return DateTime.UtcNow.Ticks;
        }

        internal void Reset()
        {
            this.elapsed = 0L;
            this.isRunning = false;
            this.startTimeStamp = 0L;
        }

        internal void Restart()
        {
            this.elapsed = 0L;
            this.startTimeStamp = GetTimestamp();
            this.isRunning = true;
        }

        internal void Start()
        {
            if (!this.isRunning)
            {
                this.startTimeStamp = GetTimestamp();
                this.isRunning = true;
            }
        }

        internal static StopWatch StartNew()
        {
            StopWatch stopWatch = new StopWatch();
            stopWatch.Start();
            return stopWatch;
        }

        internal void Stop()
        {
            if (this.isRunning)
            {
                long num2 = GetTimestamp() - this.startTimeStamp;
                this.elapsed += num2;
                this.isRunning = false;
                if (this.elapsed < 0L)
                {
                    this.elapsed = 0L;
                }
            }
        }

        // Properties
        internal TimeSpan Elapsed
        {
            get
            {
                return new TimeSpan(this.GetElapsedDateTimeTicks());
            }
        }

        internal long ElapsedMilliseconds
        {
            get
            {
                return (this.GetElapsedDateTimeTicks() / 0x2710L);
            }
        }

        internal long ElapsedTicks
        {
            get
            {
                return this.GetRawElapsedTicks();
            }
        }

        internal bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
        }
    }
}
