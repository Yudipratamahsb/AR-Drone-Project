#include "headder.h"


const char* message = "abcdefghijklmnopqrstuvwxyz";

int main(int argc, char* argv[])
{
	program_name = argv[0];
	struct sockaddr_in remoteAddress;
	int connectionSocket = 0;
	bool verbose = false;
	
	
	
	opterr = 0;
    int opt;
    while ((opt = getopt(argc, argv, "v")) != -1)
    {
        switch (opt)
        {
            case 'v':
                verbose = true;
                break;
            default:
                abort();
        }
    }
	
	
	connectionSocket = socket(AF_INET, SOCK_DGRAM, 0);
    if (connectionSocket < 0)
    {
        fprintf(stderr, "%s > ERROR: Cannot open socket!\n", argv[0]);
        exit(1);
    }
    
    // If we reach this point we are up and running!
    
           
    struct sockaddr_in bindAddr;
    memset(&bindAddr, 0, sizeof(struct sockaddr_in));
    bindAddr.sin_family = AF_INET;
    bindAddr.sin_addr.s_addr = inet_addr("0.0.0.0");//10.200.170.78
    bindAddr.sin_port = htons(5555);
           
           
    int errorNo = bind(connectionSocket, (struct sockaddr*)&bindAddr, sizeof(bindAddr));
    if (errorNo != 0) {
        printf("%s > Error binding socket %s.\n",
            argv[0],
            strerror(errno));
        exit(-1);
    }
    
    char b;
    printf("%s > AR.Drone Emulator Running: Waiting for signal packet.\n",
           argv[0]);
    remoteAddress = recvfrom_s(connectionSocket, &b, 1);
    
    printf("%s > Recieved signal packet from %s:%u.\n",
        argv[0],
        inet_ntoa(remoteAddress.sin_addr),
        ntohs(remoteAddress.sin_port)
        );
        
        
    printf("%s > Sending data.\n",
        argv[0]);
        
        
    // Server infinite loop, use ctrl+c to kill proc
    while (1)
    {
        // Init/clear buffer
        //memset(message, 0x0, MAX_MESSAGE_LENGTH);
        
        // Read line from serial
        //returnNo = fscanf(inputFile, "%s", message);
        //if (returnNo < 0)
        //{
         //   fprintf(stderr, "%s > ERROR: Cannot read data from device!\n", argv[0]);
            
        //    continue;
        //}
        
        if (verbose)
        {
            // Print received message, only use for debugging!
            printf("%s > %s\n", argv[0], message);
        }
        
        // Send message to remote server
        sendto_s(connectionSocket,
                 (char*)message,
                 remoteAddress);
                 
        sleep(1);
            
    }
}
