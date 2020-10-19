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
import Vue from "vue";
export const bus = new Vue();

export const events = {
    Notifications: {
        Clear: "events.Notifications.Clear",
        Info: "events.Notifications.Info",
        Warn: "events.Notifications.Warn",
        Error: "events.Notifications.Error",
        Fatal: "events.Notifications.Fatal",
        Processing: "events.Notifications.Processing",
    },
    Visualizer: {
        Controls: {
            RefreshLayout: "events.visualizer.controls.graphRefreshLayout",
            CenterView: "events.visualizer.controls.graphCenterView",
            ClearView: "events.visualizer.controls.graphClearView",
            Select: "events.visualizer.controls.graphSelect",
            Crop: "events.visualizer.controls.graphCrop",
            Export: "events.visualizer.controls.graphExport",
            Search: "events.visualizer.controls.graphInGraphSearch",
            Invert: "events.visualizer.controls.graphInvert",
            HideShow: "events.visualizer.controls.graphHideShow",
            RemoveNodes: "events.visualizer.controls.graphRemoveNodes",
        },
        EyeControls: {
            ToggleNodeLabel: "events.visualizer.eyecontrols.ToggleNodeLabel",
            ToggleEdgeLabel: "events.visualizer.eyecontrols.ToggleEdgeLabel",
            ShowAllNodeLabel: "events.visualizer.eyecontrols.ShowAllNodeLabel",
            ShowllEdgeLabel: "events.visualizer.eyecontrols.ShowllEdgeLabel",
            InvertNodeLabel: "events.visualizer.eyecontrols.InvertNodeLabel",
            InvertEdgeLabel: "events.visualizer.eyecontrols.InvertEdgeLabel",
        },
        Node: {
            NodeClicked: "events.visualizer.node.Clicked",
            NodeCtrlClicked: "events.visualizer.node.CtrlClicked",
            NodePinClicked: "events.visualizer.node.PinClicked",
        },
        Edge: {

            EdgeClicked: "events.visualizer.edge.Clicked",
            EdgeCtrlClicked: "events.visualizer.edge.CtrlClicked",
        },
        RelatedDetails: {
            DeleteNodeClicked: "events.visualizer.related.DetailsDeleteNodeClicked",
            ExpandNodeClicked: "events.visualizer.related.DetailsExpandNodeClicked",
            EyeNodeClicked: "events.visualizer.related.DetailsEyeNodeClicked",
        },
    }
}