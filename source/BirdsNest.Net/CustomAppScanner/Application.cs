using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ADScanner.ActiveDirectory;
using FSScanner;

namespace CustomAppScanner
{
    public class Application
    {
        public string ScanId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("business_owner")]
        public ADUser BusinessOwner { get; set; }

        [JsonProperty("technical_sme")]
        public ADUser TechnicalSME { get; set; }

        [JsonProperty("access_groups")]
        public List<ADGroup> AccessGroups { get; set; }

        [JsonProperty("svc_users")]
        public List<ADUser> SvcUsers { get; set; }

        [JsonProperty("folders_read")]
        public List<Folder> FoldersToReadFrom { get; set; }

        [JsonProperty("folders_write")]
        public List<Folder> FoldersToWriteTo { get; set; }
    }
}
