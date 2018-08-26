using System.Collections.Generic;

namespace CMScanner.Sccm
{
    public class SccmPackage: ViewModelBase, ISccmObject
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PackageType PackageType { get; set; }

        protected bool _ishighlighted;
        public bool IsHighlighted
        {
            get { return this._ishighlighted; }
            set
            {
                this._ishighlighted = value;
                this.OnPropertyChanged(this, "IsHighlighted");
            }
        }

        protected List<SccmPackageProgram> _programs;
        public List<SccmPackageProgram> Programs
        {
            get { return this._programs; }
            set
            {
                this._programs = value;
                this.OnPropertyChanged(this, "Programs");
            }

        }

        public SccmItemType Type { get { return SccmItemType.Package; } }

        public SccmPackage()
        {
            this._programs = new List<SccmPackageProgram>();
        }

        public SccmPackage(string name, string id)
        {
            this._programs = new List<SccmPackageProgram>();
            this.Name = name;
            this.ID = id;
        }

        /// <summary>
        /// Add the program to the Programs list, and register the package as the programs parent
        /// </summary>
        /// <param name="program"></param>
        public void AddProgram(SccmPackageProgram program)
        {
            this._programs.Add(program);
            program.ParentPackage = this;
        }
    }
}
