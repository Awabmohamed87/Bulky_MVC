$(document).ready(function () {
    var url = window.location.search;
    if (url.includes('inprocess'))
        loadDataTable('inprocess');
    else if (url.includes('completed'))
        loadDataTable('completed');
    else if (url.includes('pending'))
        loadDataTable('pending');
    else if (url.includes('approved'))
        loadDataTable('approved');
    else
        loadDataTable('all');
});

var dataTable;

function loadDataTable(status) {


    dataTable = $('#tblDate').DataTable({
        "ajax": { url: '/admin/order/getall?status=' + status },
        "columns": [
            { data: 'id' ,"width":"5%"},
            { data: 'name', "width": "20%" },
            { data: 'phoneNumber', "width": "20%" },
            { data: 'user.email', "width": "15%" },
            { data: 'orderStatus', "width": "10%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">   
                                <a class="btn btn-primary mx-2" href='/admin/order/details?orderId=${data}'>
                                    <i class="bi bi-pencil-fill"></i> Edit
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