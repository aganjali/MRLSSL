#pragma once
#include <math.h>
#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include "cuda.h"

enum GObjectType
{
	Ball,
	OurRobot,
	Opponent
};
struct GVector2D
{
	__host__ __device__ GVector2D()
	{
	}
	float X;
	float Y;
	__host__ __device__ GVector2D(float x, float y)
	{
		X = x;
		Y = y;
	}
	__host__ __device__ GVector2D(GVector2D *vec)
	{
		X = vec[0].X;
		Y = vec[0].Y;
	}
	__host__ __device__ float Size()
	{
		return sqrtf(X * X + Y * Y);
	}
	__host__ __device__ float SquareSize()
	{
		return X * X + Y * Y;
	}
	__host__ __device__ bool Scale(float a)
	{
		X *= a;
		Y *= a;
		return true;
	}
	__host__ __device__ bool Normnalize()
	{
		float num = Size();
		if (num == 0)
		{
			X = (Y = 0);
		}
		else
		{
			X /= num;
			Y /= num;
		}
		return true;
	}
	__host__ __device__ bool NormalizeTo(float NewLength)
	{
		float num = Size();
		if (num == 0)
		{
			X = (Y = 0);
		}
		else
		{
			X *= NewLength / num;
			Y *= NewLength / num;
		}
		return true;
	}
	__host__ __device__ GVector2D GetNormnalizedCopy()
	{
		GVector2D result = GVector2D(X, Y);
		result.Normnalize();
		return result;
	}
	__host__ __device__ GVector2D GetNormalizeToCopy(float NewLength)
	{
		GVector2D result = GVector2D(X, Y);
		result.NormalizeTo(NewLength);
		return result;
	}
	__host__ __device__ float AngleBetweenInRadians(GVector2D P1)
	{
		float num = (float)atan2((double)P1.Y, (double)P1.X);
		float num2 = (float)atan2((double)Y, (double)X);
		float num3;
		for (num3 = num - num2; num3 > 3.14159274; num3 -= 6.28318548)
		{
		}
		while (num3 < -3.14159274)
		{
			num3 += 6.28318548;
		}
		return num3;
	}
	__host__ __device__ float AngleBetweenInDegrees(GVector2D P1)
	{
		return AngleBetweenInRadians(P1) * 180 / 3.141593;
	}
	__host__ __device__ float InnerProduct(GVector2D V)
	{
		return X * V.X + Y * V.Y;
	}
	__host__ __device__ GVector2D FromAngleSize(float Angle, float Size)
	{
		return GVector2D(Size * cosf(Angle), Size * sinf(Angle));
	}
	__host__ __device__ float AngleInRadians()
	{
		return atan2f(Y, X);
	}
	__host__ __device__ float AngleInDegrees()
	{
		return AngleInRadians() * 180 / 3.141593;
	}
	__host__ __device__ GVector2D Add(GVector2D v1)
	{
		return GVector2D(X + v1.X, Y + v1.Y);
	}
	__host__ __device__ GVector2D Sub(GVector2D v1)
	{
		return GVector2D(X - v1.X, Y - v1.Y);
	}
	__host__ __device__ float Distance(float x, float y)
	{
		float x2 = x + X;
		float y2 = y + Y;
		float A = y2 - y;
		float B = x - x2;
		float C = -(A * x + B * y);
		if(X == 0 && Y == 0)
			return 0;
		return abs(A * x + B * y + C) / sqrtf((A * A + B * B));
	}

};

struct GPosition2D
{
	__host__ __device__ GPosition2D()
	{
	}
	float X;
	float Y;
	__host__ __device__ GPosition2D(float x, float y)
	{
		X = x;
		Y = y;
	}
	__host__ __device__ GPosition2D(GPosition2D *pos)
	{
		X = pos[0].X;
		Y = pos[0].Y;
	}
	__host__ __device__ float Size()
	{
		return (float)sqrt((double)(X * X + Y * Y));
	}
	__host__ __device__ float SquareSize()
	{
		return X * X + Y * Y;
	}
	__host__ __device__ float DistanceFrom(GPosition2D From)
	{
		return sqrtf(((X - From.X) * (X - From.X) + (Y - From.Y) * (Y - From.Y)));
	}
	__host__ __device__ GPosition2D Add(GVector2D p1)
	{
		return GPosition2D(X + p1.X, Y + p1.Y);
	}
	__device__ GVector2D Sub(GPosition2D p1)
	{
		return GVector2D(X - p1.X, Y - p1.Y);
	}
	__host__ __device__ GPosition2D Interpolate(GPosition2D End, float Amount)
	{
		return GPosition2D(X * (1 - Amount) + End.X * Amount, Y * (1 - Amount) + End.Y * Amount);
	}
};

struct GLine
{
	__device__ GLine()
	{
	}
	float C;
	float B;
	float A;
	GPosition2D Head;
	GPosition2D Tail;
	__device__ float Angle()
	{
		return (float)atan2((double)B, (double)A);
	}
	__device__ GLine(GPosition2D P1, GPosition2D P2)
	{
		A = P2.Y - P1.Y;
		B = P1.X - P2.X;
		C = -(A * P1.X + B * P1.Y);
		Head = P1;
		Tail = P2;
	}
	__device__ GLine(GLine *l)
	{
		A = l[0].A;
		B = l[0].B;
		C = l[0].C;
		Head = l[0].Head;
		Tail = l[0].Tail;
	}
	__device__ GLine(float a, float b, float c)
	{
		A = a;
		B = b;
		C = c;
		Tail = GPosition2D(0, -C / A);
		Head = GPosition2D(0, -C / A);
	}
	__device__ float Distance(GPosition2D P)
	{
		return abs(A * P.X + B * P.Y + C) / sqrtf((A * A + B * B));
	}
	__device__ GPosition2D CalculateY(float x)
	{
		GPosition2D result = GPosition2D(0, 0);
		result.X = x;
		result.Y = (-C - A * x) / B;
		return result;
	}
	__device__ GPosition2D IntersectWithLine(GLine l2, bool *HasValue)
	{
		GPosition2D result = GPosition2D(0, 0);
		float num = A * l2.B - l2.A * B;
		if (fabsf(num) > 0.0001)
		{
			result.X = (l2.C * B - C * l2.B) / num;
			result.Y = (C * l2.A - l2.C * A) / num;
			HasValue[0] = true;
		}
		else
		{
			HasValue[0] = false;
		}
		return result;
	}
	__device__ GLine PerpenducilarLineToPoint(GPosition2D From)
	{
		return GLine(B, -A, -(B * From.X + -A * From.Y));
	}
	__device__ bool IsPointInRange(GPosition2D P)
	{
		GVector2D gVector2D = P.Sub(Head);
		bool result;
		if (gVector2D.InnerProduct(Tail.Sub(Head)) < 0)
		{
			result = false;
		}
		else
		{
			gVector2D = P.Sub(Tail);
			result = (gVector2D.InnerProduct(Head.Sub(Tail)) >= 0);
		}
		return result;
	}
	
};

struct GPartLine
{
	__device__ GPartLine()
	{
	}
	GPosition2D _tail;
	GPosition2D _head;
	float _a;
	float _b;
	float _c;
	__device__ GPosition2D GetTail()
	{
		return _tail;
	}
	__device__ void SetTail(GPosition2D tail)
	{
		_tail = tail;
		CalculateLineParameters();
	}
	__device__ GPosition2D GetHead()
	{
		return _head;
	}
	__device__ void SetHead(GPosition2D head)
	{
		_head = head;
		CalculateLineParameters();
	}
	__device__ float GetA()
	{
		return _a;
	}
	__device__ float GetB()
	{
		return _b;
	}
	__device__ float GetC()
	{
		return _c;
	}
	__device__ float Angle()
	{
		return (float)atan2((double)_b, (double)_a);
	}
	__device__ GPartLine(GPosition2D P1, GPosition2D P2)
	{
		_head = P1;
		_tail = P2;
		_a = P2.Y - P1.Y;
		_b = P1.X - P2.X;
		_c = -(_a * P1.X + _b * P1.Y);
	}
	__device__ bool IntersectsWithPerpendicularFrom(GPosition2D P)
	{
		GVector2D gVector2D = P.Sub(_head);
		bool arg_61_0;
		if (gVector2D.InnerProduct(_tail.Sub(_head)) >= 0)
		{
			gVector2D = P.Sub(_tail);
			arg_61_0 = (gVector2D.InnerProduct(_head.Sub(_tail)) >= 0);
		}
		else
		{
			arg_61_0 = false;
		}
		return arg_61_0;
	}
	__device__ GPartLine(GPartLine *PL)
	{
		_head = PL[0]._head;
		_tail = PL[0]._tail;
		_a = PL[0]._a;
		_b = PL[0]._b;
		_c = PL[0]._c;
	}
	__device__ void CalculateLineParameters()
	{
		_a = _tail.Y - _head.Y;
		_b = _head.X - _tail.X;
		_c = -(_a * _head.X + _b * _tail.Y);
	}
	__device__ float Distance(GPosition2D P)
	{
		GVector2D gVector2D = P.Sub(_head);
		float result;
		if (gVector2D.InnerProduct(_tail.Sub(_head)) < 0)
		{
			gVector2D = P.Sub(_head);
			result = gVector2D.Size();
		}
		else
		{
			gVector2D = P.Sub(_tail);
			if (gVector2D.InnerProduct(_head.Sub(_tail)) < 0)
			{
				gVector2D = P.Sub(_tail);
				result = gVector2D.Size();
			}
			else
			{
				GLine gLine = GLine(_head, _tail);
				result = gLine.Distance(P);
			}
		}
		return result;
	}
	__device__ float DistanceFromLine(GPosition2D P)
	{
		GLine gLine = GLine(_head, _tail);
		return gLine.Distance(P);
	}
	__device__ bool IsPointInRange(GPosition2D P)
	{
		GVector2D gVector2D = P.Sub(_head);
		bool result;
		if (gVector2D.InnerProduct(_tail.Sub(_head)) < 0)
		{
			result = false;
		}
		else
		{
			gVector2D = P.Sub(_tail);
			result = (gVector2D.InnerProduct(_head.Sub(_tail)) >= 0);
		}
		return result;
	}
};

struct GCircle
{
	__device__ GCircle()
	{
	}
	GPosition2D Center;
	float Radious;
	__device__ GCircle(GPosition2D center, float radious)
	{
		Center = center;
		Radious = radious;
	}
	__device__ GPosition2D Intersect(GLine l, int *count, GPosition2D *intersect2)
	{
		GLine l2 = l.PerpenducilarLineToPoint(Center);
		bool flag;
		GPosition2D gPosition2D = l.IntersectWithLine(l2, &flag);
		GVector2D gVector2D = gPosition2D.Sub(Center);
		float num = gVector2D.Size();
		GPosition2D result = GPosition2D(0, 0);
		count[0] = 0;
		if (num < Radious)
		{
			gVector2D = gPosition2D.Sub(Center);
			float num2 = gVector2D.AngleInRadians();
			float num3 = acosf(num / Radious);
			GVector2D gVector2D2 = GVector2D(0, 0);
			result = Center.Add(gVector2D2.FromAngleSize(num2 + num3, Radious));
			intersect2[0] = Center.Add(gVector2D2.FromAngleSize(num2 - num3, Radious));
			count[0] = 2;
		}
		else
		{
			if (num == Radious)
			{
				count[0] = 1;
				result = gPosition2D;
			}
		}
		return result;
	}
	__device__ int GetTangent(GPosition2D P, GLine *TangentLine1, GLine *TangentLine2, GPosition2D *TangentPoint1, GPosition2D *TangentPoint2)
	{
		GVector2D gVector2D = Center.Sub(P);
		float num = gVector2D.Size();
		TangentLine1[0] = GLine(GPosition2D(0, 0), GPosition2D(0, 0));
		TangentLine2[0] = GLine(GPosition2D(0, 0), GPosition2D(0, 0));
		TangentPoint1[0] = GPosition2D(0, 0);
		TangentPoint2[0] = GPosition2D(0, 0);
		int result;
		if (num >= Radious)
		{
			GLine gLine = GLine(P, Center);
			if (num == Radious)
			{
				TangentPoint1[0] = P;
				TangentLine1[0] = gLine.PerpenducilarLineToPoint(Center);
				result = 1;
			}
			else
			{
				float num2 = gVector2D.AngleInRadians();
				float num3 = asinf(Radious / num);
				float size = sqrtf(num * num - Radious * Radious);
				GVector2D gVector2D2 = GVector2D(0, 0);
				GVector2D p = gVector2D2.FromAngleSize(num2 + num3, size);
				TangentLine1[0] = GLine(P, P.Add(p));
				TangentPoint1[0] = P.Add(p);
				p = gVector2D2.FromAngleSize(num2 - num3, size);
				TangentLine2[0] = GLine(P, P.Add(p));
				TangentPoint2[0] = P.Add(p);
				result = 2;
			}
		}
		else
		{
			result = 0;
		}
		return result;
	}
	__device__ GPosition2D Intersect(GCircle circle, int *count, GPosition2D *intersect2)
	{
		GPosition2D gPosition2D = GPosition2D(0, 0);
		float num = Center.DistanceFrom(circle.Center);
		count[0] = 0;
		GPosition2D result;
		if (num > Radious + circle.Radious || num < abs(Radious - circle.Radious))
		{
			result = GPosition2D(0, 0);
		}
		else
		{
			if (num == 0 && Radious == circle.Radious)
			{
				result = GPosition2D(0, 0);
			}
			else
			{
				float radious = Radious;
				float radious2 = circle.Radious;
				float x = Center.X;
				float y = Center.Y;
				float x2 = circle.Center.X;
				float y2 = circle.Center.Y;
				float num2 = 0.25 * sqrtf(((radious + radious2) * (radious + radious2) - num * num) * (num * num - (radious - radious2) * (radious - radious2)));
				float x3 = 0.5 * (x2 + x) + 0.5 * (x2 - x) * (radious * radious - radious2 * radious2) / (num * num) + 2 * (y2 - y) * num2 / (num * num);
				float y3 = 0.5 * (y2 + y) + 0.5 * (y2 - y) * (radious * radious - radious2 * radious2) / (num * num) - 2 * (x2 - x) * num2 / (num * num);
				float x4 = 0.5 * (x2 + x) + 0.5 * (x2 - x) * (radious * radious - radious2 * radious2) / (num * num) - 2 * (y2 - y) * num2 / (num * num);
				float y4 = 0.5 * (y2 + y) + 0.5 * (y2 - y) * (radious * radious - radious2 * radious2) / (num * num) + 2 * (x2 - x) * num2 / (num * num);
				count[0] = 2;
				gPosition2D = GPosition2D(x3, y3);
				intersect2[0] = GPosition2D(x4, y4);
				result = gPosition2D;
			}
		}
		return result;
	}
	__device__ bool IsInCircle(GPosition2D P)
	{
		return Center.DistanceFrom(P) < Radious;
	}
	__device__ int HasIntersect(GVector2D Vec, GPosition2D From)
	{
		if(IsInCircle(From) )
			return 1;
		else if (IsInCircle(From.Add(Vec)))
			return 1;
		/*else if (Vec.Distance(From.X, From.Y) < Radious && Vec.InnerProduct(Center.Sub(From)) > 0 && GVector2D(-Vec.X,-Vec.Y).InnerProduct(Center.Sub(From.Add(Vec))) > 0)
			return 1;*/
		return 0;
	}

};

struct GObjectState
{
	__host__ __device__ GObjectState()
	{
	}
	GPosition2D Location;
	GVector2D Speed;
	GObjectType Type;
	GVector2D Acceleration;
	__host__ __device__ GObjectState(GObjectState *From)
	{
		Type = From[0].Type;
		Location = From[0].Location;
		Speed = From[0].Speed;
		Acceleration = From[0].Acceleration;
	}
	__host__ __device__ GObjectState(GObjectType type, GPosition2D location, GVector2D speed, GVector2D acceleration)
	{
		Type = type;
		Location = location;
		Speed = speed;
		Acceleration = acceleration;
	}
	__host__ __device__ GObjectState(GPosition2D location, GVector2D speed)
	{
		Location = location;
		Speed = speed;
		Acceleration = GVector2D(0, 0);
		Type = OurRobot;
	}
};

//unsigned int sizeof(float) ;
//unsigned int sizeof(int)   ;
//unsigned int sizeof(GPosition2D);
//unsigned int sizeof(GObjectState);
//unsigned int sizeof(GPosition2D) = sizeof(GPosition2D);
//unsigned int sizeof(GObjectState) = sizeof(GObjectState);
//unsigned int sizeof(float) = sizeof(float);
//unsigned int sizeof(int)   = sizeof(int);