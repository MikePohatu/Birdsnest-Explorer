using Microsoft.UpdateServices.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WUScanner
{
    public class UpdateResults
    {
        public List<object> Updates { get; private set; } = new List<object>();

        /// <summary>
        /// Mappings of superseded updates. Key Value pairs in format update_id,superseding_update_id
        /// </summary>
        public List<object> SupersededUpdates { get; private set; } = new List<object>();
        public List<object> BundledUpdates { get; private set; } = new List<object>();
    }
}
