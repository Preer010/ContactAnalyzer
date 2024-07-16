using System;
using System.Collections.Generic;
using System.Linq;

namespace FatsharkTest.Utils;


// I used this as a reference when writing this algorithm
// https://web.stanford.edu/~cpiech/cs221/handouts/kmeans.html
public class KMeans
{
    private int _k;
    private int _maxIt;

    public List<GeoPoint> GeoPoints { get; private set; }
    private List<Centroid> _centroids;

    public KMeans(List<(double, double, string)> coords, int inK, int inMaxIt)
    {
        _k = inK;
        _maxIt = inMaxIt;

        GeoPoints = coords.Select(coord => new GeoPoint(coord.Item1, coord.Item2, coord.Item3)).ToList();
        _centroids = new List<Centroid>();
    }
    public KMeans(List<GeoPoint> coords, int inK, int inMaxIt)
    {
        _k = inK;
        _maxIt = inMaxIt;
        GeoPoints = coords;
        _centroids = new List<Centroid>();
    }

    public void Run()
    {
        InitCentroids();
        for (int i = 0; i < _maxIt; i++)
        {
            bool centroidDirty = AssignPointsToClusters();
            if (!centroidDirty)
            {
                break;
            }

            UpdateCentroids();
        }
    }

    private void InitCentroids()
    {
        Random rand = new Random();
        HashSet<int> chosenIndices = new HashSet<int>();

        int counter = 0;
        while (_centroids.Count < _k && counter < (_k + _maxIt))
        {
            int index = rand.Next(GeoPoints.Count);
            if (!chosenIndices.Contains(index))
            {
                chosenIndices.Add(index);
                GeoPoint chosenPoint = GeoPoints[index];
                _centroids.Add(new Centroid(chosenPoint.Latitude, chosenPoint.Longitude));
            }
            counter++;
        }
    }

    private bool AssignPointsToClusters()
    {
        bool centroidDirty = false;

        foreach (var point in GeoPoints)
        {
            double minDist = double.MaxValue;
            int closestCentroid = -1;

            for (int i = 0; i < _centroids.Count; i++)
            {
                double dist = CalculateDistance(point, _centroids[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestCentroid = i;
                }
            }

            if (point.ClusterId != closestCentroid)
            {
                point.ClusterId = closestCentroid;
                centroidDirty = true;
            }
        }

        return centroidDirty;
    }

    private double CalculateDistance(GeoPoint p1, Centroid c)
    {
        // Found these calculations online to use for latitude and longitude
        // This could probably be rewritten to something more simple to help bigger datasets
        
        // earths radius in km
        double radius = 6371;
        double varPI = Math.PI / 180;
        
        double deltaLat = (c.Latitude - p1.Latitude) * varPI;
        double deltaLon = (c.Longitude - p1.Longitude) * varPI;

        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(p1.Latitude * varPI) * Math.Cos(c.Latitude * varPI) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        
        double cFactor = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double dist = radius * cFactor;
        return dist;
    }

    private void UpdateCentroids()
    {
        var newCentroids = new List<Centroid>();

        // Create new centroids with the average cordinate of its geopoints
        for (int i = 0; i < _k; i++)
        {
            var clusterPoints = GeoPoints.Where(gP => gP.ClusterId == i).ToList();

            if (clusterPoints.Any())
            {
                // find the average coordinate of the geopoints in the cluster
                double newLat = clusterPoints.Average(cP => cP.Latitude);
                double newLon = clusterPoints.Average(cP => cP.Longitude);
                newCentroids.Add(new Centroid(newLat, newLon));
            }
            else
            {
                newCentroids.Add(_centroids[i]);
            }
        }

        _centroids = newCentroids;
    }
    

}

public class GeoPoint
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public string Postal { get; set; }
    public int ClusterId { get; set; }

    public GeoPoint(double latitude, double longitude, string postal)
    {
        Latitude = latitude;
        Longitude = longitude;
        Postal = postal;
        ClusterId = -1;
    }
}

public class Centroid
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public Centroid(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}