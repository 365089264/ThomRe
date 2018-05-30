/*
source:tree,menu,tab
Action Type: 1,add report;2,refresh tree;3,redirect page;0,none
*/
function MessageRouter(event) {
    //Message Format:{nodeid:123,reportid:456,action:1,nodetext:'xxx',source:'tree'}
    switch (event.action) {
        case '1':
            amplify.publish('OpenReport', { reportid: event.reportid, reportName: event.nodetext, source: event.source, treeid: event.nodeid,trace:event.trace });
            break;
        case '2':
            if(event.source != 'tree')
            amplify.publish('RefreshTree', { nodeid: event.nodeid, source: event.source });
            break;
        case '3':
            LogUsage(event.trace);
            window.open(event.ric);
            break;
        default:
    }
}

//topic:OpenReport data{ reportid:123, reportName: 'abc',source:'tree',treeid:1 }
amplify.subscribe('OpenReport', function (data) {
    OpenReport(data.reportid, data.reportName,data.trace);
});

amplify.subscribe('RefreshTree', function (data) {
    $.ajax({
        type: 'Get',
        url: window.treeRefreshURL,
        data: { id: data.nodeid },
        success: function (treehtml) {
            $('#treeView').html(treehtml);
        },
        error: function (xrequest, textStatus, errorThrown) {

        },
        async: true
    });

});

//Tab control label template
var tabTemplate = "<li><a href='#{href}'>#{label}</a></li>";
//Open report by id
function OpenReport(id, name, trace, targetRoute,title,extraParameter) {
    targetRoute = targetRoute || window.reportUrl;
    var queryString = '#tabs>.ui-tabs-nav li:has(a[href="#' + id + '"])';
    var tabIndex = $('#tabs>.ui-tabs-nav li').index($(queryString));
    if (tabIndex != -1) {
        window.tabs.tabs({ active: tabIndex });
        return;
    }
    var li = $(tabTemplate.replace(/#\{href\}/g, "#" + id).replace(/#\{label\}/g, name));
    if (title) {
        li.attr('title', title);
    }
    window.tabs.children(".ui-tabs-nav").append(li);
    var newTab = $('<div></div>').addClass('tabMaxHeight');
    newTab.attr('id', id);
    window.tabs.append(newTab);
    window.tabs.tabs("refresh");
    window.tabs[0].addtab();
    newTab.block(
        {
            css: { border: '0px solid transparent' },
            message: $('#busyLoadingDiv').clone()
        }
    );
        $.ajax({
            url: targetRoute + id,
            data: extraParameter,
            type: "GET",
            dataType: "html",
            success: function (data) {
                newTab.html(data);
                trace = trace || 'General';
                LogUsage(trace);
            },

            // code to run if the request fails;
            // the raw request and status codes are
            // passed to the function
            error: function (xhr, status) {
                debugger;
                alert(xhr.statusText);
            },

            // code to run regardless of success or failure
            complete: function (xhr, status) {
                newTab.unblock();
            }
        });
    }

function CloseReport(id) {
    window.tabs[0].removeTab(id);
}