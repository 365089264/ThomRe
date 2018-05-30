function GetSelectedRow(reportId) {
    var row = $('#gridWithHoverStyle' + reportId + ' .HoverRow');
    $('#gridWithHoverStyle' + reportId + ' tr').removeClass('SelectedRow');
    row.addClass('SelectedRow');
    var rowName = row.first().find('.Hide').text();
    if (rowName != "") {
        GetDetailedReport(reportId, rowName);
    }
}

function GetOpenMarketMaRASelectedRow(reportId) {
    var row = $('#gridWithHoverStyle' + reportId + ' .HoverRow');
    $('#gridWithHoverStyle' + reportId + ' tr').removeClass('SelectedRow');
    row.addClass('SelectedRow');
    var rowName = row.first().find('.Hide').text();
    if (rowName != "") {
        GetOpenMarketMaRADetailedReport(reportId, rowName);
    }
}

function RefreshRatesAnalysisReport(reportId, ratesAnalysisReportUrl) {
    var startDate = $("#datePicker" + reportId).val();
    var endDate = $("#endDatePicker" + reportId).val();
    var unit = $('#uselect' + reportId + ' option:selected').val();
    var typeList = "";
    $(".openMarketOption li input").each(function () {
        if ($(this).is(':checked')) {
            var id = $(this).val();
            typeList += "-" + id;
        }
    });

    var targetObj = $("#contentDiv" + reportId);
        var height = targetObj.height();
        var width = targetObj.width();
        targetObj.html("");
        targetObj.block(
            {
                message: $('<img src="' + window.busyImageUrl + '" style="display: none;" />'),
                css: {
                    top: (height - 54) / 2 + 'px',
                    left: (width - 54) / 2 + 'px',
                    width: '54px',
                    height: '54px',
                    border: '0px'
                },
                overlayCSS: { backgroundColor: '#131313' }
            }
        );
    $.ajax({
        type: 'POST',
        url: ratesAnalysisReportUrl,
        data: {
            reportId: reportId,
            typeList: typeList,
            startDate: startDate,
            endDate: endDate,
            unit: unit
        },
        success: function (data) {
            $("#contentDiv" + reportId).html(data);
        },
        async: true
    });
}

function ExportRatesAnalysisReport(link, reportId) {
    var startDate = $("#exportStartDate_" + reportId).val();
    var endDate = $("#exportEndDate_" + reportId).val();
    var unit = $("#exportUnit_" + reportId).val();
    var typeList = $("#exportType_" + reportId).val();
    $(link).attr("href", "/OpenMarket/ExportRatesAnalysisReport?reportId=" + reportId + "&startDate=" + startDate + "&endDate=" + endDate + "&unit=" + unit + "&typeList=" + typeList + "");
}

function GetGridDataByChartSeriesName(chartName, seriesName) {
    if (chartName == "chart33"){
        var reportId = 33;
        var startDate = $("#exportStartDate_" + reportId).val();
        var endDate = $("#exportEndDate_" + reportId).val();
        var unit = $("#exportUnit_" + reportId).val();
        var targetObj = $("#detailedGridDiv_" + reportId);
        var height = targetObj.height();
        var width = targetObj.width();
        targetObj.html("");
        targetObj.block(
            {
                message: $('<img src="' + window.busyImageUrl + '" style="display: none;" />'),
                css: {
                    top: (height - 54) / 2 + 'px',
                    left: (width - 54) / 2 + 'px',
                    width: '54px',
                    height: '54px',
                    border: '0px'
                },
                overlayCSS: { backgroundColor: '#131313' }
            }
        );
        $.ajax({
            type: 'POST',
            url: '/OpenMarket/GetOpenMarketRatesAnalysisDetailedReport',
            data: {
                reportId: reportId,
                seriesName: seriesName,
                startDate: startDate,
                endDate: endDate,
                unit: unit
            },
            success: function (data) {
                $("#detailedGridDiv_" + reportId).html(data);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }
}

function GetOpenMarketMaRADetailedReport(reportId, rowName) {
        var startDate = $("#exportStartDateSum_" + reportId).val();
        var endDate = $("#exportEndDateSum_" + reportId).val();
        var unit = $("#exportUnitSum_" + reportId).val();
        var category = $("#exportTypeSum_" + reportId).val();
        if(category == "")
        return;
        var targetObj = $("#detailedGridDiv_" + reportId);
        var height = targetObj.height();
        var width = targetObj.width();
        targetObj.html("");
        targetObj.block(
            {
                message: $('<img src="' + window.busyImageUrl + '" style="display: none;" />'),
                css: {
                    top: (height - 54) / 2 + 'px',
                    left: (width - 54) / 2 + 'px',
                    width: '54px',
                    height: '54px',
                    border: '0px'
                },
                overlayCSS: { backgroundColor: '#131313' }
            }
        );
        var operationType = $('#tselect_' + reportId).val();
        if (operationType != null) operationType = operationType.join();
        $.ajax({
            type: 'POST',
            url: '/OpenMarket/GetOpenMarketMaRADetailedReport',
            data: {
                reportId: reportId,
                category: category,
                startDate: startDate,
                endDate: endDate,
                unit: unit,
                operationType: operationType,
                rowName: rowName
            },
            success: function (data) {
                targetObj.html(data);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }

    function ExportOpenMarketMaRADetailedReport(link, reportId) {
        var startDate = $("#exportStartDate_" + reportId).val();
        var endDate = $("#exportEndDate_" + reportId).val();
        var unit = $("#exportUnit_" + reportId).val();
        var category = $("#exportType_" + reportId).val();
        var rowName = $("#exportCategory_" + reportId).val();
        var operationType = $('#tselect_' + reportId).val();
        if (operationType != null) operationType = operationType.join();
        if(rowName == "" || rowName == "ALL")
        {
            var row = $('#gridWithHoverStyle' + reportId + ' .SelectedRow');
            rowName = row.first().find('.Hide').text();
        }
        if (rowName != "") {
            $(link).attr("href", "/OpenMarket/ExportOpenMarketMaRADetailedReport?reportId=" + reportId + "&category=" + category + "&startDate=" + startDate + "&endDate=" + endDate + "&unit=" + unit + "&rowName=" + rowName + "&operationType=" + operationType);
        }
    }

function GetReportTable(reportId, reportType) {
    var startDate = $("#datePicker" + reportId).val();
    var endDate = $("#endDatePicker" + reportId).val();
    var unit = $('#select' + reportId + ' option:selected').val();
    var height = $("#contentDiv" + reportId).height();
    var width = $("#contentDiv" + reportId).width();

    $("#datePicker").prop('disabled', true);
    $("#endDatePicker").prop('disabled', true);
    $("#select" + reportId).prop('disabled', true);

    $("#contentDiv" + reportId).block(
        {
            message: $('<img src="' + window.busyImageUrl + '" style="display: none;" />'),
            css: {
                top: (height - 54) / 2 + 'px',
                left: (width - 54) / 2 + 'px',
                width: '54px',
                height: '54px',
                border: '0px'
            },
            overlayCSS: { backgroundColor: '#272727' ,opacity: 1}
        }
    );

    $.ajax({
        type: 'POST',
        url: window.getReportTableUrl,
        data: {
            reportId: reportId,
            reportType: reportType,
            startDate: startDate,
            endDate: endDate,
            unit: unit
        },
        success: function (data) {
            $("#contentDiv" + reportId).html(data);
            $("#datePicker").prop('disabled', false);
            $("#endDatePicker").prop('disabled', false);
            $("#select" + reportId).prop('disabled', false);
        },
        async: true
    });
};

function GetIssueAmountReport(reportId) {
    var startDate = $("#datePicker" + reportId).val();
    var endDate = $("#endDatePicker" + reportId).val();

    var type = $('#tselect' + reportId + ' option:selected').val();
    var useSecType = $("#useSecType").is(':checked') ? "true" : "false";
    var secType = $('#secTypeSelectId' + reportId + ' option:selected').val();
    var unit = $('#uselect' + reportId + ' option:selected').val();
    var reportName = $("#issueAmountReportName").val();

    var typeList = $('#subTypeOptionSelect').val();
    if(typeList != null) typeList = typeList.join();

    var height = $("#contentDiv" + reportId).height();
    var width = $("#contentDiv" + reportId).width();
    $("#contentDiv" + reportId).html("");
    $("#datePicker").prop('disabled', true);
    $("#endDatePicker").prop('disabled', true);

    $("#contentDiv" + reportId).block(
        {
            message: $('<img src="' + window.busyImageUrl + '" style="display: none;" />'),
            css: {
                top: (height - 54) / 2 + 'px',
                left: (width - 54) / 2 + 'px',
                width: '54px',
                height: '54px',
                border: '0px'
            },
            overlayCSS: { backgroundColor: '#272727' }
        }
    );

    $.ajax({
        type: 'POST',
        url: window.issueAmountReportUrl,
        data: {
            reportId: reportId,
            reportName: reportName,
            type: type,
            typeList: typeList,
            useSubType: useSecType,
            subType: secType,
            startDate: startDate,
            endDate: endDate,
            unit: unit
        },
        success: function (data) {
            $("#contentDiv" + reportId).html(data);
            $("#datePicker").prop('disabled', false);
            $("#endDatePicker").prop('disabled', false);
            triggerTopChartsChange(reportId);
        },
        async: true
    });
}

function GetDepositoryBalanceContent(reportId) {
    var startDate = $("#datePicker" + reportId).val();
    var endDate = $("#endDatePicker" + reportId).val();
    var type = $('#tselect' + reportId + ' option:selected').val();
    var unit = $('#uselect' + reportId + ' option:selected').val();
    var reportName = $("#bondDepositoryBalanceName").val();

    var height = $("#contentDiv" + reportId).height();
    var width = $("#contentDiv" + reportId).width();
    $("#contentDiv" + reportId).html("");

    $("#datePicker").prop('disabled', true);
    $("#endDatePicker").prop('disabled', true);
    //$("#select" + reportId).prop('disabled', true);

    $("#contentDiv" + reportId).block(
        {
            message: $('<img src="' + window.busyImageUrl + '" style="display: none;" />'),
            css: {
                top: (height - 54) / 2 + 'px',
                left: (width - 54) / 2 + 'px',
                width: '54px',
                height: '54px',
                border: '0px'
            },
            overlayCSS: { backgroundColor: '#272727' }
        }
    );

    $.ajax({
        type: 'POST',
        url: window.depositoryBalancetUrl,
        data: {
            reportId: reportId,
            reportName: reportName,
            type: type,
            startDate: startDate,
            endDate: endDate,
            unit: unit
        },
        success: function (data) {
            $("#contentDiv" + reportId).html(data);
            $("#datePicker").prop('disabled', false);
            $("#endDatePicker").prop('disabled', false);
            triggerTopChartsChange(reportId);
            //$("#select" + reportId).prop('disabled', false);
        },
        async: true
    });
}
function UpdateIssAmtDetailPagination(total, currentPage, reportId, text, ftext, ltext, typeValue, subTypeValue, isParent, callback) {
    if (total == 0) {
        $('#pricePagePaginateOfLabel' + reportId + ',#pricePagePaginate' + reportId).hide();
        return;
    } else {
        $('#pricePagePaginateOfLabel' + reportId + ',#pricePagePaginate' + reportId).show();
    }

    UpdateIssAmtDetailPaginationLabel(currentPage, 300, total, reportId, text);
    var row = $("#gridDiv_" + reportId + " .SelectedRow");
    $('#pricePagePaginate' + reportId).paginate({
        count: Math.ceil(total / 300),
        start: currentPage,
        display: 10,
        border: false,
        text_color: '#00B3E3',
        background_color: 'none',
        text_hover_color: '#28D2FF',
        background_hover_color: 'none',
        images: false,
        mouse: 'press',
        onChange: function (page) {
            callback(row,total,text, typeValue, subTypeValue, isParent, page);
        },
        firstText: ftext,
        lastText: ltext
    });
};

function UpdateIssAmtDetailPaginationLabel  (current, pageSize, total, reportId, text) {
    var start = (current - 1) * pageSize + 1;
    var end = current * pageSize;
    if (end > total) end = total;
    $('#pricePagePaginateOfLabel' + reportId).html(start + '-' + end + text + total);
};
function RefreshBondDetail(row,total,text, typeValue, subTypeValue, isParent, page) {
    debugger;
    if (page == null) page = 1;
    var itemClass = $('#subTypeOptionSelect').val();
    if (itemClass != null) itemClass = itemClass.join();
    var id = $("#issueAmountReportId").val();
    $('#currentPage' + id).val(page);
    var startDate = $("#datePicker" + id).val();
    var endDate = $("#endDatePicker" + id).val();
    var unit = $('#uselect' + id + ' option:selected').val();
    var type = $('#tselect' + id + ' option:selected').val();
    var secType = $('#secTypeSelectId' + id + ' option:selected').val();
    var useSecType = $("#useSecType").is(':checked');
    if (page == 1) {
        $("#gridDiv_" + id + " table .SelectedRow").removeClass("SelectedRow");
        $(row).addClass("SelectedRow");
    }
    $("#typeValue").val(typeValue);
    $("#subTypeValue").val(subTypeValue);
    $("#isParent").val(isParent);

    var height = $("#bottomDiv" + id).height();
    var width = $("#bottomDiv" + id).width();
    $("#bottomDiv" + id).html("");
    
    $("#bottomDiv" + id).block(
        {
            message: $('<img src="' + window.busyImageUrl + '" style="display: none;" />'),
            css: {
                top: (height - 54) / 2 + 'px',
                left: (width - 54) / 2 + 'px',
                width: '54px',
                height: '54px',
                border: '0px'
            },
            overlayCSS: { backgroundColor: '#131313' }
        }
    );

    $.ajax({
        type: 'POST',
        url: window.refreshBondDetailUrl,
        data: {
            type: type,
            typeValue: typeValue,
            subType: secType,
            subTypeValue: subTypeValue,
            useSubType: useSecType,
            isParent: isParent,
            reportId: id,
            startDate: startDate,
            endDate: endDate,
            unit: unit,
            itemList: itemClass,
            startPage: page,
            pageSize: 300
        },
        success: function (data) {
            $("#bottomDiv" + id).html(data);
            UpdateIssAmtDetailPagination(total, page, id, text, Price.paginateCommon.firstText, Price.paginateCommon.lastText, typeValue, subTypeValue, isParent, RefreshBondDetail);
        },
        async: true
    });
}


function ExportIssueAmountReport(link, reportId, reportName, startDate, endDate, type, typeList, subType, useSecType, unit, reportType) {
    var typeValue = $("#typeValue").val();
    var subTypeValue = $("#subTypeValue").val();
    var isParent = $("#isParent").val();
    var page=$('#currentPage' + reportId).val();
    debugger;
    if (typeList == '') return;
    
    $(link).attr("href", ("/BondReport/ExportReport?reportId=" + reportId
                                        + "&startDate=" + startDate
                                        + "&endDate=" + endDate
                                        + "&unit=" + unit
                                        + "&type=" + type
                                        + "&typeValue=" + typeValue
                                        + "&typeList=" + typeList
                                        + "&subType=" + subType
                                        + "&subTypeValue=" + subTypeValue
                                        + "&useSecType=" + useSecType
                                        + "&unit=" + unit
                                        + "&isParent=" + isParent
                                        + "&reportName=" + reportName
                                        + "&reportType=" + reportType
                                        + "&startPage=" + page).replace("+", "%2B"));
}




function HideMonthPicker(e) {
    $(e).monthpicker('hide');
}

function SetMonthPickerStyle(id) {
    $("table.mtz-monthpicker").parent().css("width", $('#' + id).width());
    $('.mtz-monthpicker-month').css("cursor", "pointer");
    $('.mtz-monthpicker-month').hover(
            function () {
                $(this).css({ "font-weight": "bold"});
            },
            function () {
                $(this).css({ "font-weight": "normal"});
            }
        );
}

function ShowDetail(img, selector, e) {
    if ($(img).attr("class") == "expand") 
        $(img).toggleClass('expand close');
    else
        $(img).toggleClass('close expand');

    if (selector != "") {
        $("." + selector).toggle();
    }
        
    if(e)
        e.stopPropagation();
}

function ExportReportOpenMarketMaRReport(link, reportId, openMarketExportUrl) {
            var startDate = $("#exportStartDateSum_" + reportId).val();
            var endDate = $("#exportEndDateSum_" + reportId).val();
            var unit = $("#exportUnitSum_" + reportId).val();
            var type = $("#exportTypeSum_" + reportId).val();
            var operationType = $('#tselect_' + reportId).val();
            if (operationType != null) operationType = operationType.join();
            $(link).attr("href", openMarketExportUrl + "?reportId=" + reportId + "&startDate=" + startDate + "&endDate=" + endDate + "&unit=" + unit + "&type=" + type + "&operationType=" + operationType);
    }

;

	function GetStatisticalReportTable(reportId, reportType) {
		var startDate = $("#datePicker" + reportId).val();
		var endDate = $("#endDatePicker" + reportId).val();
		var unit = $('#select' + reportId + ' option:selected').val();
		
		$("#datePicker").prop('disabled', true);
		$("#endDatePicker").prop('disabled', true);
		$("#select" + reportId).prop('disabled', true);
		  
		$("#contentDiv" + reportId).html("");
		$("#contentDiv" + reportId).block(
			{
				message: $('<img src="' + window.busyImageUrl + '" style="display: none;">'),
				css: {
					top: ($("#contentDiv" + reportId).height() - 54) / 2 + 'px',
					left: ($("#contentDiv" + reportId).width() - 54) / 2 + 'px',
					width: '54px',
					height: '54px',
					border: '0px'
				},
				overlayCSS: { backgroundColor: 'transparent' }
			}
		);
		$.ajax({
			type: 'POST',
			url: window.getReportTableUrl,
			data: {
				reportId: reportId,
				reportType: reportType,
				startDate: startDate,
				endDate: endDate,
				unit: unit
			},
			success: function (data) {
				$("#contentDiv" + reportId).html(data);
				$("#datePicker").prop('disabled', false);
				$("#endDatePicker").prop('disabled', false);
				$("#select" + reportId).prop('disabled', false);
			},
			async: true
		});
	}

	function GetDetailedReport(reportId, rowName) {
		var startDate = $("#exportStartDate_" + reportId).val();
		var endDate = $("#exportEndDate_" + reportId).val();
		var unit = $('#exportUnit_' + reportId).val();
		$.ajax({
			type: 'POST',
			url: '/CDC/GetDetailedReportByRowName',
			data: {
				reportId: reportId,
				startDate: startDate,
				endDate: endDate,
				unit: unit,
				rowName: rowName
			},
			success: function (data) {
				$("#statistical_report_detail_"+reportId).html(data);
			},
			error: function (XMLHttpRequest, textStatus, errorThrown) {
			}
		});
	}

	function refreshTopChart(id, val) {
		var selected = $("#topchart_ddl1_" + id + " option:selected").text();
		var chartType = $("#topchart_ddl2_" + id + " option:selected").text();
		setTimeout(function () {
			$('#chart_container_top_' + id).html("");
			$('#chart_container_top_' + id).block(
				{
					message: $('<img src="' + window.busyImageUrl + '" style="display: none;" />'),
					css: {
						top: ($('#chart_container_top_' + id).height() - 54) / 2 + 'px',
						left: ($('#chart_container_top_' + id).width() - 54) / 2 + 'px',
						width: '54px',
						height: '54px',
						border: '0px'
					},
					overlayCSS: { backgroundColor: '#272727' ,opacity: 1}
				});
			var model = { model: val, selected: selected, chartType: chartType, isLarge:false ,isResizeable:true};
			$.ajax({
				type: 'POST',
				url: window.refreshChartUrl,
				data: model,
				success: function (result) {
					$('#chart_container_top_' + id).html(result);
					$('#chart_container_top_' + id).unblock();
					var model2 = { model: val, selected: selected, chartType: chartType, isLarge:true,isResizeable: false};
					$.ajax({
						type: 'POST',
						url: window.refreshChartUrl,
						data: model2,
						success: function (result) {
							$('#chart_container_top_large_' + id).html(result);
						}
					})
				}
			})
		},10);
	}

	function refreshBottomChart(id, val) {
		var selected = $("#bottomchart_ddl1_" + id + " option:selected").text();
		var chartType = $("#bottomchart_ddl2_" + id + " option:selected").text();
		setTimeout(function () {
			$('#chart_container_bottom_' + id).html("");
			$('#chart_container_bottom_' + id).block(
				{
					message: $('<img src="' + window.busyImageUrl + '" style="display: none;" />'),
					css: {
						top: ($('#chart_container_bottom_' + id).height() - 54) / 2 + 'px',
						left: ($('#chart_container_bottom_' + id).width() - 54) / 2 + 'px',
						width: '54px',
						height: '54px',
						border: '0px'
					},
					overlayCSS: { backgroundColor: '#272727' ,opacity: 1}
				});
			var model = { model: val, selected: selected, chartType: chartType, isLarge:false,isResizeable:true };
		    $.ajax({
		        type: 'POST',
		        url: window.refreshChartUrl,
		        data: model,
		        success: function(result) {
		            $('#chart_container_bottom_' + id).html(result);
		            $('#chart_container_bottom_' + id).unblock();
		            var model2 = { model: val, selected: selected, chartType: chartType, isLarge: true, isResizeable: false };
		            $.ajax({
		                type: 'POST',
		                url: window.refreshChartUrl,
		                data: model2,
		                success: function(result) {
		                    $('#chart_container_bottom_large_' + id).html(result);
		                }
		            });
		        }
		    });
		},0);

	}

	function ExportReport(link, reportId, isIncludeParam, isDetailedReport) {
	    var startDate = $("#datePicker" + reportId).val();
		var endDate = $("#exportEndDate_" + reportId).val();
		
		if(isIncludeParam)
		{
			endDate = "";
		}
		var unit = $('#select' + reportId).val();
		var rowName = "";

		if(isDetailedReport)
		{
			rowName = $('#exportRowName_' + reportId).val();
			if(rowName == "")
			{
				var row = $('#gridWithHoverStyle' + reportId + ' .SelectedRow');
				rowName = rowName = row.first().find('.Hide').text();
			}
		}
		
		$(link).attr("href", "/CDC/ExportReport?isIncludeParam="+isIncludeParam+"&reportId="+reportId+"&startDate="+startDate+"&endDate="+endDate+"&unit="+unit+"&rowName="+rowName+"");
	}

	function showLargeChart(id, isTop) {
		var h = 580, w = 700;
		if (isTop) {
			$("#chart_container_top_large_" + id).dialog({
				resizable: false,
				height: h,
				width: w,
				modal: true,
				close: function () {
					$("#chart_container_top_large_" + id).dialog("destroy");
				}

			});

			$("#chart_container_top_large_" + id).dialog("open");
		}
		else
		{
			$("#chart_container_bottom_large_" + id).dialog({
				resizable: false,
				height: h,
				width: w,
				modal: true,
				close: function () {
					$("#chart_container_bottom_large_" + id).dialog("destroy");
				}
			});

			$("#chart_container_bottom_large_" + id).dialog("open");
		}
	}

	;


	function showLargeBondChart(id) {
		var h = 580, w = 700;
		$("#"+id).dialog({
			resizable: false,
			height: h,
			width: w,
			modal: true,
			close: function () {
				$("#"+id).dialog("destroy");
			}
		});
		$("#"+id).dialog("open");
	}

	;
