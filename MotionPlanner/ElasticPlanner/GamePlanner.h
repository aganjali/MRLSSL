#include <math.h>
#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include "cuda.h"
#include "Types.h"

GPosition2D* devHeads2D;
GPosition2D* devTails2D;
int* devHisto2D;
float* devtimehead2D;
float* devtimetail2D;
float* devMinTime2D;
GObjectState* states;
int* zeroArray;

float* DevDataX;
float* DevDataY;
float* DevPhi ;
float*  DevKdx ;
float* DevKdy ;
float* DevRobots ;
float* DevEachDataCount ;

GPosition2D* pHeads;
GPosition2D* pTails;
cudaError_t error;
float _sigmaX, _sigmaY, _RegionCount, _maxSampleCount;

const int threadsPerBlock = 256;
const int AccelSteps = 31;
const int maxRobotCount = 6;
const int maxLengh = 700;
const int Resoloution = 10;
const int maxPoints = maxLengh / Resoloution;
const int maxLines = 4;

//unsigned int sizeof(float);
//unsigned int sizeof(int);

 float _maxRobotAccel;
 float _maxRobotSpeed;
 float _ballDecel;

__global__ void GaussianKernel(float* DataX, int DataXLen0, float* DataY, int DataYLen0, float* EachDataCount, int EachDataCountLen0, int RegionCount, float* opps, int oppsLen0, int RobotCount, float sigmax, float sigmay, int maxCount, float* DevPhi, int DevPhiLen0, float* DevKdx, int DevKdxLen0, float* DevKdy, int DevKdyLen0);
__global__ void MultiObjectKernel(GObjectState Ball, float maxBallDeccel, float* RobotTimes, int RobotTimesLen0, int RobotTimesLen1, int N, GPosition2D* Heads, int HeadsLen0, int HeadsLen1, GPosition2D* Tails, int TailsLen0, int TailsLen1, float* TimesHeads, int TimesHeadsLen0, int TimesHeadsLen1, float* TimesTails, int TimesTailsLen0, int TimesTailsLen1, int* histo, int histoLen0, int histoLen1);
__global__ void ClaculateTimeKernel(GObjectState Ball, GObjectState* States, int StatesLen0, int N, float maxRobotSpeed, float maxBallDeccel, float maxRobotAccel, float* MinTime, int MinTimeLen0, int MinTimeLen1);
__device__ float CalculateTime(float dR, float v0, float v_max, float a_max);
__device__ float sign(float x);

void GamePlannerInit(float maxRobotAccel, float maxRobotSpeed, float ballDecel,float* DataScoreX,float* DataScoreY,float* eachRegionCount, int RegionCount,int maxSampleCount,float sigmaX,float sigmaY);
void CalculateBallState(GObjectState* robots, int RobotCounts, int N, GObjectState ball,float* Heads,float* Tails, float* TimeHeads, float* TimeTails, int* histo);
void DisposeGamePlanner();