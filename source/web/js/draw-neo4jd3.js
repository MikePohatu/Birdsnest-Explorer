function DrawGraph(selectid) {
	var neo4jd3 = new Neo4jd3('#'+selectid, {
        neo4jDataUrl: 'json/sampledata.json',
		highlight: [
			{
				class: 'Project',
				property: 'name',
				value: 'neo4jd3'
			}, {
				class: 'User',
				property: 'userId',
				value: 'eisman'
			}
		],
		icons: {
			'Api': 'gear',
			'BirthDate': 'birthday-cake',
			'Cookie': 'paw',
			'Email': 'at',
			'Git': 'git',
			'Github': 'github',
			'Ip': 'map-marker',
			'Issues': 'exclamation-circle',
			'Language': 'language',
			'Options': 'sliders',
			'Password': 'asterisk',
			'Phone': 'phone',
			'Project': 'folder-open',
			'SecurityChallengeAnswer': 'commenting',
			'User': 'user',
			'zoomFit': 'arrows-alt',
			'zoomIn': 'search-plus',
			'zoomOut': 'search-minus'
		},
		minCollision: 60,
		nodeRadius: 25,
		onNodeDoubleClick: function(node) {
			switch(node.id) {
				case '25':
					// Google
					window.open(node.properties.url, '_blank');
					break;
				default:
					var maxNodes = 5,
						data = neo4jd3.randomD3Data(node, maxNodes);
					neo4jd3.updateWithD3Data(data);
					break;
			}
		},
		zoomFit: true
	});
    
    console.log(neo4jd3.size());
}