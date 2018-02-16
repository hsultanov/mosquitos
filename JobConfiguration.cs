using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace GantryPointGrey
{
    public class MyConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public MyConfigInstanceCollection Instances
        {
            get { return (MyConfigInstanceCollection)this[""]; }
            set { this[""] = value; }
        }
    }

    public class MyConfigInstanceCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MyConfigInstanceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            //set to whatever Element Property you want to use for a key
            return ((MyConfigInstanceElement)element).ID;
        }
    }

    public class MyConfigInstanceElement : ConfigurationElement
    {
        //Make sure to set IsKey=true for property exposed as the GetElementKey above
        [ConfigurationProperty("id", IsKey = true, IsRequired = true)]
        public string ID
        {
            get { return (string)base["id"]; }
            set { base["id"] = value; }

        }
        [ConfigurationProperty("description", IsRequired = true)]
        public string Description
        {
            get { return (string)base["description"]; }
            set { base["description"] = value; }
        }
        [ConfigurationProperty("script", IsRequired = true)]
        public string Script
        {
            get { return (string)base["script"]; }
            set { base["script"] = value; }
        }

        [ConfigurationProperty("paramFile", IsRequired = true)]
        public string ParamFile
        {
            get { return (string)base["paramFile"]; }
            set { base["paramFile"] = value; }
        }


        [ConfigurationProperty("posesFile", IsRequired = true)]
        public string PosesFile
        {
            get { return (string)base["posesFile"]; }
            set { base["posesFile"] = value; }
        }

        [ConfigurationProperty("modelCad", IsRequired = true)]
        public string ModelCAD
        {
            get { return (string)base["modelCad"]; }
            set { base["modelCad"] = value; }
        }

        [ConfigurationProperty("saveScans", IsRequired = false, DefaultValue = false)]
        public bool SaveScans
        {
            get { return (bool)base["saveScans"]; }
            set { base["saveScans"] = value; }
        }
    }
}
