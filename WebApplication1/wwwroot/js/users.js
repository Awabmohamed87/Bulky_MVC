$(document).ready(function () {
    loadDataTable();
});

var dataTable;

function loadDataTable() {


    dataTable = $('#tblDate').DataTable({
        "ajax": { url: '/admin/user/getall'},
        "columns": [
            { data: 'name' ,"width":"25%"},
            { data: 'email', "width": "15%" },
            { data: 'phoneNumber', "width": "10%" },
            { data: 'company.name', "width": "10%" },
            { data: 'role', "width": "15%" },
            {
                data: { id: 'id', lockoutEnd: 'lockoutEnd'},
                "render": function (data) {
                    var date = new Date().getTime();
                    var lockedDate = new Date(data.lockoutEnd).getTime();
                    if (lockedDate > date) {
                        return `<div class="w-75 btn-group" role="group">   
                                <a onClick=LockUnlock('${data.id}') class="btn btn-success mx-2" style = "width:100px">
                                    <i class="bi bi-unlock-fill"></i> Unlock
                                </a>
                                <a class="btn btn-success mx-2" href='/admin/user/RoleManagement?userId=${data.id}' style = "width:150px">
                                    <i class="bi bi-pencil-square"></i> Permission
                                </a>
                            </div>`;
                    }
                    else{
                        return `<div class="w-75 btn-group" role="group">   
                                <a onClick=LockUnlock('${data.id}') class="btn btn-danger mx-2"  style = "width:100px">
                                    <i class="bi bi-lock-fill"></i> Lock
                                </a>
                                <a class="btn btn-success mx-2" href='/admin/user/RoleManagement?userId=${data.id}' style = "width:150px">
                                    <i class="bi bi-pencil-square"></i> Permission
                                </a>
                            </div>`;
                    }
                    
                },
            }
        ]
    });
}

function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/admin/user/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
            
        }
    })
}