/**
* Nova theme for VAV
* @author Jay Wu
*/

Highcharts.theme = {
	colors: ['#dc8736', '#89a54e', '#eb9696', '#715891', '#9e3241', '#3f99b1', '#73b697', '#3b3f96', '#135e84', '#bf8bd8', '#ca5791', '#bbbb44'],

	chart: {
		//type: 'area',
		backgroundColor: '#242424',
		borderWidth: 0,
		borderColor: '#000',
		borderRadius: 3,
		plotBackgroundColor: null,
		plotShadow: false,
		plotBorderWidth: 1,
		plotBorderColor: '#000',
		//marginLeft: -1,
		//marginBottom: 20,
		zoomType: 'x',
			   
		style: {
				color: '#ccc',
				font: '12px Arial, Helvetica, sans-serif'
		}
	},
	title: {
		style: {
			color: '#ccc',
			font: 'Bold 12px Arial, Helvetica, sans-serif'
		},
	},
	subtitle: {
		style: {
			color: '#DDD',
			font: '12px Lucida Grande, Lucida Sans Unicode, Verdana, Arial, Helvetica, sans-serif'
		}
	},
	xAxis: {
		gridLineColor: '#2d2d2d',
		gridLineWidth: 1,
		minorGridLineColor: '#2d2d2d',
		alternateGridColor: '#1f1f1f',
		lineWidth: 1,
		lineColor: '#1f1f1f',
		tickWidth: 1,
		tickColor: '#2d2d2d',
		labels: {
			style: {
				color: '#999999',
				font: 'Bold 11px Arial, Helvetica, sans-serif'
			}
		},
		title: {
			style: {
				color: '#AAA',
				font: 'bold 12px Lucida Grande, Lucida Sans Unicode, Verdana, Arial, Helvetica, sans-serif'
			}
		}
	},
	yAxis: {
		gridLineColor: '#2d2d2d',
		gridLineWidth: 1,
		minorGridLineColor: '#2d2d2d',
		minorTickLength: 1,
		showFirstLabel: false,
		showLastLabel: false,
		alternateGridColor: null,
		minorTickInterval: null,
		lineWidth: 0,
		tickWidth: 0,
		labels: {
			style: {
					color: '#ccc',
					font: 'Bold 11px Arial, Helvetica, sans-serif'
			}
		},
		title: {
			align: 'high',
			offset: 0,
			rotation: 0,
			floating: true,
			y: -6,
			x: 0,
			style: {
					color: '#ccc',
					font: 'Bold 11px Arial, Helvetica, sans-serif'
			}
		}
	},
	legend: {
		borderWidth: 0,
		itemDistance: 16,
		symbolHeight: 10,
		symbolWidth: 10,
		symbolRadius: 2,
		lineHeight: 14,
		itemStyle: {
			color: '#CCC'
		},
		itemHoverStyle: {
			color: '#FFF'
		},
		itemHiddenStyle: {
			color: '#333'
		}
	},
	labels: {
		style: {
			color: '#CCC'
		}
	},
	tooltip: {
		backgroundColor: '#2d2c2c',
		borderColor: '#1f1f1f',
		crosshairs: [{
			width: 1,
			color: '#2d2c2c'
		}],
		style: {
			color: '#ccc',
			fontSize: '12px',
			padding: '8px'
		},
		useHTML: true,
		headerFormat: '<span style="color:#ccc;font:Bold 11px Arial, Helvetica, sans-serif;">{point.key}</span><table>',
		pointFormat: '<tr><td style="font-size:12px;color: {series.color}">{series.name}: </td>' +
		'<td style="text-align: right"><span>{point.y}</span></td></tr>',
		footerFormat: '</table>'
	},


	plotOptions: {
		line: {
			dataLabels: {
				color: '#CCC'
			},
			marker: {
				lineColor: '#333'
			}
		},
		spline: {
			marker: {
				lineColor: '#333'
			}
		},
		scatter: {
			marker: {
				lineColor: '#333'
			}
		},
		candlestick: {
			lineColor: 'white'
		},
		pie: {
			dataLabels: {
				enabled: true,
				color: '#fff',
				connectorColor: '#000000'
			}
		},
		series: {
				fillOpacity: 0.1,
				marker: {
						enabled: false
				}
		},
		column: {
			borderWidth: 0
			
		}

	},

	toolbar: {
		itemStyle: {
			color: '#CCC'
		}
	},

	navigation: {
		buttonOptions: {
			symbolStroke: '#DDDDDD',
			hoverSymbolStroke: '#FFFFFF',
			theme: {
				fill: {
					linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
					stops: [
						[0.4, '#606060'],
						[0.6, '#333333']
					]
				},
				stroke: '#000000',
				//'stroke-width': 1,
					r: 0,
					states: {
						hover: {
							fill: '#333333'
						},
						select: {
							fill: '#333333'
						}
					}
			}
		}
	},
	exporting: { url: '/ImageExport/TranslateSvg', width: 1200, },
	credits: { href: 'http://thomsonreuters.com/', text: window.Common.ChartSource },


	// scroll charts
	rangeSelector: {
		buttonTheme: {
			fill: {
				linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
				stops: [
					[0.4, '#888'],
					[0.6, '#555']
				]
			},
			stroke: '#000000',
			style: {
				color: '#CCC',
				fontWeight: 'bold'
			},
			states: {
				hover: {
					fill: {
						linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
						stops: [
							[0.4, '#BBB'],
							[0.6, '#888']
						]
					},
					stroke: '#000000',
					style: {
						color: 'white'
					}
				},
				select: {
					fill: {
						linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
						stops: [
							[0.1, '#000'],
							[0.3, '#333']
						]
					},
					stroke: '#000000',
					style: {
						color: 'yellow'
					}
				}
			}
		},
		inputStyle: {
			backgroundColor: '#333',
			color: 'silver'
		},
		labelStyle: {
			color: 'silver'
		},
        inputPosition:{x:-100,y:20},
        inputDateFormat: '%Y-%m-%d',
        inputEditDateFormat: '%Y-%m-%d',
        inputEnabled:true,
	},

	navigator: {
		handles: {
			backgroundColor: '#666',
			borderColor: '#AAA'
		},
		outlineColor: '#CCC',
		maskFill: 'rgba(16, 16, 16, 0.5)',
		series: {
			color: '#7798BF',
			lineColor: '#A6C7ED'
		}
	},

	scrollbar: {
		barBackgroundColor: {
			linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
			stops: [
					[0.4, '#888'],
					[0.6, '#555']
				]
		},
		barBorderColor: '#CCC',
		buttonArrowColor: '#CCC',
		buttonBackgroundColor: {
			linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
			stops: [
					[0.4, '#888'],
					[0.6, '#555']
				]
		},
		buttonBorderColor: '#CCC',
		rifleColor: '#FFF',
		trackBackgroundColor: {
			linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
			stops: [
				[0, '#000'],
				[1, '#333']
			]
		},
		trackBorderColor: '#666'
	},

	// special colors for some of the demo examples
	legendBackgroundColor: 'rgba(48, 48, 48, 0.8)',
	legendBackgroundColorSolid: 'rgb(70, 70, 70)',
	dataLabelsColor: '#444',
	textColor: '#E0E0E0',
	maskColor: 'rgba(255,255,255,0.3)'
};

// Apply the theme
var highchartsOptions = Highcharts.setOptions(Highcharts.theme);
