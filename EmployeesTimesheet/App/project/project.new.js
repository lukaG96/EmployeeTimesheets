var ProjectNew = function () {
    var dropdowns = [];

    var rebindEvents = function () {
       
    };
    var loadDropdowns = function () {
        $.ajax({
            url: '/Projects/GetProjectTypes',
            success: function (data) {
                if (data.Response === "OK") {
                    dropdowns.ProjectTypes = data.items.ProjectTypes;
                    initSelect2();
                }
                else { toastr.error("Error: " + data.Result); }
            }
        });
    };
    var initSelect2 = function () {
        Helpers.elSelect2($("#Type"), dropdowns.ProjectTypes);

    };

    return {
        init: function () {
            rebindEvents();
            loadDropdowns();
        }
    };
}();

jQuery(document).ready(function () {
    ProjectNew.init();

});