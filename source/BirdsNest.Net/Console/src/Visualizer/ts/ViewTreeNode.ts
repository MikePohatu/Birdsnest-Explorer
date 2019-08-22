//ViewTreeNode is a wrapper around raw tree data. This allows view frameworks such as D3
//to make the changes it requires to the ViewTreeNode, leaving the raw data intact so it
//can be posted or past to a backend unaltered. 
import { webcrap } from "../../Shared/webcrap/webcrap";

export default class ViewTreeNode<T> {
    Item: T;
    Children: ViewTreeNode<T>[];
    ChildProperty: string;
    ID: string;
    Index: number;
    RectHeight: number;
    RectWidth: number;
    Depth: number = 0;
    Parent: ViewTreeNode<T>;

    constructor(item: any, childProperty: string, parent: ViewTreeNode<T>) {
        this.Item = item;
        this.Children = null;
        this.ChildProperty = childProperty;
        this.ID = webcrap.misc.generateUID();
        this.Index = 0;
        this.Parent = parent;
        if (parent !== null) { this.Depth = parent.Depth + 1; }
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
                treenode.Build();

            }
        }
    }

    Rebuild() {
        this.Children = null;
        this.Build();
    }

    //traverse up the tree and rebuild from the root of tree
    BuildFromRoot() {
        if (this.Parent === null) {
            this.Rebuild();
        }
        else {
            this.Parent.BuildFromRoot();
        }
    }

    AddChildItem(item: T) {
        var child = new ViewTreeNode(item, this.ChildProperty, this);
        child.Depth = this.Depth + 1;
        if (this.Children === null) { this.Children = []; }
        this.Children.push(child);
        return child;
    }

    AddChild(child: ViewTreeNode<T>) {
        if (child.Item === null) {
            console.error("Cant add TreeNode with empty Item");
            return;
        }

        child.Depth = this.Depth + 1;
        if (this.Children === null) { this.Children = []; }
        if (this.Item[this.ChildProperty] === null) { this.Item[this.ChildProperty] = []; }

        this.Children.push(child);
        this.Item[this.ChildProperty].push(child.Item);
        return child;
    }

    RemoveChild(treenode: ViewTreeNode<T>) {
        console.log("ViewTreeNode.RemoveChild started: ");
        console.log(this);
        var item = this.Item;
        var i;
        for (i = 0; i < this.Children.length; i++) {
            if (treenode.ID === this.Children[i].ID) {
                console.log('removing child: ' + treenode.ID); 
                var removeditem = item[this.ChildProperty].splice(i, 1);
                var removednode = this.Children.splice(i, 1);
                console.log(removeditem);
                console.log(removednode);
                break;
            }
        }
    }

    ChangeParent(newparent: ViewTreeNode<T>) {
        this.Parent = newparent;
        if (this.Parent !== null) {
            this.Parent.Rebuild();
        }
    }
}