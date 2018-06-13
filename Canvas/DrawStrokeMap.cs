using System.Linq;
using System.Drawing;
using GestureRecognitionLib.CHnMM;

namespace MultiStrokeGestureRecognitionLib
{
    public class DrawStrokeMap
    {
        public static void DrawMap(StrokeMap strokeMap, CSharpCanvas DrawCanvas, Color color, bool isTranslationInvariant)
        {
            var h = DrawCanvas.ActualHeight;
            var w = DrawCanvas.ActualWidth;

            var offsetX = 0d;
            var offsetY = 0d;

            if (isTranslationInvariant)
            {
                //move translationinvariant gestures to canvas center
                var minX = strokeMap.Areas.Cast<Circle>().Min(a => a.X);
                var maxX = strokeMap.Areas.Cast<Circle>().Max(a => a.X);

                var minY = strokeMap.Areas.Cast<Circle>().Min(a => a.Y);
                var maxY = strokeMap.Areas.Cast<Circle>().Max(a => a.Y);

                double width = maxX - minX;
                double height = maxY - minY;

                double traceCenterX = minX + (width / 2);
                double traceCenterY = minY + (height / 2);

                offsetX = 0.5 - traceCenterX;
                offsetY = 0.5 - traceCenterY;
            }

            //int i = 1;
            foreach (var iarea in strokeMap.Areas)
            {
                var carea = iarea as Circle;

                var absX = (carea.X + offsetX) * w;
                var absY = (carea.Y + offsetY) * h;

                if (iarea is DiscreteCircle)
                {
                    var area = iarea as DiscreteCircle;
                    var absCircleW = area.Radius * w * 2;
                    var absCircleH = area.Radius * h * 2;
                    var absTolCircleW = area.ToleranceRadius * w * 2;
                    var absTolCircleH = area.ToleranceRadius * h * 2;

                    var circle = new Ellipse();
                    circle.Width = absCircleW;
                    circle.Height = absCircleH;
                    circle.Fill = null;
                    circle.Stroke = new SolidColorBrush(color);
                    circle.StrokeThickness = 1.5;
                    circle.SetValue(Canvas.LeftProperty, absX - absCircleW / 2);
                    circle.SetValue(Canvas.TopProperty, absY - absCircleH / 2);

                    var tolCircle = new Ellipse();
                    tolCircle.Width = absTolCircleW;
                    tolCircle.Height = absTolCircleH;
                    tolCircle.Fill = null;
                    tolCircle.Stroke = new SolidColorBrush(Color.Multiply(color, 0.9f));
                    tolCircle.StrokeDashArray = new DoubleCollection(new double[] { 3 });
                    //tolCircle.StrokeDashOffset = 2;
                    tolCircle.StrokeThickness = 1;
                    tolCircle.SetValue(Canvas.LeftProperty, absX - absTolCircleW / 2);
                    tolCircle.SetValue(Canvas.TopProperty, absY - absTolCircleH / 2);

                    //var curFillBrush = baseFillBrush.Clone();
                    //curFillBrush.Opacity = opacity;

                    //circle.Fill = curFillBrush;
                    ////circle.SetValue(Ellipse.FillProperty, FillBrush);
                    //circle.Stroke = StrokeBrush;
                    //circle.SetValue(Canvas.LeftProperty, (double)(p.X * 600 - size));
                    //circle.SetValue(Canvas.TopProperty, (double)(p.Y * 600 - size));

                    DrawCanvas.Children.Add(circle);
                    DrawCanvas.Children.Add(tolCircle);
                }
                else
                {
                    var area = iarea as ContinuousCircle;

                    var absCircleW = area.StandardDeviation * w * 2;
                    var absCircleH = area.StandardDeviation * h * 2;
                    var absTolCircleW = area.StandardDeviation * 3 * w * 2;
                    var absTolCircleH = area.StandardDeviation * 3 * h * 2;

                    var circle = new Ellipse();
                    circle.Width = absCircleW;
                    circle.Height = absCircleH;
                    circle.Fill = null;
                    circle.Stroke = new SolidColorBrush(color);
                    circle.StrokeThickness = 1.5;
                    circle.SetValue(Canvas.LeftProperty, absX - absCircleW / 2);
                    circle.SetValue(Canvas.TopProperty, absY - absCircleH / 2);

                    var tolCircle = new Ellipse();
                    tolCircle.Width = absTolCircleW;
                    tolCircle.Height = absTolCircleH;
                    tolCircle.Fill = null;
                    tolCircle.Stroke = new SolidColorBrush(Color.Multiply(color, 0.9f));
                    tolCircle.StrokeDashArray = new DoubleCollection(new double[] { 3 });
                    //tolCircle.StrokeDashOffset = 2;
                    tolCircle.StrokeThickness = 1;
                    tolCircle.SetValue(Canvas.LeftProperty, absX - absTolCircleW / 2);
                    tolCircle.SetValue(Canvas.TopProperty, absY - absTolCircleH / 2);

                    //var curFillBrush = baseFillBrush.Clone();
                    //curFillBrush.Opacity = opacity;

                    //circle.Fill = curFillBrush;
                    ////circle.SetValue(Ellipse.FillProperty, FillBrush);
                    //circle.Stroke = StrokeBrush;
                    //circle.SetValue(Canvas.LeftProperty, (double)(p.X * 600 - size));
                    //circle.SetValue(Canvas.TopProperty, (double)(p.Y * 600 - size));

                    DrawCanvas.Children.Add(circle);
                    DrawCanvas.Children.Add(tolCircle);
                }
            }
        }
    }
}