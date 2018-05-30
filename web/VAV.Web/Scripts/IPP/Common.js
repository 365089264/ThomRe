angular.module('ippCommon', [])
    .directive('ippsearch', [
        '$http', function ($http) {
            var cache = {};

            function link(scope, element) {
                element.keydown(function (e) {
                    if (e.keyCode == 13) {
                        if (element.val()) {
                            scope.SearchFile();
                        }
                    }
                });
                element.autocomplete({
                    delay: 500,
                    minLength: 1,
                    source: function (request, response) {
                        var term = request.term;
                        scope.searhKey = term;
                        if (term in cache) {
                            response(cache[term]);
                            return;
                        }
                        $http.get("/IPP/GetTopicByKeyWord", { params: { key: term} }).success(function (data) {
                            cache[term] = data;
                            response(data);
                        });
                    },
                    select: function (event, ui) {
                        scope.OpenTopic(ui.item.ID);
                        return false;
                    }
                }).data("ui-autocomplete")._renderItem = function (ul, item) {
                    return $("<li>")
                        .append("<a>" + item.Name + "</a>")
                        .appendTo(ul);
                };
            }

            return {
                restrict: "A",
                link: link
            };
        }
    ])
    .directive("paginationLabel", function () { //for use this directive, scope should contains "currentPage,pageSize,total," models
        return {
            restrict: "A",
            link: function (scope, el, attr) {
                var updateLabel = function () {
                    var start = (scope.currentPage - 1) * scope.pageSize + 1;
                    var end = scope.currentPage * scope.pageSize;
                    if (end > scope.total) end = scope.total;
                    var label = start + '-' + end + IPPLanguage.PagingTexts.ofText + scope.total;
                    el.html(label);
                };
                scope.$watch('currentPage', function (newValue, oldValue) {
                    if (newValue === oldValue) {
                        return;
                    } //  first run , old value equals new value
                    updateLabel();
                });
                scope.$watch('total', function (newValue, oldValue) {
                    if (newValue === oldValue) {
                        return;
                    } //  first run , old value equals new value
                    updateLabel();
                });
                updateLabel();
            }
        };
    })
//to use "paginationPager", scope should contain total,pageSize,currentPage and onSelectPage method
//if value of "currentPage" or "total" changed, this directive will update itself
    .directive("paginationPager", function () {
        return {
            restrict: "A",
            link: function (scope, el, attr) {
                var render = function () {
                    el.paginate({
                        count: Math.ceil(scope.total / scope.pageSize),
                        start: scope.currentPage,
                        display: 10,
                        border: false,
                        text_color: '#00B3E3',
                        background_color: 'none',
                        text_hover_color: '#28D2FF',
                        background_hover_color: 'none',
                        images: false,
                        mouse: 'press',
                        onChange: function (page) {
                            scope.onSelectPage(page);
                        },
                        firstText: IPPLanguage.PagingTexts.Paginate_First,
                        lastText: IPPLanguage.PagingTexts.Paginate_Last
                    });
                };
                scope.$watch('currentPage', function (newValue, oldValue) {
                    if (newValue === oldValue) {
                        return;
                    } //  first run , old value equals new value
                    render();
                });
                scope.$watch('total', function (newValue, oldValue) {
                    if (newValue === oldValue) {
                        return;
                    } //  first run , old value equals new value
                    if (newValue === 0) {
                        return;
                    }
                    render();
                });
                //render();
            }
        };
    })
    .directive("datepicker", function () {
        return {
            restrict: "A",
            link: function (scope, el, attr) {
                el.prop('readOnly', true);
                el.datepicker({
                    changeMonth: true,
                    changeYear: true,
                    dateFormat: 'yy-mm-dd'
                });
            }
        };
    })

//require "shouldBlock(bool)" in scope
//when need block the page, set shouldBlock true, 
//when need unblock the page, set shouldBlock false
    .directive("loadingBlock", function () {
        return {
            restrict: "A",
            link: function (scope, el, attr) {
                var render = function () {
                    if (scope.shouldBlock) {
                        el.block(
                            {
                                message: $('<img src="' + window.busyImageUrl + '" style="display: none; background-color: #191919;"  />'),
                                css: {
                                    top: (el.height() - 54) / 2 + 'px',
                                    left: (el.width() - 54) / 2 + 'px',
                                    width: '54px',
                                    height: '54px',
                                    border: '0px'
                                },
                                overlayCSS: { backgroundColor: '#131313' }
                            }
                        );
                    } else {
                        el.unblock();
                    }
                };
                scope.$watch('shouldBlock', function (newValue, oldValue) {
                    if (newValue === oldValue) {
                        return;
                    } //  first run , old value equals new value
                    render();
                });
            }
        };
    })

//use like this: <th sort order="(isEn ? 'TitleEn' : 'TitleCn')" by="orderBy.field" asc="orderBy.asc">标题</th>
//and watch "orderBy.field" and "orderBy.asc" in outter scope, refresh table when they changes
    .directive("sort", function () {
        return {
            restrict: 'AE',
            transclude: true,
            template:
                '<a ng-click="onClick()" style="width:100%;display:block;">' +
                    '<span ng-transclude></span>' +
                        '<i class="glyphicon" ng-class="{\'sort-by-asc\' : order === by && asc,  \'sort-by-desc\' : order===by && !asc}"></i>' +
                            '</a>',
            scope: {
                order: '=', //sort field name
                by: '=', //current sort field
                asc: '=' //sort order
            },
            link: function (scope, element, attrs) {
                element.css("cursor", "pointer");
                scope.onClick = function () {
                    if (scope.order === scope.by) { //if current sort field is clicked
                        if (scope.asc) { //if it's asc now, 
                            scope.asc = !scope.asc;
                        } else {
                            scope.by = "";
                            scope.asc = false;
                        }
                    } else {
                        scope.by = scope.order;
                        scope.asc = true;
                    }
                };
            }
        };
    })
    .directive('ippRating', function () {
        return {
            restrict: 'A',
            template: '<ul class="rating">' +
            '<li ng-repeat="star in stars" ng-class="star" ng-click="toggle($index)">' +
            '\u2605' +
            '</li>' +
            '</ul>',
            scope: {
                ratingValue: '=',
                max: '=',
                readonly: '@',
                onRatingSelected: '&'
            },
            link: function (scope, elem, attrs) {
                elem.css("text-align", "center");
                elem.prop("title", IPPLanguage.Texts.ratingToolTip);
                scope.ratingValue = scope.ratingValue || 0;
                scope.max = scope.max || 5;
                var updateStars = function () {
                    scope.stars = [];
                    for (var i = 0; i < scope.max; i++) {
                        scope.stars.push({ filled: i < scope.ratingValue });
                    }
                };
                updateStars();
                scope.toggle = function (index) {
                    if (scope.readonly && scope.readonly === 'true') {
                        return;
                    }
                    //freeze rating while display
                    //scope.ratingValue = index + 1;
                    scope.onRatingSelected();
                };

                scope.$watch('ratingValue', function (oldVal, newVal) {
                    if (newVal) {
                        updateStars();
                    }
                });
            }
        };
    });

 