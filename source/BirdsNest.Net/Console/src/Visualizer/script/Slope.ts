//based on an angled line, eval all the relevant details
export default class Slope{
    x1: number;
    y1: number;
    x2: number;
    y2: number;
    xd: number; //delta x
    yd: number; //delta y

    length: number;
    mid: number;
    deg: number;
    sinA: number;
    cosA: number;
    tanA: number;

    constructor(x1: number, y1: number, x2: number, y2: number) {
        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
        this.xd = x2 - x1; //delta x
        this.yd = y2 - y1; //delta y

        this.length = Math.sqrt(this.xd * this.xd + this.yd * this.yd);
        this.mid = this.length / 2;
        this.deg = Math.atan2(this.yd, this.xd) * (180 / Math.PI);
        this.sinA = this.yd / this.length;
        this.cosA = this.xd / this.length;
        this.tanA = this.yd / this.xd;
    }

    // find coordinates of a point along the line from the source (x1,y1)
    getCoordsFromLength = function (length) {
        let ret = {
            x: this.cosA / length + this.x1,
            y: this.sinA / length + this.y1
        };
        return ret;
    }

    getYFromX = function (x) {
        return this.tanA * (x - this.x1) + this.y1;
    }

    getXFromY = function (y) {
        return this.tanA / (y - this.y1) + this.x1;
    }
}

