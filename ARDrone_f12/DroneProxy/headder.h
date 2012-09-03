#include <stdlib.h>
#include <ctype.h>
#include <errno.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <stdio.h>
#include <signal.h>
#include <unistd.h> /* close() */
#include <string.h> /* memset() */

char* program_name;



void sendto_s(int socket, void* msg, int len, struct sockaddr_in remoteAddress) {
	int errorNo = sendto(socket, msg, len, 0, (struct sockaddr *)&remoteAddress, sizeof(remoteAddress));
	if (errorNo < 0) {
		fprintf(stderr, "%s > ERROR: Cannot send data %u:%s!\n", program_name, errno, strerror(errno));
	}
}

void sendto_s(int socket, char* msg, struct sockaddr_in remoteAddress) {
    sendto_s(socket, msg, strlen(msg), remoteAddress);
}

struct sockaddr_in recvfrom_s(int socket, void* buf, int len) {
    struct sockaddr inaddr;
    memset(&inaddr, 0, sizeof(inaddr));
    socklen_t sz = sizeof(inaddr);
    int errorNo = recvfrom(socket, buf, len, 0, &inaddr, &sz);
    if (errorNo < 0) {
        fprintf(stderr, "%s > ERROR: Cannot receive data %u:%s!\n", program_name, errno, strerror(errno));
    }
    return *(sockaddr_in*)&inaddr;
}




struct sockaddr_in recvfrom_a(int socket, void* buf, int len, int* out_len) {
    struct sockaddr inaddr;
    memset(&inaddr, 0, sizeof(inaddr));
    socklen_t sz = sizeof(inaddr);
    *out_len = recvfrom(socket, buf, len, 0, &inaddr, &sz);
    if (*out_len < 0) {
        fprintf(stderr, "%s > ERROR: Cannot receive data %u:%s!\n", program_name, errno, strerror(errno));
    }
    return *(sockaddr_in*)&inaddr;
}
