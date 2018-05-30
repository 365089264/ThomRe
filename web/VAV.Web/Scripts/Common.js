function parseDate(input) {
    var parts = input.split('-');
    // new Date(year, month [, date [, hours[, minutes[, seconds[, ms]]]]])
    return new Date(parts[0], parts[1] - 1, parts[2]); // months are 0-based
}

function GetUTC(input) {
    var parts = input.split('-');
    return Date.UTC(parts[0], parts[1] - 1, parts[2]||0);
}
function AjaxWithBlock(blockObj, url, requestData, success,additionalargs) {
    blockObj.block(
            {
                message: $('<img src="' + window.busyImageUrl + '" style="display: none;" />'),
                css: {
                    top: (blockObj.height() - 54) / 2 + 'px',
                    left: (blockObj.width() - 54) / 2 + 'px',
                    width: '54px',
                    height: '54px',
                    border: '0px'
                },
                overlayCSS: { backgroundColor: '#131313' }
            }
        );
            $.ajax({
                type: 'Get',
                url: url,
                data: requestData,
                success: function (data) {
                    if (success) {
                        success(data, additionalargs);
                    }
                    blockObj.unblock();
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    debugger;
                },
                async: true
            });
        }

function LogUsage(feature) {
    var log = JSON.stringify({ "AppHitsCode": "CMA", "Feature": feature });
    JET.publish("/desktop/usagelog",log);
}


//we should unsubscript the event once tab is removed
function resetChart(chartObj) {
    if ($("#tabs>#" + chartObj.ReportID).is(":visible")) {
        var parent = $(chartObj.ContainerName).parent();
        setTimeout(function () {
            chartObj.setSize(parent.width() - 10, parent.height() - 5, false);
        }, 500);
    }
}

var ColumnManager = (function (my) {
    my.OpenColumnSetting = function (reportid, callback) {
        my.targetId = reportid;
        my.refreshMethod = callback;
        $.ajax({
            type: 'Get',
            url: '/UserSetting/GetAvailableColumns',
            data: { reportId: reportid },
            success: function (data) {
                if (data) {
                    $('#AvaiableColumns').empty();
                    $('#SelectedColumns').empty();
                    $('#columnChooser_upBtn,#columnChooser_downBtn,#columnChooser_addBtn,#columnChooser_removeBtn').attr('disabled', 'disabled');
                    $.template('columnItemTemplate', '<li id="columnChooserItem_${ID}" onclick="ColumnManager.SelectItem(this);"><a href="#">${Text}</a></li>');
                    var a = [], s = [];
                    $.each(data, function (key, value) {
                        if (value.Checked) {
                            s.push(value);
                        } else {
                            a.push(value);
                        }
                    });
                    $.tmpl("columnItemTemplate", a.sort(function (x, y) {
                        if (x.Text > y.Text) return 1;
                        else {return -1;}
                    })).appendTo('#AvaiableColumns');
                    $.tmpl("columnItemTemplate", s.sort(function (x, y) {
                        return x.Checked - y.Checked;
                    })).appendTo('#SelectedColumns');
                    $('#columnChooser_OKbtn').attr('disabled', 'disabled');
                    $("#columnChooser").dialog("open");
                }
            },
            error: function (XHRequest, textStatus) {
                debugger;
            },
            async: true
        });
    };

    my.ResetColumnSetting = function (reportid, callback) {
        my.targetId = reportid;
        my.refreshMethod = callback;
        $.ajax({
            type: 'Post',
            url: '/UserSetting/UpdateOrCreateUserColumn',
            data: { reportId: my.targetId, setting: '' },
            success: function (data) {
                if (my.refreshMethod) {
                    my.refreshMethod.call(my, my.targetId);
                }
            },
            error: function (XHRequest, textStatus) {
                debugger;
            }
        });
    };
    my.AddSelected = function () {
        $('#AvaiableColumns .selected').removeClass('selected').appendTo('#SelectedColumns');
        $('#columnChooser_addBtn').attr('disabled', 'disabled');
        resetButtons();
    };
    my.RemoveSelected = function () {
        $('#SelectedColumns .selected').removeClass('selected').appendTo('#AvaiableColumns');
        resetButtons();
    };
    my.SetUp = function () {
        var selectedLis = $('#SelectedColumns .selected');
        if (selectedLis.length != 1) return;
        var currentLi = selectedLis.get(0);
        var before = $(currentLi).prev();
        if (!before) return;
        before.before(currentLi);
        resetButtons();
    };
    my.SetDown = function () {
        var selectedLis = $('#SelectedColumns .selected');
        if (selectedLis.length != 1) return;
        var currentLi = selectedLis.get(0);
        var next = $(currentLi).next();
        if (!next) return;
        $(currentLi).before(next);
        resetButtons();
    };

    my.SelectItem = function (target) {
        var item = $(target);
        //        debugger;
        item.toggleClass('selected');
        if (item.parent().get(0).id === 'SelectedColumns') {
            resetButtons();
        } else {
            if ($('#AvaiableColumns .selected').length > 0) {
                $('#columnChooser_addBtn').removeAttr('disabled');
            } else {
                $('#columnChooser_addBtn').attr('disabled', 'disabled');
            }
        }
    };
    my.SaveSetting = function () {
        var allselectd = $('#SelectedColumns li');
        var ids = $.map(allselectd, function (a, n) {
            return a.id.replace('columnChooserItem_', '');
        });
        if (ids.length == 0) return;
        $.ajax({
            type: 'Post',
            url: '/UserSetting/UpdateOrCreateUserColumn',
            data: { reportId: my.targetId, setting: ids.join('|') },
            success: function (data) {
                $("#columnChooser").dialog("close");
                if (my.refreshMethod) {
                    my.refreshMethod.call(my, my.targetId);
                }
            },
            error: function (XHRequest, textStatus) {
                debugger;
            }
        });

    };
    my.Cancel = function () {
        $("#columnChooser").dialog("close");
    };

    function resetButtons() {
        var selectedLis = $('#SelectedColumns .selected');
        var allLis = $('#SelectedColumns li');
        if (allLis.length == 0) {
            $('#columnChooser_OKbtn').attr('disabled', 'disabled');
        } else {
            $('#columnChooser_OKbtn').removeAttr('disabled');
        }
        if (selectedLis.length > 0) {
            if (selectedLis.length == 1 && allLis.length > 1) {
                $('#columnChooser_upBtn,#columnChooser_downBtn').removeAttr('disabled');
                if (allLis.index(selectedLis) == 0) {
                    $('#columnChooser_upBtn').attr('disabled', 'disabled');
                }
                if (allLis.index(selectedLis) == allLis.length - 1) {
                    $('#columnChooser_downBtn').attr('disabled', 'disabled');
                }
            } else {
                $('#columnChooser_upBtn,#columnChooser_downBtn').attr('disabled', 'disabled');
            }
            $('#columnChooser_removeBtn').removeAttr('disabled');
        } else {
            $('#columnChooser_removeBtn').attr('disabled', 'disabled');
            $('#columnChooser_upBtn,#columnChooser_downBtn').attr('disabled', 'disabled');
        }
    }
    return my;
} (ColumnManager || {}));

function AdvanceQuery(btn, target, tableDivID, advancedSearch, hideAdvancedSearch, groupHideHeight, groupShowHeight) {
    var searchButton = $(btn);
    var advGroup = $(target);
    if (advGroup.is(":visible")) {
        advGroup.hide();
        searchButton.val(advancedSearch);
        $(tableDivID).css('height', groupHideHeight);
    } else {
        advGroup.show();
        searchButton.val(hideAdvancedSearch);
        $(tableDivID).css('height', groupShowHeight);
    }
};

function ShowAlert(id, text, width, height) {
    $('#tempAlert' + id).remove();
    var d = $('<div id="tempAlert' + id + '"></div>').append(text).hide();

    $('body').append(d);
    $('#tempAlert' + id).dialog(
    {
        autoOpen: true,
        width: width,
        height: height,
        modal: true,
        buttons: [
        {
            text: WMP.wmpcomparelanguage.alertBoxOkBtnText,
            click: function () {
                $(this).dialog("close");
            }
        }]
    });
};

function ShowDialog(title, content) {
    var messageBox = $('<div title=' + title + '><pre style="word-wrap: break-word;font-size: 1.2em;line-height: 1.8em;">' + content + '</pre></div>');
    messageBox.dialog({
        dialogClass: "dialogClass",
        modal: true,
        show: {
            effect: "blind",
            duration: 400
        },
        width: window.innerWidth / 2,
        height: window.innerHeight / 2
    });
}

var CommonJet = (function(my) {

    my.OpenGraphRic = function(ricVal) {
        //openGeneralRic("Graph", ricVal);
        var url = 'cpurl://views.cp./Explorer/Default.aspx?s=' + ricVal + '&st=RIC';
        window.open(url);
    };

    my.OpenQuoteObjectRic = function(ricVal) {
        openGeneralRic("Quote Object", ricVal);
    };

    my.OpenNewsRicWithKeyword = function(ricVal, keyword) {
        var url = '';
        if (keyword !== "") {
            url = 'reuters://REALTIME/verb=Headlines/ric=' + ricVal + ' or ' + keyword;
        } else {
            url = 'reuters://REALTIME/verb=Headlines/ric=' + ricVal;
        }
        window.open(url);
    };

    my.OpenQuoteListRic = function(ricVal) {
        var data = {
            target: "popup",
            location: { x: 100, y: 100, width: 300, height: 300 },
            name: "Quote List Object",
            entities: []
        };
        var identifiers = ricVal.split(" ");
        for (var i = 0; i < identifiers.length; i++) {
            data.entities[i] = {
                "RIC": identifiers[i]
            };
        };
        my.EnsureJetLoaded(function() {
            JET.navigate(data);
        });
    };

    function openGeneralRic(ricType, ricVal) {
        var data = {
            target: "popup",
            // open a popup window
            location: {
                x: 100,
                y: 100,
                width: 300,
                height: 300
            },
            name: ricType,
            entities: [
                {
                    "RIC": ricVal
                }
            ]
        };
        my.EnsureJetLoaded(function() {
            JET.navigate(data);
        });
    }

    my.EnsureJetLoaded = function(callback) {
        if (!JET.Loaded) {
            JET.onLoad(function() {
                callback();
            });
            JET.init({ ID: "VAVWeb"});
        } else {
            callback();
        }
    };

    return my;
} (CommonJet || {}));


var IPPCommon = (function (my) {

    var isEikon4 = (function () {
        if (typeof JET !== "undefined") {
            if (JET.Loaded) {
                return /^4./.test(JET.ContainerDescription.containerVersion);
            } else {
                return false;
            }
        } else {
            return false;
        }
    })();

    my.DownloadFile = function (id) {
        $.ajax("/IPP/GetFileInfo", {
            data: { id: id },
            success: function (data) {
                if (data.type === "file") {
                    location.href = data.url;
                    return;
                }
                if (data.type === "url") {
                    if (data.url.indexOf('/ResearchReport/DownloadFile/') != -1)
                        location.href = data.url;
                    else
                        IPPCommon.JsOpenWindow(data.url); //open external link.

                    return;
                }
                if (data.type === "ric") {
                    IPPCommon.OpenRic(data.ricType, data.ricVal);
                    return;
                }
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                debugger;
            }
        });
    };

    my.OpenRic = function (ricType, ricVal) {
        var data = {
            target: "popup",
            // open a popup window
            location: {
                x: 200,
                y: 100,
                width: 600,
                height: 400
            },
            name: ricType,
            // open a Quote Object
            entities: [
                {
                    //type: ricType,
                    "RIC": ricVal
                }
            ]
        };
        CommonJet.EnsureJetLoaded(function () {
            JET.navigate(data);
        });

    };

    my.OpenTopic = function (id) {
        my.OpenWindow('/ipp/filebrowser?id=' + id);
    };

    my.ShowDailog = function (arg) {
        $('#tempDailog').remove();
        $.ajax({
            type: 'Get',
            url: arg.url,
            success: function (data) {
                var d = $('<div id="tempDailog"></div>').append(data).hide();
                $('body').append(d);
                $('#tempDailog').dialog(
                        {
                            autoOpen: true,
                            width: arg.w,
                            height: arg.h,
                            modal: true,
                            close: typeof arg.closeHandler === "undefined" ? null : arg.closeHandler
                        });
                $('#tempDailog').css('overflow', 'hidden');
            },
            error: function (xhr, status, err) {
                debugger;
            },
            async: true
        });
    };
    my.OpenRating = function (id, closeCallback) {
        IPPCommon.ShowDailog({
            url: '/IPP/RatingDailog/' + id,
            h: 400,
            w: 550,
            closeHandler: closeCallback
        });
    };

    my.ShowAlert = function (arg) {
        $('#tempAlert').remove();
        var d = $('<div id="tempAlert"></div>').append(arg.message).hide();
        var width = typeof (arg.w) === 'undefined' ? 220 : arg.w;
        var height = typeof (arg.h) === 'undefined' ? 120 : arg.h;

        $('body').append(d);
        $('#tempAlert').dialog(
                {
                    autoOpen: true,
                    width: arg.w,
                    height: arg.h,
                    modal: true,
                    buttons: [
                    {
                        text: IPPLanguage.Texts.confirmText,
                        click: function () {
                            $(this).dialog("close");
                        }
                    }]
                });
    };

    my.ShowConfirm = function (msg, callback) {
        $("#dialog-confirm").remove();
        var d = $('<div id="dialog-confirm" title=' + IPPLanguage.Texts.promptTitle + '></div>').append('<p style="margin:12px 0;">' + msg + '</p>').hide();
        $('body').append(d);
        $("#dialog-confirm").dialog({
            resizable: false,
            height: 160,
            modal: true,
            buttons: [
                    {
                        text: IPPLanguage.Texts.confirmText,
                        click: function () {
                            $(this).dialog("close");
                            callback(true);
                        }
                    }, {
                        text: IPPLanguage.Texts.cancelText,
                        click: function () {
                            $(this).dialog("close");
                            callback(false);
                        }
                    }
                ]
        });
    };

    my.OpenWindow = function (url) {
        isEikon4 ? my.JetOpenWindow(url) : my.JsOpenWindow(url);
    };

    my.JsOpenWindow = function (url) {
        window.open(url);
    };

    my.JetOpenWindow = function (url) {
        var data = {
            target: "popup",
            url: "cpurl://vav.cp." + url
        };
        if (!JET.Loaded) {
            JET.onLoad(function () {
                console.log(JET.Loaded);

                JET.navigate(data);
            });
            JET.init({ ID: "VAVWeb" });
        } else {
            JET.navigate(data);
        }
    };



    return my;
} (IPPCommon || {}));