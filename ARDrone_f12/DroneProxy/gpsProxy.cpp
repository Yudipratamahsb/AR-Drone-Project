/*TComSerialProxyServer
 *  v0.2.2
 *  ARDroneTools
 *
 *  Created by nosaari on 25.02.11.
 *
 
 Very basic proxy server that reads any lines in given tty device and sends it
 via UDP to given server.
 Any line thats not starting with 'AT' is considered as debug output and will 
 be ignored!
 
 # Options:
 
 -v
 v[erbose] - Enables output of received strings, for performance reasons only use
 while debugging!
 
 -d DEVICE
 d[evice] - Set device to read values from, eg. '/dev/ttyPA0'. If none is given
 the default (hardcoded) device will be used (can be set below, see SETUP!).
 
 -i IP.ADDRESS
 i[p] - Sets IP address commands are sent to, eg. '192.168.1.2'. If none is given
 the default (hardcoded) ip of the drone (192.168.1.1) will be used (can be set 
 below, see SETUP!).

 
 Based on server code from
 http://www.gamedev.net/topic/310343-udp-client-server-echo-example/
 
 For detailed infos read
 http://www.linuxhowtos.org/C_C++/socket.htm
 
 *
 */

#include "headder.h"

#include <stdlib.h>
#include <ctype.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <stdio.h>
#include <unistd.h> /* close() */
#include <string.h> /* memset() */


// SETUP

// The default serial device to use without options.
// Eg. /dev/ttyPA0 for regular serial port or /dev/ttyUSB0 for USB connection.
#define DEFAULT_SERIAL_DEVICE   "/dev/ttyPA0"


// OTHER SETTINGS



// Buffer size for AT comands, increase if needed
#define MAX_MESSAGE_LENGTH      512



int main(int argc, char* argv[])
{
	program_name = argv[0];
    int flags               = 0;    // Send flags, 0 should be good
    int verbose             = 0;
    int connectionSocket    = 0;
    int returnNo            = 0;
    uint addressLength      = 0;
    
    char message[MAX_MESSAGE_LENGTH];
    const char* serialDevice      = NULL;
    FILE* inputFile;

    struct sockaddr_in remoteAddress;
    
    // Read arguments
    opterr = 0;
    int opt;
    while ((opt = getopt(argc, argv, "v")) != -1)
    {
        switch (opt)
        {
            case 'v':
                verbose = 1;
                break;
            case '?':
                if (isprint(optopt))
                    fprintf(stderr, "%s > ERROR: Unknown option `-%c'.\n", argv[0], optopt);
                else
                    fprintf(stderr, "%s > ERROR: Unknown option character `\\x%x'.\n", argv[0], optopt);
                return 1;
            default:
                abort();
        }
    }
    
    // Use default device if no device was given
    if (serialDevice == NULL)
    {
        serialDevice = DEFAULT_SERIAL_DEVICE;
    }
    
    
    // Create socket with UDP setting (SOCK_DGRAM)
    connectionSocket = socket(AF_INET, SOCK_DGRAM, 0);
    if (connectionSocket < 0)
    {
        fprintf(stderr, "%s > ERROR: Cannot open socket!\n", argv[0]);
        exit(1);
    }
    
    
    
           
    struct sockaddr_in bindAddr;
    memset(&bindAddr, 0, sizeof(struct sockaddr_in));
    bindAddr.sin_family = AF_INET;
    bindAddr.sin_addr.s_addr = inet_addr("192.168.1.1");
    bindAddr.sin_port = htons(6666);
           
    
    bind(connectionSocket, (struct sockaddr*)&bindAddr, sizeof(bindAddr));
    char b;
    // If we reach this point we are up and running!
    printf("%s > Proxy Running: Waiting for signal packet.\n",
           argv[0]);
    remoteAddress = recvfrom_s(connectionSocket, &b, 1);
    
    printf("%s > Recieved signal packet from %s:%u.\n",
        argv[0],
        inet_ntoa(remoteAddress.sin_addr),
        ntohs(remoteAddress.sin_port)
        );
        
        
        
    // Open serial device
    if ((inputFile = fopen(serialDevice, "r")) == 0)
    {
        fprintf(stderr, "%s > ERROR: Cannot open file %s!\n",
                argv[0],
                serialDevice);
        
        exit(1);
    }
    
    printf("%s > Sending data.\n",
        argv[0]);
        
    
    // Server infinite loop, use ctrl+c to kill proc
    while (1)
    {
        // Init/clear buffer
        memset(message, 0x0, MAX_MESSAGE_LENGTH);
        
        // Read line from serial
        returnNo = fscanf(inputFile, "%s", message);
        if (returnNo < 0)
        {
            fprintf(stderr, "%s > ERROR: Cannot read data from device!\n", argv[0]);
            
            continue;
        }
        
        if (verbose)
        {
            // Print received message, only use for debugging!
            printf("%s > %s\n", argv[0], message);
        }
        
        // Send message to remote server
        sendto_s(connectionSocket,
                 message,
                 remoteAddress);
            
    }
    return 0;
}

