//// InsertEnrollment page - Form submit and validation
//$(document).ready(function () {
//    $("#btnSubmit").click(function () {
//        var enrollmentId = $("#EnrollmentID").val();
//        var studentId = $("#StudentID").val();
//        var courseOfferingId = $("#CourseOfferingID").val();
//        var enrollmentDate = $("#EnrollmentDate").val();
//        var status = $("#Status").val();

//        // Basic validation
//        if (!studentId || studentId === "") {
//            showMessage("Please select a Student.", "danger");
//            return;
//        }
//        if (!courseOfferingId || courseOfferingId === "") {
//            showMessage("Please select a Course Offering.", "danger");
//            return;
//        }
//        if (!enrollmentDate) {
//            showMessage("Please select an Enrollment Date.", "danger");
//            return;
//        }
//        if (!status || status === "") {
//            showMessage("Please select a Status.", "danger");
//            return;
//        }

//        var data = {
//            EnrollmentID: enrollmentId ? parseInt(enrollmentId) : null,
//            StudentID: parseInt(studentId),
//            CourseOfferingID: parseInt(courseOfferingId),
//            EnrollmentDate: enrollmentDate,
//            Status: status
//        };

//        $.ajax({
//            url: insertEnrollmentUrl,
//            type: 'POST',
//            data: data,
//            success: function (result) {

//                if (result && result.toLowerCase().indexOf("success") !== -1) {

//                    showMessage(result, "success");

//                    setTimeout(function () {

//                        window.location.href = indexUrl;

//                    }, 1500); 

//                } else {

//                    showMessage(result, "info");
//                }
//            },
//            error: function () {
//                showMessage("An error occurred. Please try again.", "danger");
//            }
//        });
//    });

//    function showMessage(msg, type) {
//        var icon = type === "success" ? "bi-check-circle-fill"
//                 : type === "danger" ? "bi-exclamation-triangle-fill"
//                 : "bi-info-circle-fill";

//        $("#message").html(
//            '<div class="alert alert-' + type + ' d-flex align-items-center" role="alert">' +
//                '<i class="bi ' + icon + ' me-2 fs-5"></i>' +
//                '<div>' + msg + '</div>' +
//            '</div>'
//        );

//        // Auto-hide after 5 seconds
//        setTimeout(function () {
//            $("#message").fadeOut(400, function () {
//                $(this).html("").show();
//            });
//        }, 5000);
//    }
  
    
//});
