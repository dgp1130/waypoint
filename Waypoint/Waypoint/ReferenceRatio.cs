using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waypoint
{
    /// <summary>
    /// Model class representing a ratio used as a reference for the map.
    /// Correlates a pixel value as equivalent to a polar value for a given PolarAxis.
    /// </summary>
    class ReferenceRatio
    {
        private readonly int pixel;
        public int Pixel { get { return pixel; } }

        private readonly float polar;
        public float Polar { get { return polar; } }

        private readonly PolarAxis axis;
        public PolarAxis Axis { get { return axis; } }

        public ReferenceRatio(int pixel, float polar, PolarAxis axis)
        {
            this.pixel = pixel;
            this.polar = polar;
            this.axis = axis;
        }

        // Return a new ReferenceRatio scaling the Pixel value with the given ratio
        public ReferenceRatio Scale(double ratio)
        {
            return new ReferenceRatio((int) (pixel * ratio), polar, axis);
        }

        public override string ToString()
        {
            return $"ReferenceRatio {{ pixel = {pixel}, polar = {polar}, axis = {axis} }}";
        }

        public enum PolarAxis
        {
            Latitude,
            Longitude
        }
    }
}
