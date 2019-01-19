using System;
using System.Collections.Generic;
using System.Net.Http;
using Model;
using System.Linq;
using System.Text.RegularExpressions;
using Util;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Manager.Util;

namespace Manager
{

    public class TimecampManager
    {
        public static string _token;

        private string From { get; set; }
        private string To { get; set; }

        public TimecampManager(string from, string to, string token)
        {
            this.From = from;
            this.To = to;
            _token = token;
        }

        public async System.Threading.Tasks.Task<List<InternalItem>> GetInfoAsync()
        {

            string baseUrl = $"https://www.timecamp.com/third_party/api/entries/format/json/api_token/{_token}/from/{this.From}/to/{this.To}/";
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage res = await client.GetAsync(baseUrl))
            using (HttpContent content = res.Content)
            {
                string data = string.Empty;
                do
                {
                    try
                    {
                        data = await content.ReadAsStringAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                } while (data.Equals(string.Empty));

                if (data != null)
                {
                    List<TimecampItemApi> timecampItemsApi = JsonConvert.DeserializeObject<List<TimecampItemApi>>(data);

                    foreach (TimecampItemApi item in timecampItemsApi)
                    {
                        item.Task = await GetTasksAsync(item.TaskId);
                    }

                    return this.MapToTimeCampItem(timecampItemsApi);
                }
            }

            return new List<InternalItem>();
        }

        
        private List<InternalItem> MapToTimeCampItem(List<TimecampItemApi> timecampItemsApi)
        {
            List<InternalItem> list = new List<InternalItem>();

            foreach (var item in timecampItemsApi)
            {
                list.Add(new InternalItem
                {
                    Date = item.Date,
                    Activity = item.Task?.Name,
                    Comment = item.Description,
                    Project = item.Task?.Parent?.Parent?.Name,
                    Task = item.Task?.Parent?.Name,
                    Time = TimeHelper.TransformSecondsToInternalTime(item.Duration),
                    Ticket = InternalHelper.GetTicketByDescription(item.Description)
                });
            }

            return list;
        }


        private async System.Threading.Tasks.Task<Task> GetTasksAsync(string taskId)
        {
            string baseUrl = "https://www.timecamp.com/third_party/api/tasks/format/json/api_token/3d09c504b7eda2ab53a51f87a0/task_id/" + taskId;

            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage res = await client.GetAsync(baseUrl))
            using (HttpContent content = res.Content)
            {
                string data = await content.ReadAsStringAsync();
                if (data != null)
                {
                    Task task = JsonConvert.DeserializeObject<Task>(data);
                    if (task.ParentId != null && !task.ParentId.Equals("0"))
                    {
                        task.Parent = await GetTasksAsync(task.ParentId);
                    }

                    return task;
                }
            }

            return new Task();
        }

        private class TimecampItemApi
        {
            public string Date { get; set; }
            public long Duration { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            [JsonProperty("Task_Id")]
            public string TaskId { get; set; }
            public Task Task { get; set; }


        }

        public class Task
        {
            public string Name { get; set; }

            [JsonProperty("Task_Id")]
            public string TaskId { get; set; }

            [JsonProperty("Parent_Id")]
            public string ParentId { get; set; }

            public Task Parent { get; set; }

        }
    }
}