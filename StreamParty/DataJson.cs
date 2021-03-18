using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmParty
{
    class FeedsJson
    {

        public class Rootobject
        {
            public FeedProprety[] Property1 { get; set; }
        }

        public class FeedProprety
        {
            public string username { get; set; }
            public Owner owner { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string license { get; set; }
            public bool history { get; set; }
            public bool enabled { get; set; }
            public string visibility { get; set; }
            public object unit_type { get; set; }
            public object unit_symbol { get; set; }
            public string last_value { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public bool status_notify { get; set; }
            public int status_timeout { get; set; }
            public string status { get; set; }
            public string key { get; set; }
            public Group group { get; set; }
            public Group1[] groups { get; set; }
            public object[] feed_webhook_receivers { get; set; }
            public Feed_Status_Changes[] feed_status_changes { get; set; }
        }

        public class Owner
        {
            public int id { get; set; }
            public string username { get; set; }
        }

        public class Group
        {
            public int id { get; set; }
            public string key { get; set; }
            public string name { get; set; }
            public int user_id { get; set; }
        }

        public class Group1
        {
            public int id { get; set; }
            public string key { get; set; }
            public string name { get; set; }
            public int user_id { get; set; }
        }

        public class Feed_Status_Changes
        {
            public DateTime created_at { get; set; }
            public string from_status { get; set; }
            public string to_status { get; set; }
            public object email_sent { get; set; }
            public object email_sent_to { get; set; }
        }
    }
}