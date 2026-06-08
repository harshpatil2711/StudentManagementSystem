// StudentInsert page - Student registration form logic
$(document).ready(function () {
    // Dynamic admission year generation
    let startYear = 2000;
    let currentYear = new Date().getFullYear();
    for (let year = currentYear; year >= startYear; year--) {
        $("#AdmissionYear").append(
            `<option value="${year}">${year}</option>`
        );
    }

    $("#btnSave").click(function () {
        // Check for basic client-side check
        let studentName = $("#StudentName").val().trim();
        if (!studentName) {
            $("#message").html(
                `<div class="alert alert-danger"><i class="bi bi-exclamation-triangle-fill me-2"></i>Please enter a student name.</div>`
            );
            return;
        }

        $("#btnSave").prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1.5" role="status" aria-hidden="true"></span>Saving...');

        $.ajax({
            url: '/Student/InsertStudent',
            type: 'POST',
            data: {
                StudentName: studentName,
                DateOfBirth: $("#DateOfBirth").val(),
                Email: $("#Email").val(),
                Phone: $("#Phone").val(),
                Gender: $("#Gender").val(),
                AdmissionYear: $("#AdmissionYear").val()
            },
            success: function (response) {
                $("#btnSave").prop("disabled", false).html('<i class="bi bi-check-lg me-1.5"></i>Save Student');
                $("#message").html(
                    `<div class="alert alert-success"><i class="bi bi-check-circle-fill me-2"></i>${response}</div>`
                );
                // Clear inputs on success
                $("#StudentName").val("");
                $("#DateOfBirth").val("");
                $("#Email").val("");
                $("#Phone").val("");
                $("#Gender").val("");
                $("#AdmissionYear").val("");
            },
            error: function () {
                $("#btnSave").prop("disabled", false).html('<i class="bi bi-check-lg me-1.5"></i>Save Student');
                $("#message").html(
                    `<div class="alert alert-danger"><i class="bi bi-exclamation-octagon-fill me-2"></i>Error saving student details. Please try again.</div>`
                );
            }
        });
    });
});
