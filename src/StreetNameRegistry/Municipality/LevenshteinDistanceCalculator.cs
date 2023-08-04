namespace StreetNameRegistry.Municipality
{
    using System;
    
    public class LevenshteinDistanceCalculator
    {
        public static double CalculatePercentage(string source, string target)
        {
            int distance = Fastenshtein.Levenshtein.Distance(source, target);
            int maxLength = Math.Max(source.Length, target.Length);

            double percentageDifference = (double)distance / maxLength * 100.0;
            return percentageDifference;
        }
    }
}
