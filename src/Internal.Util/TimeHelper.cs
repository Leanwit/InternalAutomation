using System;
using System.Text.RegularExpressions;
namespace Util
{

    public static class TimeHelper
    {
        public static string TransformSecondsToInternalTime(long seconds)
        {
            double aux = (double) seconds / 3600;
            Console.WriteLine(aux);
            double timeDouble = RoundInternalValue(aux);
            return timeDouble.ToString().Replace(",", ".");
        }
        public static string GetInternalTime(string time)
        {

            double timeDouble = GetInternalValue(time);
            timeDouble = RoundInternalValue(timeDouble);
            return timeDouble.ToString().Replace(",", ".");
        }

        public static double GetInternalValue(string time)
        {
            return Math.Round(GetHours(time) + (GetMinutes(time) / 60), 2);
        }

        public static int GetHours(string time)
        {
            MatchCollection hours = Regex.Matches(time, "[0-9]h");
            int hour = 0;
            foreach (Match stringHour in hours)
            {
                hour = int.Parse(stringHour.Value.Replace("h", ""));
            }
            return hour;
        }

        public static double GetMinutes(string time)
        {
            MatchCollection minutes = Regex.Matches(time, "[0-9]*m");
            int minute = 0;
            foreach (Match stringMinutes in minutes)
            {
                int.TryParse(stringMinutes.Value.Replace("m", ""), out minute);
            }
            return minute;
        }

        public static double RoundInternalValue(double time)
        {
            double hour = Math.Truncate(time);
            double extractMinute = time - hour;
            double minute = 0;
            if (extractMinute <= 0.05)
            {
                minute = 0;
            }
            else if (extractMinute <= 0.25 || (extractMinute > 0.25 && extractMinute <= 0.30))
            {
                minute = 0.25;
            }
            else if (extractMinute <= 0.5 || ((extractMinute > 0.5 && extractMinute <= 0.55)))
            {
                minute = 0.5;
            }
            else if (extractMinute <= 0.75 || ((extractMinute > 0.75 && extractMinute <= 0.80)))
            {
                minute = 0.75;
            }
            else
            {
                minute = 0;
                hour += 1;
            }

            return hour + minute;
        }
    }
}