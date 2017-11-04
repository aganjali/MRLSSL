using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SlimDX.Direct3D10;
using StillDesign.PhysX.MathPrimitives;
using StillDesign.PhysX;

namespace Simulator
{
	public static class ConvexLoader
	{
		public static Actor LoadConvexMesh(string adress, Scene scene, Device device)
		{
			var torusModel = ColladaLoader.Load(adress, device);
			var core = scene.Core;

			// Allocate memory for the points and triangles
			var convexMeshDesc = new ConvexMeshDescription()
			{
				PointCount = torusModel.VertexPositions.Length
			};
			convexMeshDesc.Flags |= ConvexFlag.ComputeConvex;
			convexMeshDesc.AllocatePoints<Vector3>(torusModel.VertexPositions.Length);
            //SlimDX.Matrix[] transforms = torusModel

            
			// Write in the points and triangles
			// We only want the Position component of the vertex. Also scale down the mesh.

            var t = SlimDX.Matrix.Scaling(0.0254f, 0.0254f, 0.0254f) * SlimDX.Matrix.RotationZ((float)(Math.PI)) * SlimDX.Matrix.RotationX((float)(-Math.PI / 2));
			foreach (var vertex in torusModel.VertexPositions)
			{
				var position = SlimDX.Vector3.TransformCoordinate(vertex, t);
				convexMeshDesc.PointsStream.Write(position);
			}

			//

			// Cook to memory or to a file
			ConvexMesh convexMesh;
			using (var stream = new MemoryStream())
			{
				//FileStream stream = new FileStream( @"Convex Mesh.cooked", FileMode.CreateNew );

				Cooking.InitializeCooking(new ConsoleOutputStream());
				Cooking.CookConvexMesh(convexMeshDesc, stream);
				Cooking.CloseCooking();

				stream.Position = 0;

				convexMesh = core.CreateConvexMesh(stream);
			}
			var convexShapeDesc = new ConvexShapeDescription(convexMesh);

			var actorDesc = new ActorDescription()
			{
				BodyDescription = new BodyDescription(3f),
				//GlobalPose = Matrix.Translation(30, 30, 0)
			};
			actorDesc.Shapes.Add(convexShapeDesc);

			var actor = scene.CreateActor(actorDesc);

			return actor;
		}
	}
}