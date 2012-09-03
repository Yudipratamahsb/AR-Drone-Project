
#include "headder.h"

#define MAX_MESSAGE_LENGTH 1024*20

int forkPID = 0;

void sigproc(int);

int main(int argc, char* argv[])
{
	program_name = argv[0];
	signal (SIGINT, sigproc);
    int connectionSocket = -1;
    int remoteSocket = -1;
    
    char message[MAX_MESSAGE_LENGTH];

    struct sockaddr_in remoteAddress;
    
    bool do_fork = false;
    
    opterr = 0;
    int opt;
    while ((opt = getopt(argc, argv, "f")) != -1)
    {
        switch (opt)
        {
            case 'f':
                do_fork = true;
                break;
            default:
                abort();
        }
    }
    

    
    // Create socket with UDP setting (SOCK_DGRAM)
    connectionSocket = socket(AF_INET, SOCK_DGRAM, 0);
    if (connectionSocket < 0)
    {
        fprintf(stderr, "%s > ERROR: Cannot open socket!\n", argv[0]);
        exit(1);
    }
    
    remoteSocket = socket(AF_INET, SOCK_DGRAM, 0);
    if (remoteSocket < 0)
    {
    	fprintf(stderr, "%s > ERROR: Cannot open socket!\n", argv[0]);
        exit(1);
    }
    
    
           
    struct sockaddr_in bindAddr;
    memset(&bindAddr, 0, sizeof(struct sockaddr_in));
    bindAddr.sin_family = AF_INET;
    bindAddr.sin_addr.s_addr = inet_addr("0.0.0.0");
    bindAddr.sin_port = htons(15555);
           
    
    int errorNo = bind(connectionSocket, (struct sockaddr*)&bindAddr, sizeof(bindAddr));
    if (errorNo != 0) {
        printf("%s > Error binding socket %s.\n",
            argv[0],
            strerror(errno));
        exit(-1);
    }
    
    memset(&bindAddr, 0, sizeof(struct sockaddr_in));
    bindAddr.sin_family = AF_INET;
    bindAddr.sin_addr.s_addr = inet_addr("0.0.0.0");
    bindAddr.sin_port = htons(6665);
           
    
    errorNo = bind(remoteSocket, (struct sockaddr*)&bindAddr, sizeof(bindAddr));
    if (errorNo != 0) {
        printf("%s > Error binding socket %s.\n",
            argv[0],
            strerror(errno));
        exit(-1);
    }
    
    
    
    printf("%s > Proxy Running: Waiting for signal packet.\n",
           argv[0]);
         
    if (do_fork) {
    	if (fork() == 0) {
    		return execlp("iptables", "iptables", "-t", "nat", "-A", "POSTROUTING", "-p", "UDP", "--sport", "15555", "-j", "SNAT", "--to", "192.168.1.2:5555", NULL);
    	}
    	//iptables -t nat -A POSTROUTING -p UDP --sport 15554 -j SNAT --to 192.168.1.254:5554
    	if (fork() == 0) {
    		return execlp("iptables", "iptables", "-t", "nat", "-A", "PREROUTING", "-p", "UDP", "-d", "192.168.1.2", "--dport", "5555", "-j", "DNAT", "--to", "192.168.1.1:15555", NULL);
    	}
		//iptables -t nat -A PREROUTING -p UDP -d 192.168.1.254 --dport 5554 -j DNAT --to 192.168.1.1:15554
        //if ((forkPID = fork()) == 0) {
        //    return execlp("./droneVideoemul", "./droneVideoemul", "-v", (char*)NULL);
        //}
        sleep(2);
    }
    char b;
    struct sockaddr_in contAddress = recvfrom_s(remoteSocket, &b, 1);
    printf("%s > Recieved signal packet from %s:%u.\n",
        argv[0],
        inet_ntoa(contAddress.sin_addr),
        ntohs(contAddress.sin_port)
        );
    
    
    
    
    
    // If we reach this point we are up and running!
    printf("%s > Sending signal packet.\n",
           argv[0]);
    
    
    memset(&remoteAddress, 0, sizeof(struct sockaddr_in));
    remoteAddress.sin_family = AF_INET;
    remoteAddress.sin_addr.s_addr = inet_addr("192.168.1.1");
    remoteAddress.sin_port = htons(5555);
    //char b;
    
    
        
    char a[4] = { 1, 0, 0, 0 };
    sendto_s(connectionSocket, a, 4, remoteAddress);
        
        
    
    printf("%s > Receiving data.\n",
        argv[0]);
        
    
    // Server infinite loop, use ctrl+c to kill proc
    while (1)
    {
        // Init/clear buffer
        memset(message, 0x0, MAX_MESSAGE_LENGTH);
        fd_set fds;
	    struct timeval tv;
	    int r;

	    FD_ZERO(&fds);
	    FD_SET(connectionSocket, &fds);

	    tv.tv_sec = 2;
	    tv.tv_usec = 0;
	    r = select(connectionSocket + 1, &fds, NULL, NULL, &tv);
        
        if (-1 == r) {
 	        if (EINTR == errno) continue; 
	        printf("%s > Select error: %s\n", argv[0], strerror(errno));
        }

	    if (0 == r) {
	        fprintf(stderr,"%s > Select timeout.\n", argv[0]);
	        sigproc(1);
	        exit(EXIT_FAILURE);
        }
        
        int bytes_recved = 0;

        remoteAddress = recvfrom_a(connectionSocket, message, MAX_MESSAGE_LENGTH, &bytes_recved);
        printf("%s > Receiving data from %s:%u.\n",
            argv[0],
            inet_ntoa(remoteAddress.sin_addr),
            ntohs(remoteAddress.sin_port));
        
        
        // Print received message, only use for debugging!
        //printf("%s > %s\n", argv[0], message);
        //printf (".");
        
        printf("%s > Sending to %s:%u.\n",
            argv[0],
            inet_ntoa(contAddress.sin_addr),
            ntohs(contAddress.sin_port));

        sendto_s(remoteSocket, message, bytes_recved, contAddress); 
            
    }
    sigproc(1);
    return 0;
}

void sigproc(int) {
    signal (SIGINT, sigproc);
    printf("%s > Killing child process.\n", program_name);
    if (forkPID != 0)
        kill(forkPID, SIGKILL);
    exit(0);
}



