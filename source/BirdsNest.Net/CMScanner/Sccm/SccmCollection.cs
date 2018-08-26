using System.Collections.Generic;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace CMScanner.Sccm
{
    public class SccmCollection: ViewModelBase, ISccmObject
    {
        public SccmItemType Type { get { return SccmItemType.Collection; } }
        public string LimitingCollectionID { get; set; }
        public SccmCollection LimitingCollection { get; set; }
        public CollectionType CollectionType { get; set; }

        private string _comment;
        public string Comment
        {
            get { return this._comment; }
            set
            {
                this._comment = value;
                this.OnPropertyChanged(this, "Comment");
            }
        }

        private string _name;
        public string Name
        {
            get { return this._name; }
            set
            {
                this._name = value;
                this.OnPropertyChanged(this, "Name");
            }
        }

        private string _id;
        public string ID
        {
            get { return this._id; }
            set
            {
                this._id = value.ToUpper();
                this.OnPropertyChanged(this, "ID");
            }
        }

        private bool _ishighlighted;
        public bool IsHighlighted
        {
            get { return this._ishighlighted; }
            set
            {
                this._ishighlighted = value;
                this.OnPropertyChanged(this, "IsHighlighted");
            }
        }

        private bool _islimitingcollection;
        public bool IsLimitingCollection
        {
            get { return this._islimitingcollection; }
            set
            {
                this._islimitingcollection = value;
                this.OnPropertyChanged(this, "IsLimitingCollection");
            }
        }

        private bool _ismemberpresent;
        public bool IsMemberPresent
        {
            get { return this._ismemberpresent; }
            set
            {
                this._ismemberpresent = value;
                this.OnPropertyChanged(this, "IsMemberPresent");
            }
        }

        private int _includeexcludecolcount;
        public int IncludeExcludeCollectionCount
        {
            get { return this._includeexcludecolcount; }
            set
            {
                this._includeexcludecolcount = value;
                this.OnPropertyChanged(this, "IncludeExcludeCollectionCount");
            }
        }

        public SccmCollection() { }

        public SccmCollection(string id, string name, string limitingcollectionid)
        {
            this._name = name;
            this._id = id;
            this.LimitingCollectionID = limitingcollectionid;
            this.IsHighlighted = false;
        }

        public List<SccmCollection> HighlightCollectionPathList()
        {
            this.IsHighlighted = true;
            List<SccmCollection> highlightedcols;

            if (this.LimitingCollection != null)
            {
                highlightedcols = this.LimitingCollection.HighlightLimitingCollectionPathList();
                highlightedcols.Add(this);
            }
            else
            {
                highlightedcols = new List<SccmCollection>();
                highlightedcols.Add(this);
            }
            return highlightedcols;
        }

        public List<SccmCollection> HighlightLimitingCollectionPathList()
        {
            this.IsLimitingCollection = true;
            List<SccmCollection> highlightedcols;

            if (this.LimitingCollection != null)
            {
                highlightedcols = this.LimitingCollection.HighlightLimitingCollectionPathList();
                highlightedcols.Add(this);
            }
            else
            {
                highlightedcols = new List<SccmCollection>();
                highlightedcols.Add(this);
            }
            return highlightedcols;
        }

        public List<SccmCollection> GetCollectionPathList()
        {
            List<SccmCollection> cols;

            if (this.LimitingCollection != null)
            {
                cols = this.LimitingCollection.GetCollectionPathList();
                cols.Add(this);
            }
            else
            {
                cols = new List<SccmCollection>();
                cols.Add(this);
            }
            return cols;
        }
    }
}
