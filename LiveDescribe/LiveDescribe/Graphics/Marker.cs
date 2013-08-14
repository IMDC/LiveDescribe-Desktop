using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LiveDescribe.Graphics
{
    class Marker
    {
        private Canvas _audioCanvas;

        public Marker(Canvas audioCanvas)
        {
            this._audioCanvas = audioCanvas;    
        }

        /// <summary>
        /// Draw the Position Marker
        /// </summary>
        /// <param name="xPos">X Position of where it should be drawn</param>
        /// <param name="height">Height of the Canvas</param>
        /// <param name="width">Width of the Canvas</param>
        public void draw(double xPos, double height, double width)
        {
            Line line = new Line();
            line.Stroke = Brushes.Black;
            line.StrokeThickness = 2;
            line.X1 = xPos;
            line.X2 = xPos;
            line.Y1 = 0;
            line.Y2 = height; 

            Point p1 = new Point(xPos - 10 , 0);
            Point p2 = new Point(xPos + 10, 0);
            Point p3 = new Point(xPos , 20);
            
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(p1);
            myPointCollection.Add(p2);
            myPointCollection.Add(p3);

            Polygon myPolygon = new Polygon();
            myPolygon.Points = myPointCollection;
            myPolygon.Fill = Brushes.Black;
            
            this._audioCanvas.Children.Add(line);
            this._audioCanvas.Children.Add(myPolygon);
        }
    }
}
