//ViewTreeNode is a wrapper around raw tree data. This allows view frameworks such as D3
//to make the changes it requires to the ViewTreeNode, leaving the raw data intact so it
//can be posted or past to a backend unaltered. 
var AdvancedSearch;
(function (AdvancedSearch) {
    class ViewTreeNode {
        constructor(item, childProperty) {
            this.Item = item;
            this.Children = null;
            this.ChildProperty = childProperty;
            this.ID = this.GenerateUID();
            this.Index = 0;
        }
        Build() {
            if (this.Item === null) {
                console.log("Cannot build because ViewTreeNode Item is null");
                return;
            }
            var me = this;
            var childDataList = this.Item[this.ChildProperty];
            if (childDataList && childDataList.length > 0) {
                this.Children = [];
                var i;
                for (i = 0; i < childDataList.length; i++) {
                    var item = childDataList[i];
                    var treenode = me.AddChildItem(item);
                    treenode.Index = i;
                }
            }
        }
        Rebuild() {
            this.Children = null;
            this.Build();
        }
        AddChildItem(item) {
            if (!(this.Item instanceof AdvancedSearch.AndOrCondition)) {
                console.log("Cannot add child to condition that is not an AND/OR");
            }
            else {
                var child = new ViewTreeNode(item, this.ChildProperty);
                if (this.Children === null) {
                    this.Children = [];
                }
                this.Children.push(child);
                child.Build();
                return child;
            }
        }
        RemoveChild(treenode) {
            //console.log("ViewTreeNode.RemoveChild started: ");
            //console.log(treenode);
            if (!(this.Item instanceof AdvancedSearch.AndOrCondition)) {
                console.log("Cannot remove child " + treenode.ID + " from condition that is not an AND/OR");
            }
            else {
                var item = this.Item;
                var i;
                for (i = 0; i < this.Children.length; i++) {
                    if (treenode.ID === this.Children[i].ID) {
                        item.Conditions.splice(i, 1);
                        this.Children.splice(i, 1);
                        break;
                    }
                }
            }
        }
        //https://stackoverflow.com/a/6248722
        GenerateUID() {
            // I generate the UID from two parts here 
            // to ensure the random number provide enough bits.
            var firstPart = (Math.random() * 46656) | 0;
            var secondPart = (Math.random() * 46656) | 0;
            return ("000" + firstPart.toString(36)).slice(-3) + ("000" + secondPart.toString(36)).slice(-3);
        }
    }
    AdvancedSearch.ViewTreeNode = ViewTreeNode;
})(AdvancedSearch || (AdvancedSearch = {}));
//# sourceMappingURL=ViewTreeNode.js.map