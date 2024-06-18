// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.getElementById('notificationsDropdown').addEventListener('click', function () {
    var badge = this.querySelector('.badge');
    if (badge) {
        badge.remove();
    }
});

function ShowMessage(message) {
    Swal.fire({
        text: message,
        icon: "success"
    });
}

function cancelApplication() {
        var form = document.getElementById("cancelApplicationForm");
        var id = form.getAttribute("data-application-id");
        var url = '/InternshipApplication/Delete/' + id;
    Delete(url, 'Are you sure you want to cancel your application?','Your application is canceled!');
    
}

function deleteUser(id) {
        var url = '/Administration/DeleteUser/' + id;
    Delete(url, 'Are you sure you want to delete this user?','User deleted successfully!');
}
function updateInternship(id) {
    var url = '/Internship/Edit/' + id;
    handleRequest(url, 'POST', '', 'Internship updated!', 'Something went wrong!', false)
}
function createInternship() {
    var url = '/Internship/Create';
    handleRequest(url, 'POST', '', 'Internship created!', 'Something went wrong!', false)
}
function updateCompany(id) {
    var url = '/Company/Edit/' + id;
    handleRequest(url, 'POST', '', 'Company info updated!', 'Something went wrong!', false)
}

function Delete(url, message1, message2) {
    Swal.fire({
        title: message1,
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function () {
                    Swal.fire({
                        text: message2,
                        icon: "success"
                    }).then((result) => {
                        if (result.isConfirmed) {
                            location.reload(); // Reload the page after clicking "OK"
                        }
                    });
                },
                error: function () {
                    // Handle error or show error message if needed
                    Swal.fire({
                        text: "Error occurred while deleting.",
                        icon: "error"
                    });
                }
            });
        }
    });
}

function handleRequest(url, method, confirmMessage, successMessage, errorMessage, requireConfirmation = false) {
    const executeRequest = () => {
        $.ajax({
            url: url,
            type: method,
            success: function () {
                Swal.fire({
                    text: successMessage,
                    icon: "success"
                });
            },
            error: function () {
                Swal.fire({
                    text: errorMessage,
                    icon: "error"
                });
            }
        });
    };

    if (requireConfirmation) {
        Swal.fire({
            title: confirmMessage,
            text: "You won't be able to revert this!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Yes"
        }).then((result) => {
            if (result.isConfirmed) {
                executeRequest();
            }
        });
    } else {
        executeRequest();
    }
}




