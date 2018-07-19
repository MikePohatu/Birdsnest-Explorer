function DrawGraph(selectid) {
	var config = {
		container_id: selectid,
		server_url: "bolt://dev01.home.local:7687",
		server_user: "svc_neo4j",
		server_password: "5GGX2zMgQxJuG2INz5N5",
		arrows:true,
		labels: {
			"AD_GROUP": {
				"caption": "name",
				"size": "member_count"
			},
			"AD_USER": {
				"caption": "name",
				"size": "member_count"
			},
			"AD_COMPUTER": {
				"caption": "name",
				"size": "member_count"
			},
			"FS_DATASTORE": {
				"caption": "name"
			},
			"FS_FOLDER": {
				"caption": "path"
			}
		},
		relationships: {
			"AD_MemberOf": {
				"thickness": "weight",
				"caption": true
			}
		},
		initial_cypher: "MATCH (n)-[r*]->(m) RETURN *"
	};

	viz = new NeoVis.default(config);
	viz.render();
	viz.stabilize();
}
