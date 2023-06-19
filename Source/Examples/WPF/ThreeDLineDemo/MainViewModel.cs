using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using _3DTools;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.Models;
using System.Collections.ObjectModel;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using MugenMvvmToolkit;

namespace ThreeDLineDemo
{
    public class MainViewModel : ViewModelBase
    {
        // TODO: not sure if this is the best way to bind to a model
        // check the other examples, perhaps in a non-MVVM way is better
        // or read up on how exactly the "
        // <ht:HelixViewport3D ItemsSource="{Binding LinesGroup}" Grid.Row="1" Background="White" ZoomExtentsWhenLoaded="True" x:Name="View1" ShowFrameRate="True" InfiniteSpin="True">
        // ItemsSource="{Binding LinesGroup}"
        private ObservableCollection<Visual3D> linesGroup;

        public ObservableCollection<Visual3D> LinesGroup
        {
            get => linesGroup;
            set => SetProperty(ref linesGroup, value);
        }

        public ICommand DrawLinesCommand { get; }
        public ICommand DrawPolylinesCommand { get; }

        public MainViewModel()
        {
            LinesGroup = new ObservableCollection<Visual3D>();

            DrawLinesCommand = new RelayCommand(DrawLines);
            DrawPolylinesCommand = new RelayCommand(DrawPolylines);
        }

        private void DrawLines()
        {
            /*
            int lineCount = 5000;

            for (int i = 0; i < lineCount; i++)
            {
                var line = new ScreenSpaceLines3D();
                line.Points.Add(new Point3D(0, 0, 0));
                line.Points.Add(new Point3D(i * 0.01, 0, 0));
                line.Color = Colors.Blue;
                LinesGroup.Add(line);
            }
            */

            var lines = GenerateRandomScratchLinesOnSphere(500000, 1, 0.01);
            LinesGroup.Clear();

            foreach (var line in lines)
            {
                LinesGroup.Add(line);
            }

            ExportToObj(lines);
        }

        private void DrawPolylines()
        {
            /*
            int polylineCount = 2000;

            for (int i = 0; i < polylineCount; i++)
            {
                var pointsList = new List<Point3D>
                {
                    new Point3D(0, 0, i * 0.01),
                    new Point3D(1, 0, i * 0.01),
                    new Point3D(1, 1, i * 0.01),
                    new Point3D(0, 1, i * 0.01),
                    new Point3D(0, 0, i * 0.01)
                };

                var points = new Point3DCollection(pointsList);

                var polyline = new LinesVisual3D { Points = points, Color = Colors.Red };
                LinesGroup.Add(polyline);
            }
            */
            //var polylines = GenerateRandomPolylinesOnCube(2000, 1, 5);
            //var polylines = GenerateRandomLinesOnCube(2000, 1, 0.02);
            //var polylines = GenerateRandomPolylines(2000, 5, -1, 1, -1, 1);
            var polylines = GenerateCubeFacets(1, 2000, 5);
            LinesGroup.Clear();

            foreach (var polyline in polylines)
            {
                LinesGroup.Add(polyline);
            }

            ExportToObj(polylines);
        }

        private static IEnumerable<ScreenSpaceLines3D> GenerateRandomDepthLinesOnSphere(int numLines, double radius,
            double lineLength)
        {
            var random = new Random();
            var lines = new List<ScreenSpaceLines3D>();

            for (var i = 0; i < numLines; i++)
            {
                var phi = random.NextDouble() * 2 * Math.PI;
                var theta = random.NextDouble() * Math.PI;

                var x1 = radius * Math.Sin(theta) * Math.Cos(phi);
                var y1 = radius * Math.Sin(theta) * Math.Sin(phi);
                var z1 = radius * Math.Cos(theta);

                var offset = random.NextDouble() * lineLength;

                var x2 = (radius - offset) * Math.Sin(theta) * Math.Cos(phi);
                var y2 = (radius - offset) * Math.Sin(theta) * Math.Sin(phi);
                var z2 = (radius - offset) * Math.Cos(theta);

                var line = new ScreenSpaceLines3D();
                line.Points.Add(new Point3D(x1, y1, z1));
                line.Points.Add(new Point3D(x2, y2, z2));
                line.Color = Colors.Blue;
                lines.Add(line);
            }

            return lines;
        }

        private static IEnumerable<ScreenSpaceLines3D> GenerateRandomScratchLinesOnSphere(int numLines, double radius,
            double lineLength)
        {
            var random = new Random();
            var lines = new List<ScreenSpaceLines3D>();

            for (var i = 0; i < numLines; i++)
            {
                var phi = random.NextDouble() * 2 * Math.PI;
                var theta = random.NextDouble() * Math.PI;

                var x = radius * Math.Sin(theta) * Math.Cos(phi);
                var y = radius * Math.Sin(theta) * Math.Sin(phi);
                var z = radius * Math.Cos(theta);

                var tangentX = -Math.Sin(phi);
                var tangentY = Math.Cos(phi);

                var endPointX = x + lineLength * tangentX;
                var endPointY = y + lineLength * tangentY;
                var endPointZ = z;

                var line = new ScreenSpaceLines3D();
                line.Points.Add(new Point3D(x, y, z));
                line.Points.Add(new Point3D(endPointX, endPointY, endPointZ));
                line.Color = Colors.Blue;
                lines.Add(line);
            }

            return lines;
        }

        private static IEnumerable<ScreenSpaceLines3D> GenerateRandomPolylinesAroundCube(int numPolylines,
            double sideLength)
        {
            var random = new Random();
            var polylines = new List<ScreenSpaceLines3D>();

            for (var i = 0; i < numPolylines; i++)
            {
                var x = random.NextDouble() * sideLength - sideLength / 2;
                var y = random.NextDouble() * sideLength - sideLength / 2;
                var z = random.NextDouble() * sideLength - sideLength / 2;

                var polyline = new ScreenSpaceLines3D();
                polyline.Color = Colors.Black;

                // Top face
                polyline.Points.Add(new Point3D(x - sideLength / 2, y + sideLength / 2, z + sideLength / 2));
                polyline.Points.Add(new Point3D(x + sideLength / 2, y + sideLength / 2, z + sideLength / 2));
                polyline.Points.Add(new Point3D(x + sideLength / 2, y + sideLength / 2, z - sideLength / 2));
                polyline.Points.Add(new Point3D(x - sideLength / 2, y + sideLength / 2, z - sideLength / 2));
                polyline.Points.Add(new Point3D(x - sideLength / 2, y + sideLength / 2, z + sideLength / 2));

                // Bottom face
                polyline.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z + sideLength / 2));
                polyline.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z + sideLength / 2));
                polyline.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                polyline.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                polyline.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z + sideLength / 2));

                // Connecting lines
                polyline.Points.Add(new Point3D(x - sideLength / 2, y + sideLength / 2, z + sideLength / 2));
                polyline.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z + sideLength / 2));

                polyline.Points.Add(new Point3D(x + sideLength / 2, y + sideLength / 2, z + sideLength / 2));
                polyline.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z + sideLength / 2));

                polyline.Points.Add(new Point3D(x + sideLength / 2, y + sideLength / 2, z - sideLength / 2));
                polyline.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z - sideLength / 2));

                polyline.Points.Add(new Point3D(x - sideLength / 2, y + sideLength / 2, z - sideLength / 2));
                polyline.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z - sideLength / 2));

                polylines.Add(polyline);
            }

            return polylines;
        }

        private static IEnumerable<ScreenSpaceLines3D> GenerateRandomPolylinesOnCube(int numPolylines,
            double sideLength, double scratchLength)
        {
            var random = new Random();
            var polylines = new List<ScreenSpaceLines3D>();

            for (var i = 0; i < numPolylines; i++)
            {
                var face = random.Next(6); // Randomly select a face of the cube

                var x = random.NextDouble() * sideLength - sideLength / 2;
                var y = random.NextDouble() * sideLength - sideLength / 2;
                var z = random.NextDouble() * sideLength - sideLength / 2;

                var polyline = new ScreenSpaceLines3D();
                polyline.Color = Colors.Black;

                switch (face)
                {
                    case 0: // Front face
                        polyline.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z + sideLength / 2));
                        polyline.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z + sideLength / 2));
                        polyline.Points.Add(new Point3D(x + sideLength / 2, y + sideLength / 2, z + sideLength / 2));
                        polyline.Points.Add(new Point3D(x - sideLength / 2, y + sideLength / 2, z + sideLength / 2));
                        break;

                    case 1: // Back face
                        polyline.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                        polyline.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                        polyline.Points.Add(new Point3D(x + sideLength / 2, y + sideLength / 2, z - sideLength / 2));
                        polyline.Points.Add(new Point3D(x - sideLength / 2, y + sideLength / 2, z - sideLength / 2));
                        break;

                    case 2: // Left face
                        polyline.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                        polyline.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z + sideLength / 2));
                        polyline.Points.Add(new Point3D(x - sideLength / 2, y + sideLength / 2, z + sideLength / 2));
                        polyline.Points.Add(new Point3D(x - sideLength / 2, y + sideLength / 2, z - sideLength / 2));
                        break;

                    case 3: // Right face
                        polyline.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                        polyline.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z + sideLength / 2));
                        polyline.Points.Add(new Point3D(x + sideLength / 2, y + sideLength / 2, z + sideLength / 2));
                        polyline.Points.Add(new Point3D(x + sideLength / 2, y + sideLength / 2, z - sideLength / 2));
                        break;

                    case 4: // Top face
                        polyline.Points.Add(new Point3D(x - sideLength / 2, y + sideLength / 2, z - sideLength / 2));
                        polyline.Points.Add(new Point3D(x + sideLength / 2, y + sideLength / 2, z - sideLength / 2));
                        polyline.Points.Add(new Point3D(x + sideLength / 2, y + sideLength / 2, z + sideLength / 2));
                        polyline.Points.Add(new Point3D(x - sideLength / 2, y + sideLength / 2, z + sideLength / 2));
                        break;

                    case 5: // Bottom face
                        polyline.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                        polyline.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                        polyline.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z + sideLength / 2));
                        polyline.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z + sideLength / 2));
                        break;
                }

                // Generate scratch lines on the surface of the selected face of the cube
                for (var j = 0; j < scratchLength; j++)
                {
                    var edgeIndex = random.Next(4); // Randomly select an edge of the face

                    var point1 = polyline.Points[edgeIndex];
                    var point2 = polyline.Points[(edgeIndex + 1) % 4];

                    var scratchX = random.NextDouble() * Math.Abs(point2.X - point1.X) + Math.Min(point1.X, point2.X);
                    var scratchY = random.NextDouble() * Math.Abs(point2.Y - point1.Y) + Math.Min(point1.Y, point2.Y);

                    polyline.Points.Insert(edgeIndex + 1, new Point3D(scratchX, scratchY, z));
                }

                polylines.Add(polyline);
            }

            return polylines;
        }

        //private static IEnumerable<ScreenSpaceLines3D> GenerateRandomLinesOnCube(int numLines, double sideLength, double lineLength)
        //{
        //    var random = new Random();
        //    var lines = new List<ScreenSpaceLines3D>();

        //    for (var i = 0; i < numLines; i++)
        //    {
        //        var face = random.Next(6); // Randomly select a face of the cube

        //        var x = random.NextDouble() * sideLength - sideLength / 2;
        //        var y = random.NextDouble() * sideLength - sideLength / 2;
        //        var z = random.NextDouble() * sideLength - sideLength / 2;

        //        var line = new ScreenSpaceLines3D();
        //        line.Color = Colors.Black;

        //        switch (face)
        //        {
        //            case 0: // Front face
        //                line.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z + sideLength / 2));
        //                line.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z + sideLength / 2));
        //                break;

        //            case 1: // Back face
        //                line.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z - sideLength / 2));
        //                line.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z - sideLength / 2));
        //                break;

        //            case 2: // Left face
        //                line.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z - sideLength / 2));
        //                line.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z + sideLength / 2));
        //                break;

        //            case 3: // Right face
        //                line.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z - sideLength / 2));
        //                line.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z + sideLength / 2));
        //                break;

        //            case 4: // Top face
        //                line.Points.Add(new Point3D(x - sideLength / 2, y + sideLength / 2, z - sideLength / 2));
        //                line.Points.Add(new Point3D(x + sideLength / 2, y + sideLength / 2, z - sideLength / 2));
        //                break;

        //            case 5: // Bottom face
        //                line.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z - sideLength / 2));
        //                line.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z - sideLength / 2));
        //                break;
        //        }

        //        var angle = random.NextDouble() * 2 * Math.PI; // Random angle for rotation
        //        var cosAngle = Math.Cos(angle);
        //        var sinAngle = Math.Sin(angle);

        //        // Rotate the line by the random angle
        //        for (var j = 0; j < line.Points.Count; j++)
        //        {
        //            var point = line.Points[j];
        //            var rotatedX = point.X * cosAngle - point.Y * sinAngle;
        //            var rotatedY = point.X * sinAngle + point.Y * cosAngle;
        //            line.Points[j] = new Point3D(rotatedX, rotatedY, z);
        //        }

        //        lines.Add(line);
        //    }

        //    return lines;
        //}

        private static IEnumerable<ScreenSpaceLines3D> GenerateRandomLinesOnCube(int numLines, double sideLength,
            double lineLength)
        {
            var random = new Random();
            var lines = new List<ScreenSpaceLines3D>();

            var face = random.Next(6); // Randomly select a face of the cube

            for (var i = 0; i < numLines; i++)
            {
                var x = random.NextDouble() * sideLength - sideLength / 2;
                var y = random.NextDouble() * sideLength - sideLength / 2;
                var z = random.NextDouble() * sideLength - sideLength / 2;

                var line = new ScreenSpaceLines3D();
                line.Color = Colors.Black;

                switch (face)
                {
                    case 0: // Front face
                        line.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z + sideLength / 2));
                        line.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z + sideLength / 2));
                        break;

                    case 1: // Back face
                        line.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                        line.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                        break;

                    case 2: // Left face
                        line.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                        line.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z + sideLength / 2));
                        break;

                    case 3: // Right face
                        line.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                        line.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z + sideLength / 2));
                        break;

                    case 4: // Top face
                        line.Points.Add(new Point3D(x - sideLength / 2, y + sideLength / 2, z - sideLength / 2));
                        line.Points.Add(new Point3D(x + sideLength / 2, y + sideLength / 2, z - sideLength / 2));
                        break;

                    case 5: // Bottom face
                        line.Points.Add(new Point3D(x - sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                        line.Points.Add(new Point3D(x + sideLength / 2, y - sideLength / 2, z - sideLength / 2));
                        break;
                }

                var angle = random.NextDouble() * 2 * Math.PI; // Random angle for rotation
                var cosAngle = Math.Cos(angle);
                var sinAngle = Math.Sin(angle);

                // Rotate the line by the random angle
                for (var j = 0; j < line.Points.Count; j++)
                {
                    var point = line.Points[j];
                    var rotatedX = point.X * cosAngle - point.Y * sinAngle;
                    var rotatedY = point.X * sinAngle + point.Y * cosAngle;
                    line.Points[j] = new Point3D(rotatedX, rotatedY, z);
                }

                lines.Add(line);
            }

            return lines;
        }

        private static List<ScreenSpaceLines3D> GenerateRandomPolylines(int numPolylines, int numPointsPerPolyline,
            double minX, double maxX, double minY, double maxY)
        {
            var random = new Random();
            var polylines = new List<ScreenSpaceLines3D>();

            for (var i = 0; i < numPolylines; i++)
            {
                var polyline = new ScreenSpaceLines3D();
                polyline.Color = Colors.Black;

                for (var j = 0; j < numPointsPerPolyline; j++)
                {
                    var x = random.NextDouble() * (maxX - minX) + minX;
                    var y = random.NextDouble() * (maxY - minY) + minY;
                    var z = 0.0; // Set the z-coordinate to 0 for a 2D polyline
                    var point = new Point3D(x, y, z);
                    polyline.Points.Add(point);
                }

                polylines.Add(polyline);
            }

            return polylines;
        }



        private static List<ScreenSpaceLines3D> GenerateCubeFacets(double sideLength, int numPolylines,
            int numPointsPerPolyline)
        {
            var polylines = new List<ScreenSpaceLines3D>();

            // Generate polylines for each facet of the cube
            for (var face = 0; face < 6; face++)
            {
                var facetPolylines = GenerateRandomPolylines(numPolylines, numPointsPerPolyline, -sideLength / 2,
                    sideLength / 2, -sideLength / 2, sideLength / 2);

                // Adjust the z-coordinate based on the cube face
                var zOffset = 0.0;
                switch (face)
                {
                    case 0: // Front face
                        zOffset = sideLength / 2;
                        break;

                    case 1: // Back face
                        zOffset = -sideLength / 2;
                        break;
/*
                case 2: // Left face
                    facetPolylines = RotatePolylines(facetPolylines, new Vector3D(0, 0, 1), -90);
                    break;

                case 3: // Right face
                    facetPolylines = RotatePolylines(facetPolylines, new Vector3D(0, 0, 1), 90);
                    break;

                case 4: // Top face
                    facetPolylines = RotatePolylines(facetPolylines, new Vector3D(1, 0, 0), 90);
                    break;

                case 5: // Bottom face
                    facetPolylines = RotatePolylines(facetPolylines, new Vector3D(1, 0, 0), -90);
                    break;
*/
                    default:
                        break;
                }

                // Translate the polylines to their respective cube face
                TranslatePolylines(facetPolylines, new Vector3D(0, 0, zOffset));

                polylines.AddRange(facetPolylines);
            }

            return polylines;
        }

        private static List<ScreenSpaceLines3D> RotatePolylines(List<ScreenSpaceLines3D> polylines, Vector3D axis,
            double angle)
        {
            var rotatedPolylines = new List<ScreenSpaceLines3D>();

            var rotationTransform = new RotateTransform3D(new AxisAngleRotation3D(axis, angle));

            foreach (var polyline in polylines)
            {
                var transformedPoints = new List<Point3D>();
                foreach (var point in polyline.Points)
                {
                    var transformedPoint = rotationTransform.Transform(point);
                    transformedPoints.Add(transformedPoint);
                }


                var rotatedPolyline = new ScreenSpaceLines3D
                {
                    Points = new Point3DCollection(transformedPoints),
                    Color = polyline.Color
                };

                rotatedPolylines.Add(rotatedPolyline);
            }

            return rotatedPolylines;
        }



        private static void TranslatePolylines(List<ScreenSpaceLines3D> polylines, Vector3D translation)
        {
            var translationTransform = new TranslateTransform3D(translation);

            foreach (var polyline in polylines)
            {
                for (var i = 0; i < polyline.Points.Count; i++)
                {
                    polyline.Points[i] = translationTransform.Transform(polyline.Points[i]);
                }
            }
        }


        //private static List<List<Point>> GenerateRandomPolylines(int numPolylines, int numPointsPerPolyline, double minX, double maxX, double minY, double maxY)
        //{
        //    var random = new Random();
        //    var polylines = new List<List<Point>>();

        //    for (var i = 0; i < numPolylines; i++)
        //    {
        //        var polyline = new List<Point>();

        //        for (var j = 0; j < numPointsPerPolyline; j++)
        //        {
        //            var x = random.NextDouble() * (maxX - minX) + minX;
        //            var y = random.NextDouble() * (maxY - minY) + minY;
        //            var point = new Point(x, y);
        //            polyline.Add(point);
        //        }

        //        polylines.Add(polyline);
        //    }

        //    return polylines;
        //}

        private static void ExportToObj(IEnumerable<ScreenSpaceLines3D> lines)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".obj",
                Filter = "OBJ files (*.obj)|*.obj|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                var path = dialog.FileName;

                using (var writer = new StreamWriter(path))
                {
                    int vertexIndex = 1;
                    foreach (var line in lines)
                    {
                        foreach (var point in line.Points)
                        {
                            writer.WriteLine(
                                $"v {point.X.ToString(CultureInfo.InvariantCulture)} {point.Y.ToString(CultureInfo.InvariantCulture)} {point.Z.ToString(CultureInfo.InvariantCulture)}");
                        }

                        for (int i = 0; i < line.Points.Count - 1; i++)
                        {
                            writer.WriteLine($"l {vertexIndex + i} {vertexIndex + i + 1}");
                        }

                        vertexIndex += line.Points.Count;
                    }
                }

                MessageBox.Show("OBJ file exported successfully.");
            }
        }
    }
}
