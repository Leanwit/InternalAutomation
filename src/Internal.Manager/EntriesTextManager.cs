using System;
using System.Collections.Generic;
using Model;
using Manager.Util;

namespace Manager
{
    public class EntriesTextManager
    {
        public string[] entries;
        public List<TimecampItem> timecampItems { get; private set; }

        public EntriesTextManager()
        {
            this.timecampItems = new List<TimecampItem>();
            string text = System.IO.File.ReadAllText(@"internal.txt");
            string[] entries = text.Split("\r\n");
            string[] task;
            foreach (var x in entries)
            {
                task = x.Split("   ");
                timecampItems.Add(this.EntriesToTimecampItem(task));
            }
        }


        public TimecampItem EntriesToTimecampItem(string[] task)
        {
            string comment = task[4].Trim();
            string hour = task[3].Replace(",", ".");
            string date = task[0];
            var items = new TimecampItem();
            items.Comment = comment;
            items.Date = date;
            items.Time = hour;
            items.Ticket = InternalHelper.GetTicketByDescription(comment);
            return items;
        }
    }
}