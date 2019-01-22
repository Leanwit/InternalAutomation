using System;
using System.Collections.Generic;
using Model;
using Manager.Util;

namespace Manager
{
    public class EntriesTextManager
    {
        public List<InternalItem> InternalItem { get; private set; }

        public EntriesTextManager()
        {
            this.InternalItem = new List<InternalItem>();
            string line;  

            System.IO.StreamReader file =
                new System.IO.StreamReader(@"internal.txt");
            while ((line = file.ReadLine()) != null)
            {
                InternalItem.Add(this.EntriesToInternalItem(line.Split("\t")));
            }            
        }


        public InternalItem EntriesToInternalItem(string[] task)
        {
            string comment = task[4].Trim();
            string hour = task[3].Replace(",", ".");
            string date = task[0];
            var items = new InternalItem();
            items.Comment = comment;
            items.Date = date;
            items.Time = hour;
            items.Ticket = InternalHelper.GetTicketByDescription(comment);
            return items;
        }
    }
}