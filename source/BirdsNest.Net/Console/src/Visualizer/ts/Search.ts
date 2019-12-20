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


interface ISearchItem {
    Type: string;
    Name: string;
    Label: string;
}

class SearchNode implements ISearchItem {
    Type: string = "SearchNode";
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

class SearchEdge implements ISearchItem {
    Type: string = "SearchEdge";
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

function ItemNamedExists(root: AndOrCondition, name: string): boolean {
    let i;
    for (i = 0; i < root.Conditions.length; i++) {
        let cond: ICondition = root.Conditions[i];

        if (cond.Type === "OR" || cond.Type === "AND") {
            if (ItemNamedExists((cond as AndOrCondition), name)) { return true; }
        }
        else {
            if ((cond as ConditionBase).Name === name) {
                return true;
            }
        } 
    }

    return false;
}

function RemoveConditionsForName(root: AndOrCondition, name: string): void {
    //console.log("RemoveConditionsForName");
    //console.log(root);
    var i;
    for (i = 0; i < root.Conditions.length; i++) {
        let cond: ICondition = root.Conditions[i];
        //let condbase = cond as ConditionBase;
        //console.log(condbase);

        if (cond.Type === "OR" || cond.Type === "AND") {
            //console.log("recursive remove");
            RemoveConditionsForName((cond as AndOrCondition), name);
        }
        else {
            if ((cond as ConditionBase).Name === name) {
                //console.log("trying to splice " + i);
                root.Conditions.splice(i, 1);
                i--; //bounce i backwards because an item is now gone
            }
        } 
    }
    //console.log("finished");
}

function ReplaceCondition(root: AndOrCondition, oldcondition: ICondition, newcondition: ICondition): void {
    //console.log("RemoveConditionsForName");
    //console.log(root);
    var i;
    for (i = 0; i < root.Conditions.length; i++) {
        let cond: ICondition = root.Conditions[i];
        //let condbase = cond as ConditionBase;
        //console.log(condbase);

        if (cond.Type === "OR" || cond.Type === "AND") {
            //console.log("recursive remove");
            ReplaceCondition((cond as AndOrCondition), oldcondition, newcondition);
        }
        else {
            if (cond === oldcondition) {
                //console.log("recursive remove");
                root.Conditions[i] = newcondition;
            }
        } 
    }
    //console.log("finished");
}

class AndCondition extends AndOrCondition {
    Type: string = "AND";
}

class OrCondition extends AndOrCondition {
    Type: string = "OR";
}

var ConditionOperators = {
    "NUMBER": ["=", ">", "<", "<=", ">="],
    "STRING": ["=", "StartsWith", "EndsWith", "Contains"],
    "BOOLEAN": ["="]
}

class ConditionBase implements ICondition {
    Type: string = "";
    Name: string = "";
    Property: string = "";
    Not: boolean = false;
    Value: any;
    Operator: string;
}

class NumberCondition extends ConditionBase {
    Value: number = 0;

    constructor() {
        super();
        this.Type = "NUMBER";
        this.Name = "";
        this.Property = "";
        this.Operator = "=";
        this.Not = false;
    }
}

class BooleanCondition extends ConditionBase {
    Value: boolean = true;

    constructor() {
        super();
        this.Type = "BOOLEAN";
        this.Name = "";
        this.Property = "";
        this.Operator = "=";
        this.Not = false;
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
        this.Not = false;
    }
}

var SearchTypes = {
    "SearchEdge": "SearchEdge",
    "SearchNode": "SearchNode",
    "StringCondition": "StringCondition",
    "NumberCondition": "NumberCondition",
    "BooleanCondition": "BooleanCondition",
    "OrCondition": "OrCondition",
    "AndCondition": "AndCondition",
    "Search": "Search",
    "Number": "NUMBER",
    "String": "STRING",
    "Boolean": "BOOLEAN",
    "And": "AND",
    "Or": "OR"
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
        case 'NUMBER': {
            cond = new NumberCondition();
            break;
        }
        case 'BOOLEAN': {
            cond = new BooleanCondition();
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
    NumberCondition,
    BooleanCondition,
    ConditionBase,
    ICondition,
    ISearchItem,
    OrCondition,
    AndCondition,
    AndOrCondition,
    Search,
    SearchNode,
    SearchEdge,
    RemoveConditionsForName,
    ReplaceCondition,
    ItemNamedExists,
    SearchTypes,
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