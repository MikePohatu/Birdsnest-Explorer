using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UpdateServices.Administration;

namespace WUScanner
{
    public class Update
    {
        public bool IsSuperseded { get; private set; }
        public bool HasSupersededUpdates { get; private set; }
        public bool IsBundle { get; private set; }
        public bool IsPartOfBundle { get; private set; }
        public bool IsDeclined { get; private set; }
        public string KB { get; private set; }
        public string Bulletin { get; private set; }
        public string Description { get; private set; }
        public string Title { get; private set; }
        public string ID { get; private set; }
        public string AdditionalInformationUrls { get; private set; }
        public string CreationDate { get; private set; }


        public Update (IUpdate wsusupdate)
        {
            this.IsSuperseded = wsusupdate.IsSuperseded;
            this.HasSupersededUpdates = wsusupdate.HasSupersededUpdates;
            this.IsDeclined = wsusupdate.IsDeclined;
            this.Title = wsusupdate.Title;
            this.Description = wsusupdate.Description;
            this.ID = wsusupdate.Id.UpdateId.ToString();
            this.AdditionalInformationUrls = string.Join(", ", wsusupdate.AdditionalInformationUrls);
            this.CreationDate = wsusupdate.CreationDate.ToString();

        }
    }
}
