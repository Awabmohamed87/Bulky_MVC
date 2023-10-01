$(document).ready(function () {
    loadDataTable();
});

var dataTable;

function loadDataTable() {


    dataTable = $('#tblDate').DataTable({
        "ajax": { url: '/admin/company/getall'},
        "columns": [
            { data: 'name' ,"width":"25%"},
            { data: 'streetAddress', "width": "15%" },
            { data: 'city', "width": "10%" },
            { data: 'state', "width": "15%" },
            { data: 'postalCode', "width": "10%" },
            { data: 'phoneNumber', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">   
                                <a class="btn btn-primary mx-2" href='/admin/company/upsert?id=${data}'>
                                    <i class="bi bi-pencil-fill"></i> Edit
                                </a>
                            
                                <a class="btn btn-danger mx-2" onClick=Delete('/admin/company/delete/${data}')>
                                    <i class="bi bi-trash-fill"></i> Delete
                                </a>
                            </div>`;
                },
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (Data) {
                    dataTable.ajax.reload();
                    toastr.success(Data.message);
                }
            })
        }
    })
}