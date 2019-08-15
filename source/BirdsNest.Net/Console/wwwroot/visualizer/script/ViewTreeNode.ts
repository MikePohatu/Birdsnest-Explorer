//ViewTreeNode is a wrapper around raw tree data. This allows view frameworks such as D3
//to make the changes it requires to the ViewTreeNode, leaving the raw data intact so it
//can be posted or past to a backend unaltered. 
class ViewTreeNode {
    Item: object;
    Children: ViewTreeNode[];
    ChildProperty: string;
    ID: string;
    Index: number;

    constructor (item, childProperty) {
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
        var child = new ViewTreeNode(item, this.ChildProperty);
        if (this.Children === null) { this.Children = []; }
        this.Children.push(child);
        child.Build();
        return child;
    }

    RemoveChild(treenode) {
        if (this.Children === null) { console.log("Cannot remove child " + treenode.ID + " from list. No children found"); }

        var i;
        for (i = 0; i < this.Children.length; i++) {
            if (treenode.ID === this.Children[i].ID) {
                this.Children.splice(i, 1);
                break;
            }
        }
        this.Build();
    }

    //https://stackoverflow.com/a/6248722
    GenerateUID() {
        // I generate the UID from two parts here 
        // to ensure the random number provide enough bits.
        var firstPart: number  = (Math.random() * 46656) | 0;
        var secondPart: number  = (Math.random() * 46656) | 0;
        return ("000" + firstPart.toString(36)).slice(-3) + ("000" + secondPart.toString(36)).slice(-3);
    }
}





