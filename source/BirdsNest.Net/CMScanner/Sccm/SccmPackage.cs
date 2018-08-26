using System.Collections.Generic;

namespace CMScanner.Sccm
{
    public class SccmPackage: ISccmObject
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PackageType PackageType { get; set; }

        public bool IsHighlighted { get; set; }
        public List<SccmPackageProgram> Programs { get; set; }
        public SccmItemType Type { get { return SccmItemType.Package; } }

        public SccmPackage()
        {
            this.Programs = new List<SccmPackageProgram>();
        }

        public SccmPackage(string name, string id)
        {
            this.Programs = new List<SccmPackageProgram>();
            this.Name = name;
            this.ID = id;
        }

        /// <summary>
        /// Add the program to the Programs list, and register the package as the programs parent
        /// </summary>
        /// <param name="program"></param>
        public void AddProgram(SccmPackageProgram program)
        {
            this.Programs.Add(program);
            program.ParentPackage = this;
        }
    }
}
