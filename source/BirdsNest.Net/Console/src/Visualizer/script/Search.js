"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var Search = /** @class */ (function () {
    function Search() {
        this.Nodes = [];
        this.Edges = [];
        this.AddedNodes = 0;
    }
    Search.prototype.AddNode = function () {
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
    };
    //remove the node and return the index of the node that was removed
    Search.prototype.RemoveNode = function (node) {
        var me = this;
        var foundindex = -1;
        var i;
        for (i = 0; i < me.Nodes.length; i++) {
            if (foundindex !== -1) { //if found already, start shifting nodes back on in the array
                me.Nodes[i - 1] = me.Nodes[i];
                if (i < me.Edges.length) {
                    me.Edges[i - 1] = me.Edges[i];
                }
            }
            else {
                if (me.Nodes[i] === node) {
                    foundindex = i;
                    if (i === 0) {
                        me.Nodes.shift; //remove the first node
                        if (me.Edges.length > 0) {
                            me.Edges.shift;
                        } //if there is an edge, remove that too
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
    };
    Search.prototype.MoveNodeRight = function (node) {
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
    };
    Search.prototype.MoveNodeLeft = function (node) {
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
    };
    return Search;
}());
exports.Search = Search;
var SearchNode = /** @class */ (function () {
    function SearchNode() {
        this.Name = "";
        this.Label = "";
    }
    return SearchNode;
}());
exports.SearchNode = SearchNode;
var SearchEdge = /** @class */ (function () {
    function SearchEdge() {
        this.Name = "";
        this.Label = "";
        this.Direction = ">";
        this.Min = "1";
        this.Max = "1";
    }
    return SearchEdge;
}());
exports.SearchEdge = SearchEdge;
var AndOrCondition = /** @class */ (function () {
    function AndOrCondition() {
        this.Name = "";
        this.Conditions = [];
        this._type = "AND";
    }
    Object.defineProperty(AndOrCondition.prototype, "Type", {
        get: function () {
            return this._type;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(AndOrCondition.prototype, "bar", {
        set: function (value) {
            if (value === "AND" || value === "OR") {
                this._type = value;
            }
            else {
                console.log("Cannot set type of AND/OR condition to " + value);
            }
        },
        enumerable: true,
        configurable: true
    });
    return AndOrCondition;
}());
exports.AndOrCondition = AndOrCondition;
var ConditionBase = /** @class */ (function () {
    function ConditionBase() {
        this.Type = "";
        this.Name = "";
        this.Property = "";
    }
    return ConditionBase;
}());
exports.ConditionBase = ConditionBase;
var MathCondition = /** @class */ (function (_super) {
    __extends(MathCondition, _super);
    function MathCondition() {
        var _this = _super.call(this) || this;
        _this.Value = 0;
        _this.Type = "MATH";
        _this.Name = "";
        _this.Property = "";
        _this.Operator = "=";
        return _this;
    }
    return MathCondition;
}(ConditionBase));
exports.MathCondition = MathCondition;
var StringCondition = /** @class */ (function (_super) {
    __extends(StringCondition, _super);
    function StringCondition() {
        var _this = _super.call(this) || this;
        _this.Value = "";
        _this.Type = "STRING";
        _this.Name = "";
        _this.Property = "";
        _this.Operator = "=";
        return _this;
    }
    return StringCondition;
}(ConditionBase));
exports.StringCondition = StringCondition;
//# sourceMappingURL=Search.js.map