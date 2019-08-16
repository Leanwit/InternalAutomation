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
            Console.WriteLine("8. Other");

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
                    case "8": return Activity.Other;
                    default:
                        option = Console.ReadLine();
                        break;
                }
            }
        }

        public static string GetPredictedActivityValue(string entryComment)
        {
            string aux = string.Empty;

            if (entryComment.ToLower().Contains("code review"))
            {
                return Activity.Review;
            }

            if (entryComment.ToLower().Contains("support"))
            {
                return Activity.Execution;
            }

            return aux;
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
        public static string Other = "Other";
    }
}