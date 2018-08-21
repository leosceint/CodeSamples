//Code sample: Image 3D Transformation 
//Author: MSDN-WhiteKnight (https://github.com/MSDN-WhiteKnight)
//License: BSD 3-clause

//Based on:
//https://docs.microsoft.com/en-us/dotnet/framework/wpf/graphics-multimedia/how-to-create-a-3-d-scene
//https://stackoverflow.com/questions/2945762/projecting-an-object-into-a-scene-based-on-world-coordinates-only
//https://stackoverflow.com/questions/10208798/wpf-c-rendertargetbitmap-of-viewport3d-without-assigning-it-to-a-window/10231068

/* Usage: 

Rotate 30 degreees over Y axis: 
    ImageTransformer.RotateImage(  "c:\\images\\image.jpg",  "c:\\images\\image2.jpg", 
       new System.Windows.Media.Media3D.Vector3D(0, 1, 0), 30); 
       
Apply arbitrary transform matrix:       
    ImageTransformer.ApplyTransform(
       "c:\\images\\image.jpg", "c:\\images\\image2.jpg",
       new System.Windows.Media.Media3D.Matrix3D(.615, 0, 0, .00015, 0, .615, 0, -.000005, 0, 0, 1, 0, -151, -120, 0, 1));
*/

//References: PresentationCore, PresentationFramework, WindowsBase, System.XAML
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;

namespace HelperClasses
{
    public class ImageTransformer
    {
        /*Применяет произвольное 3D-преобразование к изображению*/
        public static void ApplyTransform(string input, string output, Matrix3D matr)
        {
            Viewport3D myViewport3D;
            BitmapImage image = new BitmapImage(new Uri(input));

            // Declare scene objects.
            myViewport3D = new Viewport3D();
            Model3DGroup myModel3DGroup = new Model3DGroup();
            GeometryModel3D myGeometryModel = new GeometryModel3D();
            ModelVisual3D myModelVisual3D = new ModelVisual3D();
            // Defines the camera used to view the 3D object. In order to view the 3D object,
            // the camera must be positioned and pointed such that the object is within view 
            // of the camera.
            PerspectiveCamera myPCamera = new PerspectiveCamera();

            // Specify where in the 3D scene the camera is.
            myPCamera.Position = new Point3D(0, 0, Math.Max(image.PixelWidth,image.PixelHeight)*2);

            // Specify the direction that the camera is pointing.
            myPCamera.LookDirection = new Vector3D(0, 0, -1);

            // Define camera's horizontal field of view in degrees.
            myPCamera.FieldOfView = 60;

            // Asign the camera to the viewport
            myViewport3D.Camera = myPCamera;

            // Define the lights cast in the scene. Without light, the 3D object cannot 
            // be seen. Note: to illuminate an object from additional directions, create 
            // additional lights.                        
            AmbientLight al = new AmbientLight(Colors.White);
            myModel3DGroup.Children.Add(al);

            // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet 
            // is created.
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            // Create a collection of normal vectors for the MeshGeometry3D.
            Vector3DCollection myNormalCollection = new Vector3DCollection();
            myNormalCollection.Add(new Vector3D(0, 0, 1));
            myNormalCollection.Add(new Vector3D(0, 0, 1));
            myNormalCollection.Add(new Vector3D(0, 0, 1));
            myNormalCollection.Add(new Vector3D(0, 0, 1));
            myNormalCollection.Add(new Vector3D(0, 0, 1));
            myNormalCollection.Add(new Vector3D(0, 0, 1));
            myMeshGeometry3D.Normals = myNormalCollection;

            // Create a collection of vertex positions for the MeshGeometry3D. 
            Point3DCollection myPositionCollection = new Point3DCollection();
            myPositionCollection.Add(new Point3D(-image.PixelWidth / 2.0, -image.PixelHeight / 2.0, 0.5));
            myPositionCollection.Add(new Point3D(image.PixelWidth / 2.0, -image.PixelHeight / 2.0, 0.5));
            myPositionCollection.Add(new Point3D(image.PixelWidth / 2.0, image.PixelHeight / 2.0, 0.5));
            myPositionCollection.Add(new Point3D(image.PixelWidth / 2.0, image.PixelHeight / 2.0, 0.5));
            myPositionCollection.Add(new Point3D(-image.PixelWidth / 2.0, image.PixelHeight / 2.0, 0.5));
            myPositionCollection.Add(new Point3D(-image.PixelWidth / 2.0, -image.PixelHeight / 2.0, 0.5));
            myMeshGeometry3D.Positions = myPositionCollection;

            // Create a collection of texture coordinates for the MeshGeometry3D.
            PointCollection myTextureCoordinatesCollection = new PointCollection();

            Point p5 = new Point(0, 0);
            Point p34 = new Point(1, 0);
            Point p2 = new Point(1, 1);
            Point p16 = new Point(0, 1);

            myTextureCoordinatesCollection.Add(p16);
            myTextureCoordinatesCollection.Add(p2);
            myTextureCoordinatesCollection.Add(p34);
            myTextureCoordinatesCollection.Add(p34);
            myTextureCoordinatesCollection.Add(p5);
            myTextureCoordinatesCollection.Add(p16);

            myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;

            // Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection();
            myTriangleIndicesCollection.Add(0);
            myTriangleIndicesCollection.Add(1);
            myTriangleIndicesCollection.Add(2);
            myTriangleIndicesCollection.Add(3);
            myTriangleIndicesCollection.Add(4);
            myTriangleIndicesCollection.Add(5);
            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            // Apply the mesh to the geometry model.
            myGeometryModel.Geometry = myMeshGeometry3D;

            // The material specifies the material applied to the 3D object.
            
            ImageBrush br = new ImageBrush(image);

            // Define material and apply to the mesh geometries.
            DiffuseMaterial myMaterial = new DiffuseMaterial(br);
            myGeometryModel.Material = myMaterial;
            myGeometryModel.BackMaterial = myMaterial;

            MatrixTransform3D transform = new MatrixTransform3D(matr);
            myGeometryModel.Transform = transform;

            // Add the geometry model to the model group.
            myModel3DGroup.Children.Add(myGeometryModel);

            // Add the group of models to the ModelVisual3d.
            myModelVisual3D.Content = myModel3DGroup;
            myViewport3D.Children.Add(myModelVisual3D);

            //render Viewport3D into bitmap      
            int width = image.PixelWidth ;
            int height = image.PixelHeight;
            myViewport3D.Width = width;
            myViewport3D.Height = height;
            myViewport3D.Measure(new Size(width, height));
            myViewport3D.Arrange(new Rect(0, 0, width, height));

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(myViewport3D);

            //Save bitmap to file
            using (var fileStream = new FileStream(output, FileMode.Create))
            {
                BitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                encoder.Save(fileStream);
            }

        }

        /*Поворачивает изображение вокруг указанной оси на указанный угол*/
        public static void RotateImage(string input, string output, Vector3D axis, int angle)
        {
            var myRotateTransform3D = new RotateTransform3D();
            var myAxisAngleRotation3d = new AxisAngleRotation3D();
            myAxisAngleRotation3d.Axis = axis;
            myAxisAngleRotation3d.Angle = angle;
            myRotateTransform3D.Rotation = myAxisAngleRotation3d;

            ImageTransformer.ApplyTransform(input, output, myRotateTransform3D.Value);

        }

    }
}
