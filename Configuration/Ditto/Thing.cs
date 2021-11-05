using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Verhaeg.IoT.Configuration.Ditto
{
    public class Thing
    {
        public string ns { private set; get; }
        public string cs { private set; get; }
        public string device_type { private set; get; }
        public string data_type { private set; get; }
        public string name { private set; get; }
        public string id { private set; get; }

        private bool bId;

        public Thing(string device_type, string data_type, string name, string id)
        {
            this.ns = "Verhaeg";
            this.cs = "IoT";
            this.device_type = device_type;
            this.data_type = data_type;
            this.name = name;
            this.id = id;
            this.bId = true;
        }

        public Thing(string device_type, string data_type, string name)
        {
            this.ns = "Verhaeg";
            this.cs = "IoT";
            this.device_type = device_type;
            this.data_type = data_type;
            this.name = name;
            this.id = "";
            this.bId = false;
        }

        public Thing(string str)
        {
            int ns_index = str.IndexOf(".");
            this.ns = str.Substring(0, ns_index );
            str = str.Remove(0, ns_index + 1);

            int cs_index = str.IndexOf(".");
            this.cs = str.Substring(0, cs_index);
            str = str.Remove(0, cs_index + 1);

            int device_type_index = str.IndexOf(".");
            this.device_type = str.Substring(0, device_type_index);
            str = str.Remove(0, device_type_index + 1);

            int data_type_index = str.IndexOf(":");
            this.data_type = str.Substring(0, data_type_index);
            str = str.Remove(0, data_type_index + 1);

            if (str.Contains("."))
            {
                int name_index = str.IndexOf(".");
                this.name = str.Substring(0, name_index);
                str = str.Remove(0, name_index);

                str = str.Remove(0, 1);
                this.id = str;
                this.bId = true;
            }
            else
            {
                this.name = str;
                this.id = "";
                this.bId = false;
            }
        }

        public string ditto_thingId
        {
            get
            {
                if (bId == true)
                {
                    return ns + "." + cs + "." + device_type + "." + data_type + ":" + name + "." + id;
                }
                else
                {
                    return ns + "." + cs + "." + device_type + "." + data_type + ":" + name;
                }
            }
        }

    }
}
