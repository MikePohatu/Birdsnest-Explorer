// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
import webcrap from "@/assets/ts/webcrap/webcrap"
import { Dictionary } from "@/assets/ts/webcrap/misccrap";

export const ConditionOperators: Dictionary<string[]> = {
    "number": ["=", ">", "<", "<=", ">="],
    "string": ["=", "StartsWith", "EndsWith", "Contains"],
    "boolean": ["="]
}

// these ↓ need to match these ^
export const ConditionType = {
    Number: "number",
    String: "string",
    Boolean: "boolean",
    And: "AND",
    Or: "OR",
}

export const NewConditionType = {
    Value: "VALUE",
    And: "AND",
    Or: "OR",
}

export const SearchItemType = {
    SearchNode: "SearchNode",
    SearchEdge: "SearchEdge"
}

export interface SearchItem {
    type: string;
    name: string;
    label: string;
    id: string;
}

export class SearchNode implements SearchItem {
    type = SearchItemType.SearchNode;
    name = "";
    label = "";
    labels = [];
    index = 0;
    id = "";
    dbId = "";

    constructor() {
        this.id = webcrap.misc.generateUID();
    }
}

export function copyNode(node: SearchNode): SearchNode {
    const newNode = new SearchNode();
    newNode.name = node.name;
    newNode.label = node.label;
    newNode.index = node.index;
    return newNode;
}

export function importNode(sourceNode: SearchNode, destNode: SearchNode): void {
    destNode.name = sourceNode.name;
    destNode.label = sourceNode.label;
    destNode.index = sourceNode.index;
    destNode.dbId = sourceNode.dbId;
    destNode.labels = Array.from(sourceNode.labels);
}

export function importEdge(source: SearchEdge, dest: SearchEdge): void {
    dest.name = source.name;
    dest.label = source.label;
    dest.max = source.max;
    dest.min = source.min;
    dest.label = source.label;
    dest.direction = source.direction;
}

export class SearchEdge implements SearchItem {
    type = SearchItemType.SearchEdge;
    name = "";
    label = "";
    direction = ">";
    min = 1;
    max = 1;
    id = "";

    constructor() {
        this.id = webcrap.misc.generateUID();
    }
}

export function copyEdge(edge: SearchEdge): SearchEdge {
    const newEdge = new SearchEdge();
    newEdge.name = edge.name;
    newEdge.label = edge.label;
    newEdge.direction = edge.direction;
    newEdge.min = edge.min;
    newEdge.max = edge.max;
    return newEdge;
}

export interface Condition {
    type: string;
    id: string;
}

export class AndOrCondition implements Condition {
    type: string;
    id: string;
    conditions: Condition[] = [];
    constructor(type: string) {
        this.id = webcrap.misc.generateUID();
        this.type = type;
    }
}

export class ValueCondition implements Condition {
    type = "";
    id: string;
    name = "";
    property = "";
    not = false;
    caseSensitive = false;
    value: string | number | boolean;
    operator = "=";

    constructor(type: string) {
        if (Object.keys(ConditionOperators).includes(type)) {
            this.type = type;
        } else {
            // eslint-disable-next-line
            console.error("Invalid condition type: " + type);
        }
        this.id = webcrap.misc.generateUID();
    }
}

export class Search {
    condition: AndOrCondition = new AndOrCondition(ConditionType.And);  //root should always be an AndOr 'wrapper' condition
    nodes: SearchNode[] = [];
    edges: SearchEdge[] = [];
    includeDisabled: false;
    addedNodes = 0;
}

export function copySearch(search: Search): Search {
    const newsearch = new Search();
    const oldsearch = search as Search;
    newsearch.addedNodes = oldsearch.addedNodes;
    newsearch.condition = copyAndOrCondition(oldsearch.condition as AndOrCondition);
    newsearch.includeDisabled = oldsearch.includeDisabled;
    oldsearch.nodes.forEach((n) => {
        newsearch.nodes.push(copyNode(n));
    });
    oldsearch.edges.forEach((e) => {
        newsearch.edges.push(copyEdge(e));
    });
    return newsearch;
}

export function copyAndOrCondition(cond: AndOrCondition): AndOrCondition {
    const newcond = new AndOrCondition(cond.type);
    cond.conditions.forEach((c) => {
        if (c.type === ConditionType.And || c.type === ConditionType.Or) {
            newcond.conditions.push(copyAndOrCondition(c as AndOrCondition));
        } else {
            newcond.conditions.push(copyCondition(c as ValueCondition));
        }
    });
    return newcond;
}

export function copyCondition(cond: ValueCondition): ValueCondition {
    const newCond = new ValueCondition(cond.type);
    newCond.name = cond.name;
    newCond.property = cond.property;
    newCond.not = cond.not;
    newCond.value = cond.value;
    newCond.operator = cond.operator;
    newCond.caseSensitive = cond.caseSensitive;
    return newCond;
}

export function GetNode(name: string, search: Search): SearchNode {
    for (let i = 0; i < search.nodes.length; i++) {
        if (search.nodes[i].name === name) {
            return search.nodes[i];
        }
    }
    return null;
}

export function GetEdge(name: string, search: Search): SearchEdge {
    for (let i = 0; i < search.edges.length; i++) {
        if (search.edges[i].name === name) {
            return search.edges[i];
        }
    }
    return null;
}

export function IsConditionValid(condition: Condition): boolean {
    if (condition instanceof AndOrCondition) {
        for (let i = 0; i < condition.conditions.length; i++) {
            if (IsConditionValid(condition.conditions[i]) === false) { return false; }
        }
    }
    else if (condition instanceof ValueCondition) {
        if (webcrap.misc.isNullOrWhitespace(condition.type)) { return false; }
        if (webcrap.misc.isNullOrWhitespace(condition.name)) { return false; }
        if (webcrap.misc.isNullOrWhitespace(condition.property)) { return false; }
        if (webcrap.misc.isNullOrWhitespace(condition.operator)) { return false; }
        return true;
    }
}

export function ItemNamedExists(root: AndOrCondition, name: string): boolean {
    for (let i = 0; i < root.conditions.length; i++) {
        const cond: Condition = root.conditions[i];

        if (cond.type === "OR" || cond.type === "AND") {
            if (ItemNamedExists((cond as AndOrCondition), name)) { return true; }
        }
        else {
            if ((cond as ValueCondition).name === name) {
                return true;
            }
        }
    }

    return false;
}

export function RemoveConditionsForName(root: AndOrCondition, name: string): void {
    //console.log("RemoveConditionsForName");
    //console.log(root);
    let i;
    for (i = 0; i < root.conditions.length; i++) {
        const cond: Condition = root.conditions[i];
        //let condbase = cond as ConditionBase;
        //console.log(condbase);

        if (cond.type === "OR" || cond.type === "AND") {
            //console.log("recursive remove");
            RemoveConditionsForName((cond as AndOrCondition), name);
        }
        else {
            if ((cond as ValueCondition).name === name) {
                //console.log("trying to splice " + i);
                root.conditions.splice(i, 1);
                i--; //bounce i backwards because an item is now gone
            }
        }
    }
    //console.log("finished");
}

export function UpdateCondition(root: AndOrCondition, oldcondition: ValueCondition, newcondition: ValueCondition): void {
    if (oldcondition === null) {
        root.conditions.push(newcondition);
    } else {
        for (let i = 0; i < root.conditions.length; i++) {
            const cond: Condition = root.conditions[i];
    
            if (cond.type === ConditionType.Or || cond.type === ConditionType.And) {
                //console.log({source:"updateCondition", message: "nested call"});
                UpdateCondition((cond as AndOrCondition), oldcondition, newcondition);
            }
            else {
                if (cond === oldcondition) {
                    //root.conditions.splice(i, 1, newcondition);
                    importValueCondition(newcondition, oldcondition);
                    //console.log({source:"updateCondition", oldcondition: oldcondition, newcondition: newcondition});
                    return;
                }
            }
        }
    }
    
}

function importValueCondition(source: ValueCondition, dest: ValueCondition) {
    dest.type = source.type;
    dest.name = source.name;
    dest.property = source.property;
    dest.not = source.not;
    dest.caseSensitive = source.caseSensitive;
    dest.value = source.value;
    dest.operator = source.operator;
}

export function DeleteCondition(root: AndOrCondition, oldcondition: Condition): void {
    for (let i = 0; i < root.conditions.length; i++) {
        const cond: Condition = root.conditions[i];
        if (cond === oldcondition) {
            root.conditions.splice(i, 1);
            return;
        }
        else if (cond.type === ConditionType.And || cond.type === ConditionType.Or) {
            DeleteCondition((cond as AndOrCondition), oldcondition);
        }
    }
}


export function moveCondition(moveRight: boolean, root: AndOrCondition, lastroot: AndOrCondition, condition: Condition): boolean {
    for (let i = 0; i < root.conditions.length; i++) {
        const cond: Condition = root.conditions[i];
        if (cond === condition) {
            if (moveRight && i < root.conditions.length - 1) {
                const nextcond = root.conditions[i + 1];
                if (nextcond.type === ConditionType.And || nextcond.type === ConditionType.Or) {
                    //next item is an and/or. move condition into that one
                    (nextcond as AndOrCondition).conditions.unshift(condition);
                    root.conditions.splice(i, 1);
                } else {
                    root.conditions.splice(i, 1, root.conditions[i + 1]);
                    root.conditions.splice(i + 1, 1, condition);
                }
            } else if (!moveRight && i > 0) {
                const prevcond = root.conditions[i - 1];
                if (prevcond.type === ConditionType.And || prevcond.type === ConditionType.Or) {
                    //prev item is an and/or. move condition into that one
                    (prevcond as AndOrCondition).conditions.push(condition);
                    root.conditions.splice(i, 1);
                } else {
                    root.conditions.splice(i, 1, root.conditions[i - 1]);
                    root.conditions.splice(i - 1, 1, condition);
                }
            } else {
                //can't shift in array. push it up a layer if there is one
                if (lastroot !== null) {
                    if (moveRight) {
                        lastroot.conditions.push(condition);
                    } else {
                        lastroot.conditions.unshift(condition);
                    }
                    //remove from current location
                    root.conditions.splice(i, 1);
                }
            }
            return true;
        } else {
            if (cond.type === ConditionType.And || cond.type === ConditionType.Or) {
                if (moveCondition(moveRight, cond as AndOrCondition, root, condition)) {
                    return true;
                }
            }
        }
    }

    return false;
}