var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadDataTable("GetAllOrder?status=inprocess");
    } else {
        if (url.includes("pending")) {
            loadDataTable("GetAllOrder?status=pending");
        } else {
            if (url.includes("completed")) {
                loadDataTable("GetAllOrder?status=completed");
            } else {
                if (url.includes("rejected")) {
                    loadDataTable("GetAllOrder?status=rejected");
                } else {
                    loadDataTable("GetAllOrder?status=all");
                }
            }
        }
    }
});

function loadDataTable(url) {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/order/" + url
        },
        "columns": [
            { "data": "id", "width": "10%" },
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "15%" },

            {
                "data": "id",
                "render": function(data) {
                    return `
                        <div class="text-center">
                            <a href="/Admin/Order/Details/${data}" class="btn btn-success text-center">
                                <i class="fas fa-edit"></i>
                            </a>
                        </div>

                            `;
                },
                "width": "5%"
            }
        ]
    });
}

