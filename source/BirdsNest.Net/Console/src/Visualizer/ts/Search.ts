import { webcrap } from "../../shared/webcrap/webcrap";

//pojo to hold the search data
class Search {
    Condition: ICondition;
    Nodes: SearchNode[] = [];
    Edges: SearchEdge[] = [];
    AddedNodes: number = 0;
}

function AddNode(search: Search) {
    //console.log("AddNode start:");
    //console.log(this);
    search.AddedNodes++;
    var newNode = new SearchNode;
    search.Nodes.push(newNode);
    newNode.Name = "node" + search.AddedNodes;

    if (search.Nodes.length > 1) {
        var newEdge = new SearchEdge;
        search.Edges.push(newEdge);
        newEdge.Name = "hop" + (search.AddedNodes - 1);
    }

    return newNode;
    //console.log("AddNode end:");
    //console.log(me);
}

//remove the node and return the index of the node that was removed
function RemoveNode(node: SearchNode, search: Search) {
    //console.log("RemoveNode started");
    //console.log(node);
    //console.log(search);
    var foundindex = -1;
    var i;
    for (i = 0; i < search.Nodes.length; i++) {
        if (foundindex !== -1) { //if found already, start shifting nodes back on in the array
            search.Nodes[i - 1] = search.Nodes[i];
            if (i < search.Edges.length) { search.Edges[i - 1] = search.Edges[i]; }
        }
        else {
            if (search.Nodes[i] === node) {
                //console.log("found node");
                foundindex = i;
                if (i === 0) {
                    search.Nodes.shift(); //remove the first node
                    if (search.Edges.length > 0) { search.Edges.shift(); } //if there is an edge, remove that too

                    //console.log(search);
                    //we're done
                    return foundindex;
                }
            }
        }
    }

    if (foundindex !== -1) {
        //console.log("popping node");
        // pop off the end if the node wasn't first i.e hasn't been removed with shift
        search.Nodes.pop();
        search.Edges.pop();
    }
    //console.log(search);
    return foundindex;
}

function MoveNodeRight(node: SearchNode, search: Search) {
    var i;
    for (i = 0; i < search.Nodes.length; i++) {
        if (search.Nodes[i] === node) {
            if (i === search.Nodes.length - 1) {
                return false; //can't move any further
            }
            else {
                search.Nodes[i] = search.Nodes[i + 1];
                search.Nodes[i + 1] = node;
                return true;
            }
        }
    }
    return false;
}

function MoveNodeLeft(node: SearchNode, search: Search) {
    var i;

    for (i = 0; i < search.Nodes.length; i++) {
        if (search.Nodes[i] === node) {
            if (i === 0) {
                return false; //can't move any further
            }
            else {
                search.Nodes[i] = search.Nodes[i - 1];
                search.Nodes[i - 1] = node;
                return true;
            }
        }
    }
    return false;
}

function GetNode(name: string, search: Search): SearchNode {
    var i = 0;
    for (i = 0; i < search.Nodes.length; i++) {
        if (search.Nodes[i].Name === name) {
            return search.Nodes[i];
        }
    }
    return null;
}

function GetEdge(name: string, search: Search): SearchEdge {
	var i = 0;
	for (i = 0; i < search.Edges.length; i++) {
		if (search.Edges[i].Name === name) {
			return search.Edges[i];
		}
	}
	return null;
}

class SearchNode {
    Name: string = "";
    Label: string = "";
    x: number = -100;
    y: number = -100;
    index: number = 0;
    ID: string = "";

    constructor() {
        this.ID = webcrap.misc.generateUID();
    }
}

class SearchEdge {
    Name: string = "";
    Label: string = "";
    Direction: string = ">";
    Min: string = "1";
    Max: string = "1";
    ID: string = "";

    constructor() {
        this.ID = webcrap.misc.generateUID();
    }
}

interface ICondition {
    Type: string;
}

function IsConditionValid(condition: ICondition): boolean {
    if (condition instanceof AndOrCondition) {

        var i;
        for (i = 0; i < condition.Conditions.length; i++) {
            if (IsConditionValid(condition.Conditions[i]) === false) { return false; }
        }
    }
    else if (condition instanceof ConditionBase) {
        if (webcrap.misc.isNullOrWhitespace(condition.Type)) { return false; }
        if (webcrap.misc.isNullOrWhitespace(condition.Name)) { return false; }
        if (webcrap.misc.isNullOrWhitespace(condition.Property)) { return false; }
        if (webcrap.misc.isNullOrWhitespace(condition.Operator)) { return false; }
        return true;
    }
}

class AndOrCondition implements ICondition {
    Type: string = "ANDOR";
    Conditions: ICondition[] = [];
}

class AndCondition extends AndOrCondition {
    Type: string = "AND";
}

class OrCondition extends AndOrCondition {
    Type: string = "OR";
}

var ConditionOperators = {
    "MATH": ["=", ">", "<", "!="],
    "STRING": ["=", "StartsWith", "EndsWith", "Contains"]
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
    CaseSensitive: boolean = true;

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
            cond = new AndCondition();
            break;
        }
        case 'OR': {
            cond = new OrCondition();
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

export {
    StringCondition,
    MathCondition,
    ConditionBase,
    ICondition,
    OrCondition,
    AndCondition,
    AndOrCondition,
    Search,
    SearchNode,
    SearchEdge,
    GetCondition,
    AddNode,
    RemoveNode,
    MoveNodeRight,
    MoveNodeLeft,
    IsConditionValid,
	GetNode,
	GetEdge,
    ConditionOperators
}