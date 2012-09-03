

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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace DroneController
{
    /// <summary>
    /// This class contains the structures that will be returned by the navigation communication channel.
    /// </summary>
    internal class NavigationDataInfo
    {
        #region Private Fields

        private byte[] navigationDataBuffer;
        private NavDataCheckSum navDataCheckSum;
        private int navigationDataHeaderSize;
        private UInt32 navigationDataCheckSum;

        #endregion

        #region internal Properties

        #region Header Fields

        internal NavDataHeader navigationDataHeader;

        /// <summary>
        /// Gets the header byte sequence of the raw byte stream.
        /// </summary>
        /// <value>An array of bytes containing the header.</value>
        internal UInt32 Header
        {
            get { return navigationDataHeader.Header; }
        }
        /// <summary>
        /// Gets the drone status. This property provides status information via the on/off position of discrete bits.
        /// </summary>
        /// <value>The drone status.</value>
        internal UInt32 DroneStatus
        {
            get { return navigationDataHeader.DroneStatus; }
        }
        /// <summary>
        /// Gets the sequence of sent navigation data packets.
        /// </summary>
        /// <value>The sequence.</value>
        internal UInt32 Sequence
        {
            get { return navigationDataHeader.Sequence; }
        }

        #endregion

        #region Data Fields


        internal NavDataDrone droneData;

        /// <summary>
        /// Gets or sets the structure that contains information about detected tags.
        /// </summary>
        /// <value>The vision detect.</value>
        internal NavVisionDetect VisionDetect;

        #endregion

        /// <summary>
        /// Gets or sets the navigation data buffer with the raw byte seaquence coming from the ARDrone on the navigation channel.
        /// </summary>
        /// <value>The navigation data buffer.</value>
        internal byte[] NavigationDataBuffer
        {
            private get { return navigationDataBuffer; }
            set
            {
                navigationDataBuffer = value;
                MapNavigationDataHeader();
            }
        }

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationDataInfo"/> class.
        /// </summary>
        internal NavigationDataInfo()
        {
            navigationDataHeader = new NavDataHeader();
            navDataCheckSum = new NavDataCheckSum();
            navigationDataHeaderSize = 16;

            droneData = new NavDataDrone();
            VisionDetect = new NavVisionDetect();

        }

        #endregion

        #region internal Methods

        /// <summary>
        /// This method reads the raw byte sequence and populates the different information structures.
        /// </summary>
        internal void LoadNavigationDataStructures()
        {
            NavigationDataTag navigationDataTag = 0;
            UInt16 size = 0;

            using (MemoryStream memoryStream = new MemoryStream(navigationDataBuffer))
            {
                BinaryReader binaryReader = new BinaryReader(memoryStream);

                memoryStream.Position = navigationDataHeaderSize;

                //Check whether we are not at the end of the stream
                while (memoryStream.Position < memoryStream.Length)
                {
                    navigationDataTag = (NavigationDataTag)binaryReader.ReadUInt16();
                    size = binaryReader.ReadUInt16();

                    switch (navigationDataTag)
                    {
                        case NavigationDataTag.NavDataDemo:
                            MapNavigationData((int)(memoryStream.Position - 4));
                            memoryStream.Position += size - 4;
                            break;
                        case NavigationDataTag.NavDataCks:
                            navigationDataCheckSum = binaryReader.ReadUInt32();
                            break;
                        case NavigationDataTag.NavDataVisionDetect:
                            MapVisionDetect((int)(memoryStream.Position - 4));
                            memoryStream.Position += size - 4;
                            break;
                        default:
                            //Currently not used
                            memoryStream.Position += size - 4; // substract 4 bytes for Tag and Size field
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the byte sequence is valid.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if byte sequence is valid; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsCheckSumValid()
        {
            UInt32 calculatedCheckSum = CalculateCheckSum(navigationDataBuffer);

            return (calculatedCheckSum == this.navigationDataCheckSum);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Populates the navigation data header field.
        /// </summary>
        private void MapNavigationDataHeader()
        {
            using (MemoryStream ms = new MemoryStream(navigationDataBuffer))
            {
                BinaryReader read = new BinaryReader(ms);
                navigationDataHeader.Header = read.ReadUInt32();
                navigationDataHeader.DroneStatus = read.ReadUInt32();
                navigationDataHeader.Sequence = read.ReadUInt32();
                navigationDataHeader.VisionDefined = read.ReadUInt32();
            }

        }

        /// <summary>
        /// Populates the navigation data structure.
        /// </summary>
        /// <param name="position">The current position in the byte sequence.</param>
        private void MapNavigationData(int position)
        {

            using (MemoryStream ms = new MemoryStream(navigationDataBuffer))
            {
                BinaryReader read = new BinaryReader(ms);
                read.BaseStream.Seek(position, SeekOrigin.Begin);

                droneData.Tag = read.ReadUInt16();
                droneData.Size = read.ReadUInt16();
                droneData.ControlStatus = read.ReadUInt32();
                droneData.FlyingPercentage = read.ReadUInt32();
                droneData.Theta = read.ReadSingle();
                droneData.Phi = read.ReadSingle();
                droneData.Psi = read.ReadSingle();
                droneData.Altitude = read.ReadInt32();
                droneData.VelocityX = read.ReadSingle();
                droneData.VelocityY = read.ReadSingle();
                droneData.VelocityZ = read.ReadSingle();
                droneData.LastFrameIndex = read.ReadUInt32();
                droneData.DetectionCameraRotationm11 = read.ReadSingle();
                droneData.DetectionCameraRotationm12 = read.ReadSingle();
                droneData.DetectionCameraRotationm13 = read.ReadSingle();
                droneData.DetectionCameraRotationm21 = read.ReadSingle();
                droneData.DetectionCameraRotationm22 = read.ReadSingle();
                droneData.DetectionCameraRotationm23 = read.ReadSingle();
                droneData.DetectionCameraRotationm31 = read.ReadSingle();
                droneData.DetectionCameraRotationm32 = read.ReadSingle();
                droneData.DetectionCameraRotationm33 = read.ReadSingle();
                droneData.DetectionCameraTranslationX = read.ReadSingle();
                droneData.DetectionCameraTranslationY = read.ReadSingle();
                droneData.DetectionCameraTranslationZ = read.ReadSingle();
                droneData.DetectionTagIndex = read.ReadUInt32();
                droneData.DetectionCameraType = read.ReadUInt32();
                droneData.DroneCameraRotationm11 = read.ReadSingle();
                droneData.DroneCameraRotationm12 = read.ReadSingle();
                droneData.DroneCameraRotationm13 = read.ReadSingle();
                droneData.DroneCameraRotationm21 = read.ReadSingle();
                droneData.DroneCameraRotationm22 = read.ReadSingle();
                droneData.DroneCameraRotationm23 = read.ReadSingle();
                droneData.DroneCameraRotationm31 = read.ReadSingle();
                droneData.DroneCameraRotationm32 = read.ReadSingle();
                droneData.DroneCameraRotationm33 = read.ReadSingle();

                droneData.DroneCameraTranslationX = read.ReadSingle();
                droneData.DroneCameraTranslationY = read.ReadSingle();
                droneData.DroneCameraTranslationZ = read.ReadSingle();
            }


        }

        /// <summary>
        /// Populates the vision detect data structure.
        /// </summary>
        /// <param name="position">The current position in the byte sequence.</param>
        private void MapVisionDetect(int position)
        {
            using (MemoryStream ms = new MemoryStream(navigationDataBuffer))
            {
                BinaryReader read = new BinaryReader(ms);
                read.BaseStream.Seek(position, SeekOrigin.Begin);

                VisionDetect.Tag = read.ReadUInt16();
                VisionDetect.Size = read.ReadUInt16();

                VisionDetect.TagCount = read.ReadUInt32();

                VisionDetect.Tag1Type = read.ReadUInt32();
                VisionDetect.Tag2Type = read.ReadUInt32();
                VisionDetect.Tag3Type = read.ReadUInt32();
                VisionDetect.Tag4Type = read.ReadUInt32();

                VisionDetect.Tag1X = read.ReadUInt32();
                VisionDetect.Tag2X = read.ReadUInt32();
                VisionDetect.Tag3X = read.ReadUInt32();
                VisionDetect.Tag4X = read.ReadUInt32();

                VisionDetect.Tag1Y = read.ReadUInt32();
                VisionDetect.Tag2Y = read.ReadUInt32();
                VisionDetect.Tag3Y = read.ReadUInt32();
                VisionDetect.Tag4Y = read.ReadUInt32();

                VisionDetect.Tag1BoxWidth = read.ReadUInt32();
                VisionDetect.Tag2BoxWidth = read.ReadUInt32();
                VisionDetect.Tag3BoxWidth = read.ReadUInt32();
                VisionDetect.Tag4BoxWidth = read.ReadUInt32();

                VisionDetect.Tag1BoxHeight = read.ReadUInt32();
                VisionDetect.Tag2BoxHeight = read.ReadUInt32();
                VisionDetect.Tag3BoxHeight = read.ReadUInt32();
                VisionDetect.Tag4BoxHeight = read.ReadUInt32();

                VisionDetect.Tag1Distance = read.ReadUInt32();
                VisionDetect.Tag2Distance = read.ReadUInt32();
                VisionDetect.Tag3Distance = read.ReadUInt32();
                VisionDetect.Tag4Distance = read.ReadUInt32();

            }
        }

        /// <summary>
        /// Calculates the check sum.
        /// </summary>
        /// <param name="buffer">The raw byte sequence.</param>
        /// <returns>The calculated checksum of the byte sequence</returns>
        private UInt32 CalculateCheckSum(byte[] buffer)
        {
            Int32 index = 0;
            UInt32 checkSum = 0;
            UInt32 temp = 0;

            //Substract the size of the checksum struct
            int size = buffer.Length - 8;

            for (index = 0; index < size; index++)
            {
                temp = buffer[index];
                checkSum += temp;
            }

            return checkSum;
        }

        #endregion

        #region Nested Types

        internal struct NavDataHeader
        {
            internal UInt32 Header;
            internal UInt32 DroneStatus;
            internal UInt32 Sequence;
            internal UInt32 VisionDefined;
        }

        internal struct NavDataStructure
        {
            internal UInt16 Tag;
            internal UInt16 Size;
        }

        internal struct NavDataDrone
        {
            internal UInt16 Tag;
            internal UInt16 Size;
            internal UInt32 ControlStatus;
            internal UInt32 FlyingPercentage;
            internal Single Theta;
            internal Single Phi;
            internal Single Psi;
            internal Int32 Altitude;
            internal Single VelocityX;
            internal Single VelocityY;
            internal Single VelocityZ;
            internal UInt32 LastFrameIndex;
            internal Single DetectionCameraRotationm11;
            internal Single DetectionCameraRotationm12;
            internal Single DetectionCameraRotationm13;
            internal Single DetectionCameraRotationm21;
            internal Single DetectionCameraRotationm22;
            internal Single DetectionCameraRotationm23;
            internal Single DetectionCameraRotationm31;
            internal Single DetectionCameraRotationm32;
            internal Single DetectionCameraRotationm33;
            internal Single DetectionCameraTranslationX;
            internal Single DetectionCameraTranslationY;
            internal Single DetectionCameraTranslationZ;
            internal UInt32 DetectionTagIndex;
            internal UInt32 DetectionCameraType;
            internal Single DroneCameraRotationm11;
            internal Single DroneCameraRotationm12;
            internal Single DroneCameraRotationm13;
            internal Single DroneCameraRotationm21;
            internal Single DroneCameraRotationm22;
            internal Single DroneCameraRotationm23;
            internal Single DroneCameraRotationm31;
            internal Single DroneCameraRotationm32;
            internal Single DroneCameraRotationm33;

            internal Single DroneCameraTranslationX;
            internal Single DroneCameraTranslationY;
            internal Single DroneCameraTranslationZ;
        }

        internal class NavDataCheckSum
        {
            internal UInt16 Tag;
            internal UInt16 Size;
            internal UInt32 CheckSum;
        }

        internal class NavDataHostAngles
        {
            internal UInt16 Tag;
            internal UInt16 Size;

            internal Int32 Enable;
            internal Single AngleX;
            internal Single AngleY;
            internal Single AngleZ;

            internal UInt32 Elapsed;
        }

        internal struct Matrix
        {
            internal Single m11;
            internal Single m12;
            internal Single m13;
            internal Single m21;
            internal Single m22;
            internal Single m23;
            internal Single m31;
            internal Single m32;
            internal Single m33;
        }

        internal struct Vector
        {
            internal Single X;
            internal Single Y;
            internal Single Z;
        }

        internal struct NavVisionDetect
        {
            internal UInt16 Tag;
            internal UInt16 Size;

            internal UInt32 TagCount;

            internal UInt32 Tag1Type;
            internal UInt32 Tag2Type;
            internal UInt32 Tag3Type;
            internal UInt32 Tag4Type;

            internal UInt32 Tag1X;
            internal UInt32 Tag2X;
            internal UInt32 Tag3X;
            internal UInt32 Tag4X;

            internal UInt32 Tag1Y;
            internal UInt32 Tag2Y;
            internal UInt32 Tag3Y;
            internal UInt32 Tag4Y;

            internal UInt32 Tag1BoxWidth;
            internal UInt32 Tag2BoxWidth;
            internal UInt32 Tag3BoxWidth;
            internal UInt32 Tag4BoxWidth;

            internal UInt32 Tag1BoxHeight;
            internal UInt32 Tag2BoxHeight;
            internal UInt32 Tag3BoxHeight;
            internal UInt32 Tag4BoxHeight;

            internal UInt32 Tag1Distance;
            internal UInt32 Tag2Distance;
            internal UInt32 Tag3Distance;
            internal UInt32 Tag4Distance;
        }

        #endregion
    }
}
