namespace Model
{
    public class InternalItem
    {
        public string Date { get; set; }
        public string Activity { get; set; }

        public string Comment { get; set; }

        public string Project { get; set; }

        public string Task { get; set; }
        public string Time { get; set; }

        public string Ticket { get; set; }

        public bool IsSkip
        {
            get
            {
                if (string.IsNullOrEmpty(this.Project))
                {
                    return true;
                }

                return false;
            }
        }
    }
}