//////////////////////////////////////
// 
// OpenCV�ŗV�ڂ��I
// http://playwithopencv.blogspot.com/
//
//markerdetector2.cpp
//�}�[�J�[��T���܂��B
//
//�����̊T�v
//�}�[�J�[��T���A�}�[�J�[���X,Y,Z���������܂��B
//�}�[�J�[�̔ԍ�����ǂ��A�}�[�J�[��ɐ�����\�����܂��B

#include "cv.h"
#include "highgui.h"
void BubSort(float arr[ ], int n);


int decodeMarker(IplImage* src,int& MarkerDirection);

int main(int argc, char * argv[])
{

	///////////////////////////////////////////
	//�摜�ɕ\�������鎲�̏����B
	
	#define MARKER_SIZE (70)       /* �}�[�J�[�̓�����1�ӂ̃T�C�Y[mm] */
	CvMat *intrinsic = (CvMat*)cvLoad("intrinsic.xml");
	CvMat *distortion = (CvMat*)cvLoad("distortion.xml");
	int i,j,k;

	CvMat object_points;
	CvMat image_points;

	CvMat *rotation = cvCreateMat (1, 3, CV_32FC1);
	CvMat *translation = cvCreateMat (1 , 3, CV_32FC1);
	
	//���̍��W�����p
	CvMat *srcPoints3D = cvCreateMat (4, 1, CV_32FC3);//����3�������W
	CvMat *dstPoints2D = cvCreateMat (4, 1, CV_32FC3);//��ʂɓ��e�����Ƃ���2�������W
	
	CvPoint3D32f baseMarkerPoints[4];
	//�l�p��������ԏ�ł͂ǂ̍��W�ɂȂ邩���w�肷��B
	//�R�[�i�[      ���ۂ̍��W(mm)
	//   X   Y     X    Y  
	//   0   0   = 0    0
	//   0   1   = 0    20
	//   1   0   = 20   0
	baseMarkerPoints[0].x =(float) 0 * MARKER_SIZE;
	baseMarkerPoints[0].y =(float) 0 * MARKER_SIZE;
	baseMarkerPoints[0].z = 0.0;
		
	baseMarkerPoints[1].x =(float) 0 * MARKER_SIZE;
	baseMarkerPoints[1].y =(float) 1 * MARKER_SIZE;
	baseMarkerPoints[1].z = 0.0;

	baseMarkerPoints[2].x =(float) 1 * MARKER_SIZE;
	baseMarkerPoints[2].y =(float) 1 * MARKER_SIZE;
	baseMarkerPoints[2].z = 0.0;
 
	baseMarkerPoints[3].x =(float) 1 * MARKER_SIZE;
	baseMarkerPoints[3].y =(float) 0 * MARKER_SIZE;
	baseMarkerPoints[3].z = 0.0;
	
  //���̊�{���W�����߂�B
	for ( i=0;i<4;i++)
	{	
		switch (i)
		{
			case 0:	srcPoints3D->data.fl[0]     =0;
				    srcPoints3D->data.fl[1]     =0;
					srcPoints3D->data.fl[2]     =0;
					break;
			case 1:	srcPoints3D->data.fl[0+i*3] =(float)MARKER_SIZE;
					srcPoints3D->data.fl[1+i*3] =0;
					srcPoints3D->data.fl[2+i*3] =0;
					break;
			case 2:	srcPoints3D->data.fl[0+i*3] =0;
					srcPoints3D->data.fl[1+i*3] =(float)MARKER_SIZE;
					srcPoints3D->data.fl[2+i*3] =0;
					break;
			case 3:	srcPoints3D->data.fl[0+i*3] =0;
					srcPoints3D->data.fl[1+i*3] =0;
					srcPoints3D->data.fl[2+i*3] =-(float)MARKER_SIZE;;
					break;
		
		}
	} 

	///���̏����@�����܂�
	////////////////////////////////////


	IplImage* image;
	IplImage* gsImage;
	IplImage* gsImageContour;

	CvCapture *capture = cvCaptureFromCAM(1);
	IplImage * capimg;

	capimg=cvQueryFrame(capture);
	
	double	process_time;
	image=cvCreateImage(cvGetSize(capimg),IPL_DEPTH_8U,3);
	cvCopy(capimg,image);
	
	gsImage=cvCreateImage(cvGetSize(image),IPL_DEPTH_8U,1);
	gsImageContour=cvCreateImage(cvGetSize(image),IPL_DEPTH_8U,1);
	char presskey;
	
	//�t�H���g�̐ݒ�
	CvFont dfont;
    float hscale      = 0.5f;
    float vscale      = 0.5f;
    float italicscale = 0.0f;
    int  thickness    = 1;
    char text[255] = "";
    cvInitFont (&dfont, CV_FONT_HERSHEY_SIMPLEX , hscale, vscale, italicscale, thickness, CV_AA);

	CvFont axisfont;
    float axhscale      = 0.8f;
    float axvscale      = 0.8f;
    cvInitFont (&axisfont, CV_FONT_HERSHEY_SIMPLEX , axhscale, axvscale, italicscale, thickness, CV_AA);
	
	
	//�֊s�ۑ��p�̃X�g���[�W���m��
	CvMemStorage *storage = cvCreateMemStorage (0);//�֊s�p
	CvMemStorage *storagepoly = cvCreateMemStorage (0);//�֊s�ߎ��|���S���p
	
	CvSeq *firstcontour=NULL;
	CvSeq *polycontour=NULL;

	int contourCount;
	
	IplImage *marker_inside=cvCreateImage(cvSize(70,70),IPL_DEPTH_8U,1);
	IplImage *marker_inside_zoom=cvCreateImage(cvSize(marker_inside->width*2,marker_inside->height*2),IPL_DEPTH_8U,1);
	IplImage *tmp_img=cvCloneImage(marker_inside);
		
	CvMat *map_matrix;
	CvPoint2D32f src_pnt[4], dst_pnt[4], tmp_pnt[4];

	//�}�[�J�[�̓����̕ό`��̌`
	dst_pnt[0] = cvPoint2D32f (0, 0);
	dst_pnt[1] = cvPoint2D32f (marker_inside->width, 0);
	dst_pnt[2] = cvPoint2D32f (marker_inside->width, marker_inside->height);
    dst_pnt[3] = cvPoint2D32f (0, marker_inside->height);
	map_matrix = cvCreateMat (3, 3, CV_32FC1);
	
//	cvNamedWindow ("marker_inside", CV_WINDOW_AUTOSIZE);
//	cvNamedWindow ("inside", CV_WINDOW_AUTOSIZE);
	cvNamedWindow ("capture_image", CV_WINDOW_AUTOSIZE);

	
	/*decodeMarker�@�ŕ������킩��悤�ɂȂ����̂ŁA���̕ӂ͂���Ȃ�
	//�}�X�N�摜�̓ǂݍ��݁B
	//���o�����}�[�J�[�ƁA���̉摜��AND�����AcvCountNonZero����ԑ傫���������̂��}�[�J�[�̌����Ƃ���B
	IplImage * mask0  =cvLoadImage("C:\\OpenCVMarker\\mask_0.bmp",0);
	IplImage * mask90 =cvLoadImage("C:\\OpenCVMarker\\mask_90.bmp",0);
	IplImage * mask180=cvLoadImage("C:\\OpenCVMarker\\mask_180.bmp",0);
	IplImage * mask270=cvLoadImage("C:\\OpenCVMarker\\mask_270.bmp",0);
	IplImage * tempmask=cvCloneImage(mask0);//��Ɨp
	*/
	while(1)
	{
		cvClearMemStorage(storage);
		cvClearMemStorage(storagepoly);

		process_time = (double)cvGetTickCount();
		capimg=cvQueryFrame(capture);
		cvCopy(capimg,image);
		//�O���[�X�P�[����
		cvCvtColor(image,gsImage,CV_BGR2GRAY);		
		
		//������
		cvSmooth(gsImage,gsImage,CV_GAUSSIAN,3);
	
		//��l��
		cvThreshold (gsImage, gsImage, 0, 255, CV_THRESH_BINARY | CV_THRESH_OTSU);//�}�[�J�[�������o��

		//���]
		cvNot(gsImage,gsImage);
		
		//�֊s��T���܂��BNc�ɂ͒T�����֊s�̐�������܂��B
		//CV_RETR_LIST�ɐݒ肷��ƁA�������֊s�����ׂē������x���ɓ���܂��B�B
		//first=�֊s�P�̗֊s�Q�̗֊s�R�̗֊s�S
		
		contourCount=0;
		
		//�֊s���o
		cvCopy(gsImage,gsImageContour);		
		contourCount=cvFindContours (gsImageContour, storage, &firstcontour, sizeof (CvContour), CV_RETR_CCOMP);
		
		//�֊s�ɋߎ����Ă���|���S�������߂�i�ŏ���������3�s�N�Z���ɐݒ�j
		polycontour=cvApproxPoly(firstcontour,sizeof(CvContour),storagepoly,CV_POLY_APPROX_DP,3,1);	
	
		for(CvSeq* c=polycontour;c!=NULL;c=c->h_next)
		{
			//�O���̗֊s���傫�����Ă����������Ă��_���B����Ɏl�p�`�Ŗ����ƃ_���@�Ƃ������Ƃɂ���
			if((cvContourPerimeter(c)<2500)&&(cvContourPerimeter(c)>60)&&(c->total==4))
			{
				//�l�p�`�̒��Ɏl�p�`������΃}�[�J�[�Ƃ���B
				if(c->v_next!=NULL)
				{
					if(c->v_next->total==4)
					{
						int nearestindex=0;
												
						CvSeq* c_vnext=c->v_next;
			//			cvDrawContours(image,c,CV_RGB(255,255,0),CV_RGB(200,255,255),0);
			//			cvDrawContours(image,c_vnext,CV_RGB(255,0,0),CV_RGB(0,255,255),0);
						
						
						float xlist[4];
						float ylist[4];
						for(int n=0;n<4;n++)
						{
							CvPoint* p=CV_GET_SEQ_ELEM(CvPoint,c->v_next,n);
	
							
							tmp_pnt[n].x=(float)p->x;
							tmp_pnt[n].y=(float)p->y;	
							xlist[n]=(float)p->x;
							ylist[n]=(float)p->y;
						}
						
						//�l�p�̏�񂾂��n���B�ǂ���������Ă��邩�͂܂��킩��Ȃ�
						cvGetPerspectiveTransform (tmp_pnt, dst_pnt, map_matrix);
						
						//�}�[�J�[�̓����𐳕��`�ɕό`������
						cvWarpPerspective (gsImage, marker_inside, map_matrix, CV_INTER_LINEAR + CV_WARP_FILL_OUTLIERS, cvScalarAll (0));
									
					
						//marker_inside�i�}�[�J�[�̓��������𒊏o���A�����`�ɓ����ϊ��������́j
						//���A�}�X�N�摜���w�肵�Ĉꎞ�C���[�W�ɃR�s�[�B
						//�ꎞ�C���[�W�ɔ����_����������΁A�}�X�N�摜�Ɠ��������������Ă��邱�ƂɂȂ�B
		
						int notzeroCount=0;
						
						int maxCount=0;
						int markerDirection=0;//��{��0deg
						cvResize(marker_inside,marker_inside_zoom);
						

						//�}�[�J�[�̔ԍ���ǂݎ��
						int marker_number=decodeMarker(marker_inside,markerDirection);
						sprintf(text,"%d",marker_number);
		/*
					decodeMarker�ŕ������킩��悤�ɂȂ����̂ŁA���̕ӂ͂���Ȃ��Ȃ���

						cvCopy(marker_inside,tempmask,mask0);//cvCopy���Ƀ}�X�N�摜������ƁA�}�X�N�̂O����Ȃ��Ƃ���̂݃R�s�[�����B
						notzeroCount=cvCountNonZero(tempmask);
						if(maxCount<notzeroCount)
						{
							maxCount=notzeroCount;
							markerDirection=0;						
							sprintf(text,"0deg", notzeroCount);
						
						}
						cvZero(tempmask);
						cvCopy(marker_inside,tempmask,mask90);
						notzeroCount=cvCountNonZero(tempmask);
						if(maxCount<notzeroCount)
						{
							maxCount=notzeroCount;
							markerDirection=90;
							sprintf(text,"90deg");
						}			
						cvZero(tempmask);
						cvCopy(marker_inside,tempmask,mask180);
						notzeroCount=cvCountNonZero(tempmask);
						if(maxCount<notzeroCount)
						{
							maxCount=notzeroCount;
							markerDirection=180;
							sprintf(text,"180deg");
						}
						cvZero(tempmask);
						cvCopy(marker_inside,tempmask,mask270);						
						notzeroCount=cvCountNonZero(tempmask);
						if(maxCount<notzeroCount)
						{
							maxCount=notzeroCount;
							markerDirection=270;
							sprintf(text,"270deg");
						}						
						cvPutText(marker_inside_zoom, text, cvPoint(70, 70), &dfont, cvScalarAll(255));
										
						cvPutText(marker_inside_zoom, text, cvPoint(20, 120), &dfont, cvScalarAll(255));
						cvZero(tempmask);
*/

				//		cvShowImage ("inside", marker_inside);
			//			cvShowImage ("marker_inside", marker_inside_zoom);


						//�l�p�̌����𔽉f������B
						
						if(markerDirection==0)
						{
							src_pnt[0].x=tmp_pnt[0].x;
							src_pnt[0].y=tmp_pnt[0].y;
							src_pnt[1].x=tmp_pnt[3].x;
							src_pnt[1].y=tmp_pnt[3].y;
							src_pnt[2].x=tmp_pnt[2].x;
							src_pnt[2].y=tmp_pnt[2].y;
							src_pnt[3].x=tmp_pnt[1].x;
							src_pnt[3].y=tmp_pnt[1].y;
								
						}
						if(markerDirection==90)
						{
							src_pnt[0].x=tmp_pnt[1].x;
							src_pnt[0].y=tmp_pnt[1].y;
							src_pnt[1].x=tmp_pnt[0].x;
							src_pnt[1].y=tmp_pnt[0].y;
							src_pnt[2].x=tmp_pnt[3].x;
							src_pnt[2].y=tmp_pnt[3].y;
							src_pnt[3].x=tmp_pnt[2].x;
							src_pnt[3].y=tmp_pnt[2].y;
							
							
						}

						if(markerDirection==180)
						{


							src_pnt[0].x=tmp_pnt[2].x;
							src_pnt[0].y=tmp_pnt[2].y;
							src_pnt[1].x=tmp_pnt[1].x;
							src_pnt[1].y=tmp_pnt[1].y;
							src_pnt[2].x=tmp_pnt[0].x;
							src_pnt[2].y=tmp_pnt[0].y;
							src_pnt[3].x=tmp_pnt[3].x;
							src_pnt[3].y=tmp_pnt[3].y;
						}
			

						if(markerDirection==270)
						{
							src_pnt[0].x=tmp_pnt[3].x;
							src_pnt[0].y=tmp_pnt[3].y;
							src_pnt[1].x=tmp_pnt[2].x;
							src_pnt[1].y=tmp_pnt[2].y;
							src_pnt[2].x=tmp_pnt[1].x;
							src_pnt[2].y=tmp_pnt[1].y;
							src_pnt[3].x=tmp_pnt[0].x;
							src_pnt[3].y=tmp_pnt[0].y;
						}
					//		cvPutText(image,"0", cvPoint((int)src_pnt[0].x,(int)src_pnt[0].y), &dfont, CV_RGB(255, 0, 255));
					//		cvPutText(image,"1", cvPoint((int)src_pnt[1].x,(int)src_pnt[1].y), &dfont, CV_RGB(255, 0, 255));
						
						
					
						//�}�[�J�[�̃C���[�W��ł̍��W��ݒ�B
						cvInitMatHeader (&image_points, 4, 1, CV_32FC2, src_pnt);

						//�}�[�J�[�̊�{�ƂȂ���W��ݒ�
						cvInitMatHeader (&object_points, 4, 3, CV_32FC1, baseMarkerPoints);

						//�J�����̓����萔(intrinstic��distortion)����Arotation��translation�����߂�	
						cvFindExtrinsicCameraParams2(&object_points,&image_points,intrinsic,distortion,rotation,translation);

						//���߂����̂��g�p���āA������ԏ�̍��W����ʏゾ�Ƃǂ̈ʒu�ɗ��邩���v�Z
						cvProjectPoints2(srcPoints3D,rotation,translation,intrinsic,distortion,dstPoints2D);					
				
						//����`��
						CvPoint startpoint;
						CvPoint endpoint;

						startpoint=cvPoint((int)dstPoints2D->data.fl[0], (int)dstPoints2D->data.fl[1]);
						for(j=1;j<4;j++)
						{
							endpoint=  cvPoint((int)dstPoints2D->data.fl[(j)*3],(int)dstPoints2D->data.fl[1+(j)*3]);
						
							if(j==1)
							{
								cvLine(image,startpoint,endpoint,CV_RGB(255,0,0),2,8,0);
								cvPutText(image, "X", endpoint, &axisfont,CV_RGB(255,0,0));
							}
							if(j==2)
							{
								cvLine(image,startpoint,endpoint,CV_RGB(0,255,0),2,8,0);
								cvPutText(image, "Y", endpoint, &axisfont,CV_RGB(0,255,0));
							}
							if(j==3)
							{
								cvLine(image,startpoint,endpoint,CV_RGB(0,0,255),2,8,0);
								cvPutText(image, "Z", endpoint, &axisfont,CV_RGB(0,0,255));
							}
						}
						//�}�[�J�[�̔ԍ���`��
						cvPutText(image, text,cvPoint((int)(dstPoints2D->data.fl[3]+dstPoints2D->data.fl[6])/2,
													  (int)(dstPoints2D->data.fl[4]+dstPoints2D->data.fl[7])/2)
															, &axisfont, CV_RGB(255, 255, 100));
					}
				}
			
			}
		}

		process_time = (double)cvGetTickCount()-process_time;
	
		sprintf(text,"process_time %gms", process_time/(cvGetTickFrequency()*1000.)); 
		cvPutText(image, "http://playwithopencv.blogspot.com", cvPoint(10, 20), &dfont, CV_RGB(255, 255, 255));
		cvPutText(image, text, cvPoint(10, 40), &dfont, CV_RGB(255, 255, 255));
		cvShowImage("capture_image",image);
		
		
		presskey=cvWaitKey (50);
		if(presskey==27)break;
		
	}

	cvReleaseImage(&image);
	cvReleaseImage(&gsImage);	
	cvReleaseImage(&gsImageContour);
	cvReleaseImage(&marker_inside);	
	cvReleaseImage(&tmp_img);
//	cvReleaseImage(&mask0);
//	cvReleaseImage(&mask90);
//	cvReleaseImage(&mask180);
//	cvReleaseImage(&mask270);
//	cvReleaseImage(&tempmask);
	cvReleaseMat (&map_matrix);
	cvReleaseMemStorage(&storagepoly);
	cvReleaseMemStorage(&storage);
	cvReleaseImage(&image);
	cvReleaseImage(&gsImage);
//	cvDestroyWindow("marker_inside");
	cvDestroyWindow("capture_image");
//	cvDestroyWindow ("inside");

return 0;

}

void BubSort(float arr[ ], int n)
{
    int i, j;
	float temp;

    for (i = 0; i < n - 1; i++)
	{
        for (j = n - 1; j > i; j--)
		{
            if (arr[j - 1] > arr[j])
			{  /* �O�̗v�f�̕����傫�������� */
                temp = arr[j];        /* �������� */
                arr[j] = arr[j - 1];
                arr[j - 1]= temp;
            }
        }	
     }
}


//�}�[�J�[��2�����R�[�h��ǂݎ��
//
int decodeMarker(IplImage* src,int& MarkerDirection)
{
	if(src->nChannels!=1)
	{
		return -1;
	}

	if(src->width!=src->height)
	{
		return -1;
	}

	
	//�}�[�J�[�̓����̃h�b�g����͂���B
	//���オ��h�b�g�B
	//�~�ŕ\�������Ƃ��낪�����Ȃ��Ă��邩�����Ȃ��Ă��邩�𔻒f���āA�����ɕϊ����Ė߂��B
	//�@4�r�b�g��4�s��16�r�b�g�B�@�p�^�[����6���ʂ�H
	//������������
	//������������
	//������������
	//������������
	//������������
	//������������
	//
	int	whitecount=0;
	uchar* pointer;
	uchar* wkpointer;
	
	int cellRow=6;
	int cellCol=6;
	int result[4][4];
	int offset=5;
	int cellsize=10;
	int c_white=0;
	int cellx,celly;
	
	for (int i=0;i<4;i++)
	{
		for (int j=0;j<4;j++)
		{
			result[i][j]=0;
		}
	}

	//�h�b�g�����o
	for(int y=0;y<4;y++)
	{
		pointer=(uchar*)(src->imageData + (y+2)*cellsize*src->widthStep);
		for(int x=0;x<4;x++)
		{
			 c_white=0;
			if(pointer[src->nChannels*cellsize*(x+2)]>1)
			{
				
				for(celly=-2;celly<3;celly++)
				{
					wkpointer=(uchar*)(src->imageData + (celly+((y+2)*cellsize))*src->widthStep);
					for(cellx=-2;cellx<3;cellx++)
					{
						if(pointer[src->nChannels*(cellsize*(x+2)+cellx)]>1)
						{						
							c_white++;
						}
					}
					
				}
				if(c_white>4)
				{
					whitecount++;
					result[y][3-x]=1;
				}
			}
		}	
	}
	
	//���������o
	//
	
	//0���@�@�@(10,10)����㉺���E3�s�N�Z��
	c_white=0;
	for(celly=-3;celly<4;celly++)
	{
		wkpointer=(uchar*)(src->imageData + (celly+(10))*src->widthStep);
		for(cellx=-3;cellx<4;cellx++)
		{
			if(wkpointer[src->nChannels*(10+cellx)]>1)
			{						
				c_white++;
			}
		}			
		
	}
	if(c_white>30)
	{
		MarkerDirection=0;
	}
	
	//90deg�@(60,10)�̏㉺���E3�s�N�Z��	
	c_white=0;
	for(celly=-3;celly<4;celly++)
	{
		wkpointer=(uchar*)(src->imageData + (celly+(10))*src->widthStep);
		for(cellx=-3;cellx<4;cellx++)
		{
			if(wkpointer[src->nChannels*(60+cellx)]>1)
			{						
				c_white++;
			}
		}
		
	}
	
	
	if(c_white>30)
	{
		MarkerDirection=90;
	}
	
	
	//180deg�@(60,60)�̏㉺���E3�s�N�Z��	
	c_white=0;
	for(celly=-3;celly<4;celly++)
	{
		wkpointer=(uchar*)(src->imageData + (celly+(60))*src->widthStep);
		for(cellx=-3;cellx<4;cellx++)
		{
			if(wkpointer[src->nChannels*(60+cellx)]>1)
			{						
				c_white++;
			}
		}			
		
	}
	if(c_white>30)
	{
		MarkerDirection=180;
	}

	//270deg�@(10,60)�̏㉺���E3�s�N�Z��	
	c_white=0;
	for(celly=-3;celly<4;celly++)
	{
		wkpointer=(uchar*)(src->imageData + (celly+(60))*src->widthStep);
		for(cellx=-3;cellx<4;cellx++)
		{
			if(wkpointer[src->nChannels*(10+cellx)]>1)
			{						
				c_white++;
			}
		}
	}
	if(c_white>30)
	{
		MarkerDirection=270;
	}


	//���ꂾ�Ƃ܂��摜�ɕ\�����ꂽ�܂܂̏��ԂɂȂ��Ă���̂ŁA
	//�ǂ���������Ă��邩�����o���āA���̕����ɔz���g�݂Ȃ����K�v������B
	
	
	//
	int temp[4][4];
	for(int i=0;i<4;i++)
	{
		for(int j=0;j<4;j++)
		{
				temp[i][j]=result[i][j];
		}
	}
	
	if(MarkerDirection==0)
	{
		//�������Ȃ�
	}
	if(MarkerDirection==180)
	{
		//�S�����΂ɂ���
		for(int i=0;i<4;i++)
		{
			for(int j=0;j<4;j++)
			{
				result[3-i][3-j]=temp[i][j];
			}
		}
	}

	if(MarkerDirection==90)
	{
		//
		for(int i=0;i<4;i++)
		{
			for(int j=0;j<4;j++)
			{
				result[j][3-i]=temp[i][j];
			}
		}
	}

	if(MarkerDirection==270)
	{
		//
		for(int i=0;i<4;i++)
		{
			for(int j=0;j<4;j++)
			{
				result[3-j][i]=temp[i][j];
			}
		}
	}

	
	whitecount=0;
	int nn=0;
	for (int i=0;i<4;i++)
	{
		for (int j=0;j<4;j++)
		{		
			nn=	(int)pow((double)16,i)*result[i][j]*pow((double)2,j);
		 whitecount=nn+ whitecount;;

		}
	}

	return whitecount;
}

