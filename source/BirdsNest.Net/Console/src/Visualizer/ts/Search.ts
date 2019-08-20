

class Search {
    Condition: ICondition;
    Nodes = [];
    Edges = [];
    AddedNodes = 0;


    AddNode() {
        //console.log("AddNode start:");
        //console.log(this);
        var me = this;
        me.AddedNodes++;
        var newNode = new SearchNode;
        this.Nodes.push(newNode);
        newNode.Name = "node" + me.AddedNodes;

        if (me.Nodes.length > 1) {
            var newEdge = new SearchEdge;
            this.Edges.push(newEdge);
            newEdge.Name = "hop" + (me.AddedNodes - 1);
        }

        return newNode;
        //console.log("AddNode end:");
        //console.log(me);
    }


    //remove the node and return the index of the node that was removed
    RemoveNode(node) {
        var me = this;
        var foundindex = -1;
        var i;
        for (i = 0; i < me.Nodes.length; i++) {
            if (foundindex !== -1) { //if found already, start shifting nodes back on in the array
                me.Nodes[i - 1] = me.Nodes[i];
                if (i < me.Edges.length) { me.Edges[i - 1] = me.Edges[i]; }
            }
            else {
                if (me.Nodes[i] === node) {
                    foundindex = i;
                    if (i === 0) {
                        me.Nodes.shift; //remove the first node
                        if (me.Edges.length > 0) { me.Edges.shift; } //if there is an edge, remove that too

                        //we're done
                        return foundindex;
                    }
                }
            }
        }

        if (foundindex !== -1) {
            // pop off the end if the node wasn't first i.e hasn't been removed with shift
            me.Nodes.pop;
            me.Edges.pop;
        }

        return foundindex;
    }

    MoveNodeRight(node) {
        var me = this;
        var i;
        for (i = 0; i < me.Nodes.length; i++) {
            if (me.Nodes[i] === node) {
                if (i === me.Nodes.length - 1) {
                    return false; //can't move any further
                }
                else {
                    me.Nodes[i] = me.Nodes[i + 1];
                    me.Nodes[i + 1] = node;
                    return true;
                }
            }
        }
        return false;
    }

    MoveNodeLeft(node) {
        var me = this;
        var i;

        for (i = 0; i < me.Nodes.length; i++) {
            if (me.Nodes[i] === node) {
                if (i === 0) {
                    return false; //can't move any further
                }
                else {
                    me.Nodes[i] = me.Nodes[i - 1];
                    me.Nodes[i - 1] = node;
                    return true;
                }
            }
        }
        return false;
    }
}


class SearchNode {
    Name: string = "";
    Label: string = "";
}

class SearchEdge {
    Name: string = "";
    Label: string = "";
    Direction: string = ">";
    Min: string = "1";
    Max: string = "1";
}

interface ICondition {
    Type: string;
}

class AndOrCondition implements ICondition {
    Name: "";
    Conditions: ICondition[] = [];

    private _type: string = "AND";
    get Type(): string {
        return this._type;
    }
    set Type(value: string) {
        if (value === "AND" || value === "OR") {
            this._type = value;
        }
        else {
            console.log("Cannot set type of AND/OR condition to " + value)
        }
    }

}

class ConditionBase implements ICondition {
    Type: string = "";
    Name: string = "";
    Property: string = "";
    Value: any;
    Operator: string;
}

class MathCondition extends ConditionBase {
    Value: number = 0;

    constructor() {
        super();
        this.Type = "MATH";
        this.Name = "";
        this.Property = "";
        this.Operator = "=";
    }
}

class StringCondition extends ConditionBase {
    Value: string = "";

    constructor() {
        super();
        this.Type = "STRING";
        this.Name = "";
        this.Property = "";
        this.Operator = "=";
    }
}

function GetCondition(type: string): ICondition {
    var cond: ICondition;
    switch (type) {
        case 'AND': {
            cond = new AndOrCondition();
            break;
        }
        case 'OR': {
            cond = new AndOrCondition();
            cond.Type = 'OR';
            break;
        }
        case 'STRING': {
            cond = new StringCondition();
            break;
        }
        case 'MATH': {
            cond = new MathCondition();
            break;
        }
        default: {
            break;
        }
    }
    return cond;
}

export { StringCondition, MathCondition, ConditionBase, ICondition, AndOrCondition, Search, SearchNode, SearchEdge, GetCondition }