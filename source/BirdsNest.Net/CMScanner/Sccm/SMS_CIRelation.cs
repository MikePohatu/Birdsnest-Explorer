namespace CMScanner.Sccm
{
    public class SMS_CIRelation: ViewModelBase
    {
        private string _fromciid;
        public string FromCIID
        {
            get { return this._fromciid; }
            set
            {
                this._fromciid = value;
                this.OnPropertyChanged(this, "FromCIID");
            }
        }

        private string _tociid;
        public string ToCIID
        {
            get { return this._tociid; }
            set
            {
                this._tociid = value;
                this.OnPropertyChanged(this, "ToCIID");
            }
        }

        private int _relationtype;
        public int RelationType
        {
            get { return this._relationtype; }
            set
            {
                this._relationtype = value;
                this.OnPropertyChanged(this, "RelationType");
            }
        }
    }
}
