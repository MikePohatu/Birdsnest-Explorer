import { domcrap, DomCrap } from "./domcrap";
import { datacrap, DataCrap } from "./datacrap";
import { authcrap, AuthCrap } from "./authcrap";
import { misccrap, MiscCrap } from "./misccrap";

class WebCrap {
    dom: DomCrap = domcrap;
    data: DataCrap = datacrap;
    auth: AuthCrap = authcrap;
    misc: MiscCrap = misccrap;
}

export var webcrap = new WebCrap();
