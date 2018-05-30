angular.module('mydocument', ['ippCommon', 'ngRoute']).config([
        '$routeProvider', '$locationProvider', '$compileProvider',

        function ($routeProvider, $locationProvider, $compileProvider) {
            $routeProvider
                .when('/ipp/MyDocument/prePublish', {
                    templateUrl: '/ipp/MyDocumentPartial/prePublish',
                    controller: 'prePublishCtrl'
                })
                .when('/ipp/MyDocument/published', {
                    templateUrl: '/ipp/MyDocumentPartial/published',
                    controller: 'publishedCtrl'
                }).when('/ipp/MyDocument/preApprove', {
                    templateUrl: '/ipp/MyDocumentPartial/preApprove',
                    controller: 'preApproveCtrl'
                }).when('/ipp/MyDocument/approved', {
                    templateUrl: '/ipp/MyDocumentPartial/approved',
                    controller: 'approvedCtrl'
                })
                .when('/ipp/MyDocument/topicManagement', {
                    templateUrl: '/ipp/MyDocumentPartial/topicManagement',
                    controller: 'topicManagementCtrl'
                })
                .when('/ipp/MyDocument/FavoriteFiles', {
                    templateUrl: '/ipp/MyDocumentPartial/FavoriteFiles',
                    controller: 'favoriteFilesCtrl'
                })
                .when('/ipp/MyDocument/FavoriteTopics', {
                    templateUrl: '/ipp/MyDocumentPartial/FavoriteTopics',
                    controller: 'favoriteTopicsCtrl'
                })
                .otherwise({
                    redirectTo: function (a, href) {
                        var avaiablePath = 'prePublish|published|preApprove|approved|topicManagement|FavoriteFiles|FavoriteTopics';
                        var tabname = href.substr(href.lastIndexOf('/') + 1);
                        if (tabname && avaiablePath.toLowerCase().indexOf(tabname.toLowerCase()) != -1) {
                            return '/ipp/MyDocument/' + tabname;
                        }
                        return '/ipp/MyDocument/FavoriteFiles';
                    }
                });
            $locationProvider.html5Mode(true);
            $compileProvider.aHrefSanitizationWhitelist(/^\s*(rm|http|rmchat):/);
        }
    ])
.controller('MainCtl', ['$scope', '$location', function ($scope, $location) {
    var href = $location.path();
    $scope.selectTab = href.substr(href.lastIndexOf('/') + 1);
    $scope.updateTab = function (name) {
        $scope.selectTab = name;
    };
} ])



.controller('favoriteFilesCtrl', ['$scope', '$http', '$location', function ($scope, $http, $location) {
    $scope.updateTab('myFavorites');
    $scope.isEn = window.isEn;
    $scope.currentPage = 1;
    $scope.pageSize = 50;
    $scope.files = [];
    $scope.total = [];
    $scope.shouldBlock = false;
    $scope.showEmptyResultText = false;
    $scope.orderBy = { field: "", asc: true };

    $scope.showTopic = function () {
        $location.path('/ipp/MyDocument/FavoriteTopics');
    };

    $scope.saveRatingToServer  = function(id) {
        IPPCommon.OpenRating(id, $scope.loadData);
    };

    $scope.donwload = function (id) {
        IPPCommon.DownloadFile(id);
    };

    $scope.toggleFollowFile = function (item) {
        $http.get("/IPP/FollowFile", {
            params: {
                id: item.ID
            }
        }).success(function (data) {
            item.Following = !item.Following;
            $scope.loadData();
        });
    };

    $http.get('/IPP/GetModuleWithAll', { params: {} }).success(function (data) {
        $scope.modules = data;
        $scope.selectedModule = $scope.modules[0];
        $scope.UpdateTopic();
    });

    $scope.UpdateTopic = function () {
        $http.get('/IPP/UpdateTopicByModuleWithAll', { params: { id: $scope.selectedModule.Id} }).success(function (data) {
            $scope.topics = data;
            $scope.selectedTopic = data[0];
            $scope.orderBy = { field: "", asc: null };
            $scope.currentPage = 1;
            $scope.loadData();
        });
    };
    $scope.TopicChanged = function () {
        $scope.loadData();
    };
    $scope.queryFile = function () {
        $scope.orderBy = { field: "", asc: null };
        $scope.currentPage = 1;
        $scope.loadData();
    };
    $scope.onSelectPage = function (page) {
        $scope.currentPage = page;
        $scope.loadData();
    };

    $scope.loadData = function () {
        $scope.shouldBlock = true;
        var orderby = $scope.orderBy.field === "" ? "" : $scope.orderBy.field + " " + ($scope.orderBy.asc ? "ASC" : "DESC");
        $http.get('/IPP/GetFavoriteFiles', {
            params: {
                moduleID: $scope.selectedModule.Id,
                topicID: $scope.selectedTopic.Id,
                title: $scope.title,
                pageNo: $scope.currentPage,
                pageSize: $scope.pageSize,
                order: orderby,
                isHtml: true
            }
        })
            .success(function (data) {
                $scope.files = data.Data;
                $scope.total = data.Total;
                $scope.shouldBlock = false;
                if ($scope.files.length == 0) {
                    $scope.showEmptyResultText = true;
                }
            });
    };
    $scope.$watch("orderBy.asc", function (newValue, oldValue) {
        if (newValue === oldValue) {
            return;
        } //  first run , old value equals new value
        $scope.loadData();
    });

    $scope.$watch("orderBy.field", function (newValue, oldValue) {
        if (newValue === oldValue) {
            return;
        } //  first run , old value equals new value
        $scope.loadData();
    });
} ])

.controller('favoriteTopicsCtrl', ['$scope', '$http', '$location', function ($scope, $http, $location) {
    $scope.updateTab('myFavorites');

    $scope.isEn = window.isEn;
    $scope.currentPage = 1;
    $scope.pageSize = 50;
    $scope.total = [];
    $scope.shouldBlock = false;
    $scope.showEmptyResultText = false;
    $scope.orderBy = { field: "", asc: true };

    $scope.showFile = function () {
        $location.path('/ipp/MyDocument/FavoriteFiles');
    };

    $scope.openTopic = function (item) {
        IPPCommon.OpenTopic(item.ID);
    };

    $scope.toggleFollowTopic = function (item) {
        $http.get("/IPP/FollowTopic", {
            params: {
                id: item.ID
            }
        }).success(function (data) {
            item.FOLLOWING = !(item.FOLLOWING == '1');
            $scope.loadData();
        });
    };

    $http.get('/IPP/GetModuleWithAll', { params: {} }).success(function (data) {
        $scope.modules = data;
        $scope.selectedModule = $scope.modules[0];
        $scope.loadData();
    });

    $scope.ModuleChanged = function () {
        $scope.loadData();
    };

    $scope.queryTopic = function () {
        $scope.orderBy = { field: "", asc: null };
        $scope.currentPage = 1;
        $scope.loadData();
    };
    $scope.onSelectPage = function (page) {
        $scope.currentPage = page;
        $scope.loadData();
    };


    $scope.loadData = function () {
        $scope.shouldBlock = true;
        var orderby = $scope.orderBy.field === "" ? "" : $scope.orderBy.field + " " + ($scope.orderBy.asc ? "ASC" : "DESC");
        $http.get('/IPP/GetFavoriteTopics', {
            params: {
                moduleID: $scope.selectedModule.Id,
                topic: $scope.topic,
                pageNo: $scope.currentPage,
                pageSize: $scope.pageSize,
                order: orderby,
                isHtml: true
            }
        })
            .success(function (data) {
                $scope.topics = data.Data;
                $scope.total = data.Total;
                $scope.shouldBlock = false;
                if ($scope.topics.length == 0) {
                    $scope.showEmptyResultText = true;
                }
            });
    };
    $scope.$watch("orderBy.asc", function (newValue, oldValue) {
        if (newValue === oldValue) {
            return;
        } //  first run , old value equals new value
        $scope.loadData();
    });

    $scope.$watch("orderBy.field", function (newValue, oldValue) {
        if (newValue === oldValue) {
            return;
        } //  first run , old value equals new value
        $scope.loadData();
    });
} ])


//pre publish
.controller('prePublishCtrl', ['$scope', '$http', function ($scope, $http) {

    $scope.isEn = window.isEn;
    $scope.currentPage = 1;
    $scope.pageSize = 50;
    $scope.files = [];
    $scope.total = [];
    $scope.shouldBlock = false;
    $scope.showEmptyResultText = false;

    $scope.$on('$viewContentLoaded', function () {
        $scope.updateTab('prePublish');

        $http.get('/IPP/GetModuleWithAll', { params: {} }).success(function (data) {
            $scope.modules = data;
            $scope.selectedModule = $scope.modules[0];
            $http.get('/IPP/UpdateTopicByModuleWithAll', { params: { id: $scope.selectedModule.Id} }).success(function (data) {
                $scope.topics = data;
                $scope.selectedTopic = data[0];
                $scope.loadData();
            });
        });

    });

    $scope.deleteFile = function (id, oper) {
        IPPCommon.ShowConfirm(IPPLanguage.Texts.deleteFileConfirmMsg, function(result) {
            if (result) {
                $scope.UpdateFileStatus(id, oper);
            }
        });
    };

    $scope.UpdateFileStatus = function (id, oper) {
        $http.get("/IPP/UpdateFileStatus", {
            params: {
                id: id,
                operation: oper
            }
        }).success(function (data) {
            $scope.currentPage = 1;
            $scope.loadData();
        });
    };

    $scope.TopicChanged = function () {
        $scope.loadData();
    };

    $scope.queryFile = function () {
        $scope.currentPage = 1;
        $scope.loadData();
    };

    $scope.donwload = function (id) {
        IPPCommon.DownloadFile(id);
    };

    $scope.editFile = function (id) {
        var url = '';
        if (id == 0) {
            url = "/IPP/FileEditor?previousRequest=prePublish&topicid=" + $scope.selectedTopic.Id;
        } else {
            url = "/IPP/FileEditor?previousRequest=prePublish&id=" + id;
        }
        IPPCommon.ShowDailog({
            url: url,
            h: 710,
            w: 850
        });
    };

    $scope.UpdateTopic = function () {
        $http.get('/IPP/UpdateTopicByModuleWithAll', { params: { id: $scope.selectedModule.Id} }).success(function (data) {
            $scope.topics = data;
            $scope.selectedTopic = data[0];
            $scope.currentPage = 1;
            $scope.loadData();
        });
    };

    $scope.onSelectPage = function(page) {
        $scope.currentPage = page;
        $scope.loadData();
    };

    $scope.loadData = function () {
        $scope.shouldBlock = true;
        $http.get('/IPP/GetFilesByStatus', {
            params: {
                moduleID: $scope.selectedModule.Id,
                topicID: $scope.selectedTopic.Id,
                key: $scope.key,
                pageNo: $scope.currentPage,
                pageSize: $scope.pageSize,
                status: 0,
                isHtml: true
            }
        })
            .success(function (data) {
                $scope.files = data.Data;
                $scope.total = data.Total;
                $scope.shouldBlock = false;
                if ($scope.files.length == 0) {
                    $scope.showEmptyResultText = true;
                }
            });
    };
} ])

//published
.controller('publishedCtrl', ['$scope', '$http', function ($scope, $http) {
    $scope.isEn = window.isEn;
    $scope.currentPage = 1;
    $scope.pageSize = 50;
    $scope.files = [];
    $scope.total = [];
    $scope.shouldBlock = false;
    $scope.showEmptyResultText = false;

    $scope.$on('$viewContentLoaded', function () {
        $scope.updateTab('published');

        $http.get('/IPP/GetModuleWithAll', { params: {} }).success(function (data) {
            $scope.modules = data;
            $scope.selectedModule = $scope.modules[0];
            $http.get('/IPP/UpdateTopicByModuleWithAll', { params: { id: $scope.selectedModule.Id} }).success(function (data) {
                $scope.topics = data;
                $scope.selectedTopic = data[0];
                $scope.loadData();
            });
        });

    });

    $scope.saveRatingToServer  = function(id) {
        IPPCommon.OpenRating(id, $scope.loadData);
    };

     $scope.deleteFile = function (id, oper) {
        IPPCommon.ShowConfirm(IPPLanguage.Texts.deleteFileConfirmMsg, function(result) {
            if (result) {
               $scope.UpdateFileStatus(id, oper)
            }
        });
    };

    $scope.UpdateFileStatus = function (id, oper) {
        $http.get("/IPP/UpdateFileStatus", {
            params: {
                id: id,
                operation: oper
            }
        }).success(function (data) {
            $scope.currentPage = 1;
            $scope.loadData();
        });
    }

    $scope.TopicChanged = function () {
        $scope.loadData();
    };

    $scope.queryFile = function () {
        $scope.currentPage = 1;
        $scope.loadData();
    };

    $scope.donwload = function (id) {
        IPPCommon.DownloadFile(id);
    };

    $scope.editFile = function (id) {
        var url = '';
        if (id == 0) {
            url = "/IPP/FileEditor?previousRequest=published&topicid=" + $scope.selectedTopic.Id;
        } else {
            url = "/IPP/FileEditor?previousRequest=published&id=" + id;
        }
        IPPCommon.ShowDailog({
            url: url,
            h: 710,
            w: 850
        });
    };

    $scope.UpdateTopic = function () {
        $http.get('/IPP/UpdateTopicByModuleWithAll', { params: { id: $scope.selectedModule.Id} }).success(function (data) {
            $scope.topics = data;
            $scope.selectedTopic = data[0];
            $scope.currentPage = 1;
            $scope.loadData();
        });
    };

    $scope.onSelectPage = function (page) {
        $scope.currentPage = page;
        $scope.loadData();
    };

    $scope.loadData = function () {
        $scope.shouldBlock = true;
        $http.get('/IPP/GetFilesByStatus', {
            params: {
                moduleID: $scope.selectedModule.Id,
                topicID: $scope.selectedTopic.Id,
                key: $scope.key,
                pageNo: $scope.currentPage,
                pageSize: $scope.pageSize,
                status: 1, //action: published
                isHtml: true
            }
        })
            .success(function (data) {
                $scope.files = data.Data;
                $scope.total = data.Total;
                $scope.shouldBlock = false;
                if ($scope.files.length == 0) {
                    $scope.showEmptyResultText = true;
                }
            });
    };
} ])

//preApprove
.controller('preApproveCtrl', ['$scope', '$http', function ($scope, $http) {
    $scope.isEn = window.isEn;
    $scope.currentPage = 1;
    $scope.pageSize = 50;
    $scope.files = [];
    $scope.total = [];
    $scope.shouldBlock = false;
    $scope.showEmptyResultText = false;

    $scope.$on('$viewContentLoaded', function () {
        $scope.updateTab('preApprove');

        $http.get('/IPP/GetModuleWithAll', { params: {} }).success(function (data) {
            $scope.modules = data;
            $scope.selectedModule = $scope.modules[0];
            $http.get('/IPP/UpdateTopicByModuleWithAll', { params: { id: $scope.selectedModule.Id} }).success(function (data) {
                $scope.topics = data;
                $scope.selectedTopic = data[0];
                $scope.loadData();
            });
        });

    });

    $scope.UpdateFileStatus = function (id, oper) {
        $http.get("/IPP/UpdateFileStatus", {
            params: {
                id: id,
                operation: oper
            }
        }).success(function (data) {
            if (data.Result == 1) {
                $('#tempAlert').remove();
                var d = $('<div id="tempAlert"></div>').append(data.Message).hide();
                $('body').append(d);
                $('#tempAlert').dialog(
                {
                    autoOpen: true,
                    width: 220,
                    height: 120,
                    modal: true,
                    close: function () {
                        $scope.currentPage = 1;
                        $scope.loadData();
                    }
                });
            }
            else {
                $scope.currentPage = 1;
                $scope.loadData();
            }
        });
    };

    $scope.TopicChanged = function () {
        $scope.loadData();
    };

    $scope.queryFile = function () {
        $scope.currentPage = 1;
        $scope.loadData();
    };

    $scope.donwload = function (id) {
        IPPCommon.DownloadFile(id);
    };

    $scope.UpdateTopic = function () {
        $http.get('/IPP/UpdateTopicByModuleWithAll', { params: { id: $scope.selectedModule.Id} }).success(function (data) {
            $scope.topics = data;
            $scope.selectedTopic = data[0];
            $scope.currentPage = 1;
            $scope.loadData();
        });
    };

    $scope.onSelectPage = function (page) {
        $scope.currentPage = page;
        $scope.loadData();
    };

    $scope.loadData = function () {
        $scope.shouldBlock = true;
        $http.get('/IPP/GetFilesForApproval', {
            params: {
                moduleID: $scope.selectedModule.Id,
                topicID: $scope.selectedTopic.Id,
                key: $scope.key,
                pageNo: $scope.currentPage,
                pageSize: $scope.pageSize,
                status: 2, //action: pre approve
                isHtml: true
            }
        })
            .success(function (data) {
                $scope.files = data.Data;
                $scope.total = data.Total;
                $scope.shouldBlock = false;
                if ($scope.files.length == 0) {
                    $scope.showEmptyResultText = true;
                }
            });
    };
} ])

.controller('approvedCtrl', ['$scope', '$http', function ($scope, $http) {
    $scope.isEn = window.isEn;
    $scope.currentPage = 1;
    $scope.pageSize = 50;
    $scope.files = [];
    $scope.total = [];
    $scope.shouldBlock = false;
    $scope.showEmptyResultText = false;

    $scope.$on('$viewContentLoaded', function () {
        $scope.updateTab('approved');

        $http.get('/IPP/GetModuleWithAll', { params: {} }).success(function (data) {
            $scope.modules = data;
            $scope.selectedModule = $scope.modules[0];
            $http.get('/IPP/UpdateTopicByModuleWithAll', { params: { id: $scope.selectedModule.Id} }).success(function (data) {
                $scope.topics = data;
                $scope.selectedTopic = data[0];
                $scope.loadData();
            });
        });

    });

    $scope.saveRatingToServer  = function(id) {
        IPPCommon.OpenRating(id, $scope.loadData);
    };

     $scope.deleteFile = function (id, oper) {
        IPPCommon.ShowConfirm(IPPLanguage.Texts.deleteFileConfirmMsg, function(result) {
            if (result) {
               $scope.UpdateFileStatus(id, oper)
            }
        });
    };

    $scope.UpdateFileStatus = function (id, oper) {
        $http.get("/IPP/UpdateFileStatus", {
            params: {
                id: id,
                operation: oper
            }
        }).success(function (data) {
            $scope.currentPage = 1;
            $scope.loadData();
        });
    }

    $scope.TopicChanged = function () {
        $scope.loadData();
    };

    $scope.queryFile = function () {
        $scope.currentPage = 1;
        $scope.loadData();
    };

    $scope.donwload = function (id) {
        IPPCommon.DownloadFile(id);
    };

    $scope.editFile = function (id) {
        var url = '';
        if (id == 0) {//create
            url = "/IPP/FileEditor?previousRequest=approved&topicid=" + $scope.selectedTopic.Id;
        } else {
            url = "/IPP/FileEditor?previousRequest=approved&id=" + id;
        }
        IPPCommon.ShowDailog({
            url: url,
            h: 710,
            w: 850
        });
    };

    $scope.UpdateTopic = function () {
        $http.get('/IPP/UpdateTopicByModuleWithAll', { params: { id: $scope.selectedModule.Id} }).success(function (data) {
            $scope.topics = data;
            $scope.selectedTopic = data[0];
            $scope.currentPage = 1;
            $scope.loadData();
        });
    };

    $scope.onSelectPage = function (page) {
        $scope.currentPage = page;
        $scope.loadData();
    };

    $scope.loadData = function () {
        $scope.shouldBlock = true;
        $http.get('/IPP/GetFilesForApproval', {
            params: {
                moduleID: $scope.selectedModule.Id,
                topicID: $scope.selectedTopic.Id,
                key: $scope.key,
                pageNo: $scope.currentPage,
                pageSize: $scope.pageSize,
                status: 3, //action: approved
                isHtml: true
            }
        })
            .success(function (data) {
                $scope.files = data.Data;
                $scope.total = data.Total;
                $scope.shouldBlock = false;
                if ($scope.files.length == 0) {
                    $scope.showEmptyResultText = true;
                }
            });
    };
} ])

.controller('topicManagementCtrl', ['$scope', '$http', function ($scope, $http) {
    $scope.$on('$viewContentLoaded', function () {
        $scope.updateTab('topicManagement');
    });

    $scope.isEn = window.isEn;
    $scope.currentPage = 1;
    $scope.pageSize = 50;
    $scope.shouldBlock = false;
    $scope.orderBy = { field: "", asc: true };

    $http.get('/IPP/GetModuleWithAll', { params: {} }).success(function (data) {
        $scope.modules = data;
        $scope.selectedModule = $scope.modules[0];
        $scope.loadData();
    });

    $scope.createTopic = function () {
        IPPCommon.ShowDailog({
            url: "/IPP/TopicEditor",
            h: 670,
            w: 850
        });
    };
    $scope.editTopic = function (topic) {
        IPPCommon.ShowDailog({
            url: "/IPP/TopicEditor?id=" + topic.ID,
            h: 670,
            w: 850
        });
    };
    $scope.openTopic = function (item) {
        IPPCommon.OpenTopic(item.ID);
    };
    $scope.deleteTopic = function (item) {
        IPPCommon.ShowConfirm(IPPLanguage.Texts.deleteConfirmMsg, function(result) {
            if (result) {
                $http.get('/IPP/DeleteTopic/' + item.ID)
                    .success(function(data, status) {
                        if (data.Result === 0) {
                            $scope.loadData();
                        } else {
                            IPPCommon.ShowAlert({ message: data.Message });
                        }
                    })
                    .error(function(err) {
                        debugger;
                    });
            }
            
        });
    };
    $scope.queryTopic = function () {
        $scope.orderBy = { field: "", asc: null };
        $scope.currentPage = 1;
        $scope.loadData();
    };
    $scope.onSelectPage = function (page) {
        $scope.currentPage = page;
        $scope.loadData();
    };
    $scope.ModuleChanged = function () {
        $scope.currentPage = 1;
        $scope.loadData();
    };

    $scope.loadData = function () {
        $scope.shouldBlock = true;
        $http.get('/IPP/GetTopicsByCreater', {
            params: {
                moduleID: $scope.selectedModule.Id,
                pageNo: $scope.currentPage,
                pageSize: $scope.pageSize,
                isHtml: true
            }
        })
            .success(function (data) {
                $scope.topics = data.Data;
                $scope.total = data.Total;
                $scope.shouldBlock = false;
            });
    };
} ]);