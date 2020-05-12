namespace Manager.Util
{
    using Model;
    using System;
    using System.Text.RegularExpressions;

    public static class InternalHelper
    {
        public static string GetTicketByDescription(string text)
        {
            Regex regex = new Regex("[A-Z0-9]+-[0-9]+");
            var match = regex.Match(text);
            return match.Value;
        }

        public static string GetActivityValue(InternalItem entry)
        {
            Console.WriteLine($"Complete Activity for '{entry.Comment}'");

            Console.WriteLine("1. Analysis");
            Console.WriteLine("2. Execution");
            Console.WriteLine("3. Management");
            Console.WriteLine("4. Review");
            Console.WriteLine("5. Rework");
            Console.WriteLine("6. Tech Leading");
            Console.WriteLine("7. Testing");
            Console.WriteLine("8. Meeting");
            Console.WriteLine("9. Other");

            string option = Console.ReadLine();

            while (true)
            {
                switch (option)
                {
                    case "1": return Activity.Analysis;
                    case "2": return Activity.Execution;
                    case "3": return Activity.Management;
                    case "4": return Activity.Review;
                    case "5": return Activity.Rework;
                    case "6": return Activity.TechLeading;
                    case "7": return Activity.Testing;
                    case "8": return Activity.Meeting;
                    case "0": return Activity.Other;
                    default:
                        option = Console.ReadLine();
                        break;
                }
            }
        }

        public static string GetPredictedActivityValue(string entryComment)
        {
            entryComment = entryComment.ToLower();
            if (entryComment.Contains("code review")) return Activity.Review;
            if (entryComment.Contains("support")) return Activity.Execution;
            if (entryComment.Contains("management")) return Activity.Management;
            if (entryComment.Contains(" tl ")) return Activity.TechLeading;
            if (entryComment.Contains("testing")) return Activity.Testing;
            if (entryComment.Contains("daily") || entryComment.Contains("analysis")) return Activity.Analysis;
            if (entryComment.Contains("meeting")) return Activity.Meeting;

            return string.Empty;
        }

        public static InternalItem GetPredictedProjectValue(InternalItem entry)
        {
            string entryComment = entry.Comment.ToLower();

            if (entryComment.Contains("code review") && entryComment.Contains("dev-"))
            {
                entry.Project = "Development";
                entry.Task = "GDD";
                return entry;
            }

            if (entryComment.Contains("Development Management"))
            {
                entry.Project = "Development";
                entry.Task = "Development";
                return entry;
            }

            return entry;
        }
    }

    public static class Activity
    {
        public static string Analysis = "Analysis";
        public static string Execution = "Execution";
        public static string Management = "Management";
        public static string Review = "Review";
        public static string Rework = "Rework";
        public static string TechLeading = "Tech Leading";
        public static string Testing = "Testing";
        public static string Meeting = "Meeting";
        public static string Other = "Other";
    }
}