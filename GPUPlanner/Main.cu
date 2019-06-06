
#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include "GamePlanner.cuh"
#include "Smoothing.cuh"
#include <stdio.h>
#include <iostream>
using namespace std;

extern "C" __declspec(dllexport) void __cdecl Start(float, float, float, int, int, int, float *, float*, float*, int, int, float, float);
extern "C" __declspec(dllexport) void __cdecl GPlannerBallState(float*, int, int, float*, float*, float*, float*, float*, int*);
extern "C" __declspec(dllexport) void __cdecl GPlannerScore(float*, int, float*, float*, float*);
extern "C" __declspec(dllexport) float __cdecl ForceTree(float*, int*, int, float*, float*, int, float*, int, float, float, int);
extern "C" __declspec(dllexport) void __cdecl ShutDown();



extern void Start(float maxRobotAccel, float maxRobotSpeed, float ballDecel, int maxPathCount, int maxRRTCount, int maxRobotCount, float * DataScoreX, float* DataScoreY, float* eachRegionCount, int RegionCount, int maxSampleCount, float sigmaX, float sigmaY)
{
	cudaSetDevice(0);
	cudaDeviceReset();
	//CUcontext cuContext;
	int  device = 0;
	cudaGetDevice(&device);
	/*cuCtxCreate(&cuContext, 0, device);
	cuMemGetInfo(&memory_free, &memory_total);
	cout << "freeMem: " << memory_free << "    totalMem: " << memory_total;
	cuCtxDetach(cuContext);*/
	GamePlannerInit(maxRobotAccel, maxRobotSpeed, ballDecel, DataScoreX, DataScoreY, eachRegionCount, RegionCount, maxSampleCount, sigmaX, sigmaY);
	//ElasticInit(maxPathCount, maxRRTCount);
	/*cuMemGetInfo(&memory_free, &memory_total);
	cout << "freeMem: " << memory_free << "    totalMem: " << memory_total;*/
}
extern void GPlannerBallState(float* robots, int RobotCounts, int N, float* ball, float* Heads, float* Tails, float* TimeHeads, float* TimeTails, int* histo)
{

	GObjectState* robotStates = new GObjectState[RobotCounts];
	for (int i = 0; i < RobotCounts; i++)
	{
		robotStates[i].Location.X = robots[4 * i];
		robotStates[i].Location.Y = robots[4 * i + 1];
		robotStates[i].Speed.X = robots[4 * i + 2];
		robotStates[i].Speed.Y = robots[4 * i + 3];
		//cout<< "Robot" << i << ": " << robotStates[i].Location.X << "            " << robotStates[i].Location.Y  << "            "<< robotStates[i].Speed.X  << "            " << robotStates[i].Speed.Y  << "\n";
	}

	GObjectState BallState;
	BallState.Location.X = ball[0];
	BallState.Location.Y = ball[1];
	BallState.Speed.X = ball[2];
	BallState.Speed.Y = ball[3];

	CalculateBallState(robotStates, RobotCounts, N, BallState, Heads, Tails, TimeHeads, TimeTails, histo);
}
extern void GPlannerScore(float* Robots, int RobotCount, float* Phi, float* Kdx, float* Kdy)
{
	if (_RegionCount > 0 && _maxSampleCount > 0 && RobotCount)
	{
		dim3 Block(_RegionCount, _maxSampleCount);
		dim3 Grid(RobotCount);

		error = cudaMemcpy(DevRobots, Robots, RobotCount * sizeof(float), cudaMemcpyHostToDevice);
		//cout << "memcpyRobotsX: "<< error << "\n";
		error = cudaMemcpy(DevRobots + maxRobotCount, Robots + maxRobotCount, RobotCount * sizeof(float), cudaMemcpyHostToDevice);
		//cout << "memcpyRobotsY: "<< error << "\n";
		GaussianKernel << <Grid, Block >> >(DevDataX, _RegionCount * _maxSampleCount, DevDataY, _RegionCount * _maxSampleCount, DevEachDataCount, _RegionCount, _RegionCount, DevRobots, 2 * maxRobotCount, RobotCount, _sigmaX, _sigmaY, _maxSampleCount, DevPhi, _maxSampleCount * _RegionCount * maxRobotCount, DevKdx, _maxSampleCount * _RegionCount * maxRobotCount, DevKdy, _maxSampleCount * _RegionCount * maxRobotCount);
		//error = cudaGetLastError();
		//cout << "GaussianKernel: "<< error << "\n";
		error = cudaMemcpy(Phi, DevPhi, RobotCount *_RegionCount * _maxSampleCount * sizeof(float), cudaMemcpyDeviceToHost);
		//cout << "memcpyPhi: "<< error << "\n";
		error = cudaMemcpy(Kdx, DevKdx, RobotCount *_RegionCount * _maxSampleCount * sizeof(float), cudaMemcpyDeviceToHost);
		//cout << "memcpyKdx: "<< error << "\n";
		error = cudaMemcpy(Kdy, DevKdy, RobotCount *_RegionCount * _maxSampleCount * sizeof(float), cudaMemcpyDeviceToHost);
		//cout << "memcpyKdy: "<< error << "\n";
	}
}
extern float ForceTree(float* Path, int* eachPathCount, int RobotCount, float* avoid, float* finalPath, int SmoothingCount, float* Obstacles, int ObstacleCount, float Kspring, float Kspring2, int n)
{
	_kSpring = Kspring;
	_kSpring2 = Kspring2;
	N = n;
	cudaError_t error;
	int maxP = -MaxPathCount;

	if (RobotCount > 0)
	{
		for (int i = 0; i < RobotCount; i++)
		{
			error = cudaMemcpy2D(DevPath + i * PathPitch, PathPitch * sizeof(float), Path + i * 2 * MaxPathCount, 2 * MaxPathCount * sizeof(float), 2 * eachPathCount[i] * sizeof(float), 1, cudaMemcpyHostToDevice);
			//cout << "PathMemCpy for Robot" << i << ": "<< error << "\n";
			if (eachPathCount[i] > maxP)
				maxP = eachPathCount[i];
		}
		//	cudaDeviceSynchronize();
		error = cudaMemcpy2D(DevObs, ObsPitch * sizeof(float), Obstacles, ObstacleCount * sizeof(float), ObstacleCount * sizeof(float), 2, cudaMemcpyHostToDevice);
		//	cout << "ObsMemcpy: "<< error << "\n";
		error = cudaMemcpy(DevEachPathCount, eachPathCount, RobotCount * sizeof(int), cudaMemcpyHostToDevice);
		//	cout << "EachPathMemcpy: "<< error << "\n";
		error = cudaMemcpy2D(DevAvoid, AvoidPitch * sizeof(float), avoid, RobotCount * sizeof(float), RobotCount * sizeof(float), 4, cudaMemcpyHostToDevice);
		//	cout << "avoidMemcpy: "<< error << "\n";

		dim3 Block(128, 2, 1);
		dim3 Grid((maxP + Block.x - 1) / Block.x, RobotCount, 1);

		dim3 Block2(256, 1, 1);
		dim3 Grid2((maxP + Block2.x - 1) / Block2.x, /*ObstacleCount * */RobotCount, 1);
		/*cudaStreamSynchronize(streams[RobotCount + 1]);
		cudaStreamSynchronize(streams[RobotCount + 2]);
		cudaStreamSynchronize(streams[RobotCount]);*/

		//for(int i = 0; i < RobotCount; i ++)
		//{
		//	cudaStreamSynchronize(streams[i]);
		//}
		for (int i = 0; i < SmoothingCount; i++)
		{
			CalculateForcesKernel << <Grid, Block >> >(DevForce, DevEachPathCount, RobotCount, ForcePitch, _kSpring, _kSpring2, N);
			//	error = cudaGetLastError();
			//	cout << "CalculateForceKernell iter " << i << ": " << error << "\n";
			ReCalculatePath << <Grid2, Block2 >> >(DevPath, DevEachPathCount, RobotCount, PathPitch, ObstacleCount);
			//	error = cudaGetLastError();
			//	cout << "ReCalcPathKernell iter " << i << ": " << error << "\n";
		}
		//cudaMemcpy2D(finalPath, PathCount * sizeof(float), DevPath, PathPitch * sizeof(float), PathCount * sizeof(float), 2, cudaMemcpyDeviceToHost);
		for (int i = 0; i < RobotCount; i++)
		{
			error = cudaMemcpy2D(finalPath + i * 2 * MaxPathCount, 2 * MaxPathCount * sizeof(float), DevPath + i * PathPitch, PathPitch * sizeof(float), 2 * eachPathCount[i] * sizeof(float), 1, cudaMemcpyDeviceToHost);
			//		cout << "finalPathMemCpy for Robot" << i << ": "<< error << "\n";
		}
		//
		//	error = cudaDeviceSynchronize();
		//	cout << "Sync: " << error << "\n";
	}
	//cuEventRecord(stop, 0);
	//cuEventSynchronize(stop);
	//float time;
	//cuEventElapsedTime(&time, start, stop);
	//cudaEventDestroy(start);
	//cudaEventDestroy(stop);
	return 1;
}
extern void ShutDown()
{
	DisposeGamePlanner();
	//DisposeElastic();
}



__global__ void GaussianKernel(float* DataX, int DataXLen0, float* DataY, int DataYLen0, float* EachDataCount, int EachDataCountLen0, int RegionCount, float* opps, int oppsLen0, int RobotCount, float sigmax, float sigmay, int maxCount, float* DevPhi, int DevPhiLen0, float* DevKdx, int DevKdxLen0, float* DevKdy, int DevKdyLen0)
{
	int x = blockIdx.x;
	int x2 = threadIdx.x;
	int y = threadIdx.y;
	if (x < RobotCount && x2 < RegionCount && (float)y < EachDataCount[(x2)])
	{
		float num = DataX[(x2 * maxCount + y)];
		float num2 = DataY[(x2 * maxCount + y)];
		float num3 = expf(-(opps[(x)] - num) * (opps[(x)] - num) / sigmax - (opps[(x + 6)] - num2) * (opps[(x + 6)] - num2) / sigmay);
		DevKdx[(x * RegionCount * maxCount + x2 * maxCount + y)] = -2 * ((opps[(x)] - num) / sigmax) * num3;
		DevKdy[(x * RegionCount * maxCount + x2 * maxCount + y)] = -2 * ((opps[(x + 6)] - num2) / sigmay) * num3;
		DevPhi[(x * RegionCount * maxCount + x2 * maxCount + y)] = num3;
	}
}
__global__ void MultiObjectKernel(GObjectState Ball, float maxBallDeccel, float* RobotTimes, int RobotTimesLen0, int RobotTimesLen1, int N, GPosition2D* Heads, int HeadsLen0, int HeadsLen1, GPosition2D* Tails, int TailsLen0, int TailsLen1, float* TimesHeads, int TimesHeadsLen0, int TimesHeadsLen1, float* TimesTails, int TimesTailsLen0, int TimesTailsLen1, int* histo, int histoLen0, int histoLen1)
{
	int num = blockIdx.x * blockDim.x + threadIdx.x;
	int y = blockIdx.y;
	int x = threadIdx.x;
	bool flag = false;
	bool flag2 = false;
	bool flag3 = false;
	__shared__ bool sharedarray[257];

	int arrayLen0 = 257;
	GVector2D gVector2D = GVector2D(0, 0);
	if (num < N)
	{
		float num2 = (float)(num * 10) / 100;
		float num3 = Ball.Speed.Size();
		float num4 = num3 / maxBallDeccel;
		float num5 = num3 * num3 / (2 * maxBallDeccel);
		GPosition2D gPosition2D = Ball.Location.Add(Ball.Speed.GetNormalizeToCopy(num2));
		float num6 = RobotTimes[(y)* RobotTimesLen1 + (num)];
		if (fabsf(num2 - num5) <= 0.1)
		{
			flag = true;
		}
		else
		{
			float num7 = num3 * num3 - 2 * maxBallDeccel * num2;
			if (num7 >= 0)
			{
				float num8 = (sqrtf(num7) - num3) / -maxBallDeccel;
				flag = (num6 <= num8);
			}
			else
			{
				flag = false;
			}
		}
		if (x == 0 && flag)
		{
			flag2 = true;
		}
		sharedarray[(x)] = flag;
		if (x == 255 || num == N - 1)
		{
			sharedarray[(x + 1)] = false;
		}
		__syncthreads();
		bool flag4 = sharedarray[(x + 1)];
		__syncthreads();
		if (flag && !flag4)
		{
			flag3 = true;
		}
		if (!flag && flag4)
		{
			flag2 = true;
		}
		if (flag2)
		{
			int num9 = atomicAdd(&histo[(y)* histoLen1 + (0)], 1);

			if (num9 < 4)
			{
				Heads[(y)* HeadsLen1 + (num9)] = gPosition2D;

				TimesHeads[(y)* TimesHeadsLen1 + (num9)] = num6;
			}
		}
		if (flag3)
		{
			int num9 = atomicAdd(&histo[(y)* histoLen1 + (1)], 1);
			if (num9 < 4)
			{
				Tails[(y)* TailsLen1 + (num9)] = gPosition2D;
				TimesTails[(y)* TimesTailsLen1 + (num9)] = num6;
			}
		}
	}
}
__global__ void ClaculateTimeKernel(GObjectState Ball, GObjectState* States, int StatesLen0, int N, float maxRobotSpeed, float maxBallDeccel, float maxRobotAccel, float* MinTime, int MinTimeLen0, int MinTimeLen1)
{
	__shared__ float array[8 * 32];

	int arrayLen0 = 8;
	int arrayLen1 = 32;
	int num = blockIdx.x * blockDim.x + threadIdx.x;
	GVector2D gVector2D = GVector2D(0, 0);
	if (num < N)
	{
		int i = blockDim.y;
		float a_max = fmaxf(0.01, maxRobotAccel * cosf((float)threadIdx.y * 3.141593 / 62));
		float a_max2 = fmaxf(0.01, maxRobotAccel * sinf((float)threadIdx.y * 3.141593 / 62));
		gVector2D = Ball.Location.Add(Ball.Speed.GetNormalizeToCopy((float)(num * 10) / 100)).Sub(States[(blockIdx.y)].Location);
		if (gVector2D.Size() >= 0.09)
		{
			gVector2D.NormalizeTo(gVector2D.Size() - 0.09);
		}
		else
		{
			gVector2D.X = (gVector2D.Y = 0);
		}
		float num2 = CalculateTime(gVector2D.X, States[(blockIdx.y)].Speed.X, maxRobotSpeed, a_max);
		float num3 = CalculateTime(gVector2D.Y, States[(blockIdx.y)].Speed.Y, maxRobotSpeed, a_max2);
		array[(threadIdx.x) * arrayLen1 + (threadIdx.y)] = ((num2 > num3) ? num2 : num3);
		__syncthreads();
		while (i > 1)
		{
			i >>= 1;
			if (threadIdx.y < i)
			{
				if (array[(threadIdx.x) * arrayLen1 + (threadIdx.y + i)] < array[(threadIdx.x) * arrayLen1 + (threadIdx.y)])
				{
					array[(threadIdx.x) * arrayLen1 + (threadIdx.y)] = array[(threadIdx.x) * arrayLen1 + (threadIdx.y + i)];
				}
			}
			__syncthreads();
		}
		__syncthreads();
		if (threadIdx.y == 0)
		{
			MinTime[(blockIdx.y) * MinTimeLen1 + (num)] = array[(threadIdx.x) * arrayLen1 + (0)];
		}
	}
}
__device__ float CalculateTime(float dR, float v0, float v_max, float a_max)
{
	float num = 0;
	float num2 = sign(dR);
	float result;
	if (dR == 0)
	{
		if (fabsf(v0) <= 0.02)
		{
			result = 0;
			return result;
		}
		float num3 = a_max * -sign(v0);
		float num4 = -v0 / num3;
		float num5 = -v0 * v0 / (2 * num3);
		dR -= num5;
		v0 = 0;
		float num6 = v_max / a_max;
		float value = v_max * v_max / num3;
		float num7;
		if (fabsf(value) <= fabsf(dR))
		{
			num7 = 2 * num6 + (fabsf(dR) - fabsf(value)) / v_max;
		}
		else
		{
			num7 = 2 * sqrtf(num3 * dR) / a_max;
		}
		num = num4 + num7;
	}
	else
	{
		float num8 = sign(v0 * dR);
		if (num8 < 0)
		{
			float num3 = a_max * sign(dR);
			float num4 = -v0 / num3;
			float num5 = -v0 * v0 / (2 * num3);
			dR -= num5;
			v0 = 0;
			float num6 = v_max / a_max;
			float value = v_max * v_max / num3;
			float num7;
			if (fabsf(value) <= fabsf(dR))
			{
				num7 = 2 * num6 + (fabsf(dR) - fabsf(value)) / v_max;
			}
			else
			{
				num7 = 2 * sqrtf(num3 * dR) / a_max;
			}
			num = num4 + num7;
		}
		else
		{
			float num3 = -sign(dR) * a_max;
			float num4 = -v0 / num3;
			float num5 = -v0 * v0 / (2 * num3);
			if (fabsf(num5) > fabsf(dR))
			{
				dR -= num5;
				v0 = 0;
				num3 = sign(dR) * a_max;
				float num6 = v_max / a_max;
				float value = v_max * v_max / num3;
				float num7;
				if (fabsf(value) <= fabsf(dR))
				{
					num7 = 2 * num6 + (fabsf(dR) - fabsf(value)) / v_max;
				}
				else
				{
					num7 = 2 * sqrtf(num3 * dR) / a_max;
				}
				num = num7 + num4;
			}
			else
			{
				num3 = sign(dR) * a_max;
				float num9 = sign(dR) * v_max;
				float value = (2 * num9 * num9 - v0 * v0) / (2 * num3);
				float num6 = (2 * num9 - v0) / num3;
				float num7;
				if (fabsf(value) <= fabsf(dR))
				{
					num7 = num6 + (fabsf(dR) - fabsf(value)) / v_max;
				}
				else
				{
					float num10 = sign(dR) * sqrtf(num3 * dR + v0 * v0 / 2);
					num7 = (2 * num10 - v0) / num3;
				}
				num = num7;
			}
		}
	}
	result = num;
	return result;
}
__device__ float sign(float x)
{
	return (x == 0) ? 0 : (x / fabsf(x));
}

void GamePlannerInit(float maxRobotAccel, float maxRobotSpeed, float ballDecel, float* DataScoreX, float* DataScoreY, float* eacheRegionCount, int RegionCount, int maxSampleCount, float sigmaX, float sigmaY)
{
	_maxRobotAccel = maxRobotAccel;
	_maxRobotSpeed = maxRobotSpeed;
	_ballDecel = ballDecel;

	pHeads = new GPosition2D[2 * maxRobotCount * maxLines];
	pTails = new GPosition2D[2 * maxRobotCount * maxLines];

	error = cudaMalloc((void**)&devHeads2D, 2 * maxRobotCount * sizeof(GPosition2D) * maxLines);
	cout << "AllocHeads: " << error << "\n";
	error = cudaMalloc((void**)&devTails2D, 2 * maxRobotCount * sizeof(GPosition2D) * maxLines);
	cout << "AllocTails: " << error << "\n";
	error = cudaMalloc((void**)&devHisto2D, 2 * maxRobotCount * sizeof(int) * 2);
	cout << "AllocHisto: " << error << "\n";
	error = cudaMalloc((void**)&devtimehead2D, 2 * maxRobotCount * sizeof(float) * maxLines);
	cout << "AlloctimeHead: " << error << "\n";
	error = cudaMalloc((void**)&devtimetail2D, 2 * maxRobotCount * sizeof(float) * maxLines);
	cout << "AlloctimeTail: " << error << "\n";
	error = cudaMalloc((void**)&devMinTime2D, 2 * maxRobotCount * sizeof(float) * maxPoints);
	cout << "AllocMintime: " << error << "\n";
	error = cudaMalloc((void**)&states, 2 * maxRobotCount * sizeof(GObjectState));
	cout << "AllocStates: " << error << "\n";
	zeroArray = new int[2 * maxRobotCount * 2];
	for (int i = 0; i < 2 * maxRobotCount * 2; i++)
		zeroArray[i] = 0;

	_sigmaX = sigmaX;
	_sigmaY = sigmaY;
	_RegionCount = RegionCount;
	_maxSampleCount = maxSampleCount;
	error = cudaMalloc((void**)&DevDataX, RegionCount * maxSampleCount * sizeof(float));
	cout << "AllocDataX: " << error << "\n";
	error = cudaMalloc((void**)&DevDataY, RegionCount * maxSampleCount * sizeof(float));
	cout << "AllocDataY: " << error << "\n";
	error = cudaMalloc((void**)&DevPhi, RegionCount * maxSampleCount * maxRobotCount * sizeof(float));
	cout << "AllocPhi: " << error << "\n";
	error = cudaMalloc((void**)&DevKdx, RegionCount * maxSampleCount * maxRobotCount * sizeof(float));
	cout << "AllocKdx: " << error << "\n";
	error = cudaMalloc((void**)&DevKdy, RegionCount * maxSampleCount * maxRobotCount * sizeof(float));
	cout << "AllocKdy: " << error << "\n";
	error = cudaMalloc((void**)&DevEachDataCount, RegionCount * sizeof(float));
	cout << "AllocEachDataCount: " << error << "\n";
	error = cudaMalloc((void**)&DevRobots, 2 * maxRobotCount * sizeof(float));
	cout << "AllocRobots: " << error << "\n";
	for (int i = 0; i < RegionCount; i++)
	{
		error = cudaMemcpy(DevDataX + i * maxSampleCount, DataScoreX + i * maxSampleCount, eacheRegionCount[i] * sizeof(float), cudaMemcpyHostToDevice);
		cout << "memcpyDataX " << i << ": " << error << "\n";
		error = cudaMemcpy(DevDataY + i * maxSampleCount, DataScoreY + i * maxSampleCount, eacheRegionCount[i] * sizeof(float), cudaMemcpyHostToDevice);
		cout << "memcpyDataY " << i << ": " << error << "\n";
	}
	error = cudaMemcpy(DevEachDataCount, eacheRegionCount, RegionCount * sizeof(float), cudaMemcpyHostToDevice);
	cout << "memcpyEachRegionCount: " << error << "\n";
}
void CalculateBallState(GObjectState* robots, int RobotCounts, int N, GObjectState ball, float* Heads, float* Tails, float* TimeHeads, float* TimeTails, int* histo)
{
	if (RobotCounts>0)
	{
		error = cudaMemcpy(states, robots, RobotCounts * sizeof(GObjectState), cudaMemcpyHostToDevice);
		//cout << "MemcpyState: "<< error << "\n";
		error = cudaMemcpy(devHisto2D, zeroArray, 2 * maxRobotCount * sizeof(int) * 2, cudaMemcpyHostToDevice);
		//cout << "MemcpyZeroArray: "<< error << "\n";
		dim3 block((threadsPerBlock + AccelSteps) / (AccelSteps + 1), AccelSteps + 1);
		dim3 grid((N + block.x - 1) / block.x, RobotCounts);
		ClaculateTimeKernel << <grid, block >> >(ball, states, RobotCounts, N, _maxRobotSpeed, _ballDecel, _maxRobotAccel, devMinTime2D, 2 * maxRobotCount, maxPoints);
		//	error = cudaGetLastError();
		//cout << "CalculateTime: "<< error << "\n";
		//	error = cudaDeviceSynchronize();
		//	cout << "Sync: "<< error << "\n";
		grid = dim3((threadsPerBlock + N - 1) / threadsPerBlock, RobotCounts);
		block = dim3(threadsPerBlock);



		MultiObjectKernel << <grid, block >> >(ball, _ballDecel, devMinTime2D, 2 * maxRobotCount, maxPoints, N, devHeads2D, 2 * maxRobotCount, maxLines, devTails2D, 2 * maxRobotCount, maxLines, devtimehead2D, 2 * maxRobotCount, maxLines, devtimetail2D, 2 * maxRobotCount, maxLines, devHisto2D, 2 * maxRobotCount, 2);

		//error = cudaGetLastError();
		//cout << "MultiObj: "<< error << "\n";
		//error = cudaDeviceSynchronize();
		//	cout << "Sync: "<< error << "\n";

		error = cudaMemcpy(pHeads, devHeads2D, 2 * maxRobotCount * maxLines * sizeof(GPosition2D), cudaMemcpyDeviceToHost);
		//	cout << "memcpyheads: "<< error << "\n";
		error = cudaMemcpy(pTails, devTails2D, 2 * maxRobotCount * maxLines * sizeof(GPosition2D), cudaMemcpyDeviceToHost);
		//	cout << "memcpytails: "<< error << "\n";
		error = cudaMemcpy(TimeHeads, devtimehead2D, 2 * maxRobotCount * maxLines * sizeof(float), cudaMemcpyDeviceToHost);
		//	cout << "memcpytimeheads: "<< error << "\n";
		error = cudaMemcpy(TimeTails, devtimetail2D, 2 * maxRobotCount * maxLines * sizeof(float), cudaMemcpyDeviceToHost);
		//	cout << "memcpytimetails: "<< error << "\n";
		error = cudaMemcpy(histo, devHisto2D, 2 * RobotCounts * sizeof(int), cudaMemcpyDeviceToHost);
		//	cout << "memcpyhisto: "<< error << "\n";
		for (int i = 0; i < 2 * maxRobotCount * maxLines; i++)
		{
			Heads[i * 2] = pHeads[i].X;
			Heads[i * 2 + 1] = pHeads[i].Y;
			Tails[i * 2] = pTails[i].X;
			Tails[i * 2 + 1] = pTails[i].Y;
		}
	}
}
void DisposeGamePlanner()
{
	cudaFree(devHeads2D);
	cudaFree(devTails2D);
	cudaFree(devtimehead2D);
	cudaFree(devtimetail2D);
	cudaFree(devHisto2D);
	cudaFree(devMinTime2D);
	cudaFree(states);

	cudaFree(DevDataX);
	cudaFree(DevDataY);
	cudaFree(DevPhi);
	cudaFree(DevKdx);
	cudaFree(DevKdy);
	cudaFree(DevEachDataCount);
	free(zeroArray);
}


__device__ GPosition2D  Meet(GPosition2D P, float Ox, float Oy, float obstacleRadi)
{
	float size = (P.X - Ox) * (P.X - Ox) + (P.Y - Oy) * (P.Y - Oy);
	if (size < obstacleRadi * obstacleRadi)
	{
		if (size > 1E-5)
		{
			size = sqrtf(size);
			P.X = (P.X - Ox) * obstacleRadi / size;
			P.Y = (P.Y - Oy) * obstacleRadi / size;
		}
		else
		{
			P.X = Ox + obstacleRadi;
			P.Y = Oy + obstacleRadi;
		}
	}
	return P;
}
__device__ GVector2D MeetCircle(float Ox, float Oy, GVector2D F, float Px, float Py, float R)
{
	float Vx, Vy/*, Vx1, Vy1, Vx2, Vy2*/, tmp/*, tmp2*/;
	/*float f, l, d;
	l = F.X * F.X + F.Y * F.Y;
	f = F.X * Ox - F.X * Px + F.Y * Oy - F.Y * Py;
	if (f <= 0.0)
	d = (Px - Ox)*(Px - Ox) + (Py - Oy)*(Py - Oy);
	else if (f >= l)
	d = (Px + F.X - Ox) * (Px + F.X - Ox) + (Py + F.Y - Oy) * (Py + F.Y - Oy);
	else
	d = (Px + F.X * (f / l) - Ox) * (Px + F.X * (f / l) - Ox) + (Py + F.Y * (f / l) - Oy) * (Py + F.Y * (f / l) - Oy);*/
	Vx = Px + F.X - Ox;
	Vy = Py + F.Y - Oy;
	tmp = Vx * Vx + Vy * Vy;
	if (tmp < R * R)
	{

		if (tmp > 1E-5)
		{
			Vx *= (R / sqrtf(tmp));
			Vy *= (R / sqrtf(tmp));
		}
		else
		{
			Vx = Vy = 0;
		}
		F.X = Ox + Vx - Px;
		F.Y = Oy + Vy - Py;
		/*Vx = Ox - Px;
		Vy = Oy - Py;
		tmp = sqrtf(Vx * Vx + Vy * Vy);
		Vx *= (tmp == 0)? 0: (R / tmp);
		Vy *= (tmp == 0)? 0: (R / tmp);
		tmp = sqrtf(F.X * F.X + F.Y * F.Y) * 0.01;
		if(Vy == 0 && Vx == 0)
		{
		Vx1 = -F.X;
		Vy1 = -F.Y;
		Vx2 = -F.X;
		Vy2 = -F.Y;
		}
		else if(Vy == 0)
		{
		Vx1 = 0;
		Vy1 = tmp ;
		Vx2 = 0;
		Vy2 = -tmp;
		}
		else if(Vx == 0)
		{
		Vx1 = tmp;
		Vy1 = 0;
		Vx2 = -tmp;
		Vy2 = 0;
		}
		else
		{
		tmp2 = tmp / sqrtf(1 + (Vx * Vx)/(Vy * Vy));
		Vx1 = tmp2;
		Vy1 = -(Vx / Vy) * tmp2;
		Vx2 = -tmp2;
		Vy2 = (Vx / Vy) * tmp2;
		}
		if(Vx1 * F.X + Vy1 * F.Y >= 0)
		{
		F.X = Vx1;
		F.Y = Vy1;
		}
		else
		{
		F.X = Vx2;
		F.Y = Vy2;
		}*/
	}
	return F;
}
__global__ void CalculateForcesKernel(float* DevForce, int* DevEachPathCount, int RobotCount, size_t forcePitch, float kSpring, float kSpring2, int n)
{
	int i = blockIdx.x * blockDim.x + threadIdx.x;
	int j = threadIdx.y;
	int k = blockIdx.y;
	int  thisPathCount;
	thisPathCount = DevEachPathCount[k];
	if (i < thisPathCount && j < 2 && k < RobotCount)
	{
		float tmpCurr = tex2D(texB, (float)(2 * i + j), (float)k);
		float tmpNext = tex2D(texB, (float)(2 * (i - 1) + j), (float)k);
		float tmpPrev = tex2D(texB, (float)(2 * (i + 1) + j), (float)k);
		/*	float tmpNnext, tmpNprev;
		if(i  < n)
		tmpNprev = tex2D(texB, (float)j, (float)k);
		else
		tmpNprev = tex2D(texB, (float)2 * (i - n) + j, (float)k);
		if(i >= thisPathCount - n)
		tmpNnext = tex2D(texB, (float)2 * (thisPathCount - 1) + j,(float)k);
		else
		tmpNnext = tex2D(texB, (float)2 * (i + n) + j,(float)k);*/

		if (i == thisPathCount - 1 || i == 0)
		{
			DevForce[k * forcePitch + i * 2 + j] = 0;
		}
		else
			DevForce[k * forcePitch + i * 2 + j] = kSpring * ((tmpNext - tmpCurr) + (tmpPrev - tmpCurr));// + kSpring2 * ((tmpNnext - tmpCurr) + (tmpNprev - tmpCurr)) ;
	}
}
__global__ void ReCalculatePath(float* DevPath, int* DevEachPathCount, int RobotCount, size_t PathPitch, int ObstacleCount)
{
	int i = blockIdx.x*blockDim.x + threadIdx.x;
	int k = blockIdx.y;
	int  thisPathCount;

	thisPathCount = DevEachPathCount[k];
	GVector2D F;
	GPosition2D P;
	if (i < thisPathCount && k < RobotCount)
	{
		F = GVector2D(tex2D(texF, (float)2 * i, k), tex2D(texF, (float)2 * i + 1, k));
		P = GPosition2D(tex2D(texB, (float)2 * i, k), tex2D(texB, (float)2 * i + 1, k));
		int ab = tex2D(texV, (float)k, 0);
		int az = tex2D(texV, (float)k, 1);
		int ar = tex2D(texV, (float)k, 3);
		int aoz = tex2D(texV, (float)k, 2);
		for (int j = 0; j < ObstacleCount; j++)
		{
			//O = GPosition2D(tex2D(texC, (float)j, 0), tex2D(texC, (float)j, 1));
			if (i != thisPathCount - 1 && i != 0)
			{
				if (ab == 1 && j == 0)
				{
					F = MeetCircle(tex2D(texC, (float)j, 0), tex2D(texC, (float)j, 1), F, P.X, P.Y, BALL_FORCE);
				}
				else if (az == 1 && j > 0 && j < 4)
				{
					F = MeetCircle(tex2D(texC, (float)j, 0), tex2D(texC, (float)j, 1), F, P.X, P.Y, ZONE_FORCE);
				}
				else if (aoz == 1 && j > 3 && j < 7)
				{
					F = MeetCircle(tex2D(texC, (float)j, 0), tex2D(texC, (float)j, 1), F, P.X, P.Y, OPP_ZONE_FORCE);
				}
				else if (ar == 1 && j > 6 && !(((j - 7) < (RobotCount / 2)) && ((k == (j - 7)) || ((k - (RobotCount / 2)) == (j - 7)))))
				{
					F = MeetCircle(tex2D(texC, (float)j, 0), tex2D(texC, (float)j, 1), F, P.X, P.Y, ROBOT_FORCE);
				}
			}
			/*if(i < thisPathCount - 1 && !(((j - 4) < (RobotCount / 2)) && ((k == (j - 4)) || ((k - (RobotCount / 2)) == (j - 4)))))
			P = Meet(P, tex2D(texC, (float)j, 0), tex2D(texC, (float)j, 1), R );*/
		}
		DevPath[k * PathPitch + 2 * i] = P.X + F.X;
		DevPath[k * PathPitch + 2 * i + 1] = P.Y + F.Y;
	}
}

void ElasticInit(int maxPathCount, int maxRRTCount)
{

	MaxPathCount = maxPathCount;
	MaxRRTCount = maxRRTCount;
	cudaChannelFormatDesc channelDescA = cudaCreateChannelDesc<float>();
	cudaChannelFormatDesc channelDescB = cudaCreateChannelDesc<float>();
	cudaChannelFormatDesc channelDescF = cudaCreateChannelDesc<float>();
	cudaChannelFormatDesc channelDescV = cudaCreateChannelDesc<float>();

	cudaError_t error;
	size_t path_pitch_in_byte, force_pitch_in_byte, obs_pitch_in_byte, avoid_pitch_in_byte;

	error = cudaMallocPitch((void**)&DevPath, &path_pitch_in_byte, 2 * MaxPathCount * sizeof(float), MaxRRTCount);
	cout << "PathMalloc: " << error << "\n";
	error = cudaMallocPitch((void**)&DevForce, &force_pitch_in_byte, 2 * MaxPathCount * sizeof(float), MaxRRTCount);
	cout << "ForceMalloc: " << error << "\n";
	error = cudaMallocPitch((void**)&DevObs, &obs_pitch_in_byte, MAX_OBS_COUNT * sizeof(float), 2);
	cout << "ObsMalloc: " << error << "\n";
	error = cudaMallocPitch((void**)&DevAvoid, &avoid_pitch_in_byte, MaxRRTCount * sizeof(float), 4);
	cout << "AvoidMalloc: " << error << "\n";
	error = cudaMalloc((void**)&DevEachPathCount, MaxRRTCount * sizeof(int));
	cout << "EachPathCountMalloc: " << error << "\n";


	PathPitch = path_pitch_in_byte / sizeof(float);
	ForcePitch = force_pitch_in_byte / sizeof(float);
	ObsPitch = obs_pitch_in_byte / sizeof(float);
	AvoidPitch = avoid_pitch_in_byte / sizeof(float);

	texB.addressMode[0] = cudaAddressModeClamp;
	texB.addressMode[1] = cudaAddressModeClamp;
	texB.filterMode = cudaFilterModePoint;
	texB.normalized = 0;

	texC.addressMode[0] = cudaAddressModeClamp;
	texC.addressMode[1] = cudaAddressModeClamp;
	texC.filterMode = cudaFilterModePoint;
	texC.normalized = 0;

	texF.addressMode[0] = cudaAddressModeClamp;
	texF.addressMode[1] = cudaAddressModeClamp;
	texF.filterMode = cudaFilterModePoint;
	texF.normalized = 0;

	texV.addressMode[0] = cudaAddressModeClamp;
	texV.addressMode[1] = cudaAddressModeClamp;
	texV.filterMode = cudaFilterModePoint;
	texV.normalized = 0;

	error = cudaBindTexture2D(NULL, &texB, DevPath, &channelDescA, MaxPathCount * 2, MaxRRTCount, path_pitch_in_byte);
	cout << "BindPath: " << error << "\n";
	error = cudaBindTexture2D(NULL, &texC, DevObs, &channelDescB, MAX_OBS_COUNT, 2, obs_pitch_in_byte);
	cout << "BindObs: " << error << "\n";
	error = cudaBindTexture2D(NULL, &texF, DevForce, &channelDescF, MaxPathCount * 2, MaxRRTCount, force_pitch_in_byte);
	cout << "BindForce: " << error << "\n";
	error = cudaBindTexture2D(NULL, &texV, DevAvoid, &channelDescV, MaxRRTCount, 4, avoid_pitch_in_byte);
	cout << "BindAvoid: " << error << "\n";
}
void DisposeElastic()
{
	cudaUnbindTexture(texB);
	cudaUnbindTexture(texC);
	cudaUnbindTexture(texF);
	cudaUnbindTexture(texV);
	cudaFree(DevForce);
	cudaFree(DevPath);
	cudaFree(DevAvoid);
	cudaFree(DevObs);
	cudaFree(DevEachPathCount);
}

