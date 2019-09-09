import * as d3 from "../js/visualizerD3";

interface ISimNode extends d3.SimulationNodeDatum {
    size: number;
    db_id: string;
    y: number;
    x: number;
    startx: number;
    starty: number;
    tark: number;
    srck: number;
}

interface ISimLink<ISimNode> extends d3.SimulationLinkDatum<ISimNode> {
    db_id: string;
    source: ISimNode;
    target: ISimNode;
}

export default class SimulationController {
    simRunning: boolean;
    onFinisedSimulation: CallableFunction;
    onFinishSimulation: CallableFunction;
    ProgressBarTag: string;
    TreeEdgeTag: string;
    NodeTag: string;
    onPercentUpdated: CallableFunction;
    graphsimulation: any;
    connectsimulation: any;
    meshsimulation: any;
    treesimulation: any;

    constructor() {
        var me = this;
        var velocityDecay = 0.5;
        var alphaDecay = 0.1;

        this.simRunning = false;
        

        this.graphsimulation = d3.forceSimulation();
        this.graphsimulation.stop();
        this.graphsimulation
            .force('collide', d3.forceCollide()
                .strength(0.7)
                .radius(function (d: ISimNode) { return d.size * 1.5; }))
            .on('end', function () { me.onSimulationFinished(); })
            .on('tick', function () { me.onGraphTick(); })
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);

        this.connectsimulation = d3.forceSimulation();
        this.connectsimulation.stop();
        this.connectsimulation
            .force("link", d3.forceLink()
                .id(function (d: ISimLink<ISimNode>) { return d.db_id; })
                .distance(200)
                .strength(0.1))
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);

        this.meshsimulation = d3.forceSimulation();
        this.meshsimulation.stop();
        this.meshsimulation
            .force("link", d3.forceLink()
                .id(function (d: ISimLink<ISimNode>) { return d.db_id; })
                .distance(150))
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);

        this.treesimulation = d3.forceSimulation();
        this.treesimulation.stop();
        this.treesimulation
            .force("link", d3.forceLink()
                .id(function (d: ISimLink<ISimNode>) { return d.db_id; })
                .distance(150))
            .velocityDecay(velocityDecay)
            .alphaDecay(alphaDecay);
    }

    onGraphTick () {
        //console.log("SimulationController.onGraphTick");
        //console.log(meshsimulation.alpha());
        //var me = this;
        var k = this.graphsimulation.alpha();
        this.onPercentUpdated(100 - k * 100);

		d3.selectAll(this.TreeEdgeTag).each(function (d: ISimLink<ISimNode>) {
            var src = d.source as ISimNode;
            var tar = d.target as ISimNode;
            if ((tar as ISimNode).y < src.y + src.size / 4) {
                (tar as ISimNode).y = src.y + src.size;
            }
            else {
                if (tar.tark !== k) {
                    tar.y += k * 8;
                    tar.tark = k;
                }
                if (src.srck !== k) {
                    src.y -= k * 6;
                    src.srck = k;
                }
            }
        });
        //if (!perfmode) { updateLocations(); }
    }


    onSimulationFinished () {
        //console.log("SimulationController.onSimulationFinished");
        //console.log(this.graphsimulation);
        this.simRunning = false;
        this.onFinishSimulation();
    }

    RestartSimulation () {
        var me = this;
        //console.log("RestartSimulation: " + this.NodeTag);
        d3.selectAll(this.NodeTag)
            .each(function (d: ISimNode) {
                if (me.simRunning === false) {
                    //console.log("set startx to x: ");
                    d.startx = d.x;
                    d.starty = d.y;
                }
                else {
                    //console.log("set x to startx: ");
                    d.x = d.startx;
                    d.y = d.starty;
                }
                //console.log(d);
            });


        this.simRunning = true;
        this.meshsimulation.alpha(1).restart();
        this.treesimulation.alpha(1).restart();
        this.connectsimulation.alpha(1).restart();
        this.graphsimulation.alpha(1).restart();
    }

    StopSimulations () {
        if (this.simRunning === true) {
            this.meshsimulation.stop();
            this.graphsimulation.stop();
            this.treesimulation.stop();
            this.connectsimulation.stop();

            //reset the datum values to before they started. this should match because the
            //layout hasn't updated yet
            d3.selectAll(this.NodeTag)
                .each(function (d: ISimNode) {
                    //console.log("set x to startx: ");
                    d.x = d.startx;
                    d.y = d.starty;
                });
        }

        this.simRunning = false;
    }

    SetNodes (graphs, meshes, trees, connects) {
        //console.log("SimulationController.SetNodes");
        //console.log(graphs);
        this.StopSimulations();
        this.meshsimulation.nodes(meshes);
        this.treesimulation.nodes(trees);
        this.connectsimulation.nodes(connects);
        this.graphsimulation.nodes(graphs);
    }

    SetEdges (meshes, trees, connects) {
        //console.log("SimulationController.SetEdges");
        this.StopSimulations();
        this.meshsimulation.force("link").links(meshes);
        this.treesimulation.force("link").links(trees);
        this.connectsimulation.force("link").links(connects);
    }
}