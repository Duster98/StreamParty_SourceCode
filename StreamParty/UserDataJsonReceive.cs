using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmParty
{
    class UserDataJsonReceive
    {
        public class Rootobject
        {
            public User user { get; set; }
            public Profile profile { get; set; }
            public Sidebar sidebar { get; set; }
            public Navigation navigation { get; set; }
            public Throttle throttle { get; set; }
            public object[] system_messages { get; set; }
        }

        public class User
        {
            public int id { get; set; }
            public string name { get; set; }
            public string color { get; set; }
            public string username { get; set; }
            public string role { get; set; }
            public int default_group_id { get; set; }
            public object default_dashboard_id { get; set; }
            public string time_zone { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public Subscription subscription { get; set; }
            public object beta_flags { get; set; }
        }

        public class Subscription
        {
            public string status { get; set; }
            public Limits limits { get; set; }
            public Plan plan { get; set; }
        }

        public class Limits
        {
            public int feeds { get; set; }
            public int dashboards { get; set; }
            public int groups { get; set; }
            public int data_ttl { get; set; }
            public int data_rate { get; set; }
        }

        public class Plan
        {
            public string name { get; set; }
            public float price { get; set; }
            public string interval { get; set; }
            public string stripe_id { get; set; }
            public Base_Limits base_limits { get; set; }
            public bool free { get; set; }
            public bool paid { get; set; }
        }

        public class Base_Limits
        {
            public int feeds { get; set; }
            public int dashboards { get; set; }
            public int groups { get; set; }
            public int data_ttl { get; set; }
            public int data_rate { get; set; }
        }

        public class Profile
        {
            public string name { get; set; }
            public string username { get; set; }
            public string time_zone { get; set; }
            public DateTime created_at { get; set; }
            public object[] connected_accounts { get; set; }
            public Subscription1 subscription { get; set; }
            public bool has_billing_history { get; set; }
            public bool has_payment_source { get; set; }
            public int account_balance { get; set; }
            public object beta_discount { get; set; }
            public Action_Items action_items { get; set; }
            public object[] promotion_discounts { get; set; }
            public object[] coupons { get; set; }
        }

        public class Subscription1
        {
            public object id { get; set; }
            public Plan1 plan { get; set; }
            public object created_at { get; set; }
            public string status { get; set; }
            public Limits1 limits { get; set; }
            public int price { get; set; }
            public object[] upgrades { get; set; }
        }

        public class Plan1
        {
            public string name { get; set; }
            public float price { get; set; }
            public string interval { get; set; }
            public string stripe_id { get; set; }
            public Base_Limits1 base_limits { get; set; }
            public bool free { get; set; }
            public bool paid { get; set; }
        }

        public class Base_Limits1
        {
            public int feeds { get; set; }
            public int dashboards { get; set; }
            public int groups { get; set; }
            public int data_ttl { get; set; }
            public int data_rate { get; set; }
        }

        public class Limits1
        {
            public int feeds { get; set; }
            public int dashboards { get; set; }
            public int groups { get; set; }
            public int data_ttl { get; set; }
            public int data_rate { get; set; }
        }

        public class Action_Items
        {
            public object[] pending_shares { get; set; }
        }

        public class Sidebar
        {
            public int feed_count { get; set; }
            public int group_count { get; set; }
            public int dashboard_count { get; set; }
            public int active_data_rate { get; set; }
        }

        public class Navigation
        {
            public Feeds feeds { get; set; }
            public Dashboards dashboards { get; set; }
            public Devices devices { get; set; }
            public Triggers triggers { get; set; }
        }

        public class Feeds
        {
            public Record[] records { get; set; }
            public int count { get; set; }
        }

        public class Record
        {
            public int id { get; set; }
            public string name { get; set; }
            public string key { get; set; }
            public DateTime updated_at { get; set; }
            public string last_value { get; set; }
        }

        public class Dashboards
        {
            public object[] records { get; set; }
            public int count { get; set; }
        }

        public class Devices
        {
            public object[] records { get; set; }
        }

        public class Triggers
        {
            public object[] records { get; set; }
        }

        public class Throttle
        {
            public int data_rate_limit { get; set; }
            public int active_data_rate { get; set; }
            public int authentication_rate { get; set; }
            public int subscribe_authorization_rate { get; set; }
            public int publish_authorization_rate { get; set; }
            public int hourly_ban_rate { get; set; }
            public object mqtt_ban_error_message { get; set; }
        }
    }
}
